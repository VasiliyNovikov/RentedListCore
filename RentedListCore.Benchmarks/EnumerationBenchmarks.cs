using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using RentedListCore.Benchmarks.CrossPlatform;

namespace RentedListCore.Benchmarks;

[ShortRunJob]
public class EnumerationBenchmarks
{
    private const int ItemCount = 10000;
    private static readonly int[] Array = [.. Enumerable.Range(0, ItemCount).Select(i => SharedRandom.Instance.Next())];
    private static readonly List<int> List = [.. Array];
    private static readonly RentedList<int> RentedList = [.. Array];

    [Benchmark(Baseline = true)]
    public int Array_Enumerator()
    {
        var list = Array;
        var sum = 0;
        foreach (var item in list)
            sum += item;
        return sum;
    }

    [Benchmark]
    public int List_Enumerator()
    {
        var list = List;
        var sum = 0;
        foreach (var item in list)
            sum += item;
        return sum;
    }

    [Benchmark]
    public int RentedList_Enumerator()
    {
        var list = RentedList;
        var sum = 0;
        foreach (var item in list)
            sum += item;
        return sum;
    }
}