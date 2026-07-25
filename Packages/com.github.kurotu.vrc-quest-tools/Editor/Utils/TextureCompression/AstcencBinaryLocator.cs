// <copyright file="AstcencBinaryLocator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Locates a usable astcenc executable for the current platform.
    /// </summary>
    internal static class AstcencBinaryLocator
    {
#if UNITY_EDITOR_OSX
        /// <summary>
        /// Minimum version required for a system-installed astcenc (macOS).
        /// </summary>
        private static readonly Version MinimumSystemVersion = new Version(4, 0, 0);
#endif

        private static Lazy<Resolution> cachedResolution = new Lazy<Resolution>(Resolve);

        /// <summary>
        /// Gets the full path of a usable astcenc executable. The result is cached.
        /// </summary>
        /// <returns>Full path of the executable, or null when no usable astcenc is found.</returns>
        internal static string GetAstcencPath()
        {
            return cachedResolution.Value.Path;
        }

        /// <summary>
        /// Gets the path and version of a usable astcenc executable, resolved (and its version queried) at most
        /// once. Callers that need both the path and the version (e.g. to build the cache key component) should
        /// use this instead of pairing <see cref="GetAstcencPath"/> with a separate <see cref="AstcencCli.GetVersion"/>
        /// call, which would spawn a second astcenc process just to re-derive information already gathered here.
        /// </summary>
        /// <returns>Path and version, or null when no usable astcenc is found.</returns>
        internal static Resolution? GetResolution()
        {
            return cachedResolution.Value.Path != null ? cachedResolution.Value : (Resolution?)null;
        }

        /// <summary>
        /// Resets the cached resolution result. For tests only.
        /// </summary>
        internal static void ResetCacheForTesting()
        {
            cachedResolution = new Lazy<Resolution>(Resolve);
        }

        private static Resolution Resolve()
        {
#if UNITY_EDITOR_WIN
            var path = ResolveBundled("win-x64", ".exe", false);
#elif UNITY_EDITOR_LINUX
            var path = ResolveBundled("linux-x64", string.Empty, true);
#elif UNITY_EDITOR_OSX
            var path = ResolveSystem();
#else
            string path = null;
#endif
            if (path != null)
            {
                var version = AstcencCli.GetVersion(path);
                Logger.Log($"Using astcenc for ASTC compression: {path} (version {version})");
                return new Resolution(path, version);
            }
            else
            {
                Logger.Log("No usable astcenc executable was found. Using Unity's texture compression.");
                return new Resolution(null, null);
            }
        }

        /// <summary>
        /// Path and version of a resolved astcenc executable.
        /// </summary>
        internal readonly struct Resolution
        {
            internal Resolution(string path, string version)
            {
                Path = path;
                Version = version;
            }

            /// <summary>
            /// Gets the full path of the astcenc executable.
            /// </summary>
            internal string Path { get; }

            /// <summary>
            /// Gets the astcenc version string (e.g. "5.6.0"), or null when it could not be determined.
            /// </summary>
            internal string Version { get; }
        }

#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
        private static string ResolveBundled(string platformFolder, string extension, bool needsChmod)
        {
            var folder = Path.Combine(Path.GetFullPath(VRCQuestTools.AssetRoot), "Editor", "Tools", "astcenc", platformFolder);
            foreach (var variant in new[] { "astcenc-avx2", "astcenc-sse2" })
            {
                var path = Path.Combine(folder, variant + extension);
                if (!File.Exists(path))
                {
                    continue;
                }
                if (needsChmod && !TryChmodExecutable(path))
                {
                    continue;
                }
                if (!AstcencCli.SelfTest(path))
                {
                    continue;
                }
                return path;
            }
            return null;
        }

        private static bool TryChmodExecutable(string path)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "/bin/chmod",
                    Arguments = $"+x \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using (var process = Process.Start(startInfo))
                {
                    if (!process.WaitForExit(10 * 1000))
                    {
                        AstcencCli.KillSilently(process);
                        return false;
                    }
                    return process.ExitCode == 0;
                }
            }
            catch (Exception e)
            {
                Logger.LogDebug($"chmod failed for {path}: {e.Message}");
                return false;
            }
        }
#endif

#if UNITY_EDITOR_OSX
        private static string ResolveSystem()
        {
            foreach (var candidate in EnumerateSystemCandidates())
            {
                var versionString = AstcencCli.GetVersion(candidate);
                if (versionString == null || !Version.TryParse(versionString, out var version) || version < MinimumSystemVersion)
                {
                    continue;
                }
                if (!AstcencCli.SelfTest(candidate))
                {
                    continue;
                }
                return candidate;
            }
            return null;
        }

        private static IEnumerable<string> EnumerateSystemCandidates()
        {
            var fromShell = ResolveFromLoginShell();
            if (fromShell != null)
            {
                yield return fromShell;
            }

            var knownDirectories = new[]
            {
                "/opt/homebrew/bin",
                "/usr/local/bin",
                "/opt/local/bin",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/bin"),
            };
            foreach (var directory in knownDirectories)
            {
                var path = Path.Combine(directory, "astcenc");
                if (File.Exists(path))
                {
                    yield return path;
                }
            }
        }

        private static string ResolveFromLoginShell()
        {
            var shell = Environment.GetEnvironmentVariable("SHELL");
            if (string.IsNullOrEmpty(shell))
            {
                return null;
            }
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = shell,
                    Arguments = "-l -c \"command -v astcenc\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                using (var process = Process.Start(startInfo))
                {
                    var stdOutTask = process.StandardOutput.ReadToEndAsync();
                    _ = process.StandardError.ReadToEndAsync(); // Drain to avoid blocking on a full buffer.
                    if (!process.WaitForExit(10 * 1000))
                    {
                        AstcencCli.KillSilently(process);
                        return null;
                    }
                    process.WaitForExit(); // Ensure redirected streams are flushed.
                    if (process.ExitCode != 0)
                    {
                        return null;
                    }
                    var path = stdOutTask.Result.Trim();
                    if (path.Length == 0 || !File.Exists(path))
                    {
                        return null;
                    }
                    return path;
                }
            }
            catch (Exception e)
            {
                Logger.LogDebug($"Failed to resolve astcenc via login shell {shell}: {e.Message}");
                return null;
            }
        }
#endif
    }
}
