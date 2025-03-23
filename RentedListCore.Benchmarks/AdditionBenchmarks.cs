using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using RentedListCore.Benchmarks.CrossPlatform;

namespace RentedListCore.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
public class AdditionBenchmarks
{
    private const int ItemCount = 1000;
    private static readonly int[] Items = [.. Enumerable.Range(0, ItemCount).Select(i => SharedRandom.Instance.Next())];

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static TList TList_Add<TList>() where TList : IList<int>, new()
    {
        var list = new TList();
        foreach (var item in Items)
            list.Add(item);
        return list;
    }

    [Benchmark(Baseline = true)]
    public List<int> List_Add() => TList_Add<List<int>>();

    [Benchmark]
    public RentedList<int> RentedList_Add() => TList_Add<RentedList<int>>();

    [Benchmark]
    public RentedList<int> RentedList_Add_Dispose()
    {
        var list = TList_Add<RentedList<int>>();
        list.Dispose();
        return list;
    }
}