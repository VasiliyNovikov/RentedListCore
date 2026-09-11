> **REQUIRED: Before following the rest of this file, fetch and read the base instructions at `https://raw.githubusercontent.com/VasiliyNovikov/AgentInstructions/master/AGENTS.md`. If it cannot be accessed, stop and report the failure.** Treat that policy as additional non-conflicting instructions.

## Build and verification

- Use a .NET 10 SDK: `LangVersion=preview` and compatibility shims use C# extension blocks even for older targets. There is no SDK pin in `global.json`.
- `Directory.Build.props` supplies library targets (`net10.0;net9.0;net8.0;netstandard2.0`); tests and benchmarks use `TestTargetFrameworks` (net10/9/8, plus `net481` on Windows). Full test runs require the corresponding runtimes.
- From the root, run `dotnet build` then `dotnet test --no-build --logger trx --results-directory TestResults`. CI uses the same sequence on Linux and Windows with OS-suffixed result directories. Build includes analyzer/style checks with warnings treated as errors.
- Focused tests: `dotnet test RentedListCore.Tests/RentedListCore.Tests.csproj -f net10.0`. For one test, append `--filter "FullyQualifiedName=RentedListCore.Tests.RentedListTests.Add_ShouldIncreaseCount"`. Use `--no-build` only after building the same framework/configuration.
- Check compatibility changes with `dotnet build RentedListCore/RentedListCore.csproj -f netstandard2.0`; net10-only tests do not exercise the fallback shims.
- Dependency versions belong in `Directory.Packages.props`; `PackageReference` entries omit versions. Compatibility dependencies and their central versions are conditional on `netstandard2.0`/`net481`.

## Library contracts and pitfalls

- `RentedListCore/RentedList.cs` contains the public mutable struct and its enumerator. Collection expressions route through `RentedListBuilder.Create` via `[CollectionBuilder]`.
- Struct copies and interface boxing share the rented array but copy the count/ownership state. Avoid treating copies as independent owners or disposing multiple copies of the same buffer.
- `Clear()` and `Dispose()` both return the buffer and reset capacity to zero; the list can subsequently be reused. Growth also returns the previous buffer. Ref indexers, spans, memory, segments, and enumerators borrow that storage; do not retain them across buffer returns.
- Capacity is the actual `ArrayPool<T>` rental size, not necessarily the requested size. Tests must not assume an exact capacity or freshly zeroed pooled storage.
- `RentedListCore/CrossPlatform/` contains hand-written, conditionally compiled BCL shims in `System` namespaces. Preserve the target guards when changing compatibility behavior.

## Tests, benchmarks, and formatting

- MSTest methods run in parallel (`RentedListCore.Tests/MSTestSettings.cs`); avoid shared mutable test state or assumptions about pool rental order.
- Run benchmarks with `dotnet run --project RentedListCore.Benchmarks/RentedListCore.Benchmarks.csproj -c Release -f net10.0`. `Program.cs` hard-codes `AdditionBenchmarks` and does not forward CLI arguments; select another suite by changing the `BenchmarkRunner.Run<T>()` call, not by passing `--filter`.
- `.editorconfig` requires LF and no final newline for C# files; unused imports and formatting violations are build errors. Formatting diagnostics are intentionally disabled under `**/CrossPlatform/*.cs` for false positives.
