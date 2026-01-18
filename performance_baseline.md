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

Captured from `BenchmarkDotNet.Artifacts/results/BenchmarkRun-joined-2026-01-18-23-41-50-report-github.md`.

```
BenchmarkDotNet v0.15.8, Linux Debian GNU/Linux 12 (bookworm)
Intel N100 0.81GHz, 4 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
```

| Type                      | Method                | Size    | Chars | Mean           | Error         | StdDev        | Median          | Allocated |
|-------------------------- |---------------------- |-------- |------ |---------------:|--------------:|--------------:|----------------:|----------:|
| **A5Hash32BatchedBenchmarks** | **&#39;a5hash32 scalar x4&#39;**  | **?**       | **?**     |       **7.203 ns** |     **1.4298 ns** |      **4.148 ns** |       **6.0977 ns** |         **-** |
| A5Hash32BatchedBenchmarks | &#39;a5hash32 batched x4&#39; | ?       | ?     |      19.676 ns |     1.5599 ns |      4.575 ns |      19.8657 ns |         - |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **4**       | **?**     |       **6.021 ns** |     **0.7853 ns** |      **2.315 ns** |       **5.6951 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **8**     |       **5.031 ns** |     **0.9142 ns** |      **2.652 ns** |       **4.8232 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **8**       | **?**     |       **2.173 ns** |     **0.7822 ns** |      **2.294 ns** |       **1.3986 ns** |       **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **16**    |      **13.415 ns** |     **1.3063 ns** |      **3.852 ns** |      **12.4996 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **16**      | **?**     |       **2.031 ns** |     **0.8660 ns** |      **2.512 ns** |       **0.6088 ns** |       **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **32**      | **?**     |       **8.069 ns** |     **0.9138 ns** |      **2.666 ns** |       **7.7133 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **64**    |      **20.874 ns** |     **1.5794 ns** |      **4.632 ns** |      **20.6160 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **64**      | **?**     |      **16.050 ns** |     **1.3969 ns** |      **4.053 ns** |      **15.3277 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **128**     | **?**     |      **22.732 ns** |     **0.9016 ns** |      **2.630 ns** |      **22.8330 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **256**   |      **93.720 ns** |     **3.4645 ns** |     **10.106 ns** |      **95.7341 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **256**     | **?**     |      **40.548 ns** |     **1.9934 ns** |      **5.878 ns** |      **41.6758 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **512**     | **?**     |      **93.445 ns** |     **3.1100 ns** |      **9.170 ns** |      **95.5220 ns** |         **-** |
| **A5HashStringBenchmarks**    | **&#39;a5hash32 string&#39;**     | **?**       | **1024**  |     **409.616 ns** |    **13.0488 ns** |     **37.857 ns** |     **417.4566 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **1024**    | **?**     |     **197.145 ns** |     **5.7965 ns** |     **16.631 ns** |     **200.8683 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **4096**    | **?**     |     **813.502 ns** |    **29.7715 ns** |     **85.897 ns** |     **821.0532 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **16384**   | **?**     |   **3,086.066 ns** |   **130.8648 ns** |    **385.858 ns** |   **3,161.9700 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **65536**   | **?**     |  **12,690.301 ns** |   **470.6267 ns** |  **1,387.653 ns** |  **13,124.1001 ns** |         **-** |
| **A5HashBenchmarks**          | **&#39;a5hash32 bytes&#39;**      | **1048576** | **?**     | **217,191.671 ns** | **9,321.4223 ns** | **27,338.120 ns** | **225,660.9493 ns** |         **-** |
