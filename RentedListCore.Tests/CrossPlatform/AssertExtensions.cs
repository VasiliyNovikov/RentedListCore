#if NET481
using System;

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

internal static class AssertExtensions
{
    extension(Assert)
    {
        public static void HasCount<T>(int expected, Span<T> actual, string? message = null)
        {
            Assert.AreEqual(expected, actual.Length, message);
        }

        public static void HasCount<T>(int expected, ReadOnlySpan<T> actual, string? message = null)
        {
            Assert.AreEqual(expected, actual.Length, message);
        }

        public static void HasCount<T>(int expected, Memory<T> actual, string? message = null)
        {
            Assert.AreEqual(expected, actual.Length, message);
        }

        public static void HasCount<T>(int expected, ReadOnlyMemory<T> actual, string? message = null)
        {
            Assert.AreEqual(expected, actual.Length, message);
        }

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