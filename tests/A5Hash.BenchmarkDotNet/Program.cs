using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace A5Hash.BenchmarkDotNet;

public class RandomObjectsHashBenchmarks
{
    private const int N = 1000;

    private object[] _objects = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _objects = new object[N];

        for (int i = 0; i < N; i++)
        {
            int choice = rng.Next(6);
            _objects[i] = choice switch
            {
                0 => rng.Next(),
                1 => (long)rng.Next() << 32 | (uint)rng.Next(),
                2 => rng.NextDouble(),
                3 => Guid.NewGuid(),
                4 => "str_" + rng.Next() + "_" + rng.Next(),
                _ => new Payload(rng.Next(), rng.Next(), rng.Next(), rng.Next())
            };
        }
    }

    [Benchmark(Baseline = true)]
    public int DotNet_GetHashCode_Sum()
    {
        int sum = 0;
        var objs = _objects;

        for (int i = 0; i < objs.Length; i++)
        {
            sum = unchecked(sum + objs[i].GetHashCode());
        }

        return sum;
    }

    [Benchmark]
    public ulong A5Hash_1000_GetHashCodeBytes()
    {
        ulong sum = 0;
        var objs = _objects;

        for (int i = 0; i < objs.Length; i++)
        {
            sum ^= global::A5Hash.A5Hash.Hash((uint)objs[i].GetHashCode(), 0);
        }

        return sum;
    }

    private readonly record struct Payload(int A, int B, int C, int D);
}

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
