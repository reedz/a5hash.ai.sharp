namespace A5Hash.Tests;

/// <summary>
/// Test vector for verifying a5hash implementation produces correct output.
/// </summary>
public record TestVector(
    string Name,
    byte[] Input,
    ulong Seed64,
    uint Seed32,
    ulong ExpectedHash64,
    uint ExpectedHash32,
    ulong ExpectedHash128Low,
    ulong ExpectedHash128High
);

/// <summary>
/// Test vector for large inputs (sequential bytes starting from 0x00).
/// </summary>
public record LargeTestVector(
    int Length,
    ulong ExpectedHash64,
    uint ExpectedHash32,
    ulong ExpectedHash128Low,
    ulong ExpectedHash128High
);

/// <summary>
/// Auto-generated test vectors from native a5hash implementation.
/// </summary>
public static class TestVectors
{
    public static readonly TestVector[] Vectors = new TestVector[]
    {
        new TestVector(
            "empty",
            new byte[] {  },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0x0f2d4f1152e2fd91UL,  // expected hash64
            0x969d3f21,      // expected hash32
            0x0f2d4f1152e2fd91UL,  // expected hash128 low
            0xd945ac0d4f73ca5dUL   // expected hash128 high
        ),
        new TestVector(
            "empty_seeded",
            new byte[] {  },
            0x12345678abcdef00UL,  // seed64
            0x12345678,      // seed32
            0x509fba71029f8ab4UL,  // expected hash64
            0xdecabf9e,      // expected hash32
            0x509fba71029f8ab4UL,  // expected hash128 low
            0x2628c6679323d20aUL   // expected hash128 high
        ),
        new TestVector(
            "1byte",
            new byte[] { 0x00 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xe16325b37108afeeUL,  // expected hash64
            0x4f4aa1e0,      // expected hash32
            0xe16325b37108afeeUL,  // expected hash128 low
            0xf6bf70b1301bc4b6UL   // expected hash128 high
        ),
        new TestVector(
            "1byte_val",
            new byte[] { 0xab },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0x6d290c7cc483cba5UL,  // expected hash64
            0xe68bffab,      // expected hash32
            0x8c6cc077ec738ccaUL,  // expected hash128 low
            0xc2f703e0ef338872UL   // expected hash128 high
        ),
        new TestVector(
            "2bytes",
            new byte[] { 0x01, 0x02 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xbfe436f6fa18029fUL,  // expected hash64
            0xc9896e78,      // expected hash32
            0x19441a609058c0c5UL,  // expected hash128 low
            0x66298bed3738b52cUL   // expected hash128 high
        ),
        new TestVector(
            "3bytes",
            new byte[] { 0x01, 0x02, 0x03 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xc82129fd511c0c1cUL,  // expected hash64
            0xb96ae160,      // expected hash32
            0x5a320da64b97f25fUL,  // expected hash128 low
            0x8a80f6270330b008UL   // expected hash128 high
        ),
        new TestVector(
            "4bytes",
            new byte[] { 0x01, 0x02, 0x03, 0x04 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0x58f16aeabad3e124UL,  // expected hash64
            0x9c3517d8,      // expected hash32
            0x0f56c160db680bb5UL,  // expected hash128 low
            0x7fe7261ea9236d9fUL   // expected hash128 high
        ),
        new TestVector(
            "5bytes",
            new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xdc301264d033a6ddUL,  // expected hash64
            0xa6e50b7b,      // expected hash32
            0x6e54d8c411342567UL,  // expected hash128 low
            0x3137eef72cf5e81eUL   // expected hash128 high
        ),
        new TestVector(
            "8bytes",
            new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xe9b2e9af9245d5aeUL,  // expected hash64
            0x6c110d63,      // expected hash32
            0xa0e8f52dc863c4f2UL,  // expected hash128 low
            0x01496a5232238c62UL   // expected hash128 high
        ),
        new TestVector(
            "9bytes",
            new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0x5029a744a31e35daUL,  // expected hash64
            0x9d6d8996,      // expected hash32
            0x18a4f8e2db0b26d7UL,  // expected hash128 low
            0x2b1fc549b4338e3eUL   // expected hash128 high
        ),
        new TestVector(
            "16bytes",
            new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xd7938cd3f5371973UL,  // expected hash64
            0xcc64a728,      // expected hash32
            0x23dbf92d70c3a9e3UL,  // expected hash128 low
            0x5406be07b64ab897UL   // expected hash128 high
        ),
        new TestVector(
            "17bytes",
            new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xc8b4616cc61eef88UL,  // expected hash64
            0xa88afedc,      // expected hash32
            0xc6ea7fc549666438UL,  // expected hash128 low
            0xcf64287c14f3e26dUL   // expected hash128 high
        ),
        new TestVector(
            "32bytes",
            new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xf765f48bfdbf928aUL,  // expected hash64
            0xf346f993,      // expected hash32
            0x6e1d0b3ef77da7a6UL,  // expected hash128 low
            0x900e03a6808431aaUL   // expected hash128 high
        ),
        new TestVector(
            "hello_seeded",
            new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64, 0x21 },
            0xdeadbeefcafebabeUL,  // seed64
            0xdeadbeef,      // seed32
            0x885137eaa6a7f5e8UL,  // expected hash64
            0xa8d1436e,      // expected hash32
            0x4421f8bc2f763a9dUL,  // expected hash128 low
            0x8a21bffa0eefaa02UL   // expected hash128 high
        ),
        new TestVector(
            "ascii",
            new byte[] { 0x54, 0x68, 0x65, 0x20, 0x71, 0x75, 0x69, 0x63, 0x6b, 0x20, 0x62, 0x72, 0x6f, 0x77, 0x6e, 0x20, 0x66, 0x6f, 0x78, 0x20, 0x6a, 0x75, 0x6d, 0x70, 0x73, 0x20, 0x6f, 0x76, 0x65, 0x72, 0x20, 0x74, 0x68, 0x65, 0x20, 0x6c, 0x61, 0x7a, 0x79, 0x20, 0x64, 0x6f, 0x67 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xddd577a973b0996bUL,  // expected hash64
            0xc5d35ba3,      // expected hash32
            0xa2d7775b627ee6c0UL,  // expected hash128 low
            0x63f183ae6a157678UL   // expected hash128 high
        ),
        new TestVector(
            "zeros16",
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xf6c885ba9accd3e1UL,  // expected hash64
            0x0d90f13b,      // expected hash32
            0xf6c885ba9accd3e1UL,  // expected hash128 low
            0x6de152461a74b54bUL   // expected hash128 high
        ),
        new TestVector(
            "ones16",
            new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff },
            0x0000000000000000UL,  // seed64
            0x00000000,      // seed32
            0xdcf7d4710e21d606UL,  // expected hash64
            0xadcf76c9,      // expected hash32
            0xf2d6fe4ed547cc96UL,  // expected hash128 low
            0xe0c07fda19142717UL   // expected hash128 high
        )
    };

    /// <summary>
    /// Large input test vectors (sequential bytes 0x00, 0x01, 0x02, ...).
    /// </summary>
    public static readonly LargeTestVector[] LargeVectors = new LargeTestVector[]
    {
        new LargeTestVector(
            33,  // length (sequential bytes 0x00-0x20)
            0x9003c96b9f384a5eUL,  // expected hash64
            0xf2365846,      // expected hash32
            0x22ca1a10c8c876c2UL,  // expected hash128 low
            0xe9465ad40546cec5UL   // expected hash128 high
        ),
        new LargeTestVector(
            64,  // length (sequential bytes 0x00-0x3f)
            0x28c1bfd6895d7468UL,  // expected hash64
            0xadab12ca,      // expected hash32
            0x1a2936014ca9b223UL,  // expected hash128 low
            0xbed13321e2e18fe8UL   // expected hash128 high
        ),
        new LargeTestVector(
            65,  // length (sequential bytes 0x00-0x40)
            0x0d5e85b646c12accUL,  // expected hash64
            0xe63ca96e,      // expected hash32
            0x68e796cc003240e2UL,  // expected hash128 low
            0xbf2a5d4d843ff357UL   // expected hash128 high
        ),
        new LargeTestVector(
            100,  // length (sequential bytes 0x00-0x63)
            0xca53bf1c33c9bb00UL,  // expected hash64
            0x2a2c3eed,      // expected hash32
            0x136c63d94c762753UL,  // expected hash128 low
            0xe8c20e13361679d9UL   // expected hash128 high
        ),
        new LargeTestVector(
            128,  // length (sequential bytes 0x00-0x7f)
            0x2276d989d05bde4dUL,  // expected hash64
            0x365d1e40,      // expected hash32
            0xbd6568458d0aca4fUL,  // expected hash128 low
            0x007f07d1d6973860UL   // expected hash128 high
        ),
        new LargeTestVector(
            256,  // length (sequential bytes 0x00-0xff)
            0x92a52c8586258560UL,  // expected hash64
            0xce642dc8,      // expected hash32
            0x389d24736fc07578UL,  // expected hash128 low
            0xb528a757bfd3c6f1UL   // expected hash128 high
        )
    };

    /// <summary>
    /// Generate sequential byte array (0x00, 0x01, 0x02, ...) for large tests.
    /// </summary>
    public static byte[] GenerateSequentialBytes(int length)
    {
        var data = new byte[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = (byte)i;
        }
        return data;
    }
}
