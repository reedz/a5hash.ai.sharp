# Agent Guidelines

## Performance work

When changing hashing code for performance:

1. Use **BenchmarkDotNet** (not ad-hoc Stopwatch loops) to validate performance.
2. Do **not** hardcode iteration counts / warmup counts / job types; rely on BenchmarkDotNet defaults.
3. Benchmarks must show the change is **better than or equal to** the values in `performance_baseline.md`.
4. If benchmarks regress, **revert the code changes** (e.g. `git restore <paths>`) and do **not** commit/push.
5. If benchmarks improve, update `performance_baseline.md` with the new values **before committing**, then commit and push.

Suggested commands:

```bash
# run full suite
cd tests/A5Hash.Benchmark
dotnet run -c Release

# optional: filter to a subset
cd tests/A5Hash.Benchmark
dotnet run -c Release -- --filter *Hash32*
```
