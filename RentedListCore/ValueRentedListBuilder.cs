using System;
using System.Runtime.CompilerServices;

namespace RentedListCore;

public static class ValueRentedListBuilder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueRentedList<T> Create<T>(scoped ReadOnlySpan<T> items) => new(items);
}