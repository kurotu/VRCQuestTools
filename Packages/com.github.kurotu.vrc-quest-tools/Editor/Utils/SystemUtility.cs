// <copyright file="SystemUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for C# and System.
    /// </summary>
    internal static class SystemUtility
    {
        /// <summary>
        /// Gets type object by full name.
        /// </summary>
        /// <param name="fullName">Full type name to get.</param>
        /// <returns>Type or null.</returns>
        internal static System.Type GetTypeByName(string fullName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets path to local cache directory for the application.
        /// </summary>
        /// <param name="appName">Application name.</param>
        /// <returns>Cache directory for the app.</returns>
        internal static string GetAppLocalCachePath(string appName)
        {
#if UNITY_EDITOR_WIN
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "Cache");
#elif UNITY_EDITOR_OSX
            var home = Environment.GetEnvironmentVariable("HOME");
            return Path.Combine(home, "Library/Caches", appName);
#elif UNITY_EDITOR_LINUX
            var xdgCacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
            if (!string.IsNullOrEmpty(xdgCacheHome))
            {
                return Path.Combine(xdgCacheHome, appName);
            }
            return Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".cache", appName);
#else
            throw new InvalidProgramException("Unsupported editor platform");
#endif
        }

        /// <summary>
        /// Open folder in file explorer.
        /// </summary>
        /// <param name="path">Path to open.</param>
        internal static void OpenFolder(string path)
        {
            if (Directory.Exists(path))
            {
#if UNITY_EDITOR_WIN
                System.Diagnostics.Process.Start(path);
#elif UNITY_EDITOR_OSX
                System.Diagnostics.Process.Start("open", path);
#elif UNITY_EDITOR_LINUX
                System.Diagnostics.Process.Start("xdg-open", path);
#else
                throw new InvalidProgramException("Unsupported editor platform");
#endif
            }
            else
            {
                throw new DirectoryNotFoundException($"Directory not found: {path}");
            }
        }
    }
}
