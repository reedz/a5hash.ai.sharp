# Performance baseline

> Generated via BenchmarkDotNet. Update this file whenever performance-affecting changes are committed.

## How to reproduce

```bash
cd tests/A5Hash.Benchmark

# full suite (non-interactive; defaults + joined report)
dotnet run -c Release

# a5hash32-focused subset (used for the baseline below)
dotnet run -c Release -- --filter "*Hash32*" --join
```

## Results (a5hash32 subset)

Captured from `tests/A5Hash.Benchmark/BenchmarkDotNet.Artifacts/results/BenchmarkRun-joined-2026-01-19-10-24-23-report-github.md`.

```
BenchmarkDotNet v0.15.8, Linux Debian GNU/Linux 12 (bookworm)
Intel N100 0.81GHz, 4 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
```

| Type                      | Method                | Size    | Chars | Mean           | Error         | StdDev        | Median         | Allocated |
|-------------------------- |---------------------- |-------- |------ |---------------:|--------------:|--------------:|---------------:|----------:|
| **A5Hash32BatchedBenchmarks** | **&#39;a5hash32 scalar x4&#39;**  | **?**       | **?**     |       **5.792 ns** |     **0.0785 ns** |     **0.0656 ns** |       **5.781 ns** |         **-** |
| A5Hash32BatchedBenchmarks | &#39;a5hash32 batched x4&#39; | ?       | ?     |      12.026 ns |     0.1746 ns |     0.1547 ns |      11.991 ns |         - |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **4**       | **?**     |       **2.099 ns** |     **0.0633 ns** |     **0.0592 ns** |       **2.116 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **8**     |       **3.967 ns** |     **0.1997 ns** |     **0.5729 ns** |       **3.746 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **8**       | **?**     |       **2.606 ns** |     **0.1007 ns** |     **0.1988 ns** |       **2.507 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **16**    |       **8.168 ns** |     **0.1797 ns** |     **0.2851 ns** |       **8.077 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **16**      | **?**     |       **3.421 ns** |     **0.1126 ns** |     **0.0998 ns** |       **3.406 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **32**      | **?**     |       **7.127 ns** |     **0.1647 ns** |     **0.1286 ns** |       **7.101 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **64**    |      **17.746 ns** |     **0.3978 ns** |     **0.4582 ns** |      **17.623 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **64**      | **?**     |      **10.485 ns** |     **0.1178 ns** |     **0.1044 ns** |      **10.490 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **128**     | **?**     |      **16.453 ns** |     **0.2059 ns** |     **0.1926 ns** |      **16.434 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **256**   |      **68.761 ns** |     **0.5372 ns** |     **0.4762 ns** |      **68.809 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **256**     | **?**     |      **32.071 ns** |     **0.3967 ns** |     **0.3313 ns** |      **32.044 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **512**     | **?**     |      **68.964 ns** |     **0.4210 ns** |     **0.3938 ns** |      **68.883 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **1024**  |     **301.214 ns** |     **2.5885 ns** |     **2.4213 ns** |     **301.400 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **1024**    | **?**     |     **143.301 ns** |     **1.4458 ns** |     **1.3524 ns** |     **143.144 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **4096**    | **?**     |     **593.169 ns** |     **5.1680 ns** |     **4.5813 ns** |     **594.169 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **16384**   | **?**     |   **2,395.673 ns** |    **21.7128 ns** |    **19.2479 ns** |   **2,402.051 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **65536**   | **?**     |   **9,765.532 ns** |    **62.1274 ns** |    **58.1141 ns** |   **9,766.658 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **1048576** | **?**     | **156,690.547 ns** | **1,499.8254 ns** | **1,252.4221 ns** | **156,510.258 ns** |         **-** |
