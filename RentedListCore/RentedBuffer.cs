using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace RentedListCore;

/// <summary>A disposable buffer backed by <see cref="ArrayPool{T}.Shared"/>.</summary>
/// <remarks>
/// Views expose the entire rental, which may be larger than requested and is not guaranteed to be zeroed.
/// Copies share storage; dispose only one owner and do not use borrowed views after disposal.
/// </remarks>
public struct RentedBuffer<T> : IDisposable
{
    private static readonly ArrayPool<T> ArrayPool = ArrayPool<T>.Shared;

    private T[]? _array;

    public static readonly RentedBuffer<T> Empty;

    public readonly int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array?.Length ?? 0;
    }

    public readonly T[] Array
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array ?? [];
    }

    public readonly Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array.AsSpan();
    }

    public readonly Memory<T> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array.AsMemory();
    }

    public readonly ArraySegment<T> Segment
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array is null ? ArraySegment<T>.Empty : new(_array);
    }

    public readonly ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)Capacity)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RentedBuffer(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _array = capacity > 0 ? ArrayPool.Rent(capacity) : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_array is null)
            return;

        ArrayPool.Return(_array);
        _array = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[](RentedBuffer<T> value) => value.Array;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T>(RentedBuffer<T> value) => value.Span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(RentedBuffer<T> value) => value.Span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Memory<T>(RentedBuffer<T> value) => value.Memory;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyMemory<T>(RentedBuffer<T> value) => value.Memory;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ArraySegment<T>(RentedBuffer<T> value) => value.Segment;
}