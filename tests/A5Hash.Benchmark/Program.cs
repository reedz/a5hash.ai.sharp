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
