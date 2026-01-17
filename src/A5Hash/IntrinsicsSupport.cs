using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace A5Hash;

/// <summary>
/// Centralized feature checks for optional hardware intrinsics.
/// Kept behind TFM guards so the library can multi-target net8.0 + net10.0.
/// </summary>
internal static class IntrinsicsSupport
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasSse2() => Sse2.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasAvx2() => Avx2.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasAvx512F() => Avx512F.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasBmi2X64() => Bmi2.X64.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasArmBaseArm64() => ArmBase.Arm64.IsSupported;

    // NEON (AdvSimd) is the .NET API surface for ARM SIMD.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasNeon() => AdvSimd.IsSupported;

#if NET10_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasAvx10v1() => Avx10v1.IsSupported;

#pragma warning disable SYSLIB5003 // SVE/SVE2 intrinsics are evaluation-only in .NET 10
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasSve() => Sve.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasSve2() => Sve2.IsSupported;
#pragma warning restore SYSLIB5003
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasAvx10v1() => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasSve() => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasSve2() => false;
#endif
}
