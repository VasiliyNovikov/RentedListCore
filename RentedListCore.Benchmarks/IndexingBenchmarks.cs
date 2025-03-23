﻿using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using RentedListCore.Benchmarks.CrossPlatform;

namespace RentedListCore.Benchmarks;

[ShortRunJob]
public class IndexingBenchmarks
{
    private const int ItemCount = 10000;
    private static readonly int[] Array = [.. Enumerable.Range(0, ItemCount).Select(i => SharedRandom.Instance.Next())];
    private static readonly List<int> List = [.. Array];
    private static readonly RentedList<int> RentedList = [.. Array];

    [Benchmark(Baseline = true)]
    public int Array_Indexer()
    {
        var list = Array;
        var sum = 0;
        for (var i = 0; i < list.Length; ++i)
            sum += list[i];
        return sum;
    }

    [Benchmark]
    public int List_Indexer()
    {
        var list = List;
        var sum = 0;
        for (var i = 0; i < list.Count; ++i)
            sum += list[i];
        return sum;
    }

    [Benchmark]
    public int RentedList_Indexer()
    {
        var list = RentedList;
        var sum = 0;
        for (var i = 0; i < list.Count; ++i)
            sum += list[i];
        return sum;
    }
}