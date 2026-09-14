# RentedListCore

Disposable List structure allocating memory from ArrayPool

[![RentedListCore release](https://img.shields.io/nuget/v/RentedListCore)](https://www.nuget.org/packages/RentedListCore/)
[![RentedListCore download count](https://img.shields.io/nuget/dt/RentedListCore)](https://www.nuget.org/packages/RentedListCore/)

## Usage

- `RentedList<T>`: a disposable struct backed by `ArrayPool<T>.Shared`, with list operations, collection interfaces, and `Span`, `Memory`, and `Segment` views.
- `ValueRentedList<T>`: a disposable `ref struct` that starts empty with a scratch `Span<T>` and switches to the pool when it outgrows it. Supports the same list operations and span views.

```csharp
using RentedList<int> rented = [1, 2, 3];
rented.Add(4);

using var value = new ValueRentedList<int>(stackalloc int[4]);
value.AddRange(1, 2, 3, 4);
value.Add(5); // Grows into a rented array.
```

For both types, `Clear()` and `Dispose()` release storage and reset count/capacity to zero.
Copies share storage; avoid disposing multiple copies or retaining views across growth or disposal.

### Rented buffers

`RentedBuffer<T>` is a disposable struct for a fixed rental from `ArrayPool<T>.Shared`.
The constructor accepts a minimum capacity; `Capacity` reports the actual rental size.
`Array`, `Span`, `Memory`, and `Segment` expose the entire rental without copying.
Implicit conversions are available to `T[]`, `Span<T>`, `ReadOnlySpan<T>`, `Memory<T>`,
`ReadOnlyMemory<T>`, and `ArraySegment<T>`, along with ref and range indexers.

```csharp
using var buffer = new RentedBuffer<byte>(1024);
Span<byte> data = buffer;
data.Clear(); // Pooled storage is not guaranteed to be zeroed.
buffer[0] = 42;
Memory<byte> firstKilobyte = buffer.Memory[..1024];
```

Negative capacities throw; zero capacity and default buffers have empty views.
`Dispose()` returns the rental and resets capacity to zero; repeated disposal of the
same instance is harmless. Copies share storage, so dispose only one owner and stop
using all borrowed views (including the array) before disposal. Arrays are returned
without clearing their contents.

### Stack-or-pool buffers

`ValueRentedBuffer<T>` is a disposable `ref struct` for a fixed buffer. Its constructor
accepts a minimum capacity and an optional scratch `Span<T>`. It borrows the entire
scratch span if large enough; otherwise it rents directly from `ArrayPool<T>.Shared`,
without copying or modifying the scratch storage. There is no growth.

Allocate stack storage conditionally in the caller so larger requests rent without
executing a stack allocation:

```csharp
ArgumentOutOfRangeException.ThrowIfNegative(length);

const int StackLimit = 1024; // Bytes, since T is byte.
using var buffer = new ValueRentedBuffer<byte>(
    length,
    length <= StackLimit ? stackalloc byte[length] : default);

Span<byte> data = buffer.Span[..length];
data.Clear(); // Storage is not guaranteed to be zeroed.
```

Stack storage must be allocated in the caller's scope; a constructor or factory
cannot return a buffer referencing its own `stackalloc` storage. Choose stack limits
in bytes, accounting for the element size. `T` is unconstrained, but `stackalloc`
requires unmanaged elements; reference types can use array-backed scratch spans or rentals.
Omitting the scratch span rents directly for positive capacities.

`Capacity` and `Span` expose the entire selected storage, potentially larger than
requested. Ref and range indexers and implicit conversions to `Span<T>` and
`ReadOnlySpan<T>` share that storage. Negative capacities throw; zero capacity,
even with scratch storage, and default buffers have empty views.

`Dispose()` returns only owned rentals, without clearing their contents, and resets
the buffer to empty. Repeated disposal of the same instance is harmless. Copies share
storage: dispose only one owner and stop using borrowed views before disposal.
Caller-owned scratch storage remains owned by the caller.

# Benchmarks

## Adding elements
### [Code](https://github.com/VasiliyNovikov/RentedListCore/blob/master/RentedListCore.Benchmarks/AdditionBenchmarks.cs)
### Results
| Method                 | Mean     | Error     | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------- |---------:|----------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| List_Add               | 797.3 ns | 185.45 ns | 10.17 ns |  1.00 |    0.02 | 0.4473 | 0.0076 |    8424 B |        1.00 |
| RentedList_Add         | 516.5 ns |  61.73 ns |  3.38 ns |  0.65 |    0.01 | 0.2174 | 0.0029 |    4120 B |        0.49 |
| RentedList_Add_Dispose | 467.9 ns |  55.01 ns |  3.02 ns |  0.59 |    0.01 |      - |      - |         - |        0.00 |

## Enumerating elements
### [Code](https://github.com/VasiliyNovikov/RentedListCore/blob/master/RentedListCore.Benchmarks/EnumerationBenchmarks.cs)
### Results
| Method                | Mean     | Error     | StdDev    | Ratio |
|---------------------- |---------:|----------:|----------:|------:|
| Array_Enumerator      | 1.770 us | 0.0873 us | 0.0048 us |  1.00 |
| List_Enumerator       | 2.506 us | 0.2307 us | 0.0126 us |  1.42 |
| RentedList_Enumerator | 2.364 us | 0.4903 us | 0.0269 us |  1.34 |

## Indexing elements
### [Code](https://github.com/VasiliyNovikov/RentedListCore/blob/master/RentedListCore.Benchmarks/IndexingBenchmarks.cs)
### Results
| Method             | Mean     | Error     | StdDev    | Ratio | RatioSD |
|------------------- |---------:|----------:|----------:|------:|--------:|
| Array_Indexer      | 1.793 us | 0.4019 us | 0.0220 us |  1.00 |    0.02 |
| List_Indexer       | 2.650 us | 0.1405 us | 0.0077 us |  1.48 |    0.02 |
| RentedList_Indexer | 1.915 us | 0.0873 us | 0.0048 us |  1.07 |    0.01 |

## Creating collection
### [Code](https://github.com/VasiliyNovikov/RentedListCore/blob/master/RentedListCore.Benchmarks/CreationBenchmarks.cs)
### Results
| Method                    | Mean     | Error    | StdDev  | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |---------:|---------:|--------:|------:|--------:|-------:|-------:|----------:|------------:|
| Array_Create              | 119.7 ns | 23.92 ns | 1.31 ns |  1.00 |    0.01 | 0.2136 |      - |   3.93 KB |        1.00 |
| List_Create               | 130.5 ns | 45.66 ns | 2.50 ns |  1.09 |    0.02 | 0.2153 | 0.0031 |   3.96 KB |        1.01 |
| RentedList_Create         | 250.2 ns | 23.45 ns | 1.29 ns |  2.09 |    0.02 | 0.4325 | 0.0067 |   7.95 KB |        2.02 |
| RentedList_Create_Dispose | 146.4 ns | 42.75 ns | 2.34 ns |  1.22 |    0.02 | 0.2136 |      - |   3.93 KB |        1.00 |
