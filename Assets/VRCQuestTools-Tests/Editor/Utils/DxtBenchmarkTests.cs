// <copyright file="DxtBenchmarkTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Manual benchmark measuring how long Unity's built-in DXT5 compression
    /// (<see cref="EditorUtility.CompressTexture"/> with <see cref="TextureCompressionQuality.Best"/>, the same
    /// call <see cref="UnityTextureCompressor.CompressTexture"/> makes for the PC/Standalone fallback format)
    /// takes on the main thread. Used to decide whether the same out-of-process/async pattern used for ASTC
    /// (see <see cref="AstcencTextureCompressor"/>, justified by <see cref="AstcencBenchmarkTests"/>) is worth
    /// building for DXT too. This is not part of the normal (CI) test run: it is marked
    /// <see cref="ExplicitAttribute"/> and must be invoked directly from the Test Runner.
    /// </summary>
    /// <remarks>
    /// Measured result (Windows, procedurally generated non-flat textures, DXT5/DXT1, median of 5 after warmup
    /// with the compressed bytes read back inside the timed region): 512px 0.8-1.3 ms, 1024px 2.6-11 ms, 2048px
    /// 11.5-14.1 ms, 4096px 33.8-43.2 ms. Unlike ASTC's "-thorough"-equivalent Unity encoding (which astcenc's
    /// multi-core, out-of-process path was built to speed up and to keep off the main thread for NDMF preview),
    /// Unity's built-in DXT encoder is already sub-frame even at 4096px -- there is no multi-second stall to
    /// hide and no editor freeze to fix. <see cref="DxtCliBenchmarkTests"/> then confirmed the other side of the
    /// comparison: actual DXT/BC encoder CLIs (texconv, Compressonator) are 10-75x *slower* than Unity here even
    /// though both already run multi-threaded, and their per-invocation process overhead alone exceeds Unity's
    /// entire compression time. Conclusion: an external CLI DXT/BC encoder run out-of-process (mirroring
    /// <see cref="AstcencCli"/>) is not worth building;
    /// <see cref="UnityTextureCompressor.CompressTexture"/> remains synchronous for DXT/BC.
    /// </remarks>
    [Explicit("Benchmark")]
    public class DxtBenchmarkTests
    {
        private static readonly int[] Sizes = { 512, 1024, 2048, 4096 };

        /// <summary>
        /// Measures <see cref="EditorUtility.CompressTexture"/> wall-clock time for DXT5 across texture sizes.
        /// Each measurement is logged as a CSV-style row (size,format,timeMs), followed by a summary table.
        /// </summary>
        [Test]
        public void SpeedMatrix()
        {
            var rows = new List<Row>();
            Debug.Log("size,format,timeMs");

            foreach (var size in Sizes)
            {
                MeasureUnity(size, TextureFormat.DXT5, rows);
                MeasureUnity(size, TextureFormat.DXT1, rows);
            }

            LogSummary(rows);
        }

        private static void MeasureUnity(int size, TextureFormat format, List<Row> rows)
        {
            // One warmup plus a median over several runs: a single cold measurement is dominated by one-time
            // initialization, which showed up as 512px appearing *slower* than 1024px when this was measured
            // once per size.
            const int warmups = 1;
            const int iterations = 5;
            var samples = new List<double>(iterations);

            for (var i = 0; i < warmups + iterations; i++)
            {
                var candidate = CreateNaturalisticTexture(size);
                var sw = Stopwatch.StartNew();
                EditorUtility.CompressTexture(candidate, format, TextureCompressionQuality.Best);

                // Inside the timed region on purpose: reading the compressed bytes back forces them to actually
                // be materialized, so a deferred or lazily-evaluated compression cannot make this look faster
                // than it is. Costs a memcpy of the (already small) compressed data on top of the encode.
                var bytes = candidate.GetRawTextureData();
                sw.Stop();

                // Guards the measurement itself: if the requested format were silently not applied, or no data
                // produced, the timing would be of something other than the compression this benchmark claims.
                Assert.AreEqual(format, candidate.format, $"Texture was not compressed to {format} at size={size}.");
                Assert.Greater(bytes.Length, 0, $"Compressed texture has no data at size={size}.");

                if (i >= warmups)
                {
                    samples.Add(sw.Elapsed.TotalMilliseconds);
                }

                UnityEngine.Object.DestroyImmediate(candidate);
            }

            samples.Sort();
            var median = samples[samples.Count / 2];

            rows.Add(new Row(size, format.ToString(), median));
            Debug.Log($"{size},{format},{median:F1}");
        }

        private static void LogSummary(List<Row> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Summary (median ms over 5 runs, after warmup, incl. forced readback) ===");
            foreach (var row in rows)
            {
                sb.AppendLine($"  size={row.Size,5} format={row.Format,-6} time={row.TimeMs,9:F1}ms");
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Generates a fully opaque procedural texture mixing several sine/cosine frequencies per channel
        /// (same generator as <see cref="AstcencBenchmarkTests.CreateNaturalisticTexture(int, bool)"/>) so it
        /// has both smooth gradients and higher-frequency detail instead of a flat placeholder image, which
        /// would let the encoder take shortcuts unrepresentative of real content.
        /// </summary>
        private static Texture2D CreateNaturalisticTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
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
            tex.Apply(false, false);
            return tex;
        }

        private readonly struct Row
        {
            internal Row(int size, string format, double timeMs)
            {
                Size = size;
                Format = format;
                TimeMs = timeMs;
            }

            internal int Size { get; }

            internal string Format { get; }

            internal double TimeMs { get; }
        }
    }
}
