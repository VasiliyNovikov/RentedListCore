using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

namespace RentedListCore.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
public class AdditionBenchmarks
{
    private const int ItemCount = 10000;

    [Benchmark(Baseline = true)]
    public List<int> List_Add()
    {
        var list = new List<int>();
        for (var i = 0; i < ItemCount; ++i)
            list.Add(i);
        return list;
    }

    [Benchmark]
    public RentedList<int> RentedList_Add()
    {
        var list = new RentedList<int>();
        for (var i = 0; i < ItemCount; ++i)
            list.Add(i);
        return list;
    }

    [Benchmark]
    public RentedList<int> RentedList_Add_Dispose()
    {
        var list = new RentedList<int>();
        for (var i = 0; i < ItemCount; ++i)
            list.Add(i);
        list.Dispose();
        return list;
    }
}