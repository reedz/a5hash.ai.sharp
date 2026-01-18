using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace A5Hash.Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
        // Avoid interactive benchmark selection; run the full suite by default.
        if (args.Length == 0)
            args = new[] { "--filter", "*", "--join" };

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

[MemoryDiagnoser]
public class A5HashBenchmarks
{
    private static readonly int[] ByteSizes = new[] { 4, 8, 16, 32, 64, 128, 256, 512, 1024, 4096, 16384, 65536, 1048576 };

    private readonly byte[] _data = new byte[1048576];

    private readonly global::A5Hash.A5Hash _hasher = new(0);

    public A5HashBenchmarks()
    {
        for (int i = 0; i < _data.Length; i++)
            _data[i] = (byte)(i * 31 + 17);
    }

    [ParamsSource(nameof(Sizes))]
    public int Size;

    public int[] Sizes => ByteSizes;

    [Benchmark(Description = "a5hash64 bytes")]
    public ulong Hash64Bytes()
    {
        return _hasher.Hash(_data.AsSpan(0, Size));
    }

    [Benchmark(Description = "a5hash32 bytes")]
    public uint Hash32Bytes()
    {
        return _hasher.Hash32(_data.AsSpan(0, Size));
    }

    [Benchmark(Description = "a5hash128 bytes")]
    public (ulong Low, ulong High) Hash128Bytes()
    {
        return _hasher.Hash128(_data.AsSpan(0, Size));
    }
}

[MemoryDiagnoser]
public class A5HashStringBenchmarks
{
    private readonly global::A5Hash.A5Hash _hasher = new(0);

    [Params(8, 16, 64, 256, 1024)]
    public int Chars;

    private string _s = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _s = new string('a', Chars);
    }

    [Benchmark(Description = "a5hash64 string")]
    public ulong Hash64String() => _hasher.Hash(_s);

    [Benchmark(Description = "a5hash32 string")]
    public uint Hash32String() => _hasher.Hash32(_s);

    [Benchmark(Description = "a5hash128 string")]
    public (ulong Low, ulong High) Hash128String() => _hasher.Hash128(_s);
}

[MemoryDiagnoser]
public class A5Hash32BatchedBenchmarks
{
    private readonly global::A5Hash.A5Hash _hasher = new(0);

    private uint _v0 = 0x12345678;
    private uint _v1 = 0x90ABCDEF;
    private uint _v2 = 0x0BADF00D;
    private uint _v3 = 0xC001D00D;

    [Benchmark(Description = "a5hash32 scalar x4")]
    public uint Hash32ScalarX4()
    {
        uint h = _hasher.Hash32(_v0);
        h ^= _hasher.Hash32(_v1);
        h ^= _hasher.Hash32(_v2);
        h ^= _hasher.Hash32(_v3);
        return h;
    }

    [Benchmark(Description = "a5hash32 batched x4")]
    public uint Hash32BatchedX4()
    {
        _hasher.Hash32x4(_v0, _v1, _v2, _v3, out uint h0, out uint h1, out uint h2, out uint h3);
        return h0 ^ h1 ^ h2 ^ h3;
    }
}
