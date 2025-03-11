using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RentedListCore;

[CollectionBuilder(typeof(RentedListBuilder), nameof(RentedListBuilder.Create))]
public struct RentedList<T> : IList<T>, ICollection, IDisposable
{
    private static readonly ArrayPool<T> ArrayPool = ArrayPool<T>.Shared;

    private T[]? _array;
    private int _count;

    public readonly int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public readonly int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array?.Length ?? 0;
    }

    public readonly ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ref _array![index];
        }
    }

    public readonly ref T this[Index index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Span[index];
    }

    public readonly Span<T> this[Range range]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Span[range];
    }

    public readonly Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array.AsSpan(0, _count);
    }

    public readonly Memory<T> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array.AsMemory(0, _count);
    }

    public readonly ArraySegment<T> Segment
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array is null ? new ArraySegment<T>([]) : new ArraySegment<T>(_array, 0, _count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RentedList(int initialCapacity)
    {
        _array = initialCapacity > 0 ? ArrayPool.Rent(initialCapacity) : null;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RentedList(params ReadOnlySpan<T> items)
    {
        if (items.Length == 0)
            _array = null;
        else
            items.CopyTo(_array = ArrayPool.Rent(items.Length));
        _count = items.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (_array is null)
            return;

        ArrayPool.Return(_array);
        _array = null;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) => Insert(_count, item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)_count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (_array is null)
            _array = ArrayPool.Rent(1);
        else
        {
            var tailCount = _count - index;
            if (_count >= _array.Length)
            {
                var newArray = ArrayPool.Rent(_array.Length + 1);
                Array.Copy(_array, 0, newArray, 0, index);
                if (tailCount > 0)
                    Array.Copy(_array, index, newArray, index + 1, tailCount);
                ArrayPool.Return(_array);
                _array = newArray;
            }
            else if (tailCount > 0)
                Array.Copy(_array, index, _array, index + 1, tailCount);
        }

        _array[index] = item;
        ++_count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)_count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var tailCount = _count - index - 1;
        if (tailCount > 0)
            Array.Copy(_array!, index + 1, _array!, index, tailCount);
        --_count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;
        RemoveAt(index);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(T[] array, int arrayIndex) => Span.CopyTo(array.AsSpan(arrayIndex));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int IndexOf(T item) => _array is null ? -1 : Array.IndexOf(_array!, item, 0, _count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(T item) => IndexOf(item) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator() => new(_array, _count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T>(RentedList<T> value) => value.Span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(RentedList<T> value) => value.Span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Memory<T>(RentedList<T> value) => value.Memory;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyMemory<T>(RentedList<T> value) => value.Memory;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ArraySegment<T>(RentedList<T> value) => value.Segment;

    #region IList<T> implementation

    readonly T IList<T>.this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[index] = value;
    }

    readonly bool ICollection<T>.IsReadOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    readonly bool ICollection.IsSynchronized => false;

    readonly object ICollection.SyncRoot => throw new NotSupportedException();

    readonly void ICollection.CopyTo(Array array, int index)
    {
        if (_array is not null)
            Array.Copy(_array, 0, array, index, _count);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[]? _array;
        private readonly int _count;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _index = -1;

        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array![_index];
        }

        readonly object? IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(T[]? array, int count)
        {
            _array = array;
            _count = count;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() { }
    }
}