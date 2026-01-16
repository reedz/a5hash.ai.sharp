# A5Hash.Sharp - High-Performance Hash Functions for .NET

A C# port of the [a5hash](https://github.com/avaneev/a5hash) hash function family by Aleksey Vaneev, providing high-performance 64-bit, 32-bit, and 128-bit hash functions optimized for hash-tables, hash-maps, and bloom filters.

> **Note:** This is an AI-assisted port of the original C implementation. The C# implementation uses unsafe code, hardware intrinsics (BMI2), and optimized memory access patterns to achieve near-native performance.

## Features

- **64-bit hash** (`Hash`) - Fast general-purpose hash for string keys and small data
- **32-bit hash** (`Hash32`) - Native 32-bit variant for platforms where 64-bit performance is limited
- **128-bit hash** (`Hash128`) - High-throughput hash for larger data with collision resistance

## Quick Start

### Installation

Add a reference to the `A5Hash` project or copy `src/A5Hash/A5Hash.cs` into your project.

### Basic Usage

```csharp
using A5Hash;

// 64-bit hash
byte[] data = "Hello, World!"u8.ToArray();
ulong hash64 = A5Hash.Hash(data);
Console.WriteLine($"64-bit hash: 0x{hash64:X16}");

// 32-bit hash
uint hash32 = A5Hash.Hash32(data);
Console.WriteLine($"32-bit hash: 0x{hash32:X8}");

// 128-bit hash
var (low, high) = A5Hash.Hash128(data);
Console.WriteLine($"128-bit hash: 0x{high:X16}{low:X16}");

// With custom seed (recommended for hash tables to prevent collision attacks)
ulong seededHash = A5Hash.Hash(data, seed: 0xDEADBEEFCAFEBABE);
```

### Using with Spans

```csharp
// Works directly with ReadOnlySpan<byte> - zero allocations
ReadOnlySpan<byte> span = stackalloc byte[] { 1, 2, 3, 4, 5 };
ulong hash = A5Hash.Hash(span);

// Hash a portion of an array
byte[] buffer = new byte[1024];
ulong partialHash = A5Hash.Hash(buffer.AsSpan(0, 256));
```

### Dictionary/HashSet Key Hashing

```csharp
public readonly struct FastStringKey : IEquatable<FastStringKey>
{
    private readonly string _value;
    private readonly int _hashCode;

    public FastStringKey(string value)
    {
        _value = value;
        // Use a5hash for the hash code
        _hashCode = (int)A5Hash.Hash32(System.Text.Encoding.UTF8.GetBytes(value));
    }

    public override int GetHashCode() => _hashCode;
    public bool Equals(FastStringKey other) => _value == other._value;
    public override bool Equals(object? obj) => obj is FastStringKey other && Equals(other);
}
```

## Performance Benchmarks

Benchmarks run on .NET 8.0, Release build. The C# implementation achieves near-parity with native C code compiled with `-O3 -march=native`.

### a5hash (64-bit)

| Size | C# (GB/s) | C (GB/s) | C# / C |
|------|-----------|----------|--------|
| 8B | 0.90 | 2.63 | 34% |
| 64B | 4.62 | 7.84 | 59% |
| 256B | 6.71 | 7.81 | 86% |
| 1KB | 6.17 | 6.21 | 99% |
| 64KB | 6.10 | 4.75 | 128% |
| 1MB | 6.10 | 3.62 | 169% |

### a5hash32 (32-bit)

| Size | C# (GB/s) | C (GB/s) | C# / C |
|------|-----------|----------|--------|
| 8B | 1.22 | 2.10 | 58% |
| 64B | 5.31 | 5.34 | 99% |
| 256B | 5.86 | 4.09 | 143% |
| 1KB | 5.66 | 4.85 | 117% |
| 64KB | 5.61 | 4.17 | 135% |
| 1MB | 5.53 | 4.62 | 120% |

### a5hash128 (128-bit)

| Size | C# (GB/s) | C (GB/s) | C# / C |
|------|-----------|----------|--------|
| 8B | 0.61 | 2.91 | 21% |
| 64B | 3.54 | 9.18 | 39% |
| 256B | 6.67 | 11.69 | 57% |
| 1KB | 8.51 | 12.31 | 69% |
| 64KB | 9.37 | 14.46 | 65% |
| 1MB | 9.20 | 11.67 | 79% |

### Summary

| Function | Avg C# Throughput | Avg C Throughput |
|----------|-------------------|------------------|
| a5hash | ~5.5 GB/s | ~5.9 GB/s |
| a5hash32 | ~5.0 GB/s | ~4.2 GB/s |
| a5hash128 | ~6.8 GB/s | ~10.1 GB/s |

## Implementation Notes

This C# port uses several optimizations to achieve high performance:

- **Unsafe code with pointers** - Eliminates bounds checking overhead in hot loops
- **`Unsafe.ReadUnaligned<T>`** - Direct memory loads without byte-by-byte assembly
- **BMI2 hardware intrinsics** - Uses `Bmi2.X64.MultiplyNoFlags` for 128-bit multiplication when available
- **Aggressive inlining** - All methods marked with `MethodImplOptions.AggressiveInlining`

## Requirements

- .NET 8.0 or later
- `AllowUnsafeBlocks` must be enabled in the project

## Running Tests

```bash
cd tests/A5Hash.Tests
dotnet test
```

## Running Benchmarks

```bash
cd tests/A5Hash.Benchmark
dotnet run -c Release
```

## Credits

- **Original C implementation**: [a5hash](https://github.com/avaneev/a5hash) by Aleksey Vaneev
- **C# port**: AI-assisted implementation

## License

MIT License - See [LICENSE](LICENSE) for details.

The original a5hash is also released under the MIT license by Aleksey Vaneev.
