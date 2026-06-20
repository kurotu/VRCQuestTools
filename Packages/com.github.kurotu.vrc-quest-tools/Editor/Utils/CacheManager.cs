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
        internal static readonly CacheManager Texture = new CacheManager(() => VRCQuestToolsSettings.TextureCacheFolder, true);

        private readonly object lockObject = new object();
        private readonly Func<string> getCachePath;
        private readonly bool hashFileNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class.
        /// </summary>
        /// <param name="cachePathFunc">Function to get cache path.</param>
        public CacheManager(Func<string> cachePathFunc)
            : this(cachePathFunc, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class with option to hash filenames.
        /// </summary>
        /// <param name="cachePathFunc">Function to get cache path.</param>
        /// <param name="hashFileNames">Whether to hash file names when storing/looking up files.</param>
        public CacheManager(Func<string> cachePathFunc, bool hashFileNames = false)
        {
            getCachePath = cachePathFunc;
            this.hashFileNames = hashFileNames;
        }

        private string CachePath => getCachePath();

        private string MapFileNameForSave(string fileName)
        {
            return hashFileNames ? CacheUtility.HashFileName(fileName) : fileName;
        }

        private string ResolveExistingFileName(string fileName)
        {
            if (!hashFileNames)
            {
                return fileName;
            }
            var hashed = CacheUtility.HashFileName(fileName);
            var hashedPath = Path.Combine(CachePath, hashed);
            if (File.Exists(hashedPath))
            {
                return hashed;
            }
            var originalPath = Path.Combine(CachePath, fileName);
            if (File.Exists(originalPath))
            {
                return fileName;
            }
            // If neither exists, return the hashed name as the preferred save target.
            return hashed;
        }

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
                var target = MapFileNameForSave(fileName);
                File.WriteAllText(Path.Combine(CachePath, target), data);
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
                var resolved = ResolveExistingFileName(fileName);
                var file = Path.Combine(CachePath, resolved);
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
                var target = MapFileNameForSave(fileName);
                File.Copy(srcPath, Path.Combine(CachePath, target), true);
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
                var resolved = ResolveExistingFileName(fileName);
                var file = Path.Combine(CachePath, resolved);
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
            if (!hashFileNames)
            {
                return File.Exists(Path.Combine(CachePath, fileName));
            }
            var hashed = Path.Combine(CachePath, CacheUtility.HashFileName(fileName));
            if (File.Exists(hashed))
            {
                return true;
            }
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
