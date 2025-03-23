using System;

namespace RentedListCore.Benchmarks.CrossPlatform;

public static class SharedRandom
{
#if NET6_0_OR_GREATER
    public static Random Instance => Random.Shared;
#else
    [ThreadStatic]
    private static Random? _instance;

    public static Random Instance => _instance ??= new Random(Environment.TickCount);
#endif
}