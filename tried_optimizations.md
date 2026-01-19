# Tried performance optimizations

This file is a running log of **attempted** performance optimizations (kept or reverted).

## Template

- **Date:** YYYY-MM-DD
- **Area:** (e.g. a5hash32 4B/8B)
- **Change:** short description
- **Files:** `path:line` / key methods
- **Bench command:**
  - `cd tests/A5Hash.Benchmark && dotnet run -c Release -- --filter "*Hash32*" --join`
- **Reports:** (BenchmarkDotNet joined report paths)
- **Perf vs `performance_baseline.md`:** key deltas (Mean)
- **Outcome:** kept (commit SHA) / reverted (reason)

---

## Entries

- **Date:** 2026-01-19
- **Area:** a5hash32 4B/8B
- **Change:** attempt to speed up 4B/8B fast paths by inlining parts of finalization and using precomputed seed constants in `Hash32_4`/`Hash32_8`.
- **Files:** `src/A5Hash/A5Hash.cs` (`Hash32_4`, `Hash32_8`)
- **Bench command:**
  - `cd tests/A5Hash.Benchmark && dotnet run -c Release -- --filter "*Hash32*" --join`
- **Reports:**
  - `tests/A5Hash.Benchmark/BenchmarkDotNet.Artifacts/results/BenchmarkRun-joined-2026-01-19-10-40-44-report-github.md`
  - `tests/A5Hash.Benchmark/BenchmarkDotNet.Artifacts/results/BenchmarkRun-joined-2026-01-19-10-51-29-report-github.md`
  - `tests/A5Hash.Benchmark/BenchmarkDotNet.Artifacts/results/BenchmarkRun-joined-2026-01-19-11-06-03-report-github.md`
- **Perf vs `performance_baseline.md`:**
  - Run `10-40-44`: Size=4 **1.885 ns** (-10.2%), Size=8 **2.473 ns** (-5.1%)
  - Run `10-51-29`: Size=4 **2.163 ns** (+3.1%), Size=8 **2.464 ns** (-5.5%)
  - Run `11-06-03`: Size=4 **2.427 ns** (+15.6%), Size=8 **2.635 ns** (+1.1%)
- **Outcome:** reverted — results were not consistently better than `performance_baseline.md` (baseline gate violated on some runs).
