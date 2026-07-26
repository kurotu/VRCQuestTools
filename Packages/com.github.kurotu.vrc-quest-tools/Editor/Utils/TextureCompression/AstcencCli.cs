// <copyright file="AstcencCli.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
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
        /// Every astcenc process currently running via <see cref="RunProcess"/>, whether started from the main
        /// thread (every synchronous facade method) or from the background thread pool worker behind
        /// <see cref="AstcencTextureCompressor.CompressTextureAsync"/> / <see cref="AstcencTextureCompressor.CompressNormalMapAsync"/>
        /// (used only by the NDMF preview's progressive texture replacement queue, <see cref="PreviewTextureCompressionQueue"/>).
        /// Guarded by <see cref="runningProcessesLock"/> since the async path adds/removes from a thread pool
        /// thread while <see cref="KillAllRunningProcesses"/> may be called from the main thread.
        /// </summary>
        private static readonly HashSet<Process> RunningProcesses = new HashSet<Process>();

        private static readonly object runningProcessesLock = new object();

        private static bool tempDirectoryCleaned = false;

        /// <summary>
        /// Deletes any files already present in <see cref="TempDirectory"/>, once per editor session. This runs
        /// the first time an <see cref="AstcencTextureCompressor"/> is constructed in the session (its constructor
        /// calls this unconditionally), before any astcenc invocation in that session -- synchronous or the
        /// background-thread async path used by <see cref="PreviewTextureCompressionQueue"/> -- has written a
        /// single temp file of its own yet. So anything found here on first use is necessarily a leftover from an
        /// aborted previous session (e.g. the editor crashing mid-compression), never a file some operation still
        /// in flight this session owns.
        /// </summary>
        internal static void CleanupTempDirectoryOnce()
        {
            if (tempDirectoryCleaned)
            {
                return;
            }
            tempDirectoryCleaned = true;

            try
            {
                var fullPath = Path.GetFullPath(TempDirectory);
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                }
            }
            catch (Exception e)
            {
                // Best-effort cleanup; a leftover file or two does not prevent compression from working.
                Logger.LogDebug($"Failed to clean up astcenc temp directory: {e.Message}");
            }
        }

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
                var result = RunProcess(exePath, arguments, timeoutMs);
                if (result.TimedOut)
                {
                    return new AstcencRunResult
                    {
                        Success = false,
                        ExitCode = -1,
                        StdErr = $"astcenc timed out after {timeoutMs} ms",
                        TimedOut = true,
                    };
                }
                return new AstcencRunResult
                {
                    Success = result.ExitCode == 0,
                    ExitCode = result.ExitCode,
                    StdErr = result.StdErr,
                    TimedOut = false,
                };
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
                var result = RunProcess(exePath, "-version", UtilityTimeoutMs);
                if (result.TimedOut || result.ExitCode != 0)
                {
                    return null;
                }

                // The version banner is printed to stdout: "astcenc v5.6.0, 64-bit avx2+popcnt+f16c".
                var match = Regex.Match(result.StdOut, @"astcenc v(\d+(?:\.\d+)+)");
                return match.Success ? match.Groups[1].Value : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Kills every astcenc process currently running via <see cref="RunProcess"/>, so a domain reload or
        /// editor quit does not leave an orphaned astcenc.exe process running past the editor session that
        /// started it. Called by <see cref="PreviewTextureCompressionQueue"/> alongside cancelling its own
        /// background work, since killing the process is what actually unblocks a worker thread currently
        /// blocked in <see cref="Process.WaitForExit(int)"/> for the in-flight mip level -- cancellation alone
        /// only stops the *next* level from starting. Safe to call when nothing is running (a no-op).
        /// </summary>
        internal static void KillAllRunningProcesses()
        {
            Process[] processes;
            lock (runningProcessesLock)
            {
                processes = new Process[RunningProcesses.Count];
                RunningProcesses.CopyTo(processes);
            }

            foreach (var process in processes)
            {
                KillSilently(process);
            }
        }

        /// <summary>
        /// Kills a process, ignoring errors caused by the process having already exited (or, for
        /// <see cref="KillAllRunningProcesses"/>'s snapshot-then-kill pattern, already having been disposed by
        /// <see cref="RunProcess"/>'s own <c>using</c> block in the narrow window between the snapshot and this
        /// call -- <see cref="ObjectDisposedException"/> derives from <see cref="InvalidOperationException"/>, so
        /// the existing catch below already covers it too).
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
                // The process has already exited, or (see this method's remarks) was already disposed.
            }
            catch (Win32Exception)
            {
                // The process could not be terminated.
            }
        }

        /// <summary>
        /// Runs an executable and waits for it to exit, draining stdout/stderr asynchronously so the process
        /// never blocks on a full output buffer. On timeout, the process is killed and the output streams are
        /// not read (they may never complete).
        /// </summary>
        /// <param name="exePath">Path to the executable.</param>
        /// <param name="arguments">Command line arguments.</param>
        /// <param name="timeoutMs">Timeout in milliseconds. The process is killed on timeout.</param>
        /// <returns>Result of the run.</returns>
        private static ProcessRunResult RunProcess(string exePath, string arguments, int timeoutMs)
        {
            using (var process = Process.Start(CreateStartInfo(exePath, arguments)))
            {
                lock (runningProcessesLock)
                {
                    RunningProcesses.Add(process);
                }

                try
                {
                    var stdOutTask = process.StandardOutput.ReadToEndAsync();
                    var stdErrTask = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(timeoutMs))
                    {
                        KillSilently(process);
                        try
                        {
                            // Give the OS a moment to actually release the process's file handles (e.g. the input/output
                            // temp files) after the kill signal, so the caller's temp-file cleanup that runs right after
                            // this returns doesn't race a still-exiting process and silently fail to delete them.
                            process.WaitForExit(2000);
                        }
                        catch (Exception)
                        {
                            // Best-effort; proceed regardless of whether the post-kill wait itself succeeded.
                        }
                        return new ProcessRunResult(-1, string.Empty, string.Empty, true);
                    }
                    process.WaitForExit(); // Ensure redirected streams are flushed.
                    return new ProcessRunResult(process.ExitCode, stdOutTask.Result, stdErrTask.Result, false);
                }
                finally
                {
                    // Removed even when WaitForExit above is interrupted by KillAllRunningProcesses's own
                    // KillSilently call racing this method's timeout-triggered KillSilently call above -- both
                    // are idempotent (KillSilently swallows "already exited"), so the race is harmless.
                    lock (runningProcessesLock)
                    {
                        RunningProcesses.Remove(process);
                    }
                }
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

        /// <summary>
        /// Deletes a file, ignoring errors. Shared by every astcenc temp-file cleanup path (production and tests).
        /// </summary>
        /// <param name="path">Path of the file to delete.</param>
        internal static void DeleteFileSilently(string path)
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
        /// Result of running a process to completion (or timing out).
        /// </summary>
        private readonly struct ProcessRunResult
        {
            internal ProcessRunResult(int exitCode, string stdOut, string stdErr, bool timedOut)
            {
                ExitCode = exitCode;
                StdOut = stdOut;
                StdErr = stdErr;
                TimedOut = timedOut;
            }

            /// <summary>
            /// Gets the process exit code. -1 when the process did not produce an exit code.
            /// </summary>
            internal int ExitCode { get; }

            /// <summary>
            /// Gets the captured standard output. Empty when the process timed out.
            /// </summary>
            internal string StdOut { get; }

            /// <summary>
            /// Gets the captured standard error. Empty when the process timed out.
            /// </summary>
            internal string StdErr { get; }

            /// <summary>
            /// Gets a value indicating whether the process was killed due to timeout.
            /// </summary>
            internal bool TimedOut { get; }
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
