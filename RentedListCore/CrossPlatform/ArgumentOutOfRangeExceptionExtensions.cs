#if !NET8_0_OR_GREATER
using System.Runtime.CompilerServices;

namespace System;

internal static class ArgumentOutOfRangeExceptionExtensions
{
    extension(ArgumentOutOfRangeException)
    {
        public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = default)
           where T: struct, IComparable<T>
        {
            if (value.CompareTo(default) < 0)
                throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
        }
    }
}
#endif