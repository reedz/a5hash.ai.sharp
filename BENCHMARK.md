# a5hash Performance Comparison: C vs C#

Benchmark run: 2026-01-15

## Test Configuration
- **Duration**: 1.0 seconds per test
- **Warmup**: 1000 iterations
- **Data sizes**: 8B to 1MB
- **C compiler**: GCC with `-O3 -march=native`
- **C# runtime**: .NET 8.0 Release build

## Results

### a5hash (64-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# Ratio |
|------|----------|-----------|--------------|
| 8B | 2.631 | 0.164 | 16.0x |
| 16B | 5.115 | 0.464 | 11.0x |
| 32B | 6.916 | 0.479 | 14.4x |
| 64B | 7.842 | 0.566 | 13.9x |
| 128B | 8.514 | 0.645 | 13.2x |
| 256B | 7.809 | 0.921 | 8.5x |
| 512B | 6.832 | 0.927 | 7.4x |
| 1KB | 6.214 | 1.059 | 5.9x |
| 4KB | 5.647 | 1.193 | 4.7x |
| 16KB | 5.202 | 1.068 | 4.9x |
| 64KB | 4.753 | 1.139 | 4.2x |
| 1MB | 3.616 | 1.029 | 3.5x |

### a5hash32 (32-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# Ratio |
|------|----------|-----------|--------------|
| 8B | 2.099 | 0.421 | 5.0x |
| 16B | 3.414 | 0.776 | 4.4x |
| 32B | 4.769 | 0.781 | 6.1x |
| 64B | 5.335 | 0.782 | 6.8x |
| 128B | 4.157 | 0.945 | 4.4x |
| 256B | 4.087 | 1.186 | 3.4x |
| 512B | 4.366 | 1.283 | 3.4x |
| 1KB | 4.847 | 1.242 | 3.9x |
| 4KB | 4.637 | 1.309 | 3.5x |
| 16KB | 4.465 | 1.404 | 3.2x |
| 64KB | 4.168 | 1.437 | 2.9x |
| 1MB | 4.623 | 1.283 | 3.6x |

### a5hash128 (128-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# Ratio |
|------|----------|-----------|--------------|
| 8B | 2.905 | 0.249 | 11.7x |
| 16B | 5.414 | 0.708 | 7.6x |
| 32B | 5.457 | 0.861 | 6.3x |
| 64B | 9.176 | 1.107 | 8.3x |
| 128B | 9.403 | 1.321 | 7.1x |
| 256B | 11.694 | 1.567 | 7.5x |
| 512B | 12.546 | 1.565 | 8.0x |
| 1KB | 12.312 | 1.582 | 7.8x |
| 4KB | 12.526 | 1.597 | 7.8x |
| 16KB | 13.880 | 1.642 | 8.5x |
| 64KB | 14.456 | 1.590 | 9.1x |
| 1MB | 11.669 | 1.614 | 7.2x |

## Summary

| Function | Avg C Throughput | Avg C# Throughput | Avg Ratio |
|----------|------------------|-------------------|-----------|
| a5hash | 5.9 GB/s | 0.8 GB/s | ~8x |
| a5hash32 | 4.2 GB/s | 1.1 GB/s | ~4x |
| a5hash128 | 10.1 GB/s | 1.3 GB/s | ~8x |

## Notes

- Native C benefits from compiler optimizations (`-O3 -march=native`) and 128-bit multiplication intrinsics
- C# implementation uses safe managed code with `ReadOnlySpan<byte>`
- C# performance could be improved with:
  - `unsafe` code and pointer arithmetic
  - `System.Runtime.Intrinsics` for SIMD operations
  - `BinaryPrimitives` for optimized memory loads
  - Hardware intrinsics for 128-bit multiplication on supported platforms

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
