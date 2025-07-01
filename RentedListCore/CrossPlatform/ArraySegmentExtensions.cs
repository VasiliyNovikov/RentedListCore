#if NETSTANDARD2_0
namespace System;

internal static class ArraySegmentExtensions
{
    extension<T>(ArraySegment<T>)
    {
        public static ArraySegment<T> Empty => new([]);
    }
}
#endif