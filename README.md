# A5Hash.Sharp

High-performance, **AI-assisted C# port** of the original **a5hash** family of hash functions by **Aleksey Vaneev**.

- Upstream repo: https://github.com/avaneev/a5hash
- Upstream author: Aleksey Vaneev (2025), MIT licensed

This library provides:
- `new A5Hash(seed, options)` instance API (recommended)
- `A5Hash.Hash` / `A5Hash.Hash32` / `A5Hash.Hash128` static convenience APIs

> a5hash is a fast non-cryptographic hash intended for hash tables / bloom filters (not a cryptographic primitive).

## Install

### NuGet

```bash
dotnet add package A5Hash.Sharp
```

### Source

Copy `src/A5Hash/A5Hash.cs` into your project and enable `AllowUnsafeBlocks`.

## Usage

```csharp
using A5Hash;

ReadOnlySpan<byte> bytes = "Hello, World!"u8;
ReadOnlySpan<char> chars = "Hello, World!";

// Recommended: instance API (seed configured once)
var hasher = new A5Hash(seed: 0xDEADBEEFCAFEBABE);

ulong h64Bytes = hasher.Hash(bytes);
uint  h32Bytes = hasher.Hash32(bytes);
var (lowBytes, highBytes) = hasher.Hash128(bytes);

ulong h64Chars = hasher.Hash(chars);
uint  h32Chars = hasher.Hash32(chars);
var (lowChars, highChars) = hasher.Hash128(chars);

// Optional: disable intrinsics/SIMD (for diagnostics/portability)
var scalarHasher = new A5Hash(seed: 0xDEADBEEFCAFEBABE, options: new A5HashOptions { UseIntrinsics = false });

// Static convenience API still available
ulong seeded = A5Hash.Hash(chars, seed: 0xDEADBEEFCAFEBABE);
```

### Hashing a slice (zero allocations)

```csharp
byte[] buffer = new byte[1024];
var hasher = new A5Hash(seed: 0);
ulong hash = hasher.Hash(buffer.AsSpan(0, 256));
```

## Benchmarks (short)

Full tables are in **[benchmarks.md](benchmarks.md)** and include:
1) **Native C vs C#** (same algorithm)

Highlights from the latest run (see `benchmarks.md` for details):
- 64-bit throughput (1MB): **~5.9 GB/s** (C#) vs **~5.6 GB/s** (native C)
- 4-byte throughput (ops/s): **a5hash32 ~368M ops/s**

## Requirements

- Targets **net8.0** and **net10.0**
- `AllowUnsafeBlocks=true`

## Credits

- **Original algorithm + C implementation**: https://github.com/avaneev/a5hash by Aleksey Vaneev
- **This repo**: AI-assisted port + performance work

## License

MIT — see [LICENSE](LICENSE).
