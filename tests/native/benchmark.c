/**
 * Performance benchmark for native a5hash implementation.
 * Compile: gcc -O3 -march=native benchmark.c -o benchmark
 */

#include <stdio.h>
#include <stdlib.h>
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

    for (int i = 0; i < WARMUP_ITERATIONS; i++) {
        result = a5hash(data, size, 0);
    }

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

    for (int i = 0; i < WARMUP_ITERATIONS; i++) {
        result = a5hash32(data, size, 0);
    }

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

    for (int i = 0; i < WARMUP_ITERATIONS; i++) {
        result = a5hash128(data, size, 0, &high);
    }

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

int main(void) {
    printf("a5hash Native (C) Performance Benchmark\n");
    printf("========================================\n");
    printf("Each benchmark runs for %.1f seconds\n\n", BENCHMARK_DURATION_SEC);

    size_t sizes[] = {4, 8, 16, 32, 64, 128, 256, 512, 1024, 4096, 16384, 65536, 1048576};
    const char* names[] = {"4B", "8B", "16B", "32B", "64B", "128B", "256B", "512B", "1KB", "4KB", "16KB", "64KB", "1MB"};
    size_t num_sizes = sizeof(sizes) / sizeof(sizes[0]);

    uint8_t* data = (uint8_t*)malloc(1048576);
    if (!data) {
        fprintf(stderr, "Failed to allocate memory\n");
        return 1;
    }

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

    free(data);

    printf("\nBenchmark complete.\n");
    return 0;
}
