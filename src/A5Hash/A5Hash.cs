using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace A5Hash;

/// <summary>
/// High-performance hash functions ported from a5hash C implementation.
/// Provides 64-bit, 32-bit, and 128-bit hash functions.
/// </summary>
[SkipLocalsInit]
public static unsafe class A5Hash
{
    private const ulong Val10 = 0xAAAAAAAAAAAAAAAAUL; // `10` bit-pairs
    private const ulong Val01 = 0x5555555555555555UL; // `01` bit-pairs

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
        return HashCore(ref MemoryMarshal.GetReference(data), data.Length, seed);
    }

    /// <summary>
    /// Produces a 32-bit hash value of the specified data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">Optional seed value (default 0).</param>
    /// <returns>32-bit hash value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash32(ReadOnlySpan<byte> data, uint seed = 0)
    {
        return Hash32Core(ref MemoryMarshal.GetReference(data), data.Length, seed);
    }

    /// <summary>
    /// Produces a 128-bit hash value of the specified data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">Optional seed value (default 0).</param>
    /// <returns>Tuple of (low 64 bits, high 64 bits).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong Low, ulong High) Hash128(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        return Hash128Core(ref MemoryMarshal.GetReference(data), data.Length, seed);
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

    /// <summary>
    /// 64-bit by 64-bit unsigned multiplication producing a 128-bit result.
    /// Uses BMI2 mulx intrinsic when available for best performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UMul128(ulong u, ulong v, out ulong rl, out ulong rh)
    {
        rh = Math.BigMul(u, v, out rl);
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

    /// <summary>
    /// Final hash computation helper for 64-bit hash.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong FinalizeHash64(ulong seed1, ulong seed2, ulong val01)
    {
        UMul128(seed1, seed2, out seed1, out seed2);
        UMul128(val01 ^ seed1, seed2, out seed1, out seed2);
        return seed1 ^ seed2;
    }

    #endregion

    #region A5Hash 64-bit Implementation

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static ulong HashCore(ref byte msg, int msgLen, ulong useSeed)
    {
        ulong val01 = Val01;
        ulong val10 = Val10;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ (ulong)msgLen;
        ulong seed2 = 0x452821E638D01377UL ^ (ulong)msgLen;

        UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2);

        if (msgLen > 16)
        {
            val01 ^= seed1;
            val10 ^= seed2;

            do
            {
                UMul128(
                    BitOperations.RotateLeft(LoadU64(ref msg), 32) ^ seed1,
                    BitOperations.RotateLeft(LoadU64(ref Unsafe.Add(ref msg, 8)), 32) ^ seed2,
                    out seed1, out seed2);

                msgLen -= 16;
                msg = ref Unsafe.Add(ref msg, 16);

                seed1 += val01;
                seed2 += val10;

            } while (msgLen > 16);
        }

        if (msgLen == 0)
        {
            return FinalizeHash64(seed1, seed2, val01);
        }

        if (msgLen > 3)
        {
            ref byte msg4 = ref Unsafe.Add(ref msg, msgLen - 4);
            int mo = msgLen >> 3;

            seed1 ^= ((ulong)LoadU32(ref msg) << 32) | LoadU32(ref msg4);
            seed2 ^= ((ulong)LoadU32(ref Unsafe.Add(ref msg, mo * 4)) << 32) | LoadU32(ref Unsafe.Subtract(ref msg4, mo * 4));

            return FinalizeHash64(seed1, seed2, val01);
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

            return FinalizeHash64(seed1, seed2, val01);
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static uint Hash32Core(ref byte msg, int msgLen, uint useSeed)
    {
        uint val01 = unchecked((uint)Val01);
        uint val10 = unchecked((uint)Val10);

        // Seeds initialized to mantissa bits of PI
        uint seed1 = 0x243F6A88 ^ (uint)msgLen;
        uint seed2 = 0x85A308D3 ^ (uint)msgLen;
        uint seed3, seed4;
        uint a, b, c = 0, d = 0;

        // For 64-bit size_t systems - in .NET, int is 32-bit so msgLen >> 32 is always 0
        UMul64(0x452821E6, 0x38D01377, out seed3, out seed4);

        UMul64(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2);

        if (msgLen < 17)
        {
            if (msgLen > 3)
            {
                a = LoadU32(ref msg);
                b = LoadU32(ref Unsafe.Add(ref msg, msgLen - 4));

                if (msgLen >= 9)
                {
                    int mo = msgLen >> 3;
                    c = LoadU32(ref Unsafe.Add(ref msg, mo * 4));
                    d = LoadU32(ref Unsafe.Add(ref msg, msgLen - 4 - mo * 4));
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

            if (Sse2.IsSupported && msgLen >= 16)
            {
                var seedVec = Vector128.Create(seed1, seed2, seed3, seed4);
                var val01Vec = Vector128.Create(val01);
                var val10Vec = Vector128.Create(val10);
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
        UMul128(a + seed1, b + seed2, out seed1, out seed2);
        UMul128(val01 ^ seed1, seed2, out a, out b);

        a ^= b;

        UMul128(seed1 ^ seed3, seed2 ^ seed4, out seed3, out seed4);
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
        UMul128(c + seed3, d + seed4, out seed3, out seed4);

        seed1 ^= seed3;
        seed2 ^= seed4;

        UMul128(a + seed1, b + seed2, out seed1, out seed2);
        UMul128(val01 ^ seed1, seed2, out a, out b);

        a ^= b;

        UMul128(seed1 ^ seed3, seed2 ^ seed4, out seed3, out seed4);
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
        seed1 ^= seed3;
        seed2 ^= seed4;

        UMul128(a + seed1, b + seed2, out seed1, out seed2);
        UMul128(val01 ^ seed1, seed2, out a, out b);

        a ^= b;

        UMul128(seed1 ^ seed3, seed2 ^ seed4, out seed3, out seed4);
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
        ulong s1 = seed1;

        UMul128(LoadU64(ref msg) + seed1, LoadU64(ref Unsafe.Add(ref msg, 8)) + seed2, out seed1, out seed2);
        seed1 += val01;
        seed2 += seed4;

        UMul128(LoadU64(ref Unsafe.Add(ref msg, 16)) + seed3, LoadU64(ref Unsafe.Add(ref msg, 24)) + seed4, out seed3, out seed4);

        seed3 += s1;
        seed4 += val10;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static (ulong Low, ulong High) Hash128Core(ref byte msg, int msgLen, ulong useSeed)
    {
        ulong val01 = Val01;
        ulong val10 = Val10;

        // Seeds initialized to mantissa bits of PI
        ulong seed1 = 0x243F6A8885A308D3UL ^ (ulong)msgLen;
        ulong seed2 = 0x452821E638D01377UL ^ (ulong)msgLen;
        ulong seed3 = 0xA4093822299F31D0UL;
        ulong seed4 = 0xC0AC29B7C97C50DDUL;
        ulong a, b, c, d;

        UMul128(seed2 ^ (useSeed & val10), seed1 ^ (useSeed & val01), out seed1, out seed2);

        if (msgLen < 17)
        {
            if (msgLen > 3)
            {
                ref byte msg4 = ref Unsafe.Add(ref msg, msgLen - 4);
                int mo = msgLen >> 3;

                a = ((ulong)LoadU32(ref msg) << 32) | LoadU32(ref msg4);
                b = ((ulong)LoadU32(ref Unsafe.Add(ref msg, mo * 4)) << 32) | LoadU32(ref Unsafe.Subtract(ref msg4, mo * 4));

                return FinalizeHash128Short(a, b, seed1, seed2, seed3, seed4, val01);
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

                return FinalizeHash128Short(a, b, seed1, seed2, seed3, seed4, val01);
            }
        }

        if (msgLen < 33)
        {
            a = BitOperations.RotateLeft(LoadU64(ref msg), 32);
            b = BitOperations.RotateLeft(LoadU64(ref Unsafe.Add(ref msg, 8)), 32);
            c = BitOperations.RotateLeft(LoadU64(ref Unsafe.Add(ref msg, msgLen - 16)), 32);
            d = BitOperations.RotateLeft(LoadU64(ref Unsafe.Add(ref msg, msgLen - 8)), 32);

            return FinalizeHash128WithCD(a, b, c, d, seed1, seed2, seed3, seed4, val01);
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

                UMul128(LoadU64(ref msg) + seed1, LoadU64(ref Unsafe.Add(ref msg, 32)) + seed2, out seed1, out seed2);
                seed1 += val01;
                seed2 += seed8;

                UMul128(LoadU64(ref Unsafe.Add(ref msg, 8)) + seed3, LoadU64(ref Unsafe.Add(ref msg, 40)) + seed4, out seed3, out seed4);
                seed3 += s1;
                seed4 += val10;

                UMul128(LoadU64(ref Unsafe.Add(ref msg, 16)) + seed5, LoadU64(ref Unsafe.Add(ref msg, 48)) + seed6, out seed5, out seed6);
                UMul128(LoadU64(ref Unsafe.Add(ref msg, 24)) + seed7, LoadU64(ref Unsafe.Add(ref msg, 56)) + seed8, out seed7, out seed8);

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
                ProcessTail32(ref msg, ref seed1, ref seed2, ref seed3, ref seed4, val01, val10);
                msgLen -= 32;
                msg = ref Unsafe.Add(ref msg, 32);
            }
        }
        else
        {
            // 33 <= msgLen <= 64
            ProcessTail32(ref msg, ref seed1, ref seed2, ref seed3, ref seed4, val01, val10);
            msgLen -= 32;
            msg = ref Unsafe.Add(ref msg, 32);
        }

        a = LoadU64(ref Unsafe.Add(ref msg, msgLen - 16));
        b = LoadU64(ref Unsafe.Add(ref msg, msgLen - 8));

        if (msgLen < 17)
        {
            return FinalizeHash128NoCD(a, b, seed1, seed2, seed3, seed4, val01);
        }

        c = LoadU64(ref Unsafe.Add(ref msg, msgLen - 32));
        d = LoadU64(ref Unsafe.Add(ref msg, msgLen - 24));

        return FinalizeHash128WithCD(a, b, c, d, seed1, seed2, seed3, seed4, val01);
    }

    #endregion
}
