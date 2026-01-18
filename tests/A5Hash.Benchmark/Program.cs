using System;
using System.Buffers.Binary;
using System.Diagnostics;

namespace A5Hash.Benchmark;

class Program
{
    const int WarmupIterations = 1000;
    const double BenchmarkDurationSec = 1.0;

    static readonly global::A5Hash.A5Hash Hasher = new(0);

    static void Main(string[] args)
    {
        bool minimal4 = Array.IndexOf(args, "--minimal") >= 0;
        bool minimal8 = Array.IndexOf(args, "--minimal8") >= 0;
        bool stringMode = Array.IndexOf(args, "--strings") >= 0;
        bool minimalAny = minimal4 || minimal8 || stringMode;

        Console.WriteLine("a5hash C# Performance Benchmark");
        Console.WriteLine("================================");

        if (minimal4)
            Console.WriteLine("MINIMAL MODE: 4B only, ops/s only");
        else if (minimal8)
            Console.WriteLine("MINIMAL MODE: 8B only, ops/s only");
        else if (stringMode)
            Console.WriteLine("MINIMAL MODE: strings only, ops/s only");
        else
            Console.WriteLine($"Each benchmark runs for {BenchmarkDurationSec:F1} seconds");

        Console.WriteLine();

        (int size, string name)[] sizes = minimal4
            ? new[] { (4, "4B") }
            : minimal8
                ? new[] { (8, "8B") }
                : stringMode
                    ? new[] { (8, "8c"), (16, "16c"), (64, "64c"), (256, "256c"), (1024, "1Kc") }
                    : new[]
                    {
                        (4, "4B"), (8, "8B"), (16, "16B"), (32, "32B"), (64, "64B"),
                        (128, "128B"), (256, "256B"), (512, "512B"), (1024, "1KB"),
                        (4096, "4KB"), (16384, "16KB"), (65536, "64KB"), (1048576, "1MB")
                    };

        byte[] data = new byte[1048576];
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(i * 31 + 17);

        Console.WriteLine("--- a5hash (64-bit) ---");
        foreach (var (size, name) in sizes)
        {
            if (minimalAny)
            {
                if (stringMode)
                {
                    BenchmarkHash64String(new string('a', size), name);
                }
                else if (size == 4)
                {
                    BenchmarkHash64Minimal(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, size)), name);
                }
                else if (size == 8)
                {
                    BenchmarkHash64Minimal(BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(0, size)), name);
                }
                else
                {
                    BenchmarkHash64Minimal(data.AsSpan(0, size), name);
                }
            }
            else
            {
                BenchmarkHash64(data.AsSpan(0, size), name);
            }
        }

        Console.WriteLine();
        Console.WriteLine("--- a5hash32 (32-bit) ---");
        foreach (var (size, name) in sizes)
        {
            if (minimalAny)
            {
                if (stringMode)
                {
                    BenchmarkHash32String(new string('a', size), name);
                }
                else if (size == 4)
                {
                    BenchmarkHash32Minimal(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, size)), name);
                }
                else if (size == 8)
                {
                    BenchmarkHash32Minimal(BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(0, size)), name);
                }
                else
                {
                    BenchmarkHash32Minimal(data.AsSpan(0, size), name);
                }
            }
            else
            {
                BenchmarkHash32(data.AsSpan(0, size), name);
            }
        }

        if (minimal4)
        {
            Console.WriteLine();
            Console.WriteLine("--- a5hash32 4B: scalar vs batched ---");
            BenchmarkHash32ScalarVsBatched(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, 4)));
        }

        Console.WriteLine();
        Console.WriteLine("--- a5hash128 (128-bit) ---");
        foreach (var (size, name) in sizes)
        {
            if (minimalAny)
            {
                if (stringMode)
                {
                    BenchmarkHash128String(new string('a', size), name);
                }
                else if (size == 4)
                {
                    BenchmarkHash128Minimal(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, size)), name);
                }
                else if (size == 8)
                {
                    BenchmarkHash128Minimal(BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(0, size)), name);
                }
                else
                {
                    BenchmarkHash128Minimal(data.AsSpan(0, size), name);
                }
            }
            else
            {
                BenchmarkHash128(data.AsSpan(0, size), name);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Benchmark complete.");
    }

    static void BenchmarkHash64(ReadOnlySpan<byte> data, string name)
    {
        ulong result = 0;
        int size = data.Length;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash(data);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash(data);
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * (double)size) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash32(ReadOnlySpan<byte> data, string name)
    {
        uint result = 0;
        int size = data.Length;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash32(data);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash32(data);
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * (double)size) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash128(ReadOnlySpan<byte> data, string name)
    {
        ulong low = 0;
        ulong high = 0;
        int size = data.Length;

        for (int i = 0; i < WarmupIterations; i++)
            low = Hasher.Hash128(data, out high);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                low = Hasher.Hash128(data, out high);
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * (double)size) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
    }

    // Minimal benchmark methods - only print ops/s
    static void BenchmarkHash64Minimal(uint value, string name)
    {
        ulong result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash(value);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash(value);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash64Minimal(ulong value, string name)
    {
        ulong result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash(value);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash(value);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash64Minimal(ReadOnlySpan<byte> data, string name)
    {
        ulong result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash(data);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash(data);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash32Minimal(uint value, string name)
    {
        uint result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash32(value);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash32(value);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash32Minimal(ulong value, string name)
    {
        uint result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash32(value);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash32(value);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash32Minimal(ReadOnlySpan<byte> data, string name)
    {
        uint result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash32(data);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash32(data);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash128Minimal(uint value, string name)
    {
        ulong low = 0;
        ulong high = 0;

        for (int i = 0; i < WarmupIterations; i++)
            low = Hasher.Hash128(value, out high);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                low = Hasher.Hash128(value, out high);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
    }

    static void BenchmarkHash128Minimal(ulong value, string name)
    {
        ulong low = 0;
        ulong high = 0;

        for (int i = 0; i < WarmupIterations; i++)
            low = Hasher.Hash128(value, out high);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                low = Hasher.Hash128(value, out high);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
    }

    static void BenchmarkHash128Minimal(ReadOnlySpan<byte> data, string name)
    {
        ulong low = 0;
        ulong high = 0;

        for (int i = 0; i < WarmupIterations; i++)
            low = Hasher.Hash128(data, out high);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                low = Hasher.Hash128(data, out high);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
    }

    static void BenchmarkHash32String(string data, string name)
    {
        uint result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash32(data);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash32(data);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
        GC.KeepAlive(data);
    }

    static void BenchmarkHash64String(string data, string name)
    {
        ulong result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash(data);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash(data);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
        GC.KeepAlive(data);
    }

    static void BenchmarkHash128String(string data, string name)
    {
        ulong low = 0, high = 0;

        for (int i = 0; i < WarmupIterations; i++)
            low = Hasher.Hash128(data, out high);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                low = Hasher.Hash128(data, out high);
            iterations += 1000;
        }

        sw.Stop();
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
        GC.KeepAlive(data);
    }

    static void BenchmarkHash32ScalarVsBatched(uint value)
    {
        uint result = 0;

        for (int i = 0; i < WarmupIterations; i++)
            result = Hasher.Hash32(value);

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                result = Hasher.Hash32(value);
            iterations += 1000;
        }

        sw.Stop();
        double scalarOpsPerSec = iterations / sw.Elapsed.TotalSeconds;

        uint h0 = 0, h1 = 0, h2 = 0, h3 = 0;

        for (int i = 0; i < WarmupIterations; i++)
            Hasher.Hash32x4(value, value, value, value, out h0, out h1, out h2, out h3);

        iterations = 0;
        sw.Restart();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
                Hasher.Hash32x4(value, value, value, value, out h0, out h1, out h2, out h3);
            iterations += 1000;
        }

        sw.Stop();
        double batchedOpsPerSec = (iterations * 4) / sw.Elapsed.TotalSeconds;

        Console.WriteLine($"  scalar:  {scalarOpsPerSec,12:F0} ops/sec");
        Console.WriteLine($"  batched: {batchedOpsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
        GC.KeepAlive(h0);
        GC.KeepAlive(h1);
        GC.KeepAlive(h2);
        GC.KeepAlive(h3);
    }
}
