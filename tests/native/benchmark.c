/**
 * Performance benchmark for native a5hash implementation.
 * Compile: gcc -O3 -march=native benchmark.c -o benchmark
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <time.h>

#include "../../a5hash.h"

#define WARMUP_ITERATIONS 1000
#define BENCHMARK_DURATION_SEC 1.0

static double get_time_sec(void) {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return ts.tv_sec + ts.tv_nsec / 1e9;
}

static void benchmark_hash64(const uint8_t* data, size_t size, const char* name) {
    volatile uint64_t result = 0;
    
    // Warmup
    for (int i = 0; i < WARMUP_ITERATIONS; i++) {
        result = a5hash(data, size, 0);
    }
    
    // Benchmark
    uint64_t iterations = 0;
    double start = get_time_sec();
    double end;
    
    do {
        for (int i = 0; i < 1000; i++) {
            result = a5hash(data, size, 0);
        }
        iterations += 1000;
        end = get_time_sec();
    } while (end - start < BENCHMARK_DURATION_SEC);
    
    double elapsed = end - start;
    double ops_per_sec = iterations / elapsed;
    double bytes_per_sec = (iterations * size) / elapsed;
    double gb_per_sec = bytes_per_sec / (1024.0 * 1024.0 * 1024.0);
    
    printf("a5hash    %12s: %12.0f ops/sec, %8.3f GB/s\n", name, ops_per_sec, gb_per_sec);
    (void)result;
}

static void benchmark_hash32(const uint8_t* data, size_t size, const char* name) {
    volatile uint32_t result = 0;
    
    // Warmup
    for (int i = 0; i < WARMUP_ITERATIONS; i++) {
        result = a5hash32(data, size, 0);
    }
    
    // Benchmark
    uint64_t iterations = 0;
    double start = get_time_sec();
    double end;
    
    do {
        for (int i = 0; i < 1000; i++) {
            result = a5hash32(data, size, 0);
        }
        iterations += 1000;
        end = get_time_sec();
    } while (end - start < BENCHMARK_DURATION_SEC);
    
    double elapsed = end - start;
    double ops_per_sec = iterations / elapsed;
    double bytes_per_sec = (iterations * size) / elapsed;
    double gb_per_sec = bytes_per_sec / (1024.0 * 1024.0 * 1024.0);
    
    printf("a5hash32  %12s: %12.0f ops/sec, %8.3f GB/s\n", name, ops_per_sec, gb_per_sec);
    (void)result;
}

static void benchmark_hash128(const uint8_t* data, size_t size, const char* name) {
    volatile uint64_t result = 0;
    uint64_t high;
    
    // Warmup
    for (int i = 0; i < WARMUP_ITERATIONS; i++) {
        result = a5hash128(data, size, 0, &high);
    }
    
    // Benchmark
    uint64_t iterations = 0;
    double start = get_time_sec();
    double end;
    
    do {
        for (int i = 0; i < 1000; i++) {
            result = a5hash128(data, size, 0, &high);
        }
        iterations += 1000;
        end = get_time_sec();
    } while (end - start < BENCHMARK_DURATION_SEC);
    
    double elapsed = end - start;
    double ops_per_sec = iterations / elapsed;
    double bytes_per_sec = (iterations * size) / elapsed;
    double gb_per_sec = bytes_per_sec / (1024.0 * 1024.0 * 1024.0);
    
    printf("a5hash128 %12s: %12.0f ops/sec, %8.3f GB/s\n", name, ops_per_sec, gb_per_sec);
    (void)result;
}

static uint32_t crc32_table[256];
static int crc32_table_init = 0;

static void init_crc32_table(void) {
    uint32_t polynomial = 0xEDB88320;
    for (uint32_t i = 0; i < 256; i++) {
        uint32_t c = i;
        for (int j = 0; j < 8; j++) {
            if (c & 1) {
                c = polynomial ^ (c >> 1);
            } else {
                c >>= 1;
            }
        }
        crc32_table[i] = c;
    }
    crc32_table_init = 1;
}

static uint32_t crc32(const uint8_t* data, size_t length) {
    if (!crc32_table_init) init_crc32_table();
    
    uint32_t c = 0xFFFFFFFF;
    for (size_t i = 0; i < length; i++) {
        c = crc32_table[(c ^ data[i]) & 0xFF] ^ (c >> 8);
    }
    return c ^ 0xFFFFFFFF;
}

static void benchmark_crc32(const uint8_t* data, size_t size, const char* name) {
    volatile uint32_t result = 0;
    
    // Warmup
    for (int i = 0; i < WARMUP_ITERATIONS; i++) {
        result = crc32(data, size);
    }
    
    // Benchmark
    uint64_t iterations = 0;
    double start = get_time_sec();
    double end;
    
    do {
        for (int i = 0; i < 1000; i++) {
            result = crc32(data, size);
        }
        iterations += 1000;
        end = get_time_sec();
    } while (end - start < BENCHMARK_DURATION_SEC);
    
    double elapsed = end - start;
    double ops_per_sec = iterations / elapsed;
    double bytes_per_sec = (iterations * size) / elapsed;
    double gb_per_sec = bytes_per_sec / (1024.0 * 1024.0 * 1024.0);
    
    printf("crc32     %12s: %12.0f ops/sec, %8.3f GB/s\n", name, ops_per_sec, gb_per_sec);
    (void)result;
}

int main(int argc, char** argv) {
    printf("a5hash Native (C) Performance Benchmark\n");
    printf("========================================\n");
    printf("Each benchmark runs for %.1f seconds\n\n", BENCHMARK_DURATION_SEC);
    
    // Prepare test data
    size_t sizes[] = {4, 8, 16, 32, 64, 128, 256, 512, 1024, 4096, 16384, 65536, 1048576};
    const char* names[] = {"4B", "8B", "16B", "32B", "64B", "128B", "256B", "512B", "1KB", "4KB", "16KB", "64KB", "1MB"};
    size_t num_sizes = sizeof(sizes) / sizeof(sizes[0]);
    
    // Allocate largest buffer
    uint8_t* data = (uint8_t*)malloc(1048576);
    if (!data) {
        fprintf(stderr, "Failed to allocate memory\n");
        return 1;
    }
    
    // Fill with pseudo-random data
    for (size_t i = 0; i < 1048576; i++) {
        data[i] = (uint8_t)(i * 31 + 17);
    }
    
    printf("--- a5hash (64-bit) ---\n");
    for (size_t i = 0; i < num_sizes; i++) {
        benchmark_hash64(data, sizes[i], names[i]);
    }
    
    printf("\n--- a5hash32 (32-bit) ---\n");
    for (size_t i = 0; i < num_sizes; i++) {
        benchmark_hash32(data, sizes[i], names[i]);
    }
    
    printf("\n--- a5hash128 (128-bit) ---\n");
    for (size_t i = 0; i < num_sizes; i++) {
        benchmark_hash128(data, sizes[i], names[i]);
    }
    
    printf("\n--- CRC32 ---\n");
    for (size_t i = 0; i < num_sizes; i++) {
        benchmark_crc32(data, sizes[i], names[i]);
    }
    
    free(data);
    
    printf("\nBenchmark complete.\n");
    return 0;
}
