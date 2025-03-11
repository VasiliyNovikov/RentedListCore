using System;
using System.Runtime.CompilerServices;

namespace RentedListCore;

public static class RentedListBuilder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RentedList<T> Create<T>(scoped ReadOnlySpan<T> items) => new(items);
}