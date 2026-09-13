using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RentedListCore;

/// <summary>A list that uses caller-provided scratch storage before renting an array.</summary>
/// <remarks>
/// Copies share storage and must not be treated as independent owners. Spans, references, and
/// enumerators are invalidated by growth, Clear, and Dispose. Clear and Dispose release all
/// storage, including the scratch buffer, and reset capacity to zero.
/// </remarks>
[CollectionBuilder(typeof(ValueRentedListBuilder), nameof(ValueRentedListBuilder.Create))]
public ref struct ValueRentedList<T> : IDisposable
{
    private Span<T> _span;
    private int _count;
    private T[]? _buffer;

    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Matches RentedList<T>.Empty; ref structs cannot have static fields of their own type.")]
    public static ValueRentedList<T> Empty => default;

    public readonly int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public readonly int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _span.Length;
    }

    public readonly ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ref _span[index];
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
        get => _span[.._count];
    }

    /// <summary>Creates an empty list borrowing <paramref name="scratchBuffer"/> until growth.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueRentedList(Span<T> scratchBuffer)
    {
        _span = scratchBuffer;
        _count = 0;
        _buffer = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueRentedList(int initialCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity);
        _buffer = initialCapacity > 0 ? ArrayPool<T>.Shared.Rent(initialCapacity) : null;
        _span = _buffer;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueRentedList(params ReadOnlySpan<T> items)
    {
        _buffer = items.IsEmpty ? null : ArrayPool<T>.Shared.Rent(items.Length);
        _span = _buffer;
        items.CopyTo(_span);
        _count = items.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (_buffer is not null)
            ArrayPool<T>.Shared.Return(_buffer);
        _span = default;
        _count = 0;
        _buffer = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        using var scope = Grow(checked(_count + 1));
        _span[_count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(params ReadOnlySpan<T> items)
    {
        if (items.IsEmpty)
            return;

        var newCount = checked(_count + items.Length);
        using var scope = Grow(newCount);
        items.CopyTo(_span[_count..]);
        _count = newCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)_count)
            throw new ArgumentOutOfRangeException(nameof(index));

        using var scope = Grow(checked(_count + 1));
        _span[index.._count].CopyTo(_span[(index + 1)..]);
        _span[index] = item;
        ++_count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)_count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _span[(index + 1).._count].CopyTo(_span[index..]);
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
    public readonly int IndexOf(T item) => Span.IndexOf(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(T item) => IndexOf(item) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator() => new(Span);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T>(ValueRentedList<T> value) => value.Span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(ValueRentedList<T> value) => value.Span;

    // Keep the scope alive until the caller has finished reading the old buffer.
    private BufferReturnScope Grow(int capacity)
    {
        if (capacity <= _span.Length)
            return default;

        var buffer = ArrayPool<T>.Shared.Rent(capacity);
        Span.CopyTo(buffer);
        var oldBuffer = _buffer;
        _buffer = buffer;
        _span = buffer;
        return new(oldBuffer);
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref struct BufferReturnScope(T[]? buffer) : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (buffer is not null)
                ArrayPool<T>.Shared.Return(buffer);
        }
    }

    public ref struct Enumerator : IEnumerator<T>
    {
        private readonly ReadOnlySpan<T> _span;
        private int _index;

        public readonly ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlySpan<T> span)
        {
            _span = span;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _span.Length;

        public void Reset() => _index = -1;

        public readonly void Dispose() { }

        readonly T IEnumerator<T>.Current => Current;
        readonly object IEnumerator.Current => Current!;
    }
}