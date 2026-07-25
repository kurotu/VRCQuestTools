// <copyright file="AstcencCli.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Process wrapper for the astcenc CLI.
    /// </summary>
    internal static class AstcencCli
    {
        /// <summary>
        /// Temporary directory for astcenc work files, relative to the Unity project root.
        /// </summary>
        internal const string TempDirectory = "Temp/VRCQuestTools/astcenc";

        private const int UtilityTimeoutMs = 10 * 1000;

        /// <summary>
        /// Runs astcenc to compress an image file into a .astc file.
        /// </summary>
        /// <param name="exePath">Path to the astcenc executable.</param>
        /// <param name="inputPath">Path to the input image file (e.g. .tga).</param>
        /// <param name="outputPath">Path to the output .astc file.</param>
        /// <param name="blockSize">Block size string (e.g. "6x6").</param>
        /// <param name="preset">Quality preset with a leading dash (e.g. "-thorough").</param>
        /// <param name="srgb">true to compress with the sRGB transfer function (-cs), false for linear (-cl).</param>
        /// <param name="jobs">Number of threads to use (-j).</param>
        /// <param name="timeoutMs">Timeout in milliseconds. The process is killed on timeout.</param>
        /// <returns>Result of the run.</returns>
        internal static AstcencRunResult RunCompress(string exePath, string inputPath, string outputPath, string blockSize, string preset, bool srgb, int jobs, int timeoutMs)
        {
            var mode = srgb ? "-cs" : "-cl";
            var arguments = $"{mode} \"{inputPath}\" \"{outputPath}\" {blockSize} {preset} -j {jobs} -silent";
            try
            {
                using (var process = Process.Start(CreateStartInfo(exePath, arguments)))
                {
                    _ = process.StandardOutput.ReadToEndAsync(); // Drain to avoid blocking on a full buffer.
                    var stdErrTask = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(timeoutMs))
                    {
                        KillSilently(process);
                        return new AstcencRunResult
                        {
                            Success = false,
                            ExitCode = -1,
                            StdErr = $"astcenc timed out after {timeoutMs} ms",
                            TimedOut = true,
                        };
                    }
                    process.WaitForExit(); // Ensure redirected streams are flushed.
                    return new AstcencRunResult
                    {
                        Success = process.ExitCode == 0,
                        ExitCode = process.ExitCode,
                        StdErr = stdErrTask.Result,
                        TimedOut = false,
                    };
                }
            }
            catch (Exception e)
            {
                return new AstcencRunResult
                {
                    Success = false,
                    ExitCode = -1,
                    StdErr = e.Message,
                    TimedOut = false,
                };
            }
        }

        /// <summary>
        /// Tests whether the astcenc executable actually works by compressing a tiny generated image.
        /// </summary>
        /// <param name="exePath">Path to the astcenc executable.</param>
        /// <returns>true when the executable produced the expected output.</returns>
        internal static bool SelfTest(string exePath)
        {
            var id = Guid.NewGuid().ToString("N");
            var inputPath = Path.GetFullPath(Path.Combine(TempDirectory, $"selftest-{id}.tga"));
            var outputPath = Path.GetFullPath(Path.Combine(TempDirectory, $"selftest-{id}.astc"));
            try
            {
                Directory.CreateDirectory(Path.GetFullPath(TempDirectory));
                var pixels = new Color32[4 * 4];
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(128, 128, 128, 255);
                }
                AstcUtility.WriteTga(pixels, 4, 4, true, inputPath);

                var result = RunCompress(exePath, inputPath, outputPath, "4x4", "-medium", true, 1, UtilityTimeoutMs);
                if (!result.Success)
                {
                    return false;
                }
                var outputFile = new FileInfo(outputPath);
                return outputFile.Exists && outputFile.Length == AstcUtility.AstcHeaderBytes + AstcUtility.BlockBytes;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                DeleteFileSilently(inputPath);
                DeleteFileSilently(outputPath);
            }
        }

        /// <summary>
        /// Gets the version of the astcenc executable.
        /// </summary>
        /// <param name="exePath">Path to the astcenc executable.</param>
        /// <returns>Version string (e.g. "5.6.0"), or null on failure.</returns>
        internal static string GetVersion(string exePath)
        {
            try
            {
                using (var process = Process.Start(CreateStartInfo(exePath, "-version")))
                {
                    var stdOutTask = process.StandardOutput.ReadToEndAsync();
                    _ = process.StandardError.ReadToEndAsync(); // Drain to avoid blocking on a full buffer.
                    if (!process.WaitForExit(UtilityTimeoutMs))
                    {
                        KillSilently(process);
                        return null;
                    }
                    process.WaitForExit(); // Ensure redirected streams are flushed.
                    if (process.ExitCode != 0)
                    {
                        return null;
                    }

                    // The version banner is printed to stdout: "astcenc v5.6.0, 64-bit avx2+popcnt+f16c".
                    var match = Regex.Match(stdOutTask.Result, @"astcenc v(\d+(?:\.\d+)+)");
                    return match.Success ? match.Groups[1].Value : null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Kills a process, ignoring errors caused by the process having already exited.
        /// </summary>
        /// <param name="process">Process to kill.</param>
        internal static void KillSilently(Process process)
        {
            try
            {
                process.Kill();
            }
            catch (InvalidOperationException)
            {
                // The process has already exited.
            }
            catch (Win32Exception)
            {
                // The process could not be terminated.
            }
        }

        private static ProcessStartInfo CreateStartInfo(string exePath, string arguments)
        {
            return new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
        }

        private static void DeleteFileSilently(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // Leftovers in Temp/ are cleaned up together with the Temp folder eventually.
            }
            catch (UnauthorizedAccessException)
            {
                // Leftovers in Temp/ are cleaned up together with the Temp folder eventually.
            }
        }

        /// <summary>
        /// Result of an astcenc process run.
        /// </summary>
        internal struct AstcencRunResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether the process exited successfully (exit code 0).
            /// </summary>
            internal bool Success { get; set; }

            /// <summary>
            /// Gets or sets the process exit code. -1 when the process did not produce an exit code.
            /// </summary>
            internal int ExitCode { get; set; }

            /// <summary>
            /// Gets or sets the standard error output, or a wrapper-generated message on failure to run.
            /// </summary>
            internal string StdErr { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the process was killed due to timeout.
            /// </summary>
            internal bool TimedOut { get; set; }
        }
    }
}
