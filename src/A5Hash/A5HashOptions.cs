namespace A5Hash;

/// <summary>
/// Options controlling A5Hash behavior.
/// </summary>
public sealed class A5HashOptions
{
    /// <summary>
    /// When true (default), uses hardware intrinsics (SIMD, etc.) when available.
    /// </summary>
    public bool UseIntrinsics { get; init; } = true;

    public static readonly A5HashOptions Default = new();
    public static readonly A5HashOptions NoIntrinsics = new() { UseIntrinsics = false };
}
