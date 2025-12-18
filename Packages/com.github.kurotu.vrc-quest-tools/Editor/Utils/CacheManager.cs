using System;
using System.IO;
using System.Linq;
using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Cache manager.
    /// </summary>
    internal class CacheManager
    {
        /// <summary>
        /// Cache manager for textures.
        /// </summary>
        internal static readonly CacheManager Texture = new CacheManager(() => VRCQuestToolsSettings.TextureCacheFolder);

        private readonly object lockObject = new object();
        private readonly Func<string> getCachePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class.
        /// </summary>
        /// <param name="cachePathFunc">Function to get cache path.</param>
        public CacheManager(Func<string> cachePathFunc)
        {
            getCachePath = cachePathFunc;
        }

        private string CachePath => getCachePath();

        /// <summary>
        /// Save data to cache.
        /// </summary>
        /// <param name="fileName">File name to save.</param>
        /// <param name="data">Data to save.</param>
        internal void Save(string fileName, string data)
        {
            lock (lockObject)
            {
                Directory.CreateDirectory(CachePath);
                File.WriteAllText(Path.Combine(CachePath, fileName), data);
            }
        }

        /// <summary>
        /// Load data from cache.
        /// </summary>
        /// <param name="fileName">File name to load.</param>
        /// <returns>Loaded string.</returns>
        internal string LoadString(string fileName)
        {
            lock (lockObject)
            {
                var file = Path.Combine(CachePath, fileName);
                var data = File.ReadAllText(file);
                File.SetLastAccessTimeUtc(file, System.DateTime.UtcNow);
                return data;
            }
        }

        /// <summary>
        /// Copy file to cache.
        /// </summary>
        /// <param name="srcPath">Source path.</param>
        /// <param name="fileName">File name to copy.</param>
        internal void CopyToCache(string srcPath, string fileName)
        {
            lock (lockObject)
            {
                Directory.CreateDirectory(CachePath);
                File.Copy(srcPath, Path.Combine(CachePath, fileName), true);
            }
        }

        /// <summary>
        /// Copy file from cache.
        /// </summary>
        /// <param name="fileName">File name to copy.</param>
        /// <param name="destPath">Destination path.</param>
        internal void CopyFromCache(string fileName, string destPath)
        {
            lock (lockObject)
            {
                var file = Path.Combine(CachePath, fileName);
                File.Copy(file, destPath, true);
                File.SetLastAccessTimeUtc(file, System.DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Check whether the file exists in cache.
        /// </summary>
        /// <param name="fileName">File name to check.</param>
        /// <returns>true when the file exists.</returns>
        internal bool Exists(string fileName)
        {
            return File.Exists(Path.Combine(CachePath, fileName));
        }

        /// <summary>
        /// Clear cache.
        /// </summary>
        internal void Clear()
        {
            lock (lockObject)
            {
                if (!Directory.Exists(CachePath))
                {
                    return;
                }
                Directory.GetFiles(CachePath).ToList().ForEach(File.Delete);
            }
        }

        /// <summary>
        /// Clear cache to fit the total size.
        /// Files are deleted in order of last access time.
        /// </summary>
        /// <param name="totalSize">Total size to fit.</param>
        internal void Clear(ulong totalSize)
        {
            lock (lockObject)
            {
                if (!Directory.Exists(CachePath))
                {
                    return;
                }
                var files = new DirectoryInfo(CachePath).GetFiles()
                    .OrderBy(f => f.LastAccessTime)
                    .Reverse()
                    .ToArray();
                ulong size = 0;
                foreach (var file in files)
                {
                    size += (ulong)file.Length;
                    if (size > totalSize)
                    {
                        file.Delete();
                    }
                }
            }
        }
    }
}
