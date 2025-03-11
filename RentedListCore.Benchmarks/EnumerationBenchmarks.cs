using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

namespace RentedListCore.Benchmarks;

[ShortRunJob]
public class EnumerationBenchmarks
{
    private const int ItemCount = 10000;
    private readonly int[] _array = [.. Enumerable.Range(0, ItemCount)];
    private readonly List<int> _list = [.. Enumerable.Range(0, ItemCount)];
    private readonly RentedList<int> _rentedList = [.. Enumerable.Range(0, ItemCount)];

    [Benchmark(Baseline = true)]
    public int Array_Enumerator()
    {
        var list = _array;
        var sum = 0;
        foreach (var item in list)
            sum += item;
        return sum;
    }

    [Benchmark]
    public int List_Enumerator()
    {
        var list = _list;
        var sum = 0;
        foreach (var item in list)
            sum += item;
        return sum;
    }

    [Benchmark]
    public int RentedList_Enumerator()
    {
        var list = _rentedList;
        var sum = 0;
        foreach (var item in list)
            sum += item;
        return sum;
    }
}