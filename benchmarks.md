# Benchmarks

These benchmark results are **machine-dependent** and are provided for relative comparison.

## Setup

- **Benchmark run**: 2026-01-16
- **C# runtime**: .NET 10, Release
- **Native**: GCC `-O3 -march=native`
- **Duration**: 1.0s per test (warmup 1000 iterations)
- **Sizes**: 4B .. 1MB
- **Note**: Tables below are an average of **2 runs** each.

## Highlights

- 64-bit (1MB): C **5.58 GB/s** vs C# **5.89 GB/s**
- 4-byte throughput (ops/s): a5hash32 **~368M ops/s** vs HW Crc32 **~217M ops/s**

## Native C vs C#

### a5hash (64-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# |
|---:|---:|---:|---:|
| 4B | 1.169 | 1.003 | 1.17x |
| 8B | 2.313 | 1.421 | 1.63x |
| 16B | 4.628 | 2.865 | 1.62x |
| 32B | 5.731 | 4.339 | 1.32x |
| 64B | 6.864 | 6.151 | 1.12x |
| 128B | 7.572 | 7.659 | 0.99x |
| 256B | 6.981 | 7.006 | 1.00x |
| 512B | 6.366 | 6.444 | 0.99x |
| 1KB | 6.024 | 6.220 | 0.97x |
| 4KB | 5.768 | 5.974 | 0.97x |
| 16KB | 5.739 | 5.223 | 1.10x |
| 64KB | 5.623 | 5.524 | 1.02x |
| 1MB | 5.583 | 5.889 | 0.95x |


### a5hash32 (32-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# |
|---:|---:|---:|---:|
| 4B | 1.399 | 1.373 | 1.02x |
| 8B | 2.721 | 2.843 | 0.96x |
| 16B | 3.713 | 4.266 | 0.87x |
| 32B | 4.828 | 5.897 | 0.82x |
| 64B | 6.116 | 7.926 | 0.77x |
| 128B | 6.050 | 8.334 | 0.73x |
| 256B | 5.643 | 7.457 | 0.76x |
| 512B | 5.569 | 6.940 | 0.80x |
| 1KB | 5.450 | 6.696 | 0.81x |
| 4KB | 5.241 | 6.341 | 0.83x |
| 16KB | 5.221 | 6.279 | 0.83x |
| 64KB | 5.188 | 6.188 | 0.84x |
| 1MB | 5.091 | 6.171 | 0.82x |


### a5hash128 (128-bit)

| Size | C (GB/s) | C# (GB/s) | C / C# |
|---:|---:|---:|---:|
| 4B | 1.587 | 0.477 | 3.32x |
| 8B | 3.135 | 0.758 | 4.13x |
| 16B | 6.023 | 1.496 | 4.03x |
| 32B | 7.098 | 2.944 | 2.41x |
| 64B | 10.405 | 4.249 | 2.45x |
| 128B | 10.206 | 5.968 | 1.71x |
| 256B | 12.360 | 7.798 | 1.58x |
| 512B | 12.947 | 9.166 | 1.41x |
| 1KB | 12.508 | 9.861 | 1.27x |
| 4KB | 12.284 | 10.785 | 1.14x |
| 16KB | 11.691 | 9.814 | 1.19x |
| 64KB | 12.061 | 10.367 | 1.16x |
| 1MB | 11.806 | 8.758 | 1.35x |


## C# vs hardware-accelerated CRC32

`System.IO.Hashing.Crc32` is hardware-accelerated on supported CPUs (e.g. SSE4.2 / ARMv8 CRC).

| Size | a5hash32 (GB/s) | a5hash (GB/s) | a5hash128 (GB/s) | Crc32 (HW) (GB/s) |
|---:|---:|---:|---:|---:|
| 4B | 1.373 | 1.003 | 0.477 | 0.808 |
| 8B | 2.843 | 1.421 | 0.758 | 0.785 |
| 16B | 4.266 | 2.865 | 1.496 | 2.672 |
| 32B | 5.897 | 4.339 | 2.944 | 5.524 |
| 64B | 7.926 | 6.151 | 4.249 | 7.853 |
| 128B | 8.334 | 7.659 | 5.968 | 12.088 |
| 256B | 7.457 | 7.006 | 7.798 | 17.419 |
| 512B | 6.940 | 6.444 | 9.166 | 19.230 |
| 1KB | 6.696 | 6.220 | 9.861 | 20.783 |
| 4KB | 6.341 | 5.974 | 10.785 | 21.665 |
| 16KB | 6.279 | 5.223 | 9.814 | 21.816 |
| 64KB | 6.188 | 5.524 | 10.367 | 21.312 |
| 1MB | 6.171 | 5.889 | 8.758 | 20.825 |


## How to reproduce

Native:

```bash
cd tests/native
gcc -O3 -march=native benchmark.c -o benchmark
./benchmark
```

C#:

```bash
cd tests/A5Hash.Benchmark
dotnet run -c Release
```
