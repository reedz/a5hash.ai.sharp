#include <stdio.h>
#include <stdint.h>
#include "../../a5hash.h"

int main() {
    uint8_t data[64];
    for (int i = 0; i < 64; i++) data[i] = (uint8_t)i;
    
    printf("C Native a5hash quick test:\n");
    printf("  a5hash(64 bytes):    0x%016llx\n", (unsigned long long)a5hash(data, 64, 0));
    printf("  a5hash32(64 bytes):  0x%08x\n", a5hash32(data, 64, 0));
    
    uint64_t high;
    uint64_t low = a5hash128(data, 64, 0, &high);
    printf("  a5hash128(64 bytes): 0x%016llx%016llx\n", (unsigned long long)high, (unsigned long long)low);
    
    return 0;
}
