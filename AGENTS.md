# Agent Guidelines

## Performance work

When changing hashing code for performance:

1. Before implementing a performance tweak, check `tried_optimizations.md` to see if the same (or substantially similar) idea was already tried.
   - If it was already tried and **reverted** (or found inconclusive), skip it and try a different approach.
2. Use **BenchmarkDotNet** (not ad-hoc Stopwatch loops) to validate performance.
3. Do **not** hardcode iteration counts / warmup counts / job types; rely on BenchmarkDotNet defaults.
4. Benchmarks must show the change is **better than or equal to** the values in `performance_baseline.md`.
5. For **every** performance tweak attempt (kept or reverted), append an entry to `tried_optimizations.md` (what changed, benchmark command + report paths, key deltas vs baseline, and outcome).
6. If benchmarks regress, **revert the code changes** (e.g. `git restore <paths>`) and do **not** commit/push.
7. If benchmarks improve, update `performance_baseline.md` with the new values **before committing**, then commit and push.

Suggested commands:

```bash
# run full suite
cd tests/A5Hash.Benchmark
dotnet run -c Release

# optional: filter to a subset
cd tests/A5Hash.Benchmark
dotnet run -c Release -- --filter *Hash32*
```
