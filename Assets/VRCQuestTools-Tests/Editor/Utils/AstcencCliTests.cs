// <copyright file="AstcencCliTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Test AstcencCli and AstcencBinaryLocator.
    /// </summary>
    public class AstcencCliTests
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
        /// Test that a bundled astcenc binary is resolved.
        /// </summary>
        [Test]
        public void GetAstcencPathReturnsUsableBinary()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
            var path = AstcencBinaryLocator.GetAstcencPath();
            Assert.IsNotNull(path);
            Assert.IsTrue(File.Exists(path));
            StringAssert.Contains("astcenc", Path.GetFileName(path));
#elif UNITY_EDITOR_OSX
            Assert.Ignore("astcenc availability depends on the environment on macOS.");
#else
            Assert.Fail("Unsupported editor platform");
#endif
        }

        /// <summary>
        /// Test GetVersion against the bundled binary.
        /// </summary>
        [Test]
        public void GetVersionReturnsBundledVersion()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
            var path = AstcencBinaryLocator.GetAstcencPath();
            Assert.IsNotNull(path);
            Assert.AreEqual("5.3.0", AstcencCli.GetVersion(path));
#elif UNITY_EDITOR_OSX
            Assert.Ignore("astcenc availability depends on the environment on macOS.");
#else
            Assert.Fail("Unsupported editor platform");
#endif
        }

        /// <summary>
        /// Test SelfTest against the bundled binary.
        /// </summary>
        [Test]
        public void SelfTestSucceeds()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
            var path = AstcencBinaryLocator.GetAstcencPath();
            Assert.IsNotNull(path);
            Assert.IsTrue(AstcencCli.SelfTest(path));
#elif UNITY_EDITOR_OSX
            Assert.Ignore("astcenc availability depends on the environment on macOS.");
#else
            Assert.Fail("Unsupported editor platform");
#endif
        }

        /// <summary>
        /// Test compressing a 16x16 gradient with 4x4 blocks.
        /// </summary>
        [Test]
        public void RunCompressGradient4x4()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
            RunCompressGradient("4x4", 4, 4);
#elif UNITY_EDITOR_OSX
            Assert.Ignore("astcenc availability depends on the environment on macOS.");
#else
            Assert.Fail("Unsupported editor platform");
#endif
        }

        /// <summary>
        /// Test compressing a 16x16 gradient with 8x8 blocks.
        /// </summary>
        [Test]
        public void RunCompressGradient8x8()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
            RunCompressGradient("8x8", 8, 8);
#elif UNITY_EDITOR_OSX
            Assert.Ignore("astcenc availability depends on the environment on macOS.");
#else
            Assert.Fail("Unsupported editor platform");
#endif
        }

        /// <summary>
        /// Test that RunCompress reports failure for a missing input file.
        /// </summary>
        [Test]
        public void RunCompressFailsForMissingInput()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
            var path = AstcencBinaryLocator.GetAstcencPath();
            Assert.IsNotNull(path);
            var inputPath = Path.Combine(tempFolder, "missing.tga");
            var outputPath = Path.Combine(tempFolder, "missing.astc");
            var result = AstcencCli.RunCompress(path, inputPath, outputPath, "4x4", "-medium", true, 1, 60 * 1000);
            Assert.IsFalse(result.Success);
            Assert.IsFalse(result.TimedOut);
            Assert.AreNotEqual(0, result.ExitCode);
            Assert.IsNotEmpty(result.StdErr);
#elif UNITY_EDITOR_OSX
            Assert.Ignore("astcenc availability depends on the environment on macOS.");
#else
            Assert.Fail("Unsupported editor platform");
#endif
        }

        private void RunCompressGradient(string blockSize, int blockX, int blockY)
        {
            const int size = 16;
            var path = AstcencBinaryLocator.GetAstcencPath();
            Assert.IsNotNull(path);

            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    pixels[(y * size) + x] = new Color32((byte)(x * 16), (byte)(y * 16), 128, 255);
                }
            }
            var inputPath = Path.Combine(tempFolder, "gradient.tga");
            var outputPath = Path.Combine(tempFolder, $"gradient-{blockSize}.astc");
            AstcUtility.WriteTga(pixels, size, size, true, inputPath);

            var result = AstcencCli.RunCompress(path, inputPath, outputPath, blockSize, "-medium", true, 2, 60 * 1000);
            Assert.IsTrue(result.Success, $"astcenc failed: exit={result.ExitCode} stderr={result.StdErr}");
            Assert.IsFalse(result.TimedOut);
            Assert.AreEqual(0, result.ExitCode);

            var expectedDataSize = AstcUtility.GetMipDataSize(size, size, blockX, blockY);
            var fileData = File.ReadAllBytes(outputPath);
            Assert.AreEqual(AstcUtility.AstcHeaderBytes + expectedDataSize, fileData.Length);

            var blockData = AstcUtility.StripAstcHeader(fileData, size, size, blockX, blockY);
            Assert.AreEqual(expectedDataSize, blockData.Length);
        }
    }
}
