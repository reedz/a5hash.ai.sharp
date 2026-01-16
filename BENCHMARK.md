# a5hash Performance Comparison: C vs C#

Benchmark run: 2026-01-16

## Test Configuration
- **Duration**: 1.0 seconds per test
- **Warmup**: 1000 iterations
- **Data sizes**: 4B to 1MB
- **C compiler**: GCC with `-O3 -march=native`
- **C# runtime**: .NET 10.0 Release build

## Results

### a5hash (64-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# Ratio |
|------|----------|-----------|--------------|
| 4B | 1.281 | 0.841 | 1.5x |
| 8B | 2.652 | 1.679 | 1.6x |
| 16B | 5.329 | 3.398 | 1.6x |
| 32B | 6.585 | 5.022 | 1.3x |
| 64B | 7.869 | 7.385 | 1.1x |
| 128B | 8.780 | 8.383 | 1.0x |
| 256B | 7.917 | 7.534 | 1.0x |
| 512B | 7.115 | 6.815 | 1.0x |
| 1KB | 6.609 | 6.502 | 1.0x |
| 4KB | 6.237 | 6.226 | 1.0x |
| 16KB | 6.159 | 6.197 | 1.0x |
| 64KB | 6.103 | 6.215 | 1.0x |
| 1MB | 6.152 | 6.177 | 1.0x |

**Status:** Parity achieved for medium-to-large inputs. Small inputs (4-16B) improved by ~10% by removing `fixed` overhead.

### a5hash32 (32-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# Ratio |
|------|----------|-----------|--------------|
| 4B | 1.582 | 1.938 | 0.8x |
| 8B | 3.142 | 3.900 | 0.8x |
| 16B | 4.389 | 5.420 | 0.8x |
| 32B | 5.657 | 6.754 | 0.8x |
| 64B | 7.095 | 9.399 | 0.7x |
| 128B | 7.010 | 8.918 | 0.8x |
| 256B | 6.357 | 7.681 | 0.8x |
| 512B | 6.127 | 7.163 | 0.9x |
| 1KB | 5.927 | 6.909 | 0.9x |
| 4KB | 5.688 | 6.720 | 0.8x |
| 16KB | 5.510 | 6.678 | 0.8x |
| 64KB | 5.213 | 6.662 | 0.8x |
| 1MB | 5.152 | 6.234 | 0.8x |

**Status:** C# is **significantly faster** than Native C across ALL sizes. Small inputs improved by **~25%** (1.57 -> 1.94 GB/s) by removing `fixed` overhead.

### a5hash128 (128-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# Ratio |
|------|----------|-----------|--------------|
| 4B | 1.652 | 0.414 | 4.0x |
| 8B | 3.092 | 0.799 | 3.9x |
| 16B | 6.225 | 1.611 | 3.9x |
| 32B | 7.340 | 3.022 | 2.4x |
| 64B | 11.357 | 4.467 | 2.5x |
| 128B | 11.022 | 6.145 | 1.8x |
| 256B | 12.730 | 8.114 | 1.6x |
| 512B | 13.412 | 9.529 | 1.4x |
| 1KB | 12.552 | 10.683 | 1.2x |
| 4KB | 12.288 | 11.603 | 1.1x |
| 16KB | 12.207 | 11.847 | 1.0x |
| 64KB | 11.988 | 11.200 | 1.1x |
| 1MB | 11.895 | 11.579 | 1.0x |

**Status:** No significant improvement for 4B inputs. The 128-bit function likely has other overheads dominating the small input cost (e.g., more complex math, larger state).

### CRC32 Reference

| Size | C (Software) (GB/s) | C# (Hardware) (GB/s) |
|------|---------------------|----------------------|
| 4B | 1.007 | 0.811 |
| 1KB | 0.438 | 20.622 |
| 1MB | 0.438 | 21.007 |

## Running the Benchmarks

### C Benchmark
```bash
cd tests/native
gcc -O3 -march=native benchmark.c -o benchmark
./benchmark
```

### C# Benchmark
```bash
cd tests/A5Hash.Benchmark
dotnet run -c Release
```
