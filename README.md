# RentedListCore

Disposable List structure allocating memory from ArrayPool

[![RentedListCore release](https://img.shields.io/nuget/v/RentedListCore)](https://www.nuget.org/packages/RentedListCore/)
[![RentedListCore download count](https://img.shields.io/nuget/dt/RentedListCore)](https://www.nuget.org/packages/RentedListCore/)

# Benchmarks

## Adding elements
### [Code](https://github.com/VasiliyNovikov/RentedListCore/blob/master/RentedListCore.Benchmarks/AdditionBenchmarks.cs)
### Results
| Method                 | Mean     | Error       | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------- |---------:|------------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| List_Add               | 827.2 ns | 1,177.00 ns | 64.52 ns |  1.00 |    0.09 | 0.4473 | 0.0076 |    8424 B |        1.00 |
| RentedList_Add         | 282.3 ns |    54.64 ns |  2.99 ns |  0.34 |    0.02 | 0.0043 |      - |      88 B |        0.01 |
| RentedList_Add_Dispose | 278.3 ns |    11.16 ns |  0.61 ns |  0.34 |    0.02 |      - |      - |         - |        0.00 |

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