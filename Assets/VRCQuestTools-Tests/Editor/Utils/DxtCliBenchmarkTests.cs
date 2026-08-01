// <copyright file="DxtCliBenchmarkTests.cs" company="kurotu">
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
using Debug = UnityEngine.Debug;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Manual benchmark comparing Unity's built-in DXT compression against out-of-process DXT/BC encoder CLIs
    /// (Microsoft DirectXTex's texconv and AMD's Compressonator), mirroring the way
    /// <see cref="AstcencTextureCompressor"/> drives astcenc. Complements <see cref="DxtBenchmarkTests"/>, which
    /// only measures Unity's side: this one answers whether a CLI encoder could actually beat it, by measuring
    /// the CLI pipeline's own fixed cost (TGA write, process spawn, output read) alongside the encode itself.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Neither CLI is bundled with this repository. Obtain them once, relative to the Unity project root, before
    /// running:
    /// <c>curl -sL -o Temp/dxtbench/texconv_dxtex/texconv.exe https://github.com/microsoft/DirectXTex/releases/latest/download/texconv.exe</c>
    /// and the <c>compressonatorcli-*-win64.zip</c> from the Compressonator releases page extracted under
    /// <c>Temp/dxtbench/compressonator/</c>. Each encoder is skipped individually when absent, and the whole
    /// class is marked <see cref="ExplicitAttribute"/> so it never runs in CI.
    /// </para>
    /// <para>
    /// Two per-invocation costs matter and are reported separately. <see cref="PipelineBreakdown"/> measures one
    /// mip level end to end; <see cref="FixedInvocationOverhead"/> measures a 4x4 image, where the encode work is
    /// negligible, so the result is the floor cost of any CLI approach regardless of which encoder is chosen.
    /// That floor is what must be compared against Unity's whole-texture time, because
    /// <see cref="AstcencTextureCompressor"/>'s pattern spawns one process per mip level -- a 1024px texture with
    /// a full mip chain means 11 invocations, so the floor is paid 11 times over.
    /// </para>
    /// <para>
    /// Measured result (Windows, 12 cores, DXT5/BC3), CLI process wall time vs Unity's median-of-5 whole-texture
    /// time from <see cref="DxtBenchmarkTests"/>:
    /// </para>
    /// <list type="table">
    /// <item><description>1024px: unity 2.6 ms / texconv 61 ms / compressonator 523 ms</description></item>
    /// <item><description>2048px: unity 14.1 ms / texconv 108 ms / compressonator 966 ms</description></item>
    /// <item><description>4096px: unity 33.8 ms / texconv 335 ms / compressonator 2509 ms</description></item>
    /// </list>
    /// <para>
    /// The gap is NOT a missed parallelism flag, which was checked explicitly. Measuring each process's
    /// <see cref="System.Diagnostics.Process.TotalProcessorTime"/> against its wall time gives a cpu/wall ratio
    /// of 2.6-7.9x (texconv) and 3.2-9.5x (compressonator) on a 12-core machine -- both encoders already run
    /// multi-threaded and saturate most of the box by default. Compressonator's alternate backends were tried
    /// too and none help for BC3: <c>-EncodeWith HPC</c> measured the same as the default CPU path (967 vs
    /// 929 ms at 2048px, within noise; HPC targets BC6H/BC7), while <c>DXC</c> (3207 ms) and <c>GPU</c>
    /// (1299 ms) were worse. Its <c>-NumThreads</c> option is documented as BC6H/BC7-only. In CPU-time terms the
    /// difference is stark: at 4096px texconv spends 2.6 s and compressonator 23.9 s of CPU to do what Unity
    /// finishes in 33.8 ms of wall time.
    /// </para>
    /// <para>
    /// On top of the encode itself, the per-invocation floor is 49 ms (texconv) and 344 ms (compressonator), so
    /// a full mip chain costs 0.5-0.6 s resp. 3.4-4.5 s in pure process/I-O overhead before any encoding
    /// happens. Quality is not a compensating factor either: image diff came out 0.00029 (unity), 0.00056
    /// (texconv, worse) and 0.00028 (compressonator, equal within noise). Conclusion: unlike ASTC, there is
    /// nothing for an out-of-process DXT encoder to win -- it would be slower, not faster, so no async/CLI path
    /// is built for DXT. See <see cref="UnityTextureCompressor.CompressTexture"/>.
    /// </para>
    /// </remarks>
    [Explicit("Benchmark")]
    public class DxtCliBenchmarkTests
    {
        /// <summary>
        /// Temporary directory for benchmark work files, relative to the Unity project root.
        /// </summary>
        private const string TempDirectory = "Temp/dxtbench/work";

        private static readonly int[] Sizes = { 512, 1024, 2048, 4096 };

        /// <summary>
        /// Compares, for each texture size, Unity's built-in DXT5 compression against each available CLI encoder
        /// producing the same format (BC3), run out-of-process, and reports the CLI pipeline's cost broken down
        /// into TGA write, process run, and output read. Also reports the resulting image quality of each, so a
        /// speed difference is not mistaken for a free win.
        /// </summary>
        [Test]
        public void PipelineBreakdown()
        {
            var encoders = ResolveEncoders();

            Debug.Log("size,encoder,totalMs,writeMs,processMs,readMs,diff");
            var rows = new List<Row>();

            foreach (var size in Sizes)
            {
                var reference = CreateNaturalisticTexture(size);
                try
                {
                    MeasureUnity(reference, size, rows);
                    foreach (var encoder in encoders)
                    {
                        MeasureCli(encoder, reference, size, rows);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(reference);
                }
            }

            LogSummary(rows, encoders);
        }

        /// <summary>
        /// Measures the floor cost of a single CLI invocation by encoding a 4x4 image, where the actual encode
        /// work is negligible: what remains is process spawn plus temp-file I/O, which every CLI-based encoder
        /// pays per invocation. Reported as a median over several runs, since process spawn time is noisy.
        /// </summary>
        [Test]
        public void FixedInvocationOverhead()
        {
            var encoders = ResolveEncoders();

            const int iterations = 15;
            var sb = new StringBuilder();
            sb.AppendLine($"=== Fixed CLI invocation overhead (4x4 image, {iterations} runs each) ===");

            var reference = CreateNaturalisticTexture(4);
            try
            {
                foreach (var encoder in encoders)
                {
                    var samples = new List<double>(iterations);
                    for (var i = 0; i < iterations; i++)
                    {
                        var sw = Stopwatch.StartNew();
                        RunCliPipeline(encoder, reference, 4, out _, out _, out _, out _, out _);
                        sw.Stop();
                        samples.Add(sw.Elapsed.TotalMilliseconds);
                    }

                    samples.Sort();
                    var median = samples[samples.Count / 2];
                    sb.AppendLine($"-- {encoder.Name} --");
                    sb.AppendLine($"  median={median:F1}ms  min={samples[0]:F1}ms  max={samples[samples.Count - 1]:F1}ms");
                    sb.AppendLine("  Per-invocation floor. The astcenc pattern spawns one process per mip level,");
                    sb.AppendLine("  so a full mip chain multiplies it:");
                    foreach (var size in Sizes)
                    {
                        var mips = MipCount(size);
                        sb.AppendLine($"    {size,5}px -> {mips,2} mips -> {median * mips,8:F1}ms of pure overhead");
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(reference);
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Checks whether the CLI encoders are actually using multiple cores, by comparing each process's total
        /// CPU time (summed over all its threads) against its wall time. This exists to rule out the obvious
        /// explanation for the CLIs losing so badly in <see cref="PipelineBreakdown"/> -- that they were left
        /// running single-threaded while Unity's encoder was not -- rather than leaving it assumed.
        /// </summary>
        /// <remarks>
        /// Measured on a 12-core Windows machine: texconv reaches 2.6x (1024px) to 7.9x (4096px) and
        /// Compressonator 3.2x to 9.5x, i.e. both already engage most of the machine without any extra flag.
        /// Compressonator's <c>-EncodeWith HPC</c> was tried separately and made no difference for BC3 (it
        /// targets BC6H/BC7), and its <c>-NumThreads</c> is documented as BC6H/BC7-only.
        /// </remarks>
        [Test]
        public void ParallelismCheck()
        {
            var encoders = ResolveEncoders();

            var sb = new StringBuilder();
            sb.AppendLine($"=== Encoder parallelism (cpu time / wall time; machine has {SystemInfo.processorCount} cores) ===");

            foreach (var size in new[] { 1024, 2048, 4096 })
            {
                var reference = CreateNaturalisticTexture(size);
                try
                {
                    foreach (var encoder in encoders)
                    {
                        // Warm up first so the reported ratio reflects steady state rather than cold module load.
                        RunCliPipeline(encoder, reference, size, out _, out _, out _, out _, out _);

                        var blocks = RunCliPipeline(encoder, reference, size, out _, out var processMs, out _, out var cpuMs, out var output);
                        Assert.IsNotNull(blocks, $"{encoder.Name} failed for size={size}: {output}");

                        var ratio = processMs > 0 ? cpuMs / processMs : 0;
                        sb.AppendLine($"  {size,5}px {encoder.Name,-22} wall={processMs,8:F1}ms cpu={cpuMs,9:F1}ms cpu/wall={ratio,5:F2}x");
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(reference);
                }
            }

            sb.AppendLine("A ratio well above 1.00x means the encoder is already multi-threaded, so its slowness");
            sb.AppendLine("relative to Unity is not explained by a missing parallelism option.");
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Builds the list of CLI encoders that are actually present on this machine, skipping the whole test
        /// when none are.
        /// </summary>
        /// <returns>Available encoder specifications.</returns>
        private static List<EncoderSpec> ResolveEncoders()
        {
            var encoders = new List<EncoderSpec>();

            var texconv = Path.GetFullPath("Temp/dxtbench/texconv_dxtex/texconv.exe");
            if (File.Exists(texconv))
            {
                // -m 1: a single mip level, matching the astcenc pattern of encoding each level separately.
                // -y: overwrite. -nologo: suppress the banner so stdout stays small. texconv derives the output
                // file name from the input's, so the destination is given as a directory via -o.
                encoders.Add(new EncoderSpec(
                    "texconv/BC3",
                    texconv,
                    (input, output, tempDir) => $"-nologo -y -f BC3_UNORM -m 1 -o \"{tempDir}\" \"{input}\""));
            }

            var compressonator = Path.GetFullPath("Temp/dxtbench/compressonator/compressonatorcli-4.5.52-win64/compressonatorcli.exe");
            if (File.Exists(compressonator))
            {
                encoders.Add(new EncoderSpec(
                    "compressonator/BC3",
                    compressonator,
                    (input, output, tempDir) => $"-nomipmap -fd BC3 \"{input}\" \"{output}\""));
            }

            if (encoders.Count == 0)
            {
                Assert.Ignore("No DXT/BC encoder CLI is available; see this class's remarks for how to obtain them.");
            }

            return encoders;
        }

        private static void MeasureUnity(Texture2D reference, int size, List<Row> rows)
        {
            var candidate = CreateNaturalisticTexture(size);
            var sw = Stopwatch.StartNew();
            EditorUtility.CompressTexture(candidate, TextureFormat.DXT5, TextureCompressionQuality.Best);
            sw.Stop();

            var decoded = TestUtils.DecodeToRGBA32(candidate, size, size);
            var diff = OrientationAgnosticDifference(reference, decoded, size);
            AddRow(rows, size, "unity", sw.Elapsed.TotalMilliseconds, 0, 0, 0, diff);

            UnityEngine.Object.DestroyImmediate(decoded);
            UnityEngine.Object.DestroyImmediate(candidate);
        }

        private static void MeasureCli(EncoderSpec encoder, Texture2D reference, int size, List<Row> rows)
        {
            var sw = Stopwatch.StartNew();
            var blocks = RunCliPipeline(encoder, reference, size, out var writeMs, out var processMs, out var readMs, out _, out var output);
            sw.Stop();
            Assert.IsNotNull(blocks, $"{encoder.Name} failed for size={size}: {output}");

            var compressed = new Texture2D(size, size, TextureFormat.DXT5, false, false);
            compressed.LoadRawTextureData(blocks);
            compressed.Apply(false, false);

            var decoded = TestUtils.DecodeToRGBA32(compressed, size, size);
            var diff = OrientationAgnosticDifference(reference, decoded, size);
            AddRow(rows, size, encoder.Name, sw.Elapsed.TotalMilliseconds, writeMs, processMs, readMs, diff);

            UnityEngine.Object.DestroyImmediate(decoded);
            UnityEngine.Object.DestroyImmediate(compressed);
        }

        /// <summary>
        /// Runs the full out-of-process pipeline for a single image: write the source as TGA, run the encoder on
        /// it, and read the resulting DDS back as raw block data. Mirrors what
        /// <see cref="AstcencTextureCompressor"/> does per mip level, so the measured cost is comparable.
        /// </summary>
        /// <param name="encoder">Encoder to run.</param>
        /// <param name="source">Source image to encode.</param>
        /// <param name="size">Width and height of <paramref name="source"/>.</param>
        /// <param name="writeMs">Milliseconds spent writing the TGA input.</param>
        /// <param name="processMs">Milliseconds spent running the encoder process (wall clock).</param>
        /// <param name="readMs">Milliseconds spent reading the DDS output back.</param>
        /// <param name="cpuMs">CPU time consumed by the encoder process across all its threads.</param>
        /// <param name="output">Captured process output, for failure diagnosis.</param>
        /// <returns>Raw compressed block data, or null when the encoder failed.</returns>
        private static byte[] RunCliPipeline(EncoderSpec encoder, Texture2D source, int size, out double writeMs, out double processMs, out double readMs, out double cpuMs, out string output)
        {
            var tempDir = Path.GetFullPath(TempDirectory);
            Directory.CreateDirectory(tempDir);
            var id = Guid.NewGuid().ToString("N");
            var tgaPath = Path.Combine(tempDir, $"{id}.tga");
            var ddsPath = Path.Combine(tempDir, $"{id}.dds");

            try
            {
                // TgaTopToBottomOrigin = true, matching AstcencTextureCompressor (see its remarks for why).
                var swWrite = Stopwatch.StartNew();
                AstcUtility.WriteTga(source.GetPixels32(), size, size, true, tgaPath);
                swWrite.Stop();
                writeMs = swWrite.Elapsed.TotalMilliseconds;

                var arguments = encoder.BuildArguments(tgaPath, ddsPath, tempDir);
                var swProcess = Stopwatch.StartNew();
                var result = RunProcess(encoder.ExePath, arguments, 5 * 60 * 1000);
                swProcess.Stop();
                processMs = swProcess.Elapsed.TotalMilliseconds;
                output = result.Output;

                cpuMs = result.CpuMs;

                if (result.ExitCode != 0 || !File.Exists(ddsPath))
                {
                    readMs = 0;
                    return null;
                }

                var swRead = Stopwatch.StartNew();
                var fileData = File.ReadAllBytes(ddsPath);
                var blocks = StripDdsHeader(fileData);
                swRead.Stop();
                readMs = swRead.Elapsed.TotalMilliseconds;

                return blocks;
            }
            finally
            {
                AstcencCli.DeleteFileSilently(tgaPath);
                AstcencCli.DeleteFileSilently(ddsPath);
            }
        }

        /// <summary>
        /// Strips the DDS header, returning the raw compressed block data. Handles both the legacy 128-byte
        /// header and the 148-byte variant that carries an additional DDS_HEADER_DXT10 block.
        /// </summary>
        /// <param name="fileData">Full contents of a .dds file.</param>
        /// <returns>The block data following the header.</returns>
        private static byte[] StripDdsHeader(byte[] fileData)
        {
            const int LegacyHeaderBytes = 128;
            const int Dxt10HeaderBytes = 20;

            Assert.GreaterOrEqual(fileData.Length, LegacyHeaderBytes, "DDS file is shorter than its header.");
            Assert.AreEqual("DDS ", Encoding.ASCII.GetString(fileData, 0, 4), "Not a DDS file.");

            // DDS_PIXELFORMAT.dwFourCC sits 84 bytes in: 4 (magic) + 72 (offset of DDS_PIXELFORMAT within
            // DDS_HEADER) + 8 (offset of dwFourCC within DDS_PIXELFORMAT).
            var fourCC = Encoding.ASCII.GetString(fileData, 84, 4);
            var dataOffset = fourCC == "DX10" ? LegacyHeaderBytes + Dxt10HeaderBytes : LegacyHeaderBytes;

            var blocks = new byte[fileData.Length - dataOffset];
            Buffer.BlockCopy(fileData, dataOffset, blocks, 0, blocks.Length);
            return blocks;
        }

        private static ProcessRunResult RunProcess(string exePath, string arguments, int timeoutMs)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                // Compressonator loads its image-format plugins (e.g. qtga.dll for TGA input) relative to the
                // working directory, so it must run from its own install folder rather than the project root.
                WorkingDirectory = Path.GetDirectoryName(exePath),
            };

            using (var process = Process.Start(startInfo))
            {
                // Drained asynchronously so the process never blocks on a full output buffer, matching AstcencCli.
                var stdOutTask = process.StandardOutput.ReadToEndAsync();
                var stdErrTask = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(timeoutMs))
                {
                    AstcencCli.KillSilently(process);
                    return new ProcessRunResult(-1, "timed out", 0);
                }
                process.WaitForExit(); // Ensure redirected streams are flushed.

                // Read before leaving the using block: TotalProcessorTime is still available for an exited
                // process, but not for a disposed one. Summed across every thread the process used, so
                // comparing it against wall time reveals how many cores the encoder actually engaged.
                var cpuMs = process.TotalProcessorTime.TotalMilliseconds;
                return new ProcessRunResult(process.ExitCode, stdErrTask.Result + stdOutTask.Result, cpuMs);
            }
        }

        private static void AddRow(List<Row> rows, int size, string encoder, double totalMs, double writeMs, double processMs, double readMs, float diff)
        {
            rows.Add(new Row(size, encoder, totalMs, writeMs, processMs, readMs, diff));
            Debug.Log($"{size},{encoder},{totalMs:F1},{writeMs:F1},{processMs:F1},{readMs:F1},{diff:F5}");
        }

        private static void LogSummary(List<Row> rows, List<EncoderSpec> encoders)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Summary: Unity built-in DXT5 vs CLI encoders (mip 0 only, one invocation) ===");
            foreach (var size in Sizes)
            {
                var unity = rows.Find(r => r.Size == size && r.Encoder == "unity");
                sb.AppendLine($"-- size={size} --");
                sb.AppendLine($"  {"unity",-22} total={unity.TotalMs,9:F1}ms diff={unity.Diff:F5}");

                foreach (var encoder in encoders)
                {
                    var cli = rows.Find(r => r.Size == size && r.Encoder == encoder.Name);
                    var ratio = cli.TotalMs > 0 ? unity.TotalMs / cli.TotalMs : 0;
                    sb.AppendLine($"  {encoder.Name,-22} total={cli.TotalMs,9:F1}ms diff={cli.Diff:F5} (write={cli.WriteMs,6:F1} process={cli.ProcessMs,7:F1} read={cli.ReadMs,5:F1}) speed={ratio:F2}x vs Unity");
                }
            }
            sb.AppendLine("speed > 1.00x means the CLI is faster than Unity's built-in encoder.");
            Debug.Log(sb.ToString());
        }

        private static int MipCount(int size)
        {
            var count = 1;
            while (size > 1)
            {
                size >>= 1;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Computes the image difference, tolerating a vertically flipped result: the TGA origin convention used
        /// for the CLI input is not necessarily the one each encoder assumes, and a flip would otherwise show up
        /// as a large diff and be misread as poor encode quality. Returns the smaller of the two comparisons.
        /// </summary>
        /// <param name="reference">Reference image.</param>
        /// <param name="decoded">Decoded compressed image.</param>
        /// <param name="size">Width and height of both images.</param>
        /// <returns>The image difference under whichever orientation matches better.</returns>
        private static float OrientationAgnosticDifference(Texture2D reference, Texture2D decoded, int size)
        {
            var direct = TestUtils.Difference(reference, decoded);

            var flipped = FlipVertically(decoded, size);
            try
            {
                var flippedDiff = TestUtils.Difference(reference, flipped);
                return Math.Min(direct, flippedDiff);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(flipped);
            }
        }

        private static Texture2D FlipVertically(Texture2D texture, int size)
        {
            var source = TestUtils.CopyTextureAsReadable(texture).GetPixels32();
            var flipped = new Color32[source.Length];
            for (var y = 0; y < size; y++)
            {
                Array.Copy(source, y * size, flipped, (size - 1 - y) * size, size);
            }

            var result = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            result.SetPixels32(flipped);
            result.Apply(false, false);
            return result;
        }

        /// <summary>
        /// Generates a fully opaque procedural texture mixing several sine/cosine frequencies per channel, so it
        /// has both smooth gradients and higher-frequency detail instead of a flat placeholder image, which would
        /// let the encoders take shortcuts unrepresentative of real content. Matches the generator used by
        /// <see cref="AstcencBenchmarkTests"/> and <see cref="DxtBenchmarkTests"/> so numbers are comparable.
        /// </summary>
        /// <param name="size">Width and height of the generated texture.</param>
        /// <returns>The generated texture.</returns>
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

        /// <summary>
        /// A DXT/BC encoder CLI to benchmark.
        /// </summary>
        private readonly struct EncoderSpec
        {
            internal EncoderSpec(string name, string exePath, Func<string, string, string, string> buildArguments)
            {
                Name = name;
                ExePath = exePath;
                BuildArguments = buildArguments;
            }

            /// <summary>
            /// Gets the display name used in the reported rows.
            /// </summary>
            internal string Name { get; }

            /// <summary>
            /// Gets the full path to the executable.
            /// </summary>
            internal string ExePath { get; }

            /// <summary>
            /// Gets the callback building the command line from (input TGA path, output DDS path, temp directory).
            /// </summary>
            internal Func<string, string, string, string> BuildArguments { get; }
        }

        private readonly struct ProcessRunResult
        {
            internal ProcessRunResult(int exitCode, string output, double cpuMs)
            {
                ExitCode = exitCode;
                Output = output;
                CpuMs = cpuMs;
            }

            internal int ExitCode { get; }

            internal string Output { get; }

            /// <summary>
            /// Gets the total CPU time consumed across all of the process's threads, in milliseconds.
            /// </summary>
            internal double CpuMs { get; }
        }

        private readonly struct Row
        {
            internal Row(int size, string encoder, double totalMs, double writeMs, double processMs, double readMs, float diff)
            {
                Size = size;
                Encoder = encoder;
                TotalMs = totalMs;
                WriteMs = writeMs;
                ProcessMs = processMs;
                ReadMs = readMs;
                Diff = diff;
            }

            internal int Size { get; }

            internal string Encoder { get; }

            internal double TotalMs { get; }

            internal double WriteMs { get; }

            internal double ProcessMs { get; }

            internal double ReadMs { get; }

            internal float Diff { get; }
        }
    }
}
