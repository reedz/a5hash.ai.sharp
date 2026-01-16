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
| 4B | 1.322 | 0.756 | 1.7x |
| 8B | 2.659 | 1.525 | 1.7x |
| 16B | 5.328 | 3.027 | 1.8x |
| 32B | 6.597 | 4.833 | 1.4x |
| 64B | 7.480 | 7.033 | 1.1x |
| 128B | 8.314 | 8.503 | 1.0x |
| 256B | 8.015 | 7.657 | 1.0x |
| 512B | 7.083 | 6.548 | 1.1x |
| 1KB | 6.606 | 4.133 | 1.6x |
| 4KB | 6.195 | 6.217 | 1.0x |
| 16KB | 6.140 | 6.202 | 1.0x |
| 64KB | 5.774 | 6.171 | 0.9x |
| 1MB | 5.646 | 6.137 | 0.9x |

### a5hash32 (32-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# Ratio |
|------|----------|-----------|--------------|
| 4B | 1.417 | 1.517 | 0.9x |
| 8B | 2.788 | 3.060 | 0.9x |
| 16B | 3.826 | 4.678 | 0.8x |
| 32B | 4.951 | 6.201 | 0.8x |
| 64B | 6.189 | 7.275 | 0.9x |
| 128B | 6.232 | 7.585 | 0.8x |
| 256B | 5.806 | 6.478 | 0.9x |
| 512B | 5.694 | 6.033 | 0.9x |
| 1KB | 5.552 | 5.809 | 1.0x |
| 4KB | 5.339 | 5.552 | 1.0x |
| 16KB | 5.327 | 5.549 | 1.0x |
| 64KB | 5.306 | 5.517 | 1.0x |
| 1MB | 5.241 | 5.453 | 1.0x |

### a5hash128 (128-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# Ratio |
|------|----------|-----------|--------------|
| 4B | 1.667 | 0.459 | 3.6x |
| 8B | 3.284 | 0.926 | 3.5x |
| 16B | 6.511 | 1.817 | 3.6x |
| 32B | 6.916 | 3.185 | 2.2x |
| 64B | 11.376 | 4.533 | 2.5x |
| 128B | 11.367 | 6.052 | 1.9x |
| 256B | 12.904 | 8.072 | 1.6x |
| 512B | 13.259 | 9.893 | 1.3x |
| 1KB | 12.849 | 10.803 | 1.2x |
| 4KB | 10.502 | 11.813 | 0.9x |
| 16KB | 12.039 | 12.083 | 1.0x |
| 64KB | 14.291 | 11.455 | 1.2x |
| 1MB | 14.758 | 11.075 | 1.3x |

### CRC32 Reference

| Size | C (Software) (GB/s) | C# (Hardware) (GB/s) |
|------|---------------------|----------------------|
| 4B | 1.048 | 0.810 |
| 8B | 1.045 | 0.789 |
| 16B | 0.849 | 2.741 |
| 32B | 0.624 | 5.613 |
| 64B | 0.524 | 8.221 |
| 128B | 0.476 | 12.406 |
| 256B | 0.458 | 17.565 |
| 512B | 0.449 | 18.515 |
| 1KB | 0.435 | 21.025 |
| 4KB | 0.423 | 22.314 |
| 16KB | 0.443 | 22.293 |
| 64KB | 0.445 | 21.804 |
| 1MB | 0.440 | 20.569 |

## Notes

- **C# Improvements**: Since the initial .NET 8 benchmarks, the C# implementation has been optimized (using `Math.BigMul`, bit rotation, and memory access improvements) and migrated to .NET 10.
- **Parity**: The C# implementation now matches or exceeds the native C implementation for many cases, especially in 32-bit and 64-bit hashes.
- **a5hash128**: The native implementation still holds a lead for small-to-medium inputs, likely due to more aggressive compiler optimization of 128-bit arithmetic sequences.
- **CRC32**: The C# implementation uses `System.IO.Hashing.Crc32` which leverages hardware intrinsics (PCLMULQDQ), achieving extremely high throughput (~20 GB/s). The native C benchmark uses a standard software table-based implementation (~0.45 GB/s), serving as a baseline for non-accelerated performance.

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
