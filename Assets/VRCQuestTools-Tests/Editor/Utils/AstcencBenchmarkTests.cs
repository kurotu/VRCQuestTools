// <copyright file="AstcencBenchmarkTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Manual benchmark comparing astcenc quality presets against Unity's built-in ASTC compressor
    /// (<see cref="EditorUtility.CompressTexture"/> with <see cref="TextureCompressionQuality.Best"/>) for
    /// compression speed and resulting image quality. This is not part of the normal (CI) test run: it is
    /// marked <see cref="ExplicitAttribute"/> and must be invoked directly from the Test Runner. Used to
    /// decide <see cref="TextureCompressorProvider.DefaultPreset"/>.
    ///
    /// Content note: this benchmark uses procedurally generated textures rather than the repository's
    /// albedo_1024px_png.png / alpha_test.png fixtures. Those fixtures are near-solid-color placeholder
    /// images (a small text label on an otherwise flat background), so both encoders reproduce them almost
    /// losslessly regardless of preset (diff ~0 for every combination) - useless for comparing quality.
    /// The procedural textures below mix low- and high-frequency detail across all channels (and, for the
    /// alpha test, a smooth alpha gradient) to actually exercise the encoders' search.
    /// </summary>
    [Explicit("Benchmark")]
    public class AstcencBenchmarkTests
    {
        private static readonly int[] Sizes = { 512, 1024, 2048 };
        private static readonly TextureFormat[] BlockFormats = { TextureFormat.ASTC_4x4, TextureFormat.ASTC_6x6 };
        private static readonly string[] Presets = { "-fast", "-medium", "-thorough", "-exhaustive" };

        /// <summary>
        /// Resets the global log-assert flag after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;
        }

        /// <summary>
        /// Speed/quality matrix over size (512/1024/2048) x ASTC block size (4x4/6x6) x encoder (Unity's Best
        /// quality vs astcenc's -fast/-medium/-thorough/-exhaustive presets). Each measurement is logged as a
        /// CSV-style row (size,block,encoder,preset,timeMs,diff), followed by a summary table.
        /// </summary>
        [Test]
        public void SpeedAndQualityMatrix()
        {
            var astcencPath = AstcencBinaryLocator.GetAstcencPath();
            if (astcencPath == null)
            {
                Assert.Ignore("No usable astcenc executable is available in this environment.");
            }
            var version = AstcencCli.GetVersion(astcencPath);

            var rows = new List<Row>();
            Debug.Log("size,block,encoder,preset,timeMs,diff");

            foreach (var size in Sizes)
            {
                // Reference is the uncompressed mip-0 image at this size; used as the diff baseline for
                // every encoder/preset measured at this size. No mip chain: only mip 0 is ever sampled back
                // by TestUtils.DecodeToRGBA32, and generating mip 0 does not depend on whether further mips are
                // requested, so this is directly comparable to every candidate's mip 0 (candidates are
                // regenerated from the same deterministic procedural function for each measurement).
                var reference = CreateNaturalisticTexture(size, false);
                try
                {
                    foreach (var format in BlockFormats)
                    {
                        AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY);
                        var block = $"{blockX}x{blockY}";

                        MeasureUnity(reference, size, format, block, rows);

                        foreach (var preset in Presets)
                        {
                            MeasureAstcenc(reference, size, format, block, astcencPath, version, preset, rows);
                        }
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(reference);
                }
            }

            LogSummary(rows);
        }

        /// <summary>
        /// Compresses a synthetic texture whose alpha channel oscillates fully (0..255) with an 8px period
        /// (so alpha varies across the full range within every single 4x4 block) using ASTC_4x4 at the
        /// "-thorough" preset, once with astcenc's default error metric and once with per-texel error
        /// weighting scaled by the alpha channel (astcenc's "-a 0" option: weight each texel by its own
        /// alpha, radius 0). A gentler, image-wide gradient was tried first and showed no measurable
        /// difference: ASTC blocks are always exactly 128 bits and cannot borrow bits from neighboring
        /// blocks, so "-a" can only trade RGB precision for texels *within the same block* - it needs alpha
        /// to actually vary across a block's own texels to have any effect, which a slow gradient (far below
        /// one step per block) does not provide. Reports the overall diff plus diff restricted to
        /// high-alpha (&gt;=192, "visible") and low-alpha (&lt;=63, "invisible") texels, since "-a" is expected
        /// to trade quality in invisible texels for quality in visible ones rather than change the
        /// whole-image total. Both runs use "-cs" (sRGB), matching how color textures are compressed
        /// elsewhere in this codebase.
        /// </summary>
        [Test]
        public void AlphaWeighting_ThoroughPreset_QualityComparison()
        {
            var astcencPath = AstcencBinaryLocator.GetAstcencPath();
            if (astcencPath == null)
            {
                Assert.Ignore("No usable astcenc executable is available in this environment.");
            }

            const int size = 512;
            Texture2D reference = null;
            Texture2D noWeight = null;
            Texture2D withWeight = null;
            Texture2D decodedNoWeight = null;
            Texture2D decodedWithWeight = null;
            try
            {
                reference = CreateAlphaOscillatingTexture(size);
                var pixels = reference.GetPixels32();

                var swNoWeight = Stopwatch.StartNew();
                noWeight = CompressWithRawFlags(astcencPath, pixels, size, size, "4x4", "-thorough", srgb: true);
                swNoWeight.Stop();

                var swWithWeight = Stopwatch.StartNew();
                withWeight = CompressWithRawFlags(astcencPath, pixels, size, size, "4x4", "-thorough -a 0", srgb: true);
                swWithWeight.Stop();

                decodedNoWeight = TestUtils.DecodeToRGBA32(noWeight, size, size);
                decodedWithWeight = TestUtils.DecodeToRGBA32(withWeight, size, size);

                var diffNoWeight = TestUtils.Difference(reference, decodedNoWeight);
                var diffWithWeight = TestUtils.Difference(reference, decodedWithWeight);
                var highAlphaDiffNoWeight = MaskedDifference(reference, decodedNoWeight, highAlpha: true);
                var highAlphaDiffWithWeight = MaskedDifference(reference, decodedWithWeight, highAlpha: true);
                var lowAlphaDiffNoWeight = MaskedDifference(reference, decodedNoWeight, highAlpha: false);
                var lowAlphaDiffWithWeight = MaskedDifference(reference, decodedWithWeight, highAlpha: false);

                Debug.Log("size,block,encoder,preset,timeMs,diff");
                Debug.Log($"{size}x{size},4x4,astcenc,thorough,{swNoWeight.Elapsed.TotalMilliseconds:F1},{diffNoWeight:F7}");
                Debug.Log($"{size}x{size},4x4,astcenc,thorough+a0,{swWithWeight.Elapsed.TotalMilliseconds:F1},{diffWithWeight:F7}");
                Debug.Log($"=== Alpha weighting comparison ({size}x{size} synthetic alpha-oscillating texture, ASTC_4x4, -thorough) ===\n" +
                    $"  no weighting : overall diff={diffNoWeight:F7}  high-alpha (visible) diff={highAlphaDiffNoWeight:F7}  low-alpha (invisible) diff={lowAlphaDiffNoWeight:F7}\n" +
                    $"  -a 0 weighted: overall diff={diffWithWeight:F7}  high-alpha (visible) diff={highAlphaDiffWithWeight:F7}  low-alpha (invisible) diff={lowAlphaDiffWithWeight:F7}");
            }
            finally
            {
                if (decodedNoWeight != null)
                {
                    UnityEngine.Object.DestroyImmediate(decodedNoWeight);
                }
                if (decodedWithWeight != null)
                {
                    UnityEngine.Object.DestroyImmediate(decodedWithWeight);
                }
                if (noWeight != null)
                {
                    UnityEngine.Object.DestroyImmediate(noWeight);
                }
                if (withWeight != null)
                {
                    UnityEngine.Object.DestroyImmediate(withWeight);
                }
                if (reference != null)
                {
                    UnityEngine.Object.DestroyImmediate(reference);
                }
            }
        }

        private static void MeasureUnity(Texture2D reference, int size, TextureFormat format, string block, List<Row> rows)
        {
            var candidate = CreateNaturalisticTexture(size, true);
            var sw = Stopwatch.StartNew();
            EditorUtility.CompressTexture(candidate, format, TextureCompressionQuality.Best);
            sw.Stop();

            var decoded = TestUtils.DecodeToRGBA32(candidate, size, size);
            var diff = TestUtils.Difference(reference, decoded);
            LogRow(rows, size, block, "unity", "best", sw.Elapsed.TotalMilliseconds, diff);

            UnityEngine.Object.DestroyImmediate(decoded);
            UnityEngine.Object.DestroyImmediate(candidate);
        }

        private static void MeasureAstcenc(Texture2D reference, int size, TextureFormat format, string block, string exePath, string version, string preset, List<Row> rows)
        {
            var compressor = new AstcencTextureCompressor(exePath, version, preset);
            var candidate = CreateNaturalisticTexture(size, true);
            Texture2D result = null;
            var sw = Stopwatch.StartNew();
            compressor.CompressTexture(candidate, format, t => result = t).WaitForCompletion();
            sw.Stop();
            Assert.IsNotNull(result, $"astcenc compression returned null for size={size} block={block} preset={preset}");

            var decoded = TestUtils.DecodeToRGBA32(result, size, size);
            var diff = TestUtils.Difference(reference, decoded);
            LogRow(rows, size, block, "astcenc", preset.TrimStart('-'), sw.Elapsed.TotalMilliseconds, diff);

            UnityEngine.Object.DestroyImmediate(decoded);

            // On success AstcencTextureCompressor.CompressTexture already destroyed `candidate` and `result`
            // is a distinct new texture; on a fallback (e.g. astcenc failed) `result` is the same reference
            // as `candidate` and was mutated in place. Either way, only `result` needs destroying here.
            UnityEngine.Object.DestroyImmediate(result);
        }

        private static void LogRow(List<Row> rows, int size, string block, string encoder, string preset, double timeMs, float diff)
        {
            rows.Add(new Row(size, block, encoder, preset, timeMs, diff));
            Debug.Log($"{size},{block},{encoder},{preset},{timeMs:F1},{diff:F5}");
        }

        private static void LogSummary(List<Row> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Summary (time ms, diff, speed vs Unity Best) ===");
            foreach (var size in Sizes)
            {
                foreach (var format in BlockFormats)
                {
                    AstcUtility.TryGetBlockSize(format, out var blockX, out var blockY);
                    var block = $"{blockX}x{blockY}";
                    var groupRows = rows.FindAll(r => r.Size == size && r.Block == block);
                    var unityRow = groupRows.Find(r => r.Encoder == "unity");

                    sb.AppendLine($"-- size={size} block={block} --");
                    sb.AppendLine($"  unity/best       time={unityRow.TimeMs,9:F1}ms diff={unityRow.Diff:F5}");
                    foreach (var row in groupRows)
                    {
                        if (row.Encoder == "unity")
                        {
                            continue;
                        }
                        var speedRatio = row.TimeMs > 0 ? unityRow.TimeMs / row.TimeMs : 0;
                        sb.AppendLine($"  astcenc/{row.Preset,-10} time={row.TimeMs,9:F1}ms diff={row.Diff:F5} speed={speedRatio,6:F2}x vs Unity Best");
                    }
                }
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Generates a fully opaque procedural texture mixing several sine/cosine frequencies per channel, so
        /// it has both smooth gradients and higher-frequency detail (roughly approximating the mix of content
        /// found in a real albedo/detail map) instead of the large flat regions in this repo's PNG fixtures.
        /// Deterministic for a given size, so independently generated instances used as "reference" vs
        /// compression candidates are pixel-identical at mip 0.
        /// </summary>
        private static Texture2D CreateNaturalisticTexture(int size, bool mipChain)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, mipChain, false);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var fx = x / (float)size;
                    var fy = y / (float)size;
                    var r = 127f + (80f * Mathf.Sin(x * 0.13f)) + (48f * Mathf.Sin((y * 0.07f) + (x * 0.02f)));
                    var g = 127f + (80f * Mathf.Cos(y * 0.11f)) + (48f * Mathf.Sin((x + y) * 0.05f));
                    var b = 127f + (100f * fx * fy) + (40f * Mathf.Sin((x * 0.29f) + (y * 0.19f)));
                    pixels[(y * size) + x] = new Color32(
                        (byte)Mathf.Clamp(r, 0f, 255f),
                        (byte)Mathf.Clamp(g, 0f, 255f),
                        (byte)Mathf.Clamp(b, 0f, 255f),
                        255);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(mipChain, false);
            return tex;
        }

        /// <summary>
        /// Generates a procedural texture with RGB detail (same style as <see cref="CreateNaturalisticTexture"/>)
        /// and an alpha channel that oscillates through the full 0..255 range with an 8px period along x, so
        /// alpha varies across the full range within every single ASTC 4x4 block (half a period = 4px). This
        /// is the situation astcenc's "-a" alpha-weighted error metric is designed for: RGB detail under
        /// near-zero alpha is invisible in the final composite, so within a block that straddles a
        /// high-alpha/low-alpha boundary, shifting error budget toward the higher-alpha texels should
        /// improve quality there at the expense of the near-zero-alpha texels.
        /// </summary>
        private static Texture2D CreateAlphaOscillatingTexture(int size)
        {
            const int period = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var r = 127f + (100f * Mathf.Sin(x * 0.21f));
                    var g = 127f + (100f * Mathf.Cos(y * 0.17f));
                    var b = 127f + (100f * Mathf.Sin((x + y) * 0.09f));
                    var phase = x % period;
                    var half = period / 2;
                    var triangle = phase < half ? (phase / (float)half) : (1f - ((phase - half) / (float)half));
                    var a = 255f * triangle;
                    pixels[(y * size) + x] = new Color32(
                        (byte)Mathf.Clamp(r, 0f, 255f),
                        (byte)Mathf.Clamp(g, 0f, 255f),
                        (byte)Mathf.Clamp(b, 0f, 255f),
                        (byte)Mathf.Clamp(a, 0f, 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>
        /// Like TestUtils.Difference, but restricted to texels whose reference (tex1) alpha is high
        /// (highAlpha=true, alpha &gt;= 192, i.e. "visible") or low (highAlpha=false, alpha &lt;= 63, i.e.
        /// "invisible"), matching the alpha-oscillation pattern produced by
        /// <see cref="CreateAlphaOscillatingTexture"/>.
        /// </summary>
        private static float MaskedDifference(Texture2D tex1, Texture2D tex2, bool highAlpha)
        {
            var pixels1 = TestUtils.CopyTextureAsReadable(tex1).GetPixels32();
            var pixels2 = TestUtils.CopyTextureAsReadable(tex2).GetPixels32();
            Assert.AreEqual(pixels1.Length, pixels2.Length);

            long dsum = 0;
            long count = 0;
            for (var i = 0; i < pixels1.Length; i++)
            {
                var c1 = pixels1[i];
                var isHigh = c1.a >= 192;
                var isLow = c1.a <= 63;
                if (highAlpha && !isHigh)
                {
                    continue;
                }
                if (!highAlpha && !isLow)
                {
                    continue;
                }

                var c2 = pixels2[i];
                var r = c1.r - c2.r;
                var g = c1.g - c2.g;
                var b = c1.b - c2.b;
                var a = c1.a - c2.a;
                dsum += (r * r) + (g * g) + (b * b) + (a * a);
                count++;
            }

            return dsum / (float)(255L * 255L * 4L * Math.Max(1, count));
        }

        /// <summary>
        /// Runs astcenc directly (bypassing AstcencTextureCompressor) with an arbitrary preset/flags string,
        /// so that ad-hoc flags like "-a 0" (alpha-weighted error metric) can be exercised for the alpha
        /// benchmark without adding a permanent option to the production compressor.
        /// </summary>
        private static Texture2D CompressWithRawFlags(string exePath, Color32[] pixels, int width, int height, string blockSize, string presetAndFlags, bool srgb)
        {
            AstcUtility.TryGetBlockSize(TextureFormat.ASTC_4x4, out var blockX, out var blockY);

            var tempDir = Path.GetFullPath(AstcencCli.TempDirectory);
            Directory.CreateDirectory(tempDir);
            var id = Guid.NewGuid().ToString("N");
            var tgaPath = Path.Combine(tempDir, $"{id}.tga");
            var astcPath = Path.Combine(tempDir, $"{id}.astc");
            try
            {
                // TopToBottomOrigin = true, matching AstcencTextureCompressor (see its remarks for why).
                AstcUtility.WriteTga(pixels, width, height, true, tgaPath);

                var jobs = Math.Max(1, SystemInfo.processorCount);
                var runResult = AstcencCli.RunCompress(exePath, tgaPath, astcPath, blockSize, presetAndFlags, srgb, jobs, 5 * 60 * 1000);
                Assert.IsTrue(runResult.Success, $"astcenc failed (preset=\"{presetAndFlags}\"): exit={runResult.ExitCode} stderr={runResult.StdErr}");

                var fileData = File.ReadAllBytes(astcPath);
                var blockData = AstcUtility.StripAstcHeader(fileData, width, height, blockX, blockY);

                var texture = new Texture2D(width, height, TextureFormat.ASTC_4x4, false, !srgb);
                texture.LoadRawTextureData(blockData);
                texture.Apply(false, false);
                return texture;
            }
            finally
            {
                AstcencCli.DeleteFileSilently(tgaPath);
                AstcencCli.DeleteFileSilently(astcPath);
            }
        }

        private readonly struct Row
        {
            internal Row(int size, string block, string encoder, string preset, double timeMs, float diff)
            {
                Size = size;
                Block = block;
                Encoder = encoder;
                Preset = preset;
                TimeMs = timeMs;
                Diff = diff;
            }

            internal int Size { get; }

            internal string Block { get; }

            internal string Encoder { get; }

            internal string Preset { get; }

            internal double TimeMs { get; }

            internal float Diff { get; }
        }
    }
}
