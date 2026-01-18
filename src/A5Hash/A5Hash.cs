using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace A5Hash;

/// <summary>
/// High-performance hash functions ported from a5hash C implementation.
/// Provides 64-bit, 32-bit, and 128-bit hash functions.
/// </summary>
[SkipLocalsInit]
public sealed unsafe class A5Hash
{
    private const ulong Val10 = 0xAAAAAAAAAAAAAAAAUL; // `10` bit-pairs
    private const ulong Val01 = 0x5555555555555555UL; // `01` bit-pairs

    private readonly ulong _seed64;
    private readonly uint _seed32;
    private readonly bool _useIntrinsics;

    /// <summary>
    /// Creates an A5Hash hasher instance with a preconfigured seed.
    /// </summary>
    public A5Hash(ulong seed = 0, A5HashOptions? options = null)
    {
        _seed64 = seed;
        _seed32 = unchecked((uint)seed);
        _useIntrinsics = (options?.UseIntrinsics) ?? true;
    }

    /// <summary>
    /// Creates an A5Hash hasher instance with separate 64-bit and 32-bit seeds.
    /// </summary>
    public A5Hash(ulong seed64, uint seed32, A5HashOptions? options = null)
    {
        _seed64 = seed64;
        _seed32 = seed32;
        _useIntrinsics = (options?.UseIntrinsics) ?? true;
    }

    public ulong Seed64 => _seed64;
    public uint Seed32 => _seed32;
    public bool UseIntrinsics => _useIntrinsics;

    #region Public API

    /// <summary>
    /// Produces a 64-bit hash value of the specified data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">Optional seed value (default 0).</param>
    /// <returns>64-bit hash value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        return HashImpl(data, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong HashImpl(ReadOnlySpan<byte> data, ulong seed)
    {
        return HashImpl(data, seed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong HashImpl(ReadOnlySpan<byte> data, ulong seed, bool useIntrinsics)
    {
        int len = data.Length;
        ref byte r0 = ref MemoryMarshal.GetReference(data);

        if ((uint)len > 16u)
        {
            return HashCore(ref r0, len, seed, useIntrinsics);
        }

        if (len == 4)
        {
            uint x = LoadU32(ref r0);
            return Hash4(x, seed, useIntrinsics);
        }

        if (len == 8)
        {
            ulong x = LoadU64(ref r0);
            return Hash8(x, seed, useIntrinsics);
        }

        if (len == 16)
        {
            return Hash16(ref r0, seed, useIntrinsics);
        }

        return HashCore(ref r0, len, seed, useIntrinsics);
    }

    /// <summary>
    /// Produces a 64-bit hash value of the specified UTF-16 char data (no allocations).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash(ReadOnlySpan<char> data, ulong seed = 0)
    {
        return HashImpl(MemoryMarshal.AsBytes(data), seed);
    }

    /// <summary>
    /// Produces a 64-bit hash value of the specified string's UTF-16 data (no allocations).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash(string data, ulong seed = 0)
    {
        return HashImpl(MemoryMarshal.AsBytes(data.AsSpan()), seed);
    }

    /// <summary>
    /// Produces a 64-bit hash value of a 4-byte value.
    /// Useful for high-throughput hashing of fixed-size keys.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash(uint value, ulong seed = 0)
    {
        return Hash4(value, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash(ReadOnlySpan<byte> data) => HashImpl(data, _seed64, _useIntrinsics);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash(ReadOnlySpan<char> data) => HashImpl(MemoryMarshal.AsBytes(data), _seed64, _useIntrinsics);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash(string data) => HashImpl(MemoryMarshal.AsBytes(data.AsSpan()), _seed64, _useIntrinsics);

    /// <summary>
    /// Produces a 64-bit hash value of an 8-byte value.
    /// Useful for high-throughput hashing of fixed-size keys.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash(ulong value, ulong seed = 0)
    {
        return Hash8(value, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash(uint value) => Hash4(value, _seed64, _useIntrinsics);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash(ulong value) => Hash8(value, _seed64, _useIntrinsics);

    /// <summary>
    /// Produces a 32-bit hash value of the specified data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">Optional seed value (default 0).</param>
    /// <returns>32-bit hash value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32(ReadOnlySpan<byte> data, uint seed = 0)
    {
        return Hash32Impl(data, seed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash32Impl(ReadOnlySpan<byte> data, uint seed, bool useIntrinsics)
    {
        int len = data.Length;
        ref byte r0 = ref MemoryMarshal.GetReference(data);

        if ((uint)len > 16u)
        {
            return Hash32Core(ref r0, len, seed, useIntrinsics);
        }

        if (len == 4)
        {
            uint x = LoadU32(ref r0);
            return Hash32_4(value: x, seed);
        }

        if (len == 8)
        {
            ulong x = LoadU64(ref r0);
            return Hash32_8(value: x, seed);
        }

        if (len == 16)
        {
            return Hash32_16(ref r0, seed);
        }

        return Hash32Core(ref r0, len, seed, useIntrinsics);
    }

    /// <summary>
    /// Produces a 32-bit hash value of the specified UTF-16 char data (no allocations).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32(ReadOnlySpan<char> data, uint seed = 0)
    {
        return Hash32Impl(MemoryMarshal.AsBytes(data), seed, useIntrinsics: true);
    }

    /// <summary>
    /// Produces a 32-bit hash value of the specified string's UTF-16 data (no allocations).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32(string data, uint seed = 0)
    {
        return Hash32Impl(MemoryMarshal.AsBytes(data.AsSpan()), seed, useIntrinsics: true);
    }

    /// <summary>
    /// Produces a 32-bit hash value of a 4-byte value.
    /// Useful for high-throughput hashing of fixed-size keys.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32(uint value, uint seed = 0)
    {
        return Hash32_4(value, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Hash32(ReadOnlySpan<byte> data) => Hash32Impl(data, _seed32, _useIntrinsics);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Hash32(ReadOnlySpan<char> data) => Hash32Impl(MemoryMarshal.AsBytes(data), _seed32, _useIntrinsics);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Hash32(string data) => Hash32Impl(MemoryMarshal.AsBytes(data.AsSpan()), _seed32, _useIntrinsics);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Hash32(uint value) => Hash32_4(value, _seed32);

    /// <summary>
    /// Produces 4 independent 32-bit hash values for four 4-byte inputs.
    /// Useful for batching fixed-size keys (e.g., hash tables).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hash32x4(uint v0, uint v1, uint v2, uint v3,
        out uint h0, out uint h1, out uint h2, out uint h3, uint seed = 0)
    {
        Hash32x4Impl(v0, v1, v2, v3, seed, useIntrinsics: true, out h0, out h1, out h2, out h3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Hash32x4(uint v0, uint v1, uint v2, uint v3,
        out uint h0, out uint h1, out uint h2, out uint h3)
    {
        Hash32x4Impl(v0, v1, v2, v3, _seed32, _useIntrinsics, out h0, out h1, out h2, out h3);
    }

    /// <summary>
    /// Produces a 32-bit hash value of an 8-byte value.
    /// Useful for high-throughput hashing of fixed-size keys.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32(ulong value, uint seed = 0)
    {
        return Hash32_8(value, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Hash32(ulong value) => Hash32_8(value, _seed32);

    /// <summary>
    /// Produces a 128-bit hash value of the specified data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">Optional seed value (default 0).</param>
    /// <returns>Tuple of (low 64 bits, high 64 bits).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong Low, ulong High) Hash128(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        return Hash128Impl(data, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong Low, ulong High) Hash128Impl(ReadOnlySpan<byte> data, ulong seed)
    {
        return Hash128Impl(data, seed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong Low, ulong High) Hash128Impl(ReadOnlySpan<byte> data, ulong seed, bool useIntrinsics)
    {
        int len = data.Length;
        ref byte r0 = ref MemoryMarshal.GetReference(data);

        if ((uint)len > 16u)
        {
            return Hash128Core(ref r0, len, seed, useIntrinsics);
        }

        if (len == 4)
        {
            uint x = LoadU32(ref r0);
            ulong high;
            ulong low = Hash128_4(value: x, seed, out high, useIntrinsics);
            return (low, high);
        }

        if (len == 8)
        {
            ulong x = LoadU64(ref r0);
            ulong high;
            ulong low = Hash128_8(value: x, seed, out high, useIntrinsics);
            return (low, high);
        }

        if (len == 16)
        {
            return Hash128_16(ref r0, seed, useIntrinsics);
        }

        return Hash128Core(ref r0, len, seed, useIntrinsics);
    }

    /// <summary>
    /// Produces a 128-bit hash value of the specified UTF-16 char data (no allocations).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong Low, ulong High) Hash128(ReadOnlySpan<char> data, ulong seed = 0)
    {
        return Hash128Impl(MemoryMarshal.AsBytes(data), seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (ulong Low, ulong High) Hash128(ReadOnlySpan<byte> data) => Hash128Impl(data, _seed64, _useIntrinsics);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (ulong Low, ulong High) Hash128(ReadOnlySpan<char> data) => Hash128Impl(MemoryMarshal.AsBytes(data), _seed64, _useIntrinsics);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (ulong Low, ulong High) Hash128(string data) => Hash128Impl(MemoryMarshal.AsBytes(data.AsSpan()), _seed64, _useIntrinsics);

    /// <summary>
    /// Produces a 128-bit hash value of the specified string's UTF-16 data (no allocations).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong Low, ulong High) Hash128(string data, ulong seed = 0)
    {
        return Hash128Impl(MemoryMarshal.AsBytes(data.AsSpan()), seed);
    }

    /// <summary>
    /// Produces a 128-bit hash value of a 4-byte value.
    /// Useful for high-throughput hashing of fixed-size keys.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong Low, ulong High) Hash128(uint value, ulong seed = 0)
    {
        ulong high;
        ulong low = Hash128_4(value, seed, out high);
        return (low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (ulong Low, ulong High) Hash128(uint value)
    {
        ulong high;
        ulong low = Hash128_4(value, _seed64, out high, _useIntrinsics);
        return (low, high);
    }

    /// <summary>
    /// Produces a 128-bit hash value of an 8-byte value.
    /// Useful for high-throughput hashing of fixed-size keys.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong Low, ulong High) Hash128(ulong value, ulong seed = 0)
    {
        ulong high;
        ulong low = Hash128_8(value, seed, out high);
        return (low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (ulong Low, ulong High) Hash128(ulong value)
    {
        ulong high;
        ulong low = Hash128_8(value, _seed64, out high, _useIntrinsics);
        return (low, high);
    }

    /// <summary>
    /// Produces a 128-bit hash value (low) and outputs the high 64 bits.
    /// This avoids tuple copies in hot paths.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash128(uint value, out ulong high, ulong seed = 0)
    {
        return Hash128_4(value, seed, out high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash128(uint value, out ulong high)
    {
        return Hash128_4(value, _seed64, out high, _useIntrinsics);
    }

    /// <summary>
    /// Produces a 128-bit hash value (low) and outputs the high 64 bits.
    /// This avoids tuple copies in hot paths.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash128(ulong value, out ulong high, ulong seed = 0)
    {
        return Hash128_8(value, seed, out high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash128(ulong value, out ulong high)
    {
        return Hash128_8(value, _seed64, out high, _useIntrinsics);
    }

    /// <summary>
    /// Produces a 128-bit hash value (low) and outputs the high 64 bits.
    /// This avoids tuple copies in hot paths.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash128(ReadOnlySpan<byte> data, out ulong high, ulong seed = 0)
    {
        return Hash128LowHighImpl(data, out high, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash128LowHighImpl(ReadOnlySpan<byte> data, out ulong high, ulong seed)
    {
        return Hash128LowHighImpl(data, out high, seed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash128LowHighImpl(ReadOnlySpan<byte> data, out ulong high, ulong seed, bool useIntrinsics)
    {
        int len = data.Length;
        ref byte r0 = ref MemoryMarshal.GetReference(data);

        if ((uint)len > 16u)
        {
            var t2 = Hash128Core(ref r0, len, seed, useIntrinsics);
            high = t2.High;
            return t2.Low;
        }

        if (len == 4)
        {
            uint x = LoadU32(ref r0);
            return Hash128_4(value: x, seed, out high, useIntrinsics);
        }

        if (len == 8)
        {
            ulong x = LoadU64(ref r0);
            return Hash128_8(value: x, seed, out high, useIntrinsics);
        }

        if (len == 16)
        {
            var t = Hash128_16(ref r0, seed, useIntrinsics);
            high = t.High;
            return t.Low;
        }

        var t3 = Hash128Core(ref r0, len, seed, useIntrinsics);
        high = t3.High;
        return t3.Low;
    }

    /// <summary>
    /// Produces a 128-bit hash value (low) and outputs the high 64 bits for the specified UTF-16 char data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash128(ReadOnlySpan<char> data, out ulong high, ulong seed = 0)
    {
        return Hash128LowHighImpl(MemoryMarshal.AsBytes(data), out high, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash128(ReadOnlySpan<byte> data, out ulong high)
    {
        return Hash128LowHighImpl(data, out high, _seed64, _useIntrinsics);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash128(ReadOnlySpan<char> data, out ulong high)
    {
        return Hash128LowHighImpl(MemoryMarshal.AsBytes(data), out high, _seed64, _useIntrinsics);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Hash128(string data, out ulong high)
    {
        return Hash128LowHighImpl(MemoryMarshal.AsBytes(data.AsSpan()), out high, _seed64, _useIntrinsics);
    }

    /// <summary>
    /// Produces a 128-bit hash value (low) and outputs the high 64 bits for the specified string's UTF-16 data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Hash128(string data, out ulong high, ulong seed = 0)
    {
        return Hash128LowHighImpl(MemoryMarshal.AsBytes(data.AsSpan()), out high, seed);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Load 32-bit unsigned value from memory (little-endian, unaligned).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint LoadU32(ref byte p)
    {
        return Unsafe.ReadUnaligned<uint>(ref p);
    }

    /// <summary>
    /// Load 64-bit unsigned value from memory (little-endian, unaligned).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LoadU64(ref byte p)
    {
        return Unsafe.ReadUnaligned<ulong>(ref p);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotL32(ulong x)
    {
        return BitOperations.RotateLeft(x, 32);
    }

    /// <summary>
    /// 64-bit by 64-bit unsigned multiplication producing a 128-bit result.
    /// Uses BMI2 mulx intrinsic when available for best performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UMul128(ulong u, ulong v, out ulong rl, out ulong rh, bool useIntrinsics)
    {
        if (useIntrinsics && Bmi2.X64.IsSupported)
        {
            ulong low;
            rh = Bmi2.X64.MultiplyNoFlags(u, v, &low);
            rl = low;
            return;
        }

        if (useIntrinsics && ArmBase.Arm64.IsSupported)
        {
            rl = u * v;
            rh = ArmBase.Arm64.MultiplyHigh(u, v);
            return;
        }

        rh = Math.BigMul(u, v, out rl);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UMul128(ulong u, ulong v, out ulong rl, out ulong rh)
    {
        UMul128(u, v, out rl, out rh, useIntrinsics: true);
    }


    /// <summary>
    /// 32-bit by 32-bit unsigned multiplication producing a 64-bit result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UMul64(uint u, uint v, out uint rl, out uint rh)
    {
        ulong r = (ulong)u * v;
        rl = (uint)r;
        rh = (uint)(r >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash4(uint value, ulong useSeed)
    {
        return Hash4(value, useSeed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash4(uint value, ulong useSeed, bool useIntrinsics)
    {
        ulong val01 = Val01;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ 4UL;
        ulong seed2 = 0x452821E638D01377UL ^ 4UL;

        if (useSeed == 0)
        {
            // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==4
            seed1 = 0xD2C2E3CF5894ED95UL;
            seed2 = 0x09CAC66C371A7852UL;
        }
        else
        {
            ulong val10 = Val10;
            UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2, useIntrinsics);
        }

        ulong t = ((ulong)value << 32) | value;
        seed1 ^= t;
        seed2 ^= t;

        return FinalizeHash64(seed1, seed2, val01, useIntrinsics);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash8(ulong value, ulong useSeed)
    {
        return Hash8(value, useSeed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash8(ulong value, ulong useSeed, bool useIntrinsics)
    {
        ulong val01 = Val01;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ 8UL;
        ulong seed2 = 0x452821E638D01377UL ^ 8UL;

        if (useSeed == 0)
        {
            // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==8
            seed1 = 0x9A5C69CE7F79A5A5UL;
            seed2 = 0x09CAC66C371A7855UL;
        }
        else
        {
            ulong val10 = Val10;
            UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2, useIntrinsics);
        }

        uint lo = unchecked((uint)value);
        uint hi = unchecked((uint)(value >> 32));
        ulong a = ((ulong)lo << 32) | hi;
        ulong b = ((ulong)hi << 32) | lo;

        seed1 ^= a;
        seed2 ^= b;

        return FinalizeHash64(seed1, seed2, val01, useIntrinsics);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash16(ref byte msg, ulong useSeed)
    {
        return Hash16(ref msg, useSeed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash16(ref byte msg, ulong useSeed, bool useIntrinsics)
    {
        ulong val01 = Val01;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ 16UL;
        ulong seed2 = 0x452821E638D01377UL ^ 16UL;

        if (useSeed == 0)
        {
            // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==16
            seed1 = 0xB8A73F6CA4AEFF75UL;
            seed2 = 0x09CAC66C371A784BUL;
        }
        else
        {
            ulong val10 = Val10;
            UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2, useIntrinsics);
        }

        seed1 ^= ((ulong)LoadU32(ref msg) << 32) | LoadU32(ref Unsafe.Add(ref msg, 12));
        seed2 ^= ((ulong)LoadU32(ref Unsafe.Add(ref msg, 8)) << 32) | LoadU32(ref Unsafe.Add(ref msg, 4));

        return FinalizeHash64(seed1, seed2, val01, useIntrinsics);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash32_16(ref byte msg, uint useSeed)
    {
        uint val01 = unchecked((uint)Val01);

        // Seeds initialized to mantissa bits of PI
        uint seed1 = 0x243F6A88 ^ 16u;
        uint seed2 = 0x85A308D3 ^ 16u;
        uint seed3 = 0xFB0BD3EA;
        uint seed4 = 0x0F58FD47;

        if (useSeed == 0)
        {
            // Precomputed: UMul64(seed2 ^ 0, seed1 ^ 0) for msgLen==16
            seed1 = 0x6E6AF1C8;
            seed2 = 0x12EC07FF;
        }
        else
        {
            uint val10 = unchecked((uint)Val10);
            UMul64(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2);
        }

        uint a = LoadU32(ref msg);
        uint b = LoadU32(ref Unsafe.Add(ref msg, 12));
        uint c = LoadU32(ref Unsafe.Add(ref msg, 8));
        uint d = LoadU32(ref Unsafe.Add(ref msg, 4));

        UMul64(c + seed3, d + seed4, out seed3, out seed4);

        return FinalizeHash32(a, b, seed1, seed2, seed3, seed4, val01);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong Low, ulong High) Hash128_16(ref byte msg, ulong useSeed)
    {
        return Hash128_16(ref msg, useSeed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong Low, ulong High) Hash128_16(ref byte msg, ulong useSeed, bool useIntrinsics)
    {
        ulong val01 = Val01;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ 16UL;
        ulong seed2 = 0x452821E638D01377UL ^ 16UL;
        ulong seed3 = 0xA4093822299F31D0UL;
        ulong seed4 = 0xC0AC29B7C97C50DDUL;

        if (useSeed == 0)
        {
            // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==16
            seed1 = 0xB8A73F6CA4AEFF75UL;
            seed2 = 0x09CAC66C371A784BUL;
        }
        else
        {
            ulong val10 = Val10;
            UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2, useIntrinsics);
        }

        ulong a = ((ulong)LoadU32(ref msg) << 32) | LoadU32(ref Unsafe.Add(ref msg, 12));
        ulong b = ((ulong)LoadU32(ref Unsafe.Add(ref msg, 8)) << 32) | LoadU32(ref Unsafe.Add(ref msg, 4));

        return FinalizeHash128Short(a, b, seed1, seed2, seed3, seed4, val01, useIntrinsics);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash32_4(uint value, uint useSeed)
    {
        uint val01 = unchecked((uint)Val01);

        // Seeds initialized to mantissa bits of PI
        uint seed1 = 0x243F6A88 ^ 4u;
        uint seed2 = 0x85A308D3 ^ 4u;
        uint seed3 = 0xFB0BD3EA;
        uint seed4 = 0x0F58FD47;

        if (useSeed == 0)
        {
            // Precomputed: UMul64(seed2 ^ 0, seed1 ^ 0) for msgLen==4
            seed1 = 0xFFBADB94;
            seed2 = 0x12EC07FB;
        }
        else
        {
            uint val10 = unchecked((uint)Val10);
            UMul64(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2);
        }

        return FinalizeHash32(value, value, seed1, seed2, seed3, seed4, val01);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash32_8(ulong value, uint useSeed)
    {
        uint val01 = unchecked((uint)Val01);

        // Seeds initialized to mantissa bits of PI
        uint seed1 = 0x243F6A88 ^ 8u;
        uint seed2 = 0x85A308D3 ^ 8u;
        uint seed3 = 0xFB0BD3EA;
        uint seed4 = 0x0F58FD47;

        if (useSeed == 0)
        {
            // Precomputed: UMul64(seed2 ^ 0, seed1 ^ 0) for msgLen==8
            seed1 = 0x4D141B80;
            seed2 = 0x12EC07F6;
        }
        else
        {
            uint val10 = unchecked((uint)Val10);
            UMul64(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2);
        }

        uint a = unchecked((uint)value);
        uint b = unchecked((uint)(value >> 32));

        return FinalizeHash32(a, b, seed1, seed2, seed3, seed4, val01);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash128_4(uint value, ulong useSeed, out ulong high)
    {
        return Hash128_4(value, useSeed, out high, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash128_4(uint value, ulong useSeed, out ulong high, bool useIntrinsics)
    {
        ulong val01 = Val01;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ 4UL;
        ulong seed2 = 0x452821E638D01377UL ^ 4UL;
        ulong seed3 = 0xA4093822299F31D0UL;
        ulong seed4 = 0xC0AC29B7C97C50DDUL;

        if (useSeed == 0)
        {
            // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==4
            seed1 = 0xD2C2E3CF5894ED95UL;
            seed2 = 0x09CAC66C371A7852UL;
        }
        else
        {
            ulong val10 = Val10;
            UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2, useIntrinsics);
        }

        ulong a = ((ulong)value << 32) | value;
        ulong b = a;

        UMul128(a + seed1, b + seed2, out seed1, out seed2, useIntrinsics);

        UMul128(val01 ^ seed1, seed2, out a, out b, useIntrinsics);
        ulong low = a ^ b;

        UMul128(seed1 ^ seed3, seed2 ^ seed4, out seed3, out seed4, useIntrinsics);
        high = seed3 ^ seed4;

        return low;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash128_8(ulong value, ulong useSeed, out ulong high)
    {
        return Hash128_8(value, useSeed, out high, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Hash128_8(ulong value, ulong useSeed, out ulong high, bool useIntrinsics)
    {
        ulong val01 = Val01;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ 8UL;
        ulong seed2 = 0x452821E638D01377UL ^ 8UL;
        ulong seed3 = 0xA4093822299F31D0UL;
        ulong seed4 = 0xC0AC29B7C97C50DDUL;

        if (useSeed == 0)
        {
            // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==8
            seed1 = 0x9A5C69CE7F79A5A5UL;
            seed2 = 0x09CAC66C371A7855UL;
        }
        else
        {
            ulong val10 = Val10;
            UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2, useIntrinsics);
        }

        uint lo = unchecked((uint)value);
        uint hi = unchecked((uint)(value >> 32));
        ulong a = ((ulong)lo << 32) | hi;
        ulong b = ((ulong)hi << 32) | lo;

        UMul128(a + seed1, b + seed2, out seed1, out seed2, useIntrinsics);

        UMul128(val01 ^ seed1, seed2, out a, out b, useIntrinsics);
        ulong low = a ^ b;

        UMul128(seed1 ^ seed3, seed2 ^ seed4, out seed3, out seed4, useIntrinsics);
        high = seed3 ^ seed4;

        return low;
    }

    /// <summary>
    /// Final hash computation helper for 64-bit hash.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong FinalizeHash64(ulong seed1, ulong seed2, ulong val01)
    {
        return FinalizeHash64(seed1, seed2, val01, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong FinalizeHash64(ulong seed1, ulong seed2, ulong val01, bool useIntrinsics)
    {
        UMul128(seed1, seed2, out seed1, out seed2, useIntrinsics);
        UMul128(val01 ^ seed1, seed2, out seed1, out seed2, useIntrinsics);
        return seed1 ^ seed2;
    }

    #endregion

    #region A5Hash 64-bit Implementation

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static ulong HashCore(ref byte msg, int msgLen, ulong useSeed)
    {
        return HashCore(ref msg, msgLen, useSeed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static ulong HashCore(ref byte msg, int msgLen, ulong useSeed, bool useIntrinsics)
    {
        ulong val01 = Val01;
        ulong val10 = Val10;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ (ulong)msgLen;
        ulong seed2 = 0x452821E638D01377UL ^ (ulong)msgLen;

        if (useSeed == 0)
        {
            if (msgLen == 0)
            {
                // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==0
                seed1 = 0x4F2006588BE0C315UL;
                seed2 = 0x09CAC66C371A7852UL;
            }
            else if (msgLen == 4)
            {
                // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==4
                seed1 = 0xD2C2E3CF5894ED95UL;
                seed2 = 0x09CAC66C371A7852UL;
            }
            else if (msgLen == 16)
            {
                // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==16
                seed1 = 0xB8A73F6CA4AEFF75UL;
                seed2 = 0x09CAC66C371A784BUL;
            }
            else
            {
                UMul128(seed2, seed1, out seed1, out seed2, useIntrinsics);
            }
        }
        else
        {
            UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2, useIntrinsics);
        }

        if (msgLen > 16)
        {
            val01 ^= seed1;
            val10 ^= seed2;

            do
            {
                UMul128(
                    RotL32(LoadU64(ref msg)) ^ seed1,
                    RotL32(LoadU64(ref Unsafe.Add(ref msg, 8))) ^ seed2,
                    out seed1, out seed2, useIntrinsics);

                msgLen -= 16;
                msg = ref Unsafe.Add(ref msg, 16);

                seed1 += val01;
                seed2 += val10;

            } while (msgLen > 16);
        }

        if (msgLen == 0)
        {
            return FinalizeHash64(seed1, seed2, val01, useIntrinsics);
        }

        if (msgLen == 16)
        {
            ulong u0 = LoadU64(ref msg);
            ulong u1 = LoadU64(ref Unsafe.Add(ref msg, 8));
            seed1 ^= ((u0 & 0xFFFFFFFFUL) << 32) | (u1 >> 32);
            seed2 ^= ((u1 & 0xFFFFFFFFUL) << 32) | (u0 >> 32);
            return FinalizeHash64(seed1, seed2, val01, useIntrinsics);
        }

        if (msgLen > 3)
        {
            ref byte msg4 = ref Unsafe.Add(ref msg, msgLen - 4);

            uint a0 = LoadU32(ref msg);
            uint a1 = LoadU32(ref msg4);
            ulong t1 = ((ulong)a0 << 32) | a1;
            seed1 ^= t1;

            if ((uint)msgLen < 8u)
            {
                seed2 ^= t1;
            }
            else
            {
                ref byte msg4m4 = ref Unsafe.Subtract(ref msg4, 4);
                ulong t2 = ((ulong)LoadU32(ref Unsafe.Add(ref msg, 4)) << 32) | LoadU32(ref msg4m4);
                seed2 ^= t2;
            }

            return FinalizeHash64(seed1, seed2, val01, useIntrinsics);
        }
        else
        {
            seed1 ^= msg;

            if (--msgLen != 0)
            {
                seed1 ^= (ulong)Unsafe.Add(ref msg, 1) << 8;

                if (--msgLen != 0)
                {
                    seed1 ^= (ulong)Unsafe.Add(ref msg, 2) << 16;
                }
            }

            return FinalizeHash64(seed1, seed2, val01, useIntrinsics);
        }
    }

    #endregion

    #region A5Hash32 32-bit Implementation

    /// <summary>
    /// Final hash computation helper for 32-bit hash.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint FinalizeHash32(uint a, uint b, uint seed1, uint seed2, uint seed3, uint seed4, uint val01)
    {
        seed1 ^= seed3;
        seed2 ^= seed4;

        UMul64(a + seed1, b + seed2, out seed1, out seed2);
        UMul64(val01 ^ seed1, seed2, out a, out b);

        return a ^ b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint FinalizeHash32_4(uint value, uint seed1, uint seed2, uint val01)
    {
        UMul64(value + seed1, value + seed2, out seed1, out seed2);
        UMul64(val01 ^ seed1, seed2, out uint a, out uint b);
        return a ^ b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Hash32x2Sse2(uint v0, uint v1, uint seed1, uint seed2, uint val01, out uint h0, out uint h1)
    {
        var msgVec = Vector128.Create(v0, v0, v1, v1);
        var seedVec = Vector128.Create(seed1, seed2, seed1, seed2);
        var terms = Sse2.Add(msgVec, seedVec);

        var left = Sse2.Shuffle(terms, 0xA0);
        var right = Sse2.Shuffle(terms, 0xF5);
        var prod = Sse2.Multiply(left, right);

        var seeds = prod.AsUInt32();
        var xorMask = Vector128.Create(val01, 0u, val01, 0u);
        var terms2 = Sse2.Xor(seeds, xorMask);

        left = Sse2.Shuffle(terms2, 0xA0);
        right = Sse2.Shuffle(terms2, 0xF5);
        var prod2 = Sse2.Multiply(left, right);

        ulong q0 = prod2.GetElement(0);
        ulong q1 = prod2.GetElement(1);
        h0 = unchecked((uint)q0) ^ unchecked((uint)(q0 >> 32));
        h1 = unchecked((uint)q1) ^ unchecked((uint)(q1 >> 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Hash32x4Avx2(uint v0, uint v1, uint v2, uint v3, uint seed1, uint seed2, uint val01,
        out uint h0, out uint h1, out uint h2, out uint h3)
    {
        var msgVec = Vector256.Create(v0, v0, v1, v1, v2, v2, v3, v3);
        var seedVec = Vector256.Create(seed1, seed2, seed1, seed2, seed1, seed2, seed1, seed2);
        var terms = Avx2.Add(msgVec, seedVec);

        var idxL = Vector256.Create(0, 0, 2, 2, 4, 4, 6, 6);
        var idxR = Vector256.Create(1, 1, 3, 3, 5, 5, 7, 7);

        var left = Avx2.PermuteVar8x32(terms.AsInt32(), idxL).AsUInt32();
        var right = Avx2.PermuteVar8x32(terms.AsInt32(), idxR).AsUInt32();
        var prod = Avx2.Multiply(left, right);

        var seeds = prod.AsUInt32();
        var xorMask = Vector256.Create(val01, 0u, val01, 0u, val01, 0u, val01, 0u);
        var terms2 = Avx2.Xor(seeds, xorMask);

        left = Avx2.PermuteVar8x32(terms2.AsInt32(), idxL).AsUInt32();
        right = Avx2.PermuteVar8x32(terms2.AsInt32(), idxR).AsUInt32();
        var prod2 = Avx2.Multiply(left, right);

        ulong q0 = prod2.GetElement(0);
        ulong q1 = prod2.GetElement(1);
        ulong q2 = prod2.GetElement(2);
        ulong q3 = prod2.GetElement(3);

        h0 = unchecked((uint)q0) ^ unchecked((uint)(q0 >> 32));
        h1 = unchecked((uint)q1) ^ unchecked((uint)(q1 >> 32));
        h2 = unchecked((uint)q2) ^ unchecked((uint)(q2 >> 32));
        h3 = unchecked((uint)q3) ^ unchecked((uint)(q3 >> 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Hash32x4Impl(uint v0, uint v1, uint v2, uint v3, uint seed, bool useIntrinsics,
        out uint h0, out uint h1, out uint h2, out uint h3)
    {
        uint val01 = unchecked((uint)Val01);
        uint seed1 = 0x243F6A88 ^ 4u;
        uint seed2 = 0x85A308D3 ^ 4u;

        if (seed == 0)
        {
            // Precomputed: UMul64(seed2 ^ 0, seed1 ^ 0) for msgLen==4
            seed1 = 0xFFBADB94;
            seed2 = 0x12EC07FB;
        }
        else
        {
            uint val10 = unchecked((uint)Val10);
            UMul64(seed2 ^ (seed & val10), seed1 ^ (seed & val01), out seed1, out seed2);
        }

        // Fold in the constant per-hash seeds used by a5hash32.
        seed1 ^= 0xFB0BD3EA;
        seed2 ^= 0x0F58FD47;

        if (useIntrinsics && Avx2.IsSupported)
        {
            Hash32x4Avx2(v0, v1, v2, v3, seed1, seed2, val01, out h0, out h1, out h2, out h3);
            return;
        }

        if (useIntrinsics && Sse2.IsSupported)
        {
            Hash32x2Sse2(v0, v1, seed1, seed2, val01, out h0, out h1);
            Hash32x2Sse2(v2, v3, seed1, seed2, val01, out h2, out h3);
            return;
        }

        h0 = FinalizeHash32_4(v0, seed1, seed2, val01);
        h1 = FinalizeHash32_4(v1, seed1, seed2, val01);
        h2 = FinalizeHash32_4(v2, seed1, seed2, val01);
        h3 = FinalizeHash32_4(v3, seed1, seed2, val01);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static uint Hash32Core(ref byte msg, int msgLen, uint useSeed, bool useIntrinsics)
    {
        uint val01 = unchecked((uint)Val01);
        uint val10 = unchecked((uint)Val10);

        // Seeds initialized to mantissa bits of PI
        uint seed1 = 0x243F6A88 ^ (uint)msgLen;
        uint seed2 = 0x85A308D3 ^ (uint)msgLen;
        // In C, these depend on MsgLen >> 32 on 64-bit platforms; in .NET msgLen is 32-bit.
        uint seed3 = 0xFB0BD3EA;
        uint seed4 = 0x0F58FD47;
        uint a, b, c = 0, d = 0;

        if (useSeed == 0)
        {
            if (msgLen == 0)
            {
                // Precomputed: UMul64(seed2 ^ 0, seed1 ^ 0) for msgLen==0
                seed1 = 0x58310E18;
                seed2 = 0x12EC07F9;
            }
            else
            {
                UMul64(seed2, seed1, out seed1, out seed2);
            }
        }
        else
        {
            UMul64(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2);
        }

        if (msgLen < 17)
        {
            if (msgLen > 3)
            {
                a = LoadU32(ref msg);
                b = LoadU32(ref Unsafe.Add(ref msg, msgLen - 4));

                if (msgLen >= 9)
                {
                    c = LoadU32(ref Unsafe.Add(ref msg, 4));
                    d = LoadU32(ref Unsafe.Add(ref msg, msgLen - 8));
                    UMul64(c + seed3, d + seed4, out seed3, out seed4);
                }

                return FinalizeHash32(a, b, seed1, seed2, seed3, seed4, val01);
            }
            else
            {
                a = 0;
                b = 0;

                if (msgLen != 0)
                {
                    a = msg;

                    if (msgLen != 1)
                    {
                        a |= (uint)Unsafe.Add(ref msg, 1) << 8;

                        if (msgLen != 2)
                        {
                            a |= (uint)Unsafe.Add(ref msg, 2) << 16;
                        }
                    }
                }

                return FinalizeHash32(a, b, seed1, seed2, seed3, seed4, val01);
            }
        }
        else
        {
            val01 ^= seed1;
            val10 ^= seed2;

            if (useIntrinsics && Sse2.IsSupported && msgLen >= 64)
            {
                var seedVec = Vector128.Create(seed1, seed2, seed3, seed4);
                var constantMix = Vector128.Create(val01, 0, 0, val10);
                var oldMaskVec = Vector128.Create(0, 0xFFFFFFFF, 0xFFFFFFFF, 0);

                do
                {
                    var msgVec = Unsafe.ReadUnaligned<Vector128<uint>>(ref msg);
                    var terms = Sse2.Add(msgVec, seedVec);
                    
                    var left = Sse2.Shuffle(terms, 0xA0);
                    var right = Sse2.Shuffle(terms, 0xF5);
                    
                    var prod = Sse2.Multiply(left, right);
                    var newSeeds = prod.AsUInt32();
                    
                    var oldMix = Sse2.Shuffle(seedVec, 0x0C);
                    
                    seedVec = Sse2.Add(newSeeds, constantMix);
                    seedVec = Sse2.Add(seedVec, Sse2.And(oldMix, oldMaskVec));

                    msgLen -= 16;
                    msg = ref Unsafe.Add(ref msg, 16);

                } while (msgLen > 16);
                
                seed1 = seedVec.GetElement(0);
                seed2 = seedVec.GetElement(1);
                seed3 = seedVec.GetElement(2);
                seed4 = seedVec.GetElement(3);
            }
            else
            {
                do
                {
                    uint s1 = seed1;
                    uint s4 = seed4;

                    UMul64(LoadU32(ref msg) + seed1, LoadU32(ref Unsafe.Add(ref msg, 4)) + seed2, out seed1, out seed2);
                    UMul64(LoadU32(ref Unsafe.Add(ref msg, 8)) + seed3, LoadU32(ref Unsafe.Add(ref msg, 12)) + seed4, out seed3, out seed4);

                    msgLen -= 16;
                    msg = ref Unsafe.Add(ref msg, 16);

                    seed1 += val01;
                    seed2 += s4;
                    seed3 += s1;
                    seed4 += val10;

                } while (msgLen > 16);
            }

            a = LoadU32(ref Unsafe.Add(ref msg, msgLen - 8));
            b = LoadU32(ref Unsafe.Add(ref msg, msgLen - 4));

            if (msgLen >= 9)
            {
                c = LoadU32(ref Unsafe.Add(ref msg, msgLen - 16));
                d = LoadU32(ref Unsafe.Add(ref msg, msgLen - 12));
                UMul64(c + seed3, d + seed4, out seed3, out seed4);
            }

            return FinalizeHash32(a, b, seed1, seed2, seed3, seed4, val01);
        }
    }

    #endregion

    #region A5Hash128 128-bit Implementation

    /// <summary>
    /// Final hash computation for 128-bit hash (short path).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong, ulong) FinalizeHash128Short(ulong a, ulong b, ulong seed1, ulong seed2, ulong seed3, ulong seed4, ulong val01)
    {
        return FinalizeHash128Short(a, b, seed1, seed2, seed3, seed4, val01, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong, ulong) FinalizeHash128Short(ulong a, ulong b, ulong seed1, ulong seed2, ulong seed3, ulong seed4, ulong val01, bool useIntrinsics)
    {
        UMul128(a + seed1, b + seed2, out seed1, out seed2, useIntrinsics);
        UMul128(val01 ^ seed1, seed2, out a, out b, useIntrinsics);

        a ^= b;

        UMul128(seed1 ^ seed3, seed2 ^ seed4, out seed3, out seed4, useIntrinsics);
        ulong high = seed3 ^ seed4;

        return (a, high);
    }

    /// <summary>
    /// Final hash computation for 128-bit hash (with cd merge).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong, ulong) FinalizeHash128WithCD(ulong a, ulong b, ulong c, ulong d, 
        ulong seed1, ulong seed2, ulong seed3, ulong seed4, ulong val01)
    {
        return FinalizeHash128WithCD(a, b, c, d, seed1, seed2, seed3, seed4, val01, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong, ulong) FinalizeHash128WithCD(ulong a, ulong b, ulong c, ulong d,
        ulong seed1, ulong seed2, ulong seed3, ulong seed4, ulong val01, bool useIntrinsics)
    {
        UMul128(c + seed3, d + seed4, out seed3, out seed4, useIntrinsics);

        seed1 ^= seed3;
        seed2 ^= seed4;

        UMul128(a + seed1, b + seed2, out seed1, out seed2, useIntrinsics);
        UMul128(val01 ^ seed1, seed2, out a, out b, useIntrinsics);

        a ^= b;

        UMul128(seed1 ^ seed3, seed2 ^ seed4, out seed3, out seed4, useIntrinsics);
        ulong high = seed3 ^ seed4;

        return (a, high);
    }

    /// <summary>
    /// Final hash computation for 128-bit hash (without cd merge).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong, ulong) FinalizeHash128NoCD(ulong a, ulong b,
        ulong seed1, ulong seed2, ulong seed3, ulong seed4, ulong val01)
    {
        return FinalizeHash128NoCD(a, b, seed1, seed2, seed3, seed4, val01, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ulong, ulong) FinalizeHash128NoCD(ulong a, ulong b,
        ulong seed1, ulong seed2, ulong seed3, ulong seed4, ulong val01, bool useIntrinsics)
    {
        seed1 ^= seed3;
        seed2 ^= seed4;

        UMul128(a + seed1, b + seed2, out seed1, out seed2, useIntrinsics);
        UMul128(val01 ^ seed1, seed2, out a, out b, useIntrinsics);

        a ^= b;

        UMul128(seed1 ^ seed3, seed2 ^ seed4, out seed3, out seed4, useIntrinsics);
        ulong high = seed3 ^ seed4;

        return (a, high);
    }

    /// <summary>
    /// Process 32-byte tail block.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessTail32(ref byte msg, ref ulong seed1, ref ulong seed2, 
        ref ulong seed3, ref ulong seed4, ulong val01, ulong val10)
    {
        ProcessTail32(ref msg, ref seed1, ref seed2, ref seed3, ref seed4, val01, val10, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessTail32(ref byte msg, ref ulong seed1, ref ulong seed2,
        ref ulong seed3, ref ulong seed4, ulong val01, ulong val10, bool useIntrinsics)
    {
        ulong s1 = seed1;

        UMul128(LoadU64(ref msg) + seed1, LoadU64(ref Unsafe.Add(ref msg, 8)) + seed2, out seed1, out seed2, useIntrinsics);
        seed1 += val01;
        seed2 += seed4;

        UMul128(LoadU64(ref Unsafe.Add(ref msg, 16)) + seed3, LoadU64(ref Unsafe.Add(ref msg, 24)) + seed4, out seed3, out seed4, useIntrinsics);

        seed3 += s1;
        seed4 += val10;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static (ulong Low, ulong High) Hash128Core(ref byte msg, int msgLen, ulong useSeed)
    {
        return Hash128Core(ref msg, msgLen, useSeed, useIntrinsics: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static (ulong Low, ulong High) Hash128Core(ref byte msg, int msgLen, ulong useSeed, bool useIntrinsics)
    {
        ulong val01 = Val01;
        ulong val10 = Val10;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ (ulong)msgLen;
        ulong seed2 = 0x452821E638D01377UL ^ (ulong)msgLen;
        ulong seed3 = 0xA4093822299F31D0UL;
        ulong seed4 = 0xC0AC29B7C97C50DDUL;
        ulong a, b, c, d;

        if (useSeed == 0)
        {
            if (msgLen == 0)
            {
                // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==0
                seed1 = 0x4F2006588BE0C315UL;
                seed2 = 0x09CAC66C371A7852UL;
            }
            else if (msgLen == 4)
            {
                // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==4
                seed1 = 0xD2C2E3CF5894ED95UL;
                seed2 = 0x09CAC66C371A7852UL;
            }
            else if (msgLen == 16)
            {
                // Precomputed: UMul128(seed2 ^ 0, seed1 ^ 0) for msgLen==16
                seed1 = 0xB8A73F6CA4AEFF75UL;
                seed2 = 0x09CAC66C371A784BUL;
            }
            else
            {
                UMul128(seed2, seed1, out seed1, out seed2, useIntrinsics);
            }
        }
        else
        {
            UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2, useIntrinsics);
        }

        if (msgLen < 17)
        {
            if (msgLen > 3)
            {
                ref byte msg4 = ref Unsafe.Add(ref msg, msgLen - 4);

                a = ((ulong)LoadU32(ref msg) << 32) | LoadU32(ref msg4);

                if ((uint)msgLen < 8u)
                {
                    b = a;
                }
                else
                {
                    b = ((ulong)LoadU32(ref Unsafe.Add(ref msg, 4)) << 32) | LoadU32(ref Unsafe.Subtract(ref msg4, 4));
                }

                return FinalizeHash128Short(a, b, seed1, seed2, seed3, seed4, val01, useIntrinsics);
            }
            else
            {
                a = 0;
                b = 0;

                if (msgLen != 0)
                {
                    a = msg;

                    if (--msgLen != 0)
                    {
                        a |= (ulong)Unsafe.Add(ref msg, 1) << 8;

                        if (--msgLen != 0)
                        {
                            a |= (ulong)Unsafe.Add(ref msg, 2) << 16;
                        }
                    }
                }

                return FinalizeHash128Short(a, b, seed1, seed2, seed3, seed4, val01, useIntrinsics);
            }
        }

        if (msgLen < 33)
        {
            a = RotL32(LoadU64(ref msg));
            b = RotL32(LoadU64(ref Unsafe.Add(ref msg, 8)));
            c = RotL32(LoadU64(ref Unsafe.Add(ref msg, msgLen - 16)));
            d = RotL32(LoadU64(ref Unsafe.Add(ref msg, msgLen - 8)));

            return FinalizeHash128WithCD(a, b, c, d, seed1, seed2, seed3, seed4, val01, useIntrinsics);
        }

        // msgLen >= 33
        val01 ^= seed1;
        val10 ^= seed2;

        if (msgLen > 64)
        {
            ulong seed5 = 0x082EFA98EC4E6C89UL;
            ulong seed6 = 0x3F84D5B5B5470917UL;
            ulong seed7 = 0x13198A2E03707344UL;
            ulong seed8 = 0xBE5466CF34E90C6CUL;

            do
            {
                ulong s1 = seed1;
                ulong s3 = seed3;
                ulong s5 = seed5;

                UMul128(LoadU64(ref msg) + seed1, LoadU64(ref Unsafe.Add(ref msg, 32)) + seed2, out seed1, out seed2, useIntrinsics);
                seed1 += val01;
                seed2 += seed8;

                UMul128(LoadU64(ref Unsafe.Add(ref msg, 8)) + seed3, LoadU64(ref Unsafe.Add(ref msg, 40)) + seed4, out seed3, out seed4, useIntrinsics);
                seed3 += s1;
                seed4 += val10;

                UMul128(LoadU64(ref Unsafe.Add(ref msg, 16)) + seed5, LoadU64(ref Unsafe.Add(ref msg, 48)) + seed6, out seed5, out seed6, useIntrinsics);
                UMul128(LoadU64(ref Unsafe.Add(ref msg, 24)) + seed7, LoadU64(ref Unsafe.Add(ref msg, 56)) + seed8, out seed7, out seed8, useIntrinsics);

                msgLen -= 64;
                msg = ref Unsafe.Add(ref msg, 64);

                seed5 += s3;
                seed6 += val10;
                seed7 += s5;
                seed8 += val10;

            } while (msgLen > 64);

            seed1 ^= seed5;
            seed2 ^= seed6;
            seed3 ^= seed7;
            seed4 ^= seed8;

            if (msgLen > 32)
            {
                ProcessTail32(ref msg, ref seed1, ref seed2, ref seed3, ref seed4, val01, val10, useIntrinsics);
                msgLen -= 32;
                msg = ref Unsafe.Add(ref msg, 32);
            }
        }
        else
        {
            // 33 <= msgLen <= 64
            ProcessTail32(ref msg, ref seed1, ref seed2, ref seed3, ref seed4, val01, val10, useIntrinsics);
            msgLen -= 32;
            msg = ref Unsafe.Add(ref msg, 32);
        }

        a = LoadU64(ref Unsafe.Add(ref msg, msgLen - 16));
        b = LoadU64(ref Unsafe.Add(ref msg, msgLen - 8));

        if (msgLen < 17)
        {
            return FinalizeHash128NoCD(a, b, seed1, seed2, seed3, seed4, val01, useIntrinsics);
        }

        c = LoadU64(ref Unsafe.Add(ref msg, msgLen - 32));
        d = LoadU64(ref Unsafe.Add(ref msg, msgLen - 24));

        return FinalizeHash128WithCD(a, b, c, d, seed1, seed2, seed3, seed4, val01, useIntrinsics);
    }

    #endregion
}
