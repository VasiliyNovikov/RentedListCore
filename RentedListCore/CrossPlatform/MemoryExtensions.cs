#if !NET10_0_OR_GREATER
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System;

internal static class ReadOnlySpanExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf<T>(this ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
       comparer ??= EqualityComparer<T>.Default;
       for (var i = 0; i < span.Length; ++i)
           if (comparer.Equals(span[i], value))
               return i;
       return -1;
    }
}
#endif