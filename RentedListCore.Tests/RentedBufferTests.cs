using System;

namespace RentedListCore.Tests;

[TestClass]
public class RentedBufferTests
{
    [TestMethod]
    public void Constructor_ShouldRejectNegativeCapacity()
    {
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RentedBuffer<int>(-1));
        Assert.AreEqual("capacity", exception.ParamName);
    }

    [TestMethod]
    public void EmptyBuffers_ShouldExposeEmptyViews()
    {
        using RentedBuffer<int> defaultBuffer = default;
        using RentedBuffer<int> zeroBuffer = new(0);
        using var emptyBuffer = RentedBuffer<int>.Empty;

        AssertEmpty(defaultBuffer);
        AssertEmpty(zeroBuffer);
        AssertEmpty(emptyBuffer);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(17)]
    [DataRow(1025)]
    public void Constructor_ShouldExposeEntireRental(int capacity)
    {
        using RentedBuffer<int> buffer = new(capacity);

        Assert.IsGreaterThanOrEqualTo(capacity, buffer.Capacity);
        Assert.HasCount(buffer.Capacity, buffer.Array);
        Assert.HasCount(buffer.Capacity, buffer.Span);
        Assert.HasCount(buffer.Capacity, buffer.Memory);
        Assert.HasCount(buffer.Capacity, buffer.Segment);
        Assert.AreEqual(0, buffer.Segment.Offset);
        Assert.AreSame(buffer.Array, buffer.Segment.Array);

        buffer.Span.Fill(42);
        Assert.AreEqual(42, buffer.Array[buffer.Capacity - 1]);
    }

    [TestMethod]
    public void ViewsAndConversions_ShouldShareStorage()
    {
        using RentedBuffer<int> buffer = new(4);
        int[] array = buffer;
        Span<int> span = buffer;
        ReadOnlySpan<int> readOnlySpan = buffer;
        Memory<int> memory = buffer;
        ReadOnlyMemory<int> readOnlyMemory = buffer;
        ArraySegment<int> segment = buffer;

        Assert.AreSame(buffer.Array, array);
        Assert.AreSame(array, segment.Array);
        Assert.HasCount(buffer.Capacity, span);
        Assert.HasCount(buffer.Capacity, readOnlySpan);
        Assert.HasCount(buffer.Capacity, memory);
        Assert.HasCount(buffer.Capacity, readOnlyMemory);
        Assert.HasCount(buffer.Capacity, segment);

        array[0] = 10;
        span[1] = 20;
        memory.Span[2] = 30;
        segment.Array![segment.Offset + 3] = 40;

        int[] expected = [10, 20, 30, 40];
        Assert.AreSequenceEqual(expected.AsSpan(), buffer.Span[..4]);
        Assert.AreSequenceEqual(expected.AsSpan(), buffer.Memory.Span[..4]);
        Assert.AreSequenceEqual(expected.AsSpan(), readOnlySpan[..4]);
        Assert.AreSequenceEqual(expected.AsSpan(), readOnlyMemory.Span[..4]);

        buffer.Memory.Span[0] = 50;
        Assert.AreEqual(50, span[0]);
        buffer.Segment.Array![0] = 60;
        Assert.AreEqual(60, readOnlyMemory.Span[0]);
    }

    [TestMethod]
    public void Indexers_ShouldProvideWritableReferencesAndSlices()
    {
        using RentedBuffer<int> buffer = new(4);
        ref var first = ref buffer[0];
        first = 10;
        buffer[^1] = 20;
        buffer[1..3].Fill(30);

        Assert.AreEqual(10, buffer.Array[0]);
        Assert.AreEqual(20, buffer.Array[buffer.Capacity - 1]);
        Assert.AreEqual(30, buffer.Array[1]);
        Assert.AreEqual(30, buffer.Array[2]);
        Assert.HasCount(buffer.Capacity, buffer[..]);
    }

    [TestMethod]
    public void Indexers_ShouldRejectOutOfBoundsAccess()
    {
        using RentedBuffer<int> buffer = new(4);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => buffer[-1]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => buffer[buffer.Capacity]);
        Assert.ThrowsExactly<IndexOutOfRangeException>(() => buffer[^0]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = buffer[..(buffer.Capacity + 1)]; });
    }

    [TestMethod]
    public void Dispose_ShouldResetViewsAndAllowRepeatedDisposal()
    {
        RentedBuffer<string> buffer = new(4);
        buffer[0] = "value";

        buffer.Dispose();
        AssertEmpty(buffer);
        buffer.Dispose();
        AssertEmpty(buffer);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => buffer[0]);

        RentedBuffer<string> defaultBuffer = default;
        defaultBuffer.Dispose();
        AssertEmpty(defaultBuffer);
    }

    private static void AssertEmpty<T>(RentedBuffer<T> buffer)
    {
        Assert.AreEqual(0, buffer.Capacity);
        Assert.IsEmpty(buffer.Array);
        Assert.IsTrue(buffer.Span.IsEmpty);
        Assert.IsTrue(buffer.Memory.IsEmpty);
        Assert.IsEmpty(buffer.Segment);
        Assert.IsNotNull(buffer.Segment.Array);

        T[] array = buffer;
        Span<T> span = buffer;
        ReadOnlySpan<T> readOnlySpan = buffer;
        Memory<T> memory = buffer;
        ReadOnlyMemory<T> readOnlyMemory = buffer;
        ArraySegment<T> segment = buffer;
        Assert.IsEmpty(array);
        Assert.IsTrue(span.IsEmpty);
        Assert.IsTrue(readOnlySpan.IsEmpty);
        Assert.IsTrue(memory.IsEmpty);
        Assert.IsTrue(readOnlyMemory.IsEmpty);
        Assert.IsEmpty(segment);
    }
}