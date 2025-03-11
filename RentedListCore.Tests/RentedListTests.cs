using System;

namespace RentedListCore.Tests;

[TestClass]
public class RentedListTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWithDefaultCapacity()
    {
        using RentedList<int> list = new();
        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(0, list.Capacity);
    }

    [TestMethod]
    public void Constructor_ShouldInitializeFromSpan()
    {
        int[] values = [1, 2, 3];
        using RentedList<int> list = new(values);
        CollectionAssert.AreEqual(values, list);
    }

    [TestMethod]
    public void Constructor_ShouldInitializeFromCollectionExpression()
    {
        int[] values = [1, 2, 3];
        using RentedList<int> list = [1, 2, 3];
        CollectionAssert.AreEqual(values, list);
    }

    [TestMethod]
    public void Add_ShouldIncreaseCount()
    {
        int[] values = [10, 20, 30, 40];
        using RentedList<int> list = [];
        foreach (var value in values)
            list.Add(value);
        Assert.AreEqual(values.Length, list.Count);
        CollectionAssert.AreEqual(values, list);
    }

    [TestMethod]
    public void Add_ShouldResizeWhenFull()
    {
        int[] values = [10, 20, 30, 40];
        using RentedList<int> list = new(2);
        foreach (var value in values)
            list.Add(value);
        Assert.AreEqual(values.Length, list.Count);
        CollectionAssert.AreEqual(values, list);
        Assert.IsTrue(list.Capacity > 2);
    }

    [TestMethod]
    public void AsSpan_ShouldReturnValidSpan()
    {
        int[] values = [5, 10];
        using RentedList<int> list = new();
        foreach (var value in values)
            list.Add(value);
        CollectionAssert.AreEqual(values, list.Span.ToArray());
    }

    [TestMethod]
    public void Indexer_ShouldReturnCorrectSpanSlice()
    {
        Span<int> values = [1, 2, 3, 4];
        using RentedList<int> list = new();
        foreach (var value in values)
            list.Add(value);
        var sliceRange = 1..3;
        CollectionAssert.AreEqual(values[sliceRange].ToArray(), list[sliceRange].ToArray());
    }

    [TestMethod]
    public void Dispose_ShouldReturnMemoryToPool()
    {
        RentedList<int> list = new();
        list.Add(10);
        list.Dispose();
        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(0, list.Capacity);
    }
}