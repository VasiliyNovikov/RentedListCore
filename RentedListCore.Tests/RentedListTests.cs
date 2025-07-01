using System;
using System.Collections.Generic;

namespace RentedListCore.Tests;

[TestClass]
public class RentedListTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWithDefaultCapacity()
    {
        using RentedList<int> list = [];
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
        using RentedList<int> list = [];
        foreach (var value in values)
            list.Add(value);
        CollectionAssert.AreEqual(values, list.Span.ToArray());
    }

    [TestMethod]
    public void Indexer_ShouldReturnCorrectSpanSlice()
    {
        Span<int> values = [1, 2, 3, 4];
        using RentedList<int> list = [];
        foreach (var value in values)
            list.Add(value);
        var sliceRange = 1..3;
        CollectionAssert.AreEqual(values[sliceRange].ToArray(), list[sliceRange].ToArray());
    }

    [TestMethod]
    public void Dispose_ShouldReturnMemoryToPool()
    {
        RentedList<int> list = [];
        list.Add(10);
        list.Dispose();
        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(0, list.Capacity);
    }

    [TestMethod]
    public void AddRange_ShouldAppendItems()
    {
        int[] initial = [1, 2];
        int[] additional = [3, 4];
        using RentedList<int> list = new(initial);
        list.AddRange(additional);
        int[] expected = [1, 2, 3, 4];
        CollectionAssert.AreEqual(expected, list);
    }

    [TestMethod]
    public void Insert_ShouldInsertInMiddle()
    {
        using RentedList<int> list = [1, 3];
        list.Insert(1, 2);
        int[] expected = [1, 2, 3];
        CollectionAssert.AreEqual(expected, list);
    }

    [TestMethod]
    public void Remove_ShouldRemoveFirstOccurrence()
    {
        using RentedList<int> list = [1, 2, 3, 2];
        bool removed = list.Remove(2);
        Assert.IsTrue(removed);
        int[] expected = [1, 3, 2];
        CollectionAssert.AreEqual(expected, list);
    }

    [TestMethod]
    public void Enumeration_ShouldIterateAllItems()
    {
        int[] values = [4, 5, 6];
        using RentedList<int> list = [4, 5, 6];
        var result = new List<int>();
        foreach (var v in list)
            result.Add(v);
        CollectionAssert.AreEqual(values, result);
    }

    [TestMethod]
    public void CopyTo_ShouldCopyItemsToArray()
    {
        using RentedList<int> list = [7, 8];
        int[] target = new int[2];
        list.CopyTo(target, 0);
        CollectionAssert.AreEqual(list, target);
    }

    [TestMethod]
    public void ContainsAndIndexOf_ShouldReturnCorrectValues()
    {
        using RentedList<int> list = [1, 2, 3];
        Assert.IsTrue(list.Contains(2));
        Assert.IsFalse(list.Contains(4));
        Assert.AreEqual(1, list.IndexOf(2));
        Assert.AreEqual(-1, list.IndexOf(4));
    }
}