using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Test AstcUtility.
    /// </summary>
    public class AstcUtilityTests
    {
        private string tempFolder;

        /// <summary>
        /// Setup test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            tempFolder = Path.GetFullPath(Path.Combine("Temp", "VRCQuestTools", "astcenc-tests", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(tempFolder);
        }

        /// <summary>
        /// Cleanup test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }

        /// <summary>
        /// Test TryGetBlockSize for supported ASTC formats.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="expectedX">Expected block width.</param>
        /// <param name="expectedY">Expected block height.</param>
        [TestCase(TextureFormat.ASTC_4x4, 4, 4)]
        [TestCase(TextureFormat.ASTC_5x5, 5, 5)]
        [TestCase(TextureFormat.ASTC_6x6, 6, 6)]
        [TestCase(TextureFormat.ASTC_8x8, 8, 8)]
        [TestCase(TextureFormat.ASTC_10x10, 10, 10)]
        [TestCase(TextureFormat.ASTC_12x12, 12, 12)]
        public void TryGetBlockSizeSupported(TextureFormat format, int expectedX, int expectedY)
        {
            Assert.IsTrue(AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY));
            Assert.AreEqual(expectedX, blockX);
            Assert.AreEqual(expectedY, blockY);
        }

        /// <summary>
        /// Test TryGetBlockSize for unsupported formats.
        /// </summary>
        /// <param name="format">Texture format.</param>
        [TestCase(TextureFormat.RGBA32)]
        [TestCase(TextureFormat.DXT5)]
        [TestCase(TextureFormat.ASTC_HDR_4x4)]
        public void TryGetBlockSizeUnsupported(TextureFormat format)
        {
            Assert.IsFalse(AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY));
            Assert.AreEqual(0, blockX);
            Assert.AreEqual(0, blockY);
        }

        /// <summary>
        /// Test GetBlockSizeString.
        /// </summary>
        [Test]
        public void GetBlockSizeString()
        {
            Assert.AreEqual("4x4", AstcUtility.GetBlockSizeString(TextureFormat.ASTC_4x4));
            Assert.AreEqual("6x6", AstcUtility.GetBlockSizeString(TextureFormat.ASTC_6x6));
            Assert.AreEqual("12x12", AstcUtility.GetBlockSizeString(TextureFormat.ASTC_12x12));
            Assert.Throws<ArgumentException>(() => AstcUtility.GetBlockSizeString(TextureFormat.RGBA32));
        }

        /// <summary>
        /// Test GetMipDataSize.
        /// </summary>
        /// <param name="width">Mip width.</param>
        /// <param name="height">Mip height.</param>
        /// <param name="blockX">Block width.</param>
        /// <param name="blockY">Block height.</param>
        /// <param name="expected">Expected data size.</param>
        [TestCase(1024, 1024, 4, 4, 65536 * 16)]
        [TestCase(10, 6, 4, 4, 3 * 2 * 16)]
        [TestCase(4, 4, 4, 4, 16)]
        [TestCase(1, 1, 12, 12, 16)]
        [TestCase(2048, 2048, 6, 6, 342 * 342 * 16)]
        [TestCase(16, 16, 8, 8, 2 * 2 * 16)]
        public void GetMipDataSize(int width, int height, int blockX, int blockY, int expected)
        {
            Assert.AreEqual(expected, AstcUtility.GetMipDataSize(width, height, blockX, blockY));
        }

        /// <summary>
        /// Test WriteTga header fields and BGRA pixel order.
        /// </summary>
        [Test]
        public void WriteTgaHeaderAndPixels()
        {
            var pixels = new[]
            {
                new Color32(1, 2, 3, 4),
                new Color32(5, 6, 7, 8),
                new Color32(9, 10, 11, 12),
                new Color32(13, 14, 15, 16),
            };
            var path = Path.Combine(tempFolder, "test.tga");
            AstcUtility.WriteTga(pixels, 2, 2, true, path);

            var data = File.ReadAllBytes(path);
            Assert.AreEqual(18 + (4 * 4), data.Length);
            Assert.AreEqual(0, data[0]); // ID length
            Assert.AreEqual(0, data[1]); // Color map type
            Assert.AreEqual(2, data[2]); // Image type: uncompressed true-color
            for (var i = 3; i < 12; i++)
            {
                Assert.AreEqual(0, data[i]); // Color map spec + origin
            }
            Assert.AreEqual(2, data[12]); // Width LSB
            Assert.AreEqual(0, data[13]); // Width MSB
            Assert.AreEqual(2, data[14]); // Height LSB
            Assert.AreEqual(0, data[15]); // Height MSB
            Assert.AreEqual(32, data[16]); // Bits per pixel
            Assert.AreEqual(0x28, data[17]); // 8 alpha bits + top-left origin (bit 5)

            // Pixels are BGRA, rows in array order.
            var expected = new byte[]
            {
                3, 2, 1, 4,
                7, 6, 5, 8,
                11, 10, 9, 12,
                15, 14, 13, 16,
            };
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], data[18 + i], $"Pixel byte {i}");
            }
        }

        /// <summary>
        /// Test WriteTga bottom-left origin descriptor.
        /// </summary>
        [Test]
        public void WriteTgaBottomLeftOrigin()
        {
            var pixels = new Color32[1];
            var path = Path.Combine(tempFolder, "test-bottom.tga");
            AstcUtility.WriteTga(pixels, 1, 1, false, path);

            var data = File.ReadAllBytes(path);
            Assert.AreEqual(0x08, data[17]); // 8 alpha bits, origin bit cleared
        }

        /// <summary>
        /// Test WriteTga little endian dimensions.
        /// </summary>
        [Test]
        public void WriteTgaLittleEndianDimensions()
        {
            var width = 300; // 0x012C
            var height = 260; // 0x0104
            var pixels = new Color32[width * height];
            var path = Path.Combine(tempFolder, "test-le.tga");
            AstcUtility.WriteTga(pixels, width, height, true, path);

            var data = File.ReadAllBytes(path);
            Assert.AreEqual(0x2C, data[12]);
            Assert.AreEqual(0x01, data[13]);
            Assert.AreEqual(0x04, data[14]);
            Assert.AreEqual(0x01, data[15]);
        }

        /// <summary>
        /// Test WriteTga with mismatched pixel count.
        /// </summary>
        [Test]
        public void WriteTgaPixelCountMismatch()
        {
            var path = Path.Combine(tempFolder, "test-mismatch.tga");
            Assert.Throws<ArgumentException>(() => AstcUtility.WriteTga(new Color32[3], 2, 2, true, path));
        }

        /// <summary>
        /// Test StripAstcHeader with a valid header.
        /// </summary>
        [Test]
        public void StripAstcHeaderValid()
        {
            var payload = new byte[2 * 16];
            for (var i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)i;
            }
            var file = CreateAstcFile(8, 4, 4, 4, payload);

            var result = AstcUtility.StripAstcHeader(file, 8, 4, 4, 4);
            Assert.AreEqual(payload, result);
        }

        /// <summary>
        /// Test StripAstcHeader with an invalid magic number.
        /// </summary>
        [Test]
        public void StripAstcHeaderInvalidMagic()
        {
            var file = CreateAstcFile(4, 4, 4, 4, new byte[16]);
            file[0] = 0x00;
            var exception = Assert.Throws<InvalidDataException>(() => AstcUtility.StripAstcHeader(file, 4, 4, 4, 4));
            StringAssert.Contains("magic", exception.Message);
        }

        /// <summary>
        /// Test StripAstcHeader with unexpected image dimensions.
        /// </summary>
        [Test]
        public void StripAstcHeaderDimensionMismatch()
        {
            var file = CreateAstcFile(8, 4, 4, 4, new byte[2 * 16]);
            var exception = Assert.Throws<InvalidDataException>(() => AstcUtility.StripAstcHeader(file, 4, 4, 4, 4));
            StringAssert.Contains("8x4", exception.Message);
        }

        /// <summary>
        /// Test StripAstcHeader with unexpected block dimensions.
        /// </summary>
        [Test]
        public void StripAstcHeaderBlockMismatch()
        {
            var file = CreateAstcFile(8, 8, 8, 8, new byte[16]);
            var exception = Assert.Throws<InvalidDataException>(() => AstcUtility.StripAstcHeader(file, 8, 8, 4, 4));
            StringAssert.Contains("8x8", exception.Message);
        }

        /// <summary>
        /// Test StripAstcHeader with truncated data.
        /// </summary>
        [Test]
        public void StripAstcHeaderTooShort()
        {
            Assert.Throws<InvalidDataException>(() => AstcUtility.StripAstcHeader(new byte[8], 4, 4, 4, 4));
        }

        private static byte[] CreateAstcFile(int width, int height, int blockX, int blockY, byte[] payload)
        {
            var file = new byte[16 + payload.Length];
            file[0] = 0x13;
            file[1] = 0xAB;
            file[2] = 0xA1;
            file[3] = 0x5C;
            file[4] = (byte)blockX;
            file[5] = (byte)blockY;
            file[6] = 1;
            WriteUInt24(file, 7, width);
            WriteUInt24(file, 10, height);
            WriteUInt24(file, 13, 1);
            Buffer.BlockCopy(payload, 0, file, 16, payload.Length);
            return file;
        }

        private static void WriteUInt24(byte[] data, int offset, int value)
        {
            data[offset] = (byte)(value & 0xFF);
            data[offset + 1] = (byte)((value >> 8) & 0xFF);
            data[offset + 2] = (byte)((value >> 16) & 0xFF);
        }
    }
}
