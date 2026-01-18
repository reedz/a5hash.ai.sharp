# Benchmarks

Benchmarks are run via **BenchmarkDotNet** (defaults; no manual iteration/warmup/job configuration).

For the latest captured numbers, see `performance_baseline.md`.

## How to reproduce

C# (BenchmarkDotNet):

```bash
cd tests/A5Hash.Benchmark

# full suite (non-interactive; defaults + joined report)
dotnet run -c Release

# filter (examples)
dotnet run -c Release -- --filter "*Hash32*" --join
```

Native (optional comparison):

```bash
cd tests/native
gcc -O3 -march=native benchmark.c -o benchmark
./benchmark
```
