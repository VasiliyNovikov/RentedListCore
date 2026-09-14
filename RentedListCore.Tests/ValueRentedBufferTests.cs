using System;

namespace RentedListCore.Tests;

[TestClass]
public class ValueRentedBufferTests
{
    [TestMethod]
    public void Constructor_ShouldRejectNegativeCapacity()
    {
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            using ValueRentedBuffer<int> buffer = new(-1, stackalloc int[4]);
        });
        Assert.AreEqual("capacity", exception.ParamName);
    }

    [TestMethod]
    public void EmptyBuffers_ShouldExposeEmptyViews()
    {
        using ValueRentedBuffer<int> defaultBuffer = default;
        using ValueRentedBuffer<int> zeroBuffer = new(0);
        using ValueRentedBuffer<int> zeroWithScratch = new(0, stackalloc int[4]);
        using var emptyBuffer = ValueRentedBuffer<int>.Empty;

        AssertEmpty(defaultBuffer);
        AssertEmpty(zeroBuffer);
        AssertEmpty(zeroWithScratch);
        AssertEmpty(emptyBuffer);
    }

    [TestMethod]
    [DataRow(4)]
    [DataRow(8)]
    public void Constructor_ShouldBorrowEntireSufficientScratchBuffer(int scratchCapacity)
    {
        Span<int> scratch = stackalloc int[scratchCapacity];
        scratch.Fill(10);
        using ValueRentedBuffer<int> buffer = new(4, scratch);

        Assert.AreEqual(scratchCapacity, buffer.Capacity);
        Assert.AreSequenceEqual(scratch, buffer.Span);
        buffer[^1] = 20;
        Assert.AreEqual(20, scratch[^1]);
        scratch[0] = 30;
        Assert.AreEqual(30, buffer[0]);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(17)]
    [DataRow(1025)]
    public void Constructor_ShouldExposeEntireRental(int capacity)
    {
        using ValueRentedBuffer<int> buffer = new(capacity);

        Assert.IsGreaterThanOrEqualTo(capacity, buffer.Capacity);
        Assert.HasCount(buffer.Capacity, buffer.Span);
        buffer.Span.Fill(42);
        Assert.AreEqual(42, buffer[buffer.Capacity - 1]);
    }

    [TestMethod]
    public void Constructor_ShouldIgnoreInsufficientScratchBuffer()
    {
        Span<int> scratch = stackalloc int[3];
        scratch.Fill(10);
        using ValueRentedBuffer<int> buffer = new(4, scratch);

        Assert.IsGreaterThanOrEqualTo(4, buffer.Capacity);
        buffer.Span.Fill(20);
        Assert.AreEqual(10, scratch[0]);
        Assert.AreEqual(10, scratch[^1]);
        scratch.Fill(30);
        Assert.AreEqual(20, buffer[0]);
        Assert.AreEqual(20, buffer[^1]);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(4)]
    public void ViewsAndIndexers_ShouldShareWritableStorage(int scratchCapacity)
    {
        using ValueRentedBuffer<int> buffer = new(4, stackalloc int[scratchCapacity]);
        Span<int> span = buffer;
        ReadOnlySpan<int> readOnlySpan = buffer;
        Assert.HasCount(buffer.Capacity, span);
        Assert.HasCount(buffer.Capacity, readOnlySpan);

        ref var first = ref buffer[0];
        first = 10;
        buffer[^1] = 20;
        buffer[1..3].Fill(30);

        Assert.AreEqual(10, span[0]);
        Assert.AreEqual(20, readOnlySpan[^1]);
        Assert.AreEqual(30, span[1]);
        Assert.AreEqual(30, readOnlySpan[2]);
        Assert.HasCount(buffer.Capacity, buffer[..]);
        span[0] = 40;
        Assert.AreEqual(40, buffer[0]);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(4)]
    public void Indexers_ShouldRejectOutOfBoundsAccess(int scratchCapacity)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            using ValueRentedBuffer<int> buffer = new(4, stackalloc int[scratchCapacity]);
            _ = buffer[-1];
        });
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            using ValueRentedBuffer<int> buffer = new(4, stackalloc int[scratchCapacity]);
            _ = buffer[buffer.Capacity];
        });
        Assert.ThrowsExactly<IndexOutOfRangeException>(() =>
        {
            using ValueRentedBuffer<int> buffer = new(4, stackalloc int[scratchCapacity]);
            _ = buffer[^0];
        });
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            using ValueRentedBuffer<int> buffer = new(4, stackalloc int[scratchCapacity]);
            _ = buffer[..(buffer.Capacity + 1)];
        });
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(4)]
    public void Dispose_ShouldResetViewsAndAllowRepeatedDisposal(int scratchCapacity)
    {
        ValueRentedBuffer<int> buffer = new(4, stackalloc int[scratchCapacity]);
        buffer[0] = 42;

        buffer.Dispose();
        AssertEmpty(buffer);
        buffer.Dispose();
        AssertEmpty(buffer);

        ValueRentedBuffer<int> defaultBuffer = default;
        defaultBuffer.Dispose();
        AssertEmpty(defaultBuffer);
    }

    [TestMethod]
    public void Dispose_ShouldPreserveBorrowedReferenceStorage()
    {
        string[] scratch = ["first", "second", "third"];
        ValueRentedBuffer<string> buffer = new(1, scratch.AsSpan(1));
        Assert.AreEqual(2, buffer.Capacity);
        Assert.AreEqual("second", buffer[0]);
        buffer[0] = "changed";

        buffer.Dispose();

        AssertEmpty(buffer);
        Assert.AreEqual("first", scratch[0]);
        Assert.AreEqual("changed", scratch[1]);
        Assert.AreEqual("third", scratch[2]);
    }

    [TestMethod]
    public void Constructor_ShouldSupportRentedReferenceStorage()
    {
        using ValueRentedBuffer<string> buffer = new(1);
        buffer[0] = "value";
        Assert.AreEqual("value", buffer.Span[0]);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1024)]
    [DataRow(1025)]
    public void Constructor_ShouldSupportConditionalStackAllocation(int length)
    {
        const int StackLimit = 1024;
        using var buffer = new ValueRentedBuffer<byte>(
            length,
            length <= StackLimit ? stackalloc byte[length] : default);

        Assert.IsGreaterThanOrEqualTo(length, buffer.Capacity);
        if (length <= StackLimit)
            Assert.AreEqual(length, buffer.Capacity);
        var data = buffer.Span[..length];
        data.Fill(42);
        if (length > 0)
            Assert.AreEqual((byte)42, buffer[length - 1]);
    }

    private static void AssertEmpty<T>(ValueRentedBuffer<T> buffer)
    {
        Assert.AreEqual(0, buffer.Capacity);
        Assert.IsTrue(buffer.Span.IsEmpty);
        Span<T> span = buffer;
        ReadOnlySpan<T> readOnlySpan = buffer;
        Assert.IsTrue(span.IsEmpty);
        Assert.IsTrue(readOnlySpan.IsEmpty);
    }
}