using System.IO;
using System.Linq;

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
        internal static CacheManager Texture = new CacheManager(Path.Combine("Library", VRCQuestTools.Name, "TextureCache"));

        private string cachePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class.
        /// </summary>
        /// <param name="cachePath">Root path to manage.</param>
        public CacheManager(string cachePath)
        {
            this.cachePath = cachePath;
        }

        /// <summary>
        /// Save data to cache.
        /// </summary>
        /// <param name="fileName">File name to save.</param>
        /// <param name="data">Data to save.</param>
        internal void Save(string fileName, string data)
        {
            Directory.CreateDirectory(cachePath);
            File.WriteAllText(Path.Combine(cachePath, fileName), data);
        }

        /// <summary>
        /// Load data from cache.
        /// </summary>
        /// <param name="fileName">File name to load.</param>
        /// <returns>Loaded string.</returns>
        internal string LoadString(string fileName)
        {
            var file = Path.Combine(cachePath, fileName);
            var data = File.ReadAllText(file);
            File.SetLastAccessTimeUtc(file, System.DateTime.UtcNow);
            return data;
        }

        /// <summary>
        /// Copy file to cache.
        /// </summary>
        /// <param name="srcPath">Source path.</param>
        /// <param name="fileName">File name to copy.</param>
        internal void CopyToCache(string srcPath, string fileName)
        {
            Directory.CreateDirectory(cachePath);
            File.Copy(srcPath, Path.Combine(cachePath, fileName), true);
        }

        /// <summary>
        /// Copy file from cache.
        /// </summary>
        /// <param name="fileName">File name to copy.</param>
        /// <param name="destPath">Destination path.</param>
        internal void CopyFromCache(string fileName, string destPath)
        {
            var file = Path.Combine(cachePath, fileName);
            File.Copy(file, destPath, true);
            File.SetLastAccessTimeUtc(file, System.DateTime.UtcNow);
        }

        /// <summary>
        /// Check whether the file exists in cache.
        /// </summary>
        /// <param name="fileName">File name to check.</param>
        /// <returns>true when the file exists.</returns>
        internal bool Exists(string fileName)
        {
            return File.Exists(Path.Combine(cachePath, fileName));
        }

        /// <summary>
        /// Clear cache.
        /// </summary>
        internal void Clear()
        {
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
            }
        }

        /// <summary>
        /// Clear cache to fit the total size.
        /// Files are deleted in order of last access time.
        /// </summary>
        /// <param name="totalSize">Total size to fit.</param>
        internal void Clear(ulong totalSize)
        {
            var files = new DirectoryInfo(cachePath).GetFiles()
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
