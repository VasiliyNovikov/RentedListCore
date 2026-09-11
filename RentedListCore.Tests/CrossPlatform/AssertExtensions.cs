#if NET481
using System;

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

internal static class AssertExtensions
{
    extension(Assert)
    {
        public static void AreSequenceEqual<T>(Span<T> expected, Span<T> actual, string? message = null)
        {
            Assert.AreSequenceEqual((ReadOnlySpan<T>)expected, (ReadOnlySpan<T>)actual, message);
        }

        public static void AreSequenceEqual<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual, string? message = null)
        {
            Assert.AreSequenceEqual(expected.ToArray(), actual.ToArray(), message);
        }
    }
}
#endif