// <copyright file="AstcUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Pure logic utilities for ASTC compression with the astcenc CLI.
    /// </summary>
    internal static class AstcUtility
    {
        /// <summary>
        /// Size of a compressed ASTC block in bytes. Constant for all block dimensions.
        /// </summary>
        internal const int BlockBytes = 16;

        /// <summary>
        /// Size of the .astc file header in bytes.
        /// </summary>
        internal const int AstcHeaderBytes = 16;

        /// <summary>
        /// Magic number at the beginning of a .astc file (little endian).
        /// </summary>
        private static readonly byte[] AstcMagic = { 0x13, 0xAB, 0xA1, 0x5C };

        /// <summary>
        /// Gets the ASTC block dimensions for a texture format.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="blockX">Block width in pixels.</param>
        /// <param name="blockY">Block height in pixels.</param>
        /// <returns>true when the format is a supported LDR ASTC format.</returns>
        internal static bool TryGetBlockSize(TextureFormat format, out int blockX, out int blockY)
        {
            switch (format)
            {
                case TextureFormat.ASTC_4x4:
                    blockX = 4;
                    blockY = 4;
                    return true;
                case TextureFormat.ASTC_5x5:
                    blockX = 5;
                    blockY = 5;
                    return true;
                case TextureFormat.ASTC_6x6:
                    blockX = 6;
                    blockY = 6;
                    return true;
                case TextureFormat.ASTC_8x8:
                    blockX = 8;
                    blockY = 8;
                    return true;
                case TextureFormat.ASTC_10x10:
                    blockX = 10;
                    blockY = 10;
                    return true;
                case TextureFormat.ASTC_12x12:
                    blockX = 12;
                    blockY = 12;
                    return true;
                default:
                    blockX = 0;
                    blockY = 0;
                    return false;
            }
        }

        /// <summary>
        /// Gets the block size string for astcenc command line (e.g. "6x6").
        /// </summary>
        /// <param name="format">ASTC texture format.</param>
        /// <returns>Block size string.</returns>
        /// <exception cref="ArgumentException">Thrown when the format is not a supported ASTC format.</exception>
        internal static string GetBlockSizeString(TextureFormat format)
        {
            if (!TryGetBlockSize(format, out var blockX, out var blockY))
            {
                throw new ArgumentException($"Not a supported ASTC format: {format}", nameof(format));
            }
            return $"{blockX}x{blockY}";
        }

        /// <summary>
        /// Gets the compressed data size of a single mip level.
        /// </summary>
        /// <param name="width">Mip width in pixels.</param>
        /// <param name="height">Mip height in pixels.</param>
        /// <param name="blockX">Block width in pixels.</param>
        /// <param name="blockY">Block height in pixels.</param>
        /// <returns>Data size in bytes: ceil(width / blockX) * ceil(height / blockY) * 16.</returns>
        internal static int GetMipDataSize(int width, int height, int blockX, int blockY)
        {
            var blocksX = (width + blockX - 1) / blockX;
            var blocksY = (height + blockY - 1) / blockY;
            return blocksX * blocksY * BlockBytes;
        }

        /// <summary>
        /// Validates the 16 bytes header of a .astc file and returns the raw block data without the header.
        /// </summary>
        /// <param name="astcFileData">Whole content of a .astc file.</param>
        /// <param name="expectedWidth">Expected image width in pixels.</param>
        /// <param name="expectedHeight">Expected image height in pixels.</param>
        /// <param name="blockX">Expected block width in pixels.</param>
        /// <param name="blockY">Expected block height in pixels.</param>
        /// <returns>Raw ASTC block data.</returns>
        /// <exception cref="InvalidDataException">Thrown when the header does not match the expectation.</exception>
        internal static byte[] StripAstcHeader(byte[] astcFileData, int expectedWidth, int expectedHeight, int blockX, int blockY)
        {
            if (astcFileData == null)
            {
                throw new ArgumentNullException(nameof(astcFileData));
            }
            if (astcFileData.Length < AstcHeaderBytes)
            {
                throw new InvalidDataException($"ASTC file data is too short: {astcFileData.Length} bytes");
            }
            for (var i = 0; i < AstcMagic.Length; i++)
            {
                if (astcFileData[i] != AstcMagic[i])
                {
                    throw new InvalidDataException($"Invalid ASTC magic number: {astcFileData[0]:X2} {astcFileData[1]:X2} {astcFileData[2]:X2} {astcFileData[3]:X2}");
                }
            }

            int actualBlockX = astcFileData[4];
            int actualBlockY = astcFileData[5];
            int actualBlockZ = astcFileData[6];
            if (actualBlockX != blockX || actualBlockY != blockY || actualBlockZ != 1)
            {
                throw new InvalidDataException($"Unexpected ASTC block dimensions: {actualBlockX}x{actualBlockY}x{actualBlockZ} (expected {blockX}x{blockY}x1)");
            }

            var actualWidth = ReadUInt24(astcFileData, 7);
            var actualHeight = ReadUInt24(astcFileData, 10);
            var actualDepth = ReadUInt24(astcFileData, 13);
            if (actualWidth != expectedWidth || actualHeight != expectedHeight || actualDepth != 1)
            {
                throw new InvalidDataException($"Unexpected ASTC image dimensions: {actualWidth}x{actualHeight}x{actualDepth} (expected {expectedWidth}x{expectedHeight}x1)");
            }

            var expectedDataSize = GetMipDataSize(expectedWidth, expectedHeight, blockX, blockY);
            var actualDataSize = astcFileData.Length - AstcHeaderBytes;
            if (actualDataSize != expectedDataSize)
            {
                throw new InvalidDataException($"Unexpected ASTC data size: {actualDataSize} bytes (expected {expectedDataSize} bytes)");
            }

            var result = new byte[actualDataSize];
            Buffer.BlockCopy(astcFileData, AstcHeaderBytes, result, 0, actualDataSize);
            return result;
        }

        /// <summary>
        /// Writes pixels as an uncompressed 32-bit BGRA TGA (type 2) file.
        /// </summary>
        /// <remarks>
        /// Pixel rows are always written in array order (row 0 first); only the image descriptor's origin bit changes.
        /// When <paramref name="topToBottom"/> is true, the origin bit (bit 5) is set to top-left, so readers treat
        /// row 0 of the array as the top row of the image. Because Unity's <c>Texture2D.GetPixels32</c> returns row 0
        /// as the bottom row, passing its result with <paramref name="topToBottom"/> = true produces an image that
        /// readers such as astcenc see vertically flipped. When false, the origin bit is cleared (bottom-left), so
        /// row 0 of the array is treated as the bottom row.
        /// </remarks>
        /// <param name="pixels">Pixels in RGBA order as returned by <c>Texture2D.GetPixels32</c>.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="topToBottom">Whether to declare top-left origin (bit 5 of the image descriptor).</param>
        /// <param name="path">File path to write.</param>
        /// <exception cref="ArgumentException">Thrown when the pixel count does not match width * height.</exception>
        internal static void WriteTga(Color32[] pixels, int width, int height, bool topToBottom, string path)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            if (pixels.Length != width * height)
            {
                throw new ArgumentException($"Pixel count {pixels.Length} does not match {width}x{height}", nameof(pixels));
            }

            var rgba = new byte[pixels.Length * 4];
            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                var o = i * 4;
                rgba[o] = pixel.r;
                rgba[o + 1] = pixel.g;
                rgba[o + 2] = pixel.b;
                rgba[o + 3] = pixel.a;
            }

            var native = new NativeArray<byte>(rgba, Allocator.Temp);
            try
            {
                WriteTga(native, 0, native.Length, width, height, topToBottom, path);
            }
            finally
            {
                native.Dispose();
            }
        }

        /// <summary>
        /// Writes a slice of raw RGBA bytes as an uncompressed 32-bit BGRA TGA (type 2) file, converting
        /// directly from the RGBA byte layout without materializing an intermediate <see cref="Color32"/> array.
        /// </summary>
        /// <remarks>
        /// See the <see cref="WriteTga(Color32[], int, int, bool, string)"/> overload's remarks for the row-order
        /// / origin-bit semantics; they apply identically here.
        /// </remarks>
        /// <param name="rgba">Buffer containing RGBA texel bytes (e.g. <c>Texture2D.GetRawTextureData&lt;byte&gt;()</c>).</param>
        /// <param name="offset">Offset in <paramref name="rgba"/> of the first byte of this image's data.</param>
        /// <param name="length">Number of bytes belonging to this image, starting at <paramref name="offset"/>. Must equal width * height * 4.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="topToBottom">Whether to declare top-left origin (bit 5 of the image descriptor).</param>
        /// <param name="path">File path to write.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="length"/> does not match width * height * 4.</exception>
        internal static void WriteTga(NativeArray<byte> rgba, int offset, int length, int width, int height, bool topToBottom, string path)
        {
            if (length != width * height * 4)
            {
                throw new ArgumentException($"Data length {length} does not match {width}x{height}x4", nameof(length));
            }

            const int headerBytes = 18;
            var data = new byte[headerBytes + length];
            data[2] = 2; // Uncompressed true-color image.
            data[12] = (byte)(width & 0xFF);
            data[13] = (byte)((width >> 8) & 0xFF);
            data[14] = (byte)(height & 0xFF);
            data[15] = (byte)((height >> 8) & 0xFF);
            data[16] = 32; // Bits per pixel.
            data[17] = (byte)(8 | (topToBottom ? 0x20 : 0)); // 8 bits of alpha + origin bit.

            for (var i = 0; i < length; i += 4)
            {
                var src = offset + i;
                var dst = headerBytes + i;
                data[dst] = rgba[src + 2]; // B
                data[dst + 1] = rgba[src + 1]; // G
                data[dst + 2] = rgba[src]; // R
                data[dst + 3] = rgba[src + 3]; // A
            }
            File.WriteAllBytes(path, data);
        }

        private static int ReadUInt24(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);
        }
    }
}
