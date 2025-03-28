﻿using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

namespace RentedListCore.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
public class CreationBenchmarks
{
    private const int ItemCount = 1000;
    private readonly int[] _source = [.. Enumerable.Range(0, ItemCount)];

    [Benchmark(Baseline = true)]
    public int[] Array_Create() => [.. _source];

    [Benchmark]
    public List<int> List_Create() => [.. _source];

    [Benchmark]
    public RentedList<int> RentedList_Create() => [.. _source];

    [Benchmark]
    public RentedList<int> RentedList_Create_Dispose()
    {
        RentedList<int> list = [.. _source];
        list.Dispose();
        return list;
    }
}