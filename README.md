# A5Hash.Sharp

High-performance, **AI-assisted C# port** of the original **a5hash** family of hash functions by **Aleksey Vaneev**.

- Upstream repo: https://github.com/avaneev/a5hash
- Upstream author: Aleksey Vaneev (2025), MIT licensed

This library provides:
- `A5Hash.Hash` (64-bit)
- `A5Hash.Hash32` (32-bit)
- `A5Hash.Hash128` (128-bit)

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

ulong h64Bytes = A5Hash.Hash(bytes);
uint  h32Bytes = A5Hash.Hash32(bytes);
var (lowBytes, highBytes) = A5Hash.Hash128(bytes);

ulong h64Chars = A5Hash.Hash(chars);
uint  h32Chars = A5Hash.Hash32(chars);
var (lowChars, highChars) = A5Hash.Hash128(chars);

// Optional seed (recommended when hashing attacker-controlled keys)
ulong seeded = A5Hash.Hash(chars, seed: 0xDEADBEEFCAFEBABE);
```

### Hashing a slice (zero allocations)

```csharp
byte[] buffer = new byte[1024];
ulong hash = A5Hash.Hash(buffer.AsSpan(0, 256));
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
