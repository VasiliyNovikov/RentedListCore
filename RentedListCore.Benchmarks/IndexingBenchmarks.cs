using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

namespace RentedListCore.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
public class IndexingBenchmarks
{
    private const int ItemCount = 10000;
    private readonly int[] _array = [.. Enumerable.Range(0, ItemCount)];
    private readonly List<int> _list = [.. Enumerable.Range(0, ItemCount)];
    private readonly RentedList<int> _rentedList = [.. Enumerable.Range(0, ItemCount)];

    [Benchmark(Baseline = true)]
    public int Array_Indexer()
    {
        var list = _array;
        var sum = 0;
        for (var i = 0; i < list.Length; ++i)
            sum += list[i];
        return sum;
    }

    [Benchmark]
    public int List_Indexer()
    {
        var list = _list;
        var sum = 0;
        for (var i = 0; i < list.Count; ++i)
            sum += list[i];
        return sum;
    }

    [Benchmark]
    public int RentedList_Indexer()
    {
        var list = _rentedList;
        var sum = 0;
        for (var i = 0; i < list.Count; ++i)
            sum += list[i];
        return sum;
    }
}