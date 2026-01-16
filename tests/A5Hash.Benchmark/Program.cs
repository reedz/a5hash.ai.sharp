using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.IO.Hashing;
using System.Net.Http;
using System.Numerics;
using System.Text.Json;

namespace A5Hash.Benchmark;

class Program
{
    const int WarmupIterations = 1000;
    const double BenchmarkDurationSec = 1.0;

    static void Main(string[] args)
    {
        bool minimal = Array.IndexOf(args, "--minimal") >= 0;
        bool minimal8 = Array.IndexOf(args, "--minimal8") >= 0;
        bool stringsOnly = Array.IndexOf(args, "--strings") >= 0;
        bool quality = Array.IndexOf(args, "--quality") >= 0;
        int qualitySamples = GetIntArg(args, "--quality-samples", 200_000);
        int qualityAvalanche = GetIntArg(args, "--quality-avalanche", 100_000);
        int qualityJsonMaxNames = GetIntArg(args, "--quality-json-max", 200_000);
        string? qualityJsonSource = GetStringArg(args, "--quality-json");
        bool minimalAny = minimal || minimal8;
        
        Console.WriteLine("a5hash C# Performance Benchmark");
        Console.WriteLine("================================");

        if (quality)
        {
            Console.WriteLine("QUALITY MODE: uniformity/collisions/avalanche");
        }
        else if (stringsOnly)
        {
            Console.WriteLine("STRING MODE: string.GetHashCode() vs A5Hash.Hash32(string)");
        }
        else if (minimal)
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

        // Prepare test sizes
        (int size, string name)[] sizes = minimal
            ? new[] { (4, "4B") }
            : minimal8
                ? new[] { (8, "8B") }
                : (stringsOnly || quality)
                    ? new[]
                    {
                        (4, "4B"), (8, "8B"), (16, "16B"), (32, "32B"), (64, "64B"),
                        (128, "128B"), (256, "256B"), (512, "512B"), (1024, "1KB"),
                        (4096, "4KB")
                    }
                    : new[]
                    {
                        (4, "4B"), (8, "8B"), (16, "16B"), (32, "32B"), (64, "64B"),
                        (128, "128B"), (256, "256B"), (512, "512B"), (1024, "1KB"),
                        (4096, "4KB"), (16384, "16KB"), (65536, "64KB"), (1048576, "1MB")
                    };

        if (quality)
        {
            if (!string.IsNullOrEmpty(qualityJsonSource))
            {
                try
                {
                    SetQualityCorpusOverride(qualityJsonSource, qualityJsonMaxNames);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: failed to load --quality-json corpus: {ex.Message}");
                }
            }

            Console.WriteLine("--- Hash quality (strings hashed as UTF-16) ---");
            foreach (var (size, name) in sizes)
            {
                int charLen = size / 2;
                RunStringQuality(charLen, name, qualitySamples, qualityAvalanche);
                Console.WriteLine();
            }

            Console.WriteLine("Benchmark complete.");
            return;
        }

        if (stringsOnly)
        {
            Console.WriteLine("--- string.GetHashCode() vs a5hash32 (UTF-16) ---");
            foreach (var (size, name) in sizes)
            {
                int charLen = size / 2;
                string s = CreateTestString(charLen);

                BenchmarkStringGetHashCode(s, size, name);
                BenchmarkA5Hash32String(s, size, name);
                Console.WriteLine();
            }

            Console.WriteLine("Benchmark complete.");
            return;
        }

        // Allocate and fill buffer
        byte[] data = new byte[1048576];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(i * 31 + 17);
        }

        Console.WriteLine("--- a5hash (64-bit) ---");
        foreach (var (size, name) in sizes)
        {
            if (minimalAny)
            {
                if (size == 4)
                    BenchmarkHash64Minimal(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, size)), name);
                else if (size == 8)
                    BenchmarkHash64Minimal(BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(0, size)), name);
                else
                    BenchmarkHash64Minimal(data.AsSpan(0, size), name);
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
                if (size == 4)
                    BenchmarkHash32Minimal(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, size)), name);
                else if (size == 8)
                    BenchmarkHash32Minimal(BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(0, size)), name);
                else
                    BenchmarkHash32Minimal(data.AsSpan(0, size), name);
            }
            else
            {
                BenchmarkHash32(data.AsSpan(0, size), name);
            }
        }

        Console.WriteLine();
        Console.WriteLine("--- a5hash128 (128-bit) ---");
        foreach (var (size, name) in sizes)
        {
            if (minimalAny)
            {
                if (size == 4)
                    BenchmarkHash128Minimal(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, size)), name);
                else if (size == 8)
                    BenchmarkHash128Minimal(BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(0, size)), name);
                else
                    BenchmarkHash128Minimal(data.AsSpan(0, size), name);
            }
            else
            {
                BenchmarkHash128(data.AsSpan(0, size), name);
            }
        }

        Console.WriteLine();
        Console.WriteLine("--- System.IO.Hashing.Crc32 ---");
        foreach (var (size, name) in sizes)
        {
            if (minimalAny)
                BenchmarkCrc32Minimal(data.AsSpan(0, size), name);
            else
                BenchmarkCrc32(data.AsSpan(0, size), name);
        }

        Console.WriteLine();
        Console.WriteLine("Benchmark complete.");
    }

    static int GetIntArg(string[] args, string name, int defaultValue)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name && int.TryParse(args[i + 1], out int value) && value > 0)
                return value;
        }
        return defaultValue;
    }

    static string? GetStringArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return null;
    }

    static string CreateTestString(int length)
    {
        if (length <= 0)
            return string.Empty;

        var chars = new char[length];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)('a' + ((i * 31 + 17) % 26));
        }
        return new string(chars);
    }

    static string CreateRandomString(int length, Random rng)
    {
        if (length <= 0)
            return string.Empty;

        var chars = new char[length];
        for (int i = 0; i < chars.Length; i++)
        {
            // Printable ASCII range: 32..126
            chars[i] = (char)(32 + rng.Next(95));
        }
        return new string(chars);
    }

    static string[]? s_msbuildWikiCorpus;
    static string[]? s_qualityCorpusOverride;

    static string[] GetMsbuildWikiCorpus()
    {
        if (s_msbuildWikiCorpus is not null)
            return s_msbuildWikiCorpus;

        var set = new HashSet<string>(StringComparer.Ordinal);

        // Publicly obtainable corpus (inspired by MSBuildStructuredLog wiki micro-benchmark):
        // env vars + other common process-visible strings.
        foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
        {
            if (e.Key is string k && k.Length != 0)
                set.Add(k);
            if (e.Value is string v && v.Length != 0)
                set.Add(v);
        }

        // Command-line
        try
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (!string.IsNullOrEmpty(args[i])) set.Add(args[i]);
        }
        catch { }
        if (!string.IsNullOrEmpty(Environment.CommandLine)) set.Add(Environment.CommandLine);
        if (!string.IsNullOrEmpty(Environment.CurrentDirectory)) set.Add(Environment.CurrentDirectory);
        if (!string.IsNullOrEmpty(AppContext.BaseDirectory)) set.Add(AppContext.BaseDirectory);

        // Split PATH into individual entries (also captured as a single env var value).
        try
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(path))
            {
                foreach (var part in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
                    set.Add(part.Trim());
            }
        }
        catch { }

        // Runtime / OS descriptors
        try
        {
            set.Add(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            set.Add(System.Runtime.InteropServices.RuntimeInformation.OSDescription);
            set.Add(System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString());
            set.Add(System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString());
        }
        catch { }

        // Loaded assembly names (and locations where available)
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var a = assemblies[i];
                if (!string.IsNullOrEmpty(a.FullName)) set.Add(a.FullName);
                if (!a.IsDynamic && !string.IsNullOrEmpty(a.Location)) set.Add(a.Location);
            }
        }
        catch { }

        // Repo-local file names / paths (kept bounded)
        try
        {
            int added = 0;
            foreach (var p in Directory.EnumerateFileSystemEntries(".", "*", SearchOption.AllDirectories))
            {
                if (p.StartsWith("./.git", StringComparison.Ordinal) || p.StartsWith(".git", StringComparison.Ordinal))
                    continue;
                if (p.Contains("/bin/", StringComparison.Ordinal) || p.Contains("/obj/", StringComparison.Ordinal) || p.Contains("\\bin\\", StringComparison.Ordinal) || p.Contains("\\obj\\", StringComparison.Ordinal))
                    continue;
                if (p.StartsWith("./artifacts", StringComparison.Ordinal) || p.StartsWith("artifacts", StringComparison.Ordinal))
                    continue;

                set.Add(p);
                var name = Path.GetFileName(p);
                if (!string.IsNullOrEmpty(name)) set.Add(name);

                if (++added >= 50_000)
                    break;
            }
        }
        catch { }

        var arr = new string[set.Count];
        set.CopyTo(arr);
        Array.Sort(arr, StringComparer.Ordinal);
        s_msbuildWikiCorpus = arr;
        return arr;
    }

    static void SetQualityCorpusOverride(string source, int maxNames)
    {
        if (maxNames <= 0)
            maxNames = 200_000;

        var set = new HashSet<string>(StringComparer.Ordinal);

        if (source.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            source.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(30);
            using var stream = http.GetStreamAsync(source).GetAwaiter().GetResult();
            AddJsonPropertyNames(stream, set, maxNames);
        }
        else if (Directory.Exists(source))
        {
            foreach (var file in Directory.EnumerateFiles(source, "*.json", SearchOption.AllDirectories))
            {
                using var stream = File.OpenRead(file);
                AddJsonPropertyNames(stream, set, maxNames);
                if (set.Count >= maxNames)
                    break;
            }
        }
        else
        {
            using var stream = File.OpenRead(source);
            AddJsonPropertyNames(stream, set, maxNames);
        }

        var arr = new string[set.Count];
        set.CopyTo(arr);
        Array.Sort(arr, StringComparer.Ordinal);
        s_qualityCorpusOverride = arr;

        Console.WriteLine($"Loaded JSON corpus: {arr.Length} unique property names");
    }

    static void AddJsonPropertyNames(Stream utf8Json, HashSet<string> names, int maxNames)
    {
        using var doc = JsonDocument.Parse(utf8Json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        AddJsonPropertyNames(doc.RootElement, names, maxNames);
    }

    static void AddJsonPropertyNames(JsonElement el, HashSet<string> names, int maxNames)
    {
        if (names.Count >= maxNames)
            return;

        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var p in el.EnumerateObject())
                {
                    if (!string.IsNullOrEmpty(p.Name))
                    {
                        names.Add(p.Name);
                        if (names.Count >= maxNames)
                            return;
                    }

                    AddJsonPropertyNames(p.Value, names, maxNames);
                    if (names.Count >= maxNames)
                        return;
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in el.EnumerateArray())
                {
                    AddJsonPropertyNames(item, names, maxNames);
                    if (names.Count >= maxNames)
                        return;
                }
                break;
        }
    }

    static string FitToLength(string s, int length)
    {
        if (length <= 0)
            return string.Empty;
        if (string.IsNullOrEmpty(s))
            s = "x";
        if (s.Length == length)
            return s;
        if (s.Length > length)
            return s.Substring(s.Length - length, length);

        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = s[i % s.Length];
        return new string(chars);
    }

    static void RunStringQuality(int length, string name, int samples, int avalancheSamples)
    {
        // Base corpus can be overridden with JSON property names (for System.Text.Json-like workloads).
        var baseCorpus = s_qualityCorpusOverride ?? GetMsbuildWikiCorpus();
        var corpusSet = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < baseCorpus.Length; i++)
            corpusSet.Add(FitToLength(baseCorpus[i], length));

        var corpus = new string[corpusSet.Count];
        corpusSet.CopyTo(corpus);
        if (corpus.Length == 0)
            corpus = new[] { FitToLength("a", length) };

        int actualSamples = Math.Min(samples, corpus.Length);
        int actualAvalanche = Math.Min(avalancheSamples, Math.Max(1, corpus.Length));

        var rng = new Random(12345 + length);
        ShuffleInPlace(corpus, rng);

        // Bit uniformity
        long[] bitOnCountsString = new long[32];
        long[] bitOnCountsA5 = new long[32];

        // Bucket uniformity (chi-square vs uniform); size buckets based on sample count.
        int bucketBits = Math.Min(16, Math.Max(4, BitOperations.Log2((uint)Math.Max(1, actualSamples))));
        int bucketsPow2 = 1 << bucketBits;
        var bucketsString = new int[bucketsPow2];
        var bucketsA5 = new int[bucketsPow2];

        // Collision tracking (exact collisions, sample-sized hashset)
        var seenString = new HashSet<int>(Math.Min(actualSamples, 1_000_000));
        var seenA5 = new HashSet<uint>(Math.Min(actualSamples, 1_000_000));
        int collisionsString = 0;
        int collisionsA5 = 0;

        for (int i = 0; i < actualSamples; i++)
        {
            string s = corpus[i];

            int hs = s.GetHashCode();
            uint ha = global::A5Hash.A5Hash.Hash32(s, 0);

            if (!seenString.Add(hs)) collisionsString++;
            if (!seenA5.Add(ha)) collisionsA5++;

            uint uhs = unchecked((uint)hs);

            bucketsString[uhs & (bucketsPow2 - 1)]++;
            bucketsA5[ha & (bucketsPow2 - 1)]++;

            for (int b = 0; b < 32; b++)
            {
                bitOnCountsString[b] += (uhs >> b) & 1u;
                bitOnCountsA5[b] += (ha >> b) & 1u;
            }
        }

        double chiString = ChiSquareUniform(bucketsString, actualSamples);
        double chiA5 = ChiSquareUniform(bucketsA5, actualSamples);

        (double worstBiasString, double avgBiasString) = BitBiasStats(bitOnCountsString, actualSamples);
        (double worstBiasA5, double avgBiasA5) = BitBiasStats(bitOnCountsA5, actualSamples);

        // Avalanche: flip one bit in one character and measure output bit flips
        double avgFlipString = 0;
        double avgFlipA5 = 0;
        int minFlipString = int.MaxValue, maxFlipString = 0;
        int minFlipA5 = int.MaxValue, maxFlipA5 = 0;

        for (int i = 0; i < actualAvalanche; i++)
        {
            string s = corpus[rng.Next(corpus.Length)];
            if (s.Length == 0)
                s = "a";

            int pos = rng.Next(s.Length);
            int bit = rng.Next(16);

            char[] tmp = s.ToCharArray();
            tmp[pos] = (char)(tmp[pos] ^ (1 << bit));
            string s2 = new string(tmp);

            uint h1s = unchecked((uint)s.GetHashCode());
            uint h2s = unchecked((uint)s2.GetHashCode());
            uint h1a = global::A5Hash.A5Hash.Hash32(s, 0);
            uint h2a = global::A5Hash.A5Hash.Hash32(s2, 0);

            int flipsS = BitOperations.PopCount(h1s ^ h2s);
            int flipsA = BitOperations.PopCount(h1a ^ h2a);

            avgFlipString += flipsS;
            avgFlipA5 += flipsA;
            if (flipsS < minFlipString) minFlipString = flipsS;
            if (flipsS > maxFlipString) maxFlipString = flipsS;
            if (flipsA < minFlipA5) minFlipA5 = flipsA;
            if (flipsA > maxFlipA5) maxFlipA5 = flipsA;
        }

        avgFlipString /= actualAvalanche;
        avgFlipA5 /= actualAvalanche;

        Console.WriteLine($"{name,12} (len={length} chars, corpus={corpus.Length})");
        Console.WriteLine($"  collisions: string {collisionsString} / {actualSamples}  | a5hash32 {collisionsA5} / {actualSamples}");
        Console.WriteLine($"  buckets χ²: string {chiString:F1}  | a5hash32 {chiA5:F1} (df={bucketsPow2 - 1})");
        Console.WriteLine($"  bit bias  : string worst {worstBiasString:P3}, avg {avgBiasString:P3} | a5hash32 worst {worstBiasA5:P3}, avg {avgBiasA5:P3}");
        Console.WriteLine($"  avalanche : avg bit flips (of 32): string {avgFlipString:F2} (min {minFlipString}, max {maxFlipString}) | a5hash32 {avgFlipA5:F2} (min {minFlipA5}, max {maxFlipA5})");
    }

    static void ShuffleInPlace<T>(T[] array, Random rng)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
 
    static double ChiSquareUniform(int[] buckets, int samples)
    {
        double expected = (double)samples / buckets.Length;
        double chi = 0;
        for (int i = 0; i < buckets.Length; i++)
        {
            double diff = buckets[i] - expected;
            chi += (diff * diff) / expected;
        }
        return chi;
    }

    static (double worstBias, double avgBias) BitBiasStats(long[] bitOnCounts, int samples)
    {
        double worst = 0;
        double sum = 0;
        for (int b = 0; b < bitOnCounts.Length; b++)
        {
            double p = bitOnCounts[b] / (double)samples;
            double bias = Math.Abs(p - 0.5);
            if (bias > worst) worst = bias;
            sum += bias;
        }
        return (worst, sum / bitOnCounts.Length);
    }

    static void BenchmarkStringGetHashCode(string s, int sizeBytes, string name)
    {
        int result = 0;

        for (int i = 0; i < WarmupIterations; i++)
        {
            result = s.GetHashCode();
        }

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = s.GetHashCode();
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * (long)sizeBytes) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"string.GHC {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(result);
        GC.KeepAlive(s);
    }

    static void BenchmarkA5Hash32String(string s, int sizeBytes, string name)
    {
        uint result = 0;

        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash32(s, 0);
        }

        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash32(s, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * (long)sizeBytes) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(result);
        GC.KeepAlive(s);
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
        ulong low = 0;
        ulong high = 0;
        int size = data.Length;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            low = global::A5Hash.A5Hash.Hash128(data, out high, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();
        
        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                low = global::A5Hash.A5Hash.Hash128(data, out high, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;
        double bytesPerSec = (iterations * size) / elapsed;
        double gbPerSec = bytesPerSec / (1024.0 * 1024.0 * 1024.0);

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec, {gbPerSec,8:F3} GB/s");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
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
    static void BenchmarkHash64Minimal(uint value, string name)
    {
        ulong result = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash(value, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash(value, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash64Minimal(ulong value, string name)
    {
        ulong result = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash(value, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash(value, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash    {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

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

    static void BenchmarkHash32Minimal(uint value, string name)
    {
        uint result = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash32(value, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash32(value, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(result);
    }

    static void BenchmarkHash32Minimal(ulong value, string name)
    {
        uint result = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            result = global::A5Hash.A5Hash.Hash32(value, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                result = global::A5Hash.A5Hash.Hash32(value, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash32  {name,12}: {opsPerSec,12:F0} ops/sec");
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

    static void BenchmarkHash128Minimal(uint value, string name)
    {
        ulong low = 0;
        ulong high = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            low = global::A5Hash.A5Hash.Hash128(value, out high, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                low = global::A5Hash.A5Hash.Hash128(value, out high, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
    }

    static void BenchmarkHash128Minimal(ulong value, string name)
    {
        ulong low = 0;
        ulong high = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            low = global::A5Hash.A5Hash.Hash128(value, out high, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                low = global::A5Hash.A5Hash.Hash128(value, out high, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
    }

    static void BenchmarkHash128Minimal(ReadOnlySpan<byte> data, string name)
    {
        ulong low = 0;
        ulong high = 0;

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            low = global::A5Hash.A5Hash.Hash128(data, out high, 0);
        }

        // Benchmark
        long iterations = 0;
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed.TotalSeconds < BenchmarkDurationSec)
        {
            for (int i = 0; i < 1000; i++)
            {
                low = global::A5Hash.A5Hash.Hash128(data, out high, 0);
            }
            iterations += 1000;
        }

        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;
        double opsPerSec = iterations / elapsed;

        Console.WriteLine($"a5hash128 {name,12}: {opsPerSec,12:F0} ops/sec");
        GC.KeepAlive(low);
        GC.KeepAlive(high);
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
