using System;
using System.Diagnostics;
using System.IO.Hashing;

namespace A5Hash.Benchmark;

class Program
{
    const int WarmupIterations = 1000;
    const double BenchmarkDurationSec = 1.0;

    static void Main(string[] args)
    {
        bool minimal = args.Length > 0 && args[0] == "--minimal";
        bool minimal8 = args.Length > 0 && args[0] == "--minimal8";
        
        Console.WriteLine("a5hash C# Performance Benchmark");
        Console.WriteLine("================================");
        
        if (minimal)
        {
            Console.WriteLine("MINIMAL MODE: 4B only, ops/s only");
        }
        else if (minimal8)
        {
            Console.WriteLine("MINIMAL MODE: 8B only, ops/s only");
        }
        else
        {
            Console.WriteLine($"Each benchmark runs for {BenchmarkDurationSec:F1} seconds");
        }
        Console.WriteLine();

        // Prepare test data
        (int size, string name)[] sizes = minimal
            ? new[] { (4, "4B") }
            : minimal8
                ? new[] { (8, "8B") }
                : new[]
                {
                    (4, "4B"), (8, "8B"), (16, "16B"), (32, "32B"), (64, "64B"),
                    (128, "128B"), (256, "256B"), (512, "512B"), (1024, "1KB"),
                    (4096, "4KB"), (16384, "16KB"), (65536, "64KB"), (1048576, "1MB")
                };

        // Allocate and fill buffer
        byte[] data = new byte[1048576];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(i * 31 + 17);
        }

        Console.WriteLine("--- a5hash (64-bit) ---");
        foreach (var (size, name) in sizes)
        {
            if (minimal)
                BenchmarkHash64Minimal(data.AsSpan(0, size), name);
            else
                BenchmarkHash64(data.AsSpan(0, size), name);
        }

        Console.WriteLine();
        Console.WriteLine("--- a5hash32 (32-bit) ---");
        foreach (var (size, name) in sizes)
        {
            if (minimal)
                BenchmarkHash32Minimal(data.AsSpan(0, size), name);
            else
                BenchmarkHash32(data.AsSpan(0, size), name);
        }

        Console.WriteLine();
        Console.WriteLine("--- a5hash128 (128-bit) ---");
        foreach (var (size, name) in sizes)
        {
            if (minimal)
                BenchmarkHash128Minimal(data.AsSpan(0, size), name);
            else
                BenchmarkHash128(data.AsSpan(0, size), name);
        }

        Console.WriteLine();
        Console.WriteLine("--- System.IO.Hashing.Crc32 ---");
        foreach (var (size, name) in sizes)
        {
            if (minimal)
                BenchmarkCrc32Minimal(data.AsSpan(0, size), name);
            else
                BenchmarkCrc32(data.AsSpan(0, size), name);
        }

        Console.WriteLine();
        Console.WriteLine("Benchmark complete.");
    }

    static void BenchmarkHash64(ReadOnlySpan<byte> data, string name)
    {
        ulong result = 0;
        int size = data.Length;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash(data, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash(data, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * size) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash32(ReadOnlySpan<byte> data, string name)
    {
        uint result = 0;
        int size = data.Length;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash32(data, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash32(data, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * size) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash128(ReadOnlySpan<byte> data, string name)
    {
        (ulong low, ulong high) result = (0, 0);
        int size = data.Length;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash128(data, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash128(data, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * size) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(result);
    }

    static void BenchmarkCrc32(ReadOnlySpan<byte> data, string name)
    {
        uint result = 0;
        int size = data.Length;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = Crc32.HashToUInt32(data);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = Crc32.HashToUInt32(data);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * size) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"Crc32     {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(result);
    }

    // Minimal benchmark methods - only print ops/s
    static void BenchmarkHash64Minimal(ReadOnlySpan<byte> data, string name)
    {
        ulong result = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash(data, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash(data, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash32Minimal(ReadOnlySpan<byte> data, string name)
    {
        uint result = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash32(data, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash32(data, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash128Minimal(ReadOnlySpan<byte> data, string name)
    {
        (ulong low, ulong high) result = (0, 0);

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash128(data, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash128(data, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkCrc32Minimal(ReadOnlySpan<byte> data, string name)
    {
        uint result = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = Crc32.HashToUInt32(data);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = Crc32.HashToUInt32(data);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"Crc32     {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }
}
