using System;
using System.IO;
using System.Linq;
using System.Threading;
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
        internal static CacheManager Texture = new CacheManager("Global\\VRCQuestTools-Mutex-TextureCache", () => VRCQuestToolsSettings.TextureCacheFolder);

        private readonly string mutexName;
        private readonly Func<string> getCachePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class.
        /// </summary>
        /// <param name="mutexName">Mutex name.</param>
        /// <param name="cachePathFunc">Function to get cache path.</param>
        public CacheManager(string mutexName, Func<string> cachePathFunc)
        {
            this.mutexName = mutexName;
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
            using (var mutex = CreateMutex())
            {
                mutex.WaitOne();
                try
                {
                    Directory.CreateDirectory(CachePath);
                    File.WriteAllText(Path.Combine(CachePath, fileName), data);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Load data from cache.
        /// </summary>
        /// <param name="fileName">File name to load.</param>
        /// <returns>Loaded string.</returns>
        internal string LoadString(string fileName)
        {
            using (var mutex = CreateMutex())
            {
                mutex.WaitOne();
                try
                {
                    var file = Path.Combine(CachePath, fileName);
                    var data = File.ReadAllText(file);
                    File.SetLastAccessTimeUtc(file, System.DateTime.UtcNow);
                    return data;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Copy file to cache.
        /// </summary>
        /// <param name="srcPath">Source path.</param>
        /// <param name="fileName">File name to copy.</param>
        internal void CopyToCache(string srcPath, string fileName)
        {
            using (var mutex = CreateMutex())
            {
                mutex.WaitOne();
                try
                {
                    Directory.CreateDirectory(CachePath);
                    File.Copy(srcPath, Path.Combine(CachePath, fileName), true);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Copy file from cache.
        /// </summary>
        /// <param name="fileName">File name to copy.</param>
        /// <param name="destPath">Destination path.</param>
        internal void CopyFromCache(string fileName, string destPath)
        {
            using (var mutex = CreateMutex())
            {
                mutex.WaitOne();
                try
                {
                    var file = Path.Combine(CachePath, fileName);
                    File.Copy(file, destPath, true);
                    File.SetLastAccessTimeUtc(file, System.DateTime.UtcNow);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
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
            using (var mutex = CreateMutex())
            {
                mutex.WaitOne();
                try
                {
                    if (!Directory.Exists(CachePath))
                    {
                        return;
                    }
                    Directory.GetFiles(CachePath).ToList().ForEach(File.Delete);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Clear cache to fit the total size.
        /// Files are deleted in order of last access time.
        /// </summary>
        /// <param name="totalSize">Total size to fit.</param>
        internal void Clear(ulong totalSize)
        {
            using (var mutex = CreateMutex())
            {
                mutex.WaitOne();
                try
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
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Create a mutex.
        /// </summary>
        /// <returns>Created mutex, not owned.</returns>
        internal Mutex CreateMutex()
        {
            return new Mutex(false, mutexName);
        }
    }
}
