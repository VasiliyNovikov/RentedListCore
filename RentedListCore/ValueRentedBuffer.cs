using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RentedListCore;

/// <summary>A disposable buffer that borrows caller-provided storage or rents an array.</summary>
/// <remarks>
/// Views expose the entire selected storage, which may be larger than requested and is not guaranteed to be zeroed.
/// Stack storage must be allocated by the caller; pass an empty span to rent directly.
/// Copies share storage; dispose only one owner and do not use borrowed views after disposal.
/// </remarks>
public ref struct ValueRentedBuffer<T> : IDisposable
{
    private Span<T> _span;
    private T[]? _array;

    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Matches RentedBuffer<T>.Empty; ref structs cannot have static fields of their own type.")]
    public static ValueRentedBuffer<T> Empty => default;

    public readonly int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _span.Length;
    }

    public readonly Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _span;
    }

    public readonly ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)Capacity)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ref _span[index];
        }
    }

    public readonly ref T this[Index index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _span[index];
    }

    public readonly Span<T> this[Range range]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _span[range];
    }

    /// <summary>Creates a buffer with at least <paramref name="capacity"/> elements.</summary>
    /// <remarks>
    /// Zero capacity produces an empty buffer. Otherwise, borrows the entire scratch buffer if it is
    /// large enough, or rents an array without copying or modifying the scratch buffer.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueRentedBuffer(int capacity, Span<T> scratchBuffer = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _array = null;
        _span = default;
        if (capacity == 0)
            return;

        if (scratchBuffer.Length >= capacity)
            _span = scratchBuffer;
        else
            _span = _array = ArrayPool<T>.Shared.Rent(capacity);
    }

    /// <summary>Returns rented storage without clearing it and releases all views held by this instance.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_array is not null)
            ArrayPool<T>.Shared.Return(_array);
        _array = null;
        _span = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T>(ValueRentedBuffer<T> value) => value._span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(ValueRentedBuffer<T> value) => value._span;
}