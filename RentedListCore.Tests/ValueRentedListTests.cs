using System;
using System.Collections.Generic;

namespace RentedListCore.Tests;

[TestClass]
public class ValueRentedListTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWithDefaultCapacity()
    {
        using ValueRentedList<int> list = new();
        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(0, list.Capacity);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void Constructor_ShouldInitializeWithScratchBuffer(int scratchCapacity)
    {
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(scratchCapacity, list.Capacity);
        Assert.IsTrue(list.Span.IsEmpty);

        int[] values = [10, 20, 30, 40];
        list.AddRange(values);
        Assert.AreEqual(values.Length, list.Count);
        Assert.AreSequenceEqual(values.AsSpan(), list.Span);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void Add_ShouldIncreaseCount(int scratchCapacity)
    {
        int[] values = [10, 20, 30, 40];
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        foreach (var value in values)
            list.Add(value);
        Assert.AreEqual(values.Length, list.Count);
        Assert.AreSequenceEqual(values.AsSpan(), list.Span);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public void Add_ShouldResizeWhenFull(int scratchCapacity)
    {
        int[] values = [10, 20, 30, 40];
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        foreach (var value in values)
            list.Add(value);
        Assert.AreEqual(values.Length, list.Count);
        Assert.AreSequenceEqual(values.AsSpan(), list.Span);
        Assert.IsGreaterThan(2, list.Capacity);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void AsSpan_ShouldReturnValidSpan(int scratchCapacity)
    {
        int[] values = [5, 10];
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        foreach (var value in values)
            list.Add(value);
        Assert.AreSequenceEqual(values.AsSpan(), list.Span);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void Indexer_ShouldReturnCorrectSpanSlice(int scratchCapacity)
    {
        Span<int> values = [1, 2, 3, 4];
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        foreach (var value in values)
            list.Add(value);
        var sliceRange = 1..3;
        Assert.AreSequenceEqual(values[sliceRange], list[sliceRange]);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void Dispose_ShouldReturnMemoryToPool(int scratchCapacity)
    {
        ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        list.AddRange(10, 20, 30, 40);
        list.Dispose();
        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(0, list.Capacity);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void AddRange_ShouldAppendItems(int scratchCapacity)
    {
        int[] initial = [1, 2];
        int[] additional = [3, 4];
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        list.AddRange(initial);
        list.AddRange(additional);
        int[] expected = [1, 2, 3, 4];
        Assert.AreSequenceEqual(expected.AsSpan(), list.Span);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void AddRange_ShouldAppendOwnContents(int scratchCapacity)
    {
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        list.AddRange(1, 2, 3);
        var expected = new List<int> { 1, 2, 3 };
        for (var i = 0; i < 6; ++i)
        {
            expected.AddRange(expected.ToArray());
            list.AddRange(list.Span);
            Assert.AreEqual(expected.Count, list.Count);
            Assert.AreSequenceEqual(expected.ToArray().AsSpan(), list.Span);
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void Insert_ShouldInsertInMiddle(int scratchCapacity)
    {
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        list.AddRange(1, 3);
        list.Insert(1, 2);
        int[] expected = [1, 2, 3];
        Assert.AreSequenceEqual(expected.AsSpan(), list.Span);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void Remove_ShouldRemoveFirstOccurrence(int scratchCapacity)
    {
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        list.AddRange(1, 2, 3, 2);
        Assert.IsTrue(list.Remove(2));
        int[] expected = [1, 3, 2];
        Assert.AreSequenceEqual(expected.AsSpan(), list.Span);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void Enumeration_ShouldIterateAllItems(int scratchCapacity)
    {
        int[] values = [4, 5, 6];
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        list.AddRange(values);
        var result = new List<int>();
        foreach (var v in list)
            result.Add(v);
        Assert.AreSequenceEqual(values, result);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void CopyTo_ShouldCopyItemsToArray(int scratchCapacity)
    {
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        list.AddRange(7, 8);
        var target = new int[2];
        list.CopyTo(target, 0);
        Assert.AreSequenceEqual(list.Span, target.AsSpan());
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(8)]
    public void ContainsAndIndexOf_ShouldReturnCorrectValues(int scratchCapacity)
    {
        using ValueRentedList<int> list = new(stackalloc int[scratchCapacity]);
        list.AddRange(1, 2, 3);
        Assert.IsTrue(list.Contains(2));
        Assert.IsFalse(list.Contains(4));
        Assert.AreEqual(1, list.IndexOf(2));
        Assert.AreEqual(-1, list.IndexOf(4));
    }
}