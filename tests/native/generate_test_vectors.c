/**
 * Generate test vectors for a5hash C# port verification.
 * Compile: gcc -O2 generate_test_vectors.c -o generate_test_vectors
 */

#include <stdio.h>
#include <string.h>
#include <stdint.h>

#include "../../a5hash.h"

void print_hex(const uint8_t* data, size_t len) {
    for (size_t i = 0; i < len; i++) {
        printf("%02x", data[i]);
    }
}

int main() {
    printf("// Auto-generated test vectors for a5hash verification\n");
    printf("// Format: input_hex, seed, expected_hash64, expected_hash32, expected_hash128_lo, expected_hash128_hi\n\n");
    
    // Test cases with various inputs
    struct {
        const char* name;
        const uint8_t* data;
        size_t len;
        uint64_t seed64;
        uint32_t seed32;
    } tests[] = {
        // Empty input
        {"empty", (const uint8_t*)"", 0, 0, 0},
        {"empty_seeded", (const uint8_t*)"", 0, 0x12345678ABCDEF00ULL, 0x12345678},
        
        // 1-3 byte inputs (special case in algorithm)
        {"1byte", (const uint8_t*)"\x00", 1, 0, 0},
        {"1byte_val", (const uint8_t*)"\xAB", 1, 0, 0},
        {"2bytes", (const uint8_t*)"\x01\x02", 2, 0, 0},
        {"3bytes", (const uint8_t*)"\x01\x02\x03", 3, 0, 0},
        
        // 4-8 byte inputs
        {"4bytes", (const uint8_t*)"\x01\x02\x03\x04", 4, 0, 0},
        {"5bytes", (const uint8_t*)"\x01\x02\x03\x04\x05", 5, 0, 0},
        {"8bytes", (const uint8_t*)"\x01\x02\x03\x04\x05\x06\x07\x08", 8, 0, 0},
        
        // 9-16 byte inputs
        {"9bytes", (const uint8_t*)"\x01\x02\x03\x04\x05\x06\x07\x08\x09", 9, 0, 0},
        {"16bytes", (const uint8_t*)"\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f\x10", 16, 0, 0},
        
        // 17-32 byte inputs (triggers main loop once for 64-bit)
        {"17bytes", (const uint8_t*)"\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f\x10\x11", 17, 0, 0},
        {"32bytes", (const uint8_t*)"\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f\x20", 32, 0, 0},
        
        // Seeded tests
        {"hello_seeded", (const uint8_t*)"Hello, World!", 13, 0xDEADBEEFCAFEBABEULL, 0xDEADBEEF},
        
        // ASCII string
        {"ascii", (const uint8_t*)"The quick brown fox jumps over the lazy dog", 43, 0, 0},
        
        // All zeros
        {"zeros16", (const uint8_t*)"\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00", 16, 0, 0},
        
        // All 0xFF
        {"ones16", (const uint8_t*)"\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff\xff", 16, 0, 0},
    };
    
    size_t num_tests = sizeof(tests) / sizeof(tests[0]);
    
    printf("public static readonly TestVector[] TestVectors = new TestVector[]\n{\n");
    
    for (size_t i = 0; i < num_tests; i++) {
        uint64_t hash64 = a5hash(tests[i].data, tests[i].len, tests[i].seed64);
        uint32_t hash32 = a5hash32(tests[i].data, tests[i].len, tests[i].seed32);
        
        uint64_t hash128_hi;
        uint64_t hash128_lo = a5hash128(tests[i].data, tests[i].len, tests[i].seed64, &hash128_hi);
        
        printf("    new TestVector(\n");
        printf("        \"%s\",\n", tests[i].name);
        printf("        new byte[] { ");
        for (size_t j = 0; j < tests[i].len; j++) {
            printf("0x%02x%s", tests[i].data[j], j < tests[i].len - 1 ? ", " : "");
        }
        printf(" },\n");
        printf("        0x%016llxUL,  // seed64\n", (unsigned long long)tests[i].seed64);
        printf("        0x%08x,      // seed32\n", tests[i].seed32);
        printf("        0x%016llxUL,  // expected hash64\n", (unsigned long long)hash64);
        printf("        0x%08x,      // expected hash32\n", hash32);
        printf("        0x%016llxUL,  // expected hash128 low\n", (unsigned long long)hash128_lo);
        printf("        0x%016llxUL   // expected hash128 high\n", (unsigned long long)hash128_hi);
        printf("    )%s\n", i < num_tests - 1 ? "," : "");
    }
    
    printf("};\n\n");
    
    // Generate larger test cases
    printf("// Large input tests (64+ bytes for a5hash128 multi-round)\n");
    uint8_t large_buf[256];
    for (int i = 0; i < 256; i++) {
        large_buf[i] = (uint8_t)i;
    }
    
    size_t large_sizes[] = {33, 64, 65, 100, 128, 256};
    size_t num_large = sizeof(large_sizes) / sizeof(large_sizes[0]);
    
    printf("public static readonly LargeTestVector[] LargeTestVectors = new LargeTestVector[]\n{\n");
    
    for (size_t i = 0; i < num_large; i++) {
        size_t len = large_sizes[i];
        uint64_t hash64 = a5hash(large_buf, len, 0);
        uint32_t hash32 = a5hash32(large_buf, len, 0);
        
        uint64_t hash128_hi;
        uint64_t hash128_lo = a5hash128(large_buf, len, 0, &hash128_hi);
        
        printf("    new LargeTestVector(\n");
        printf("        %zu,  // length (sequential bytes 0x00-0x%02x)\n", len, (unsigned)(len - 1));
        printf("        0x%016llxUL,  // expected hash64\n", (unsigned long long)hash64);
        printf("        0x%08x,      // expected hash32\n", hash32);
        printf("        0x%016llxUL,  // expected hash128 low\n", (unsigned long long)hash128_lo);
        printf("        0x%016llxUL   // expected hash128 high\n", (unsigned long long)hash128_hi);
        printf("    )%s\n", i < num_large - 1 ? "," : "");
    }
    
    printf("};\n");
    
    return 0;
}
