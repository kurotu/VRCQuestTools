// <copyright file="CacheManager.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
        /// <remarks>
        /// The stamp combines the tool version with the cache entry layout revision. Every texture cache key
        /// already embeds <see cref="VRCQuestTools.Version"/> (see <see cref="Models.MaterialGeneratorUtility"/>),
        /// so after an update no existing entry can ever be hit again -- but the file names on disk are hashed,
        /// so the stale entries cannot be recognized individually and would just sit there occupying the size
        /// budget until eviction happened to reach them. The stamp turns that into a single wipe on first use.
        /// </remarks>
        internal static readonly CacheManager Texture = new CacheManager(
            () => VRCQuestToolsSettings.TextureCacheFolder,
            true,
            () => $"{VRCQuestTools.Version} texture{CacheUtility.TextureCache.FormatVersion}");

        /// <summary>
        /// Name of the file recording the stamp of the entries currently in the cache folder. Not a cache entry
        /// itself, so it is excluded from lookups, eviction and clearing.
        /// </summary>
        private const string StampFileName = "cache_stamp.txt";

        private readonly object lockObject = new object();
        private readonly Func<string> getCachePath;
        private readonly bool hashFileNames;
        private readonly Func<string> getCacheStamp;

        private bool isStampVerified;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class.
        /// </summary>
        /// <param name="cachePathFunc">Function to get cache path.</param>
        public CacheManager(Func<string> cachePathFunc)
            : this(cachePathFunc, false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class with option to hash filenames.
        /// </summary>
        /// <param name="cachePathFunc">Function to get cache path.</param>
        /// <param name="hashFileNames">Whether to hash file names when storing/looking up files.</param>
        public CacheManager(Func<string> cachePathFunc, bool hashFileNames = false)
            : this(cachePathFunc, hashFileNames, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class with a stamp identifying the
        /// layout of the entries it stores.
        /// </summary>
        /// <param name="cachePathFunc">Function to get cache path.</param>
        /// <param name="hashFileNames">Whether to hash file names when storing/looking up files.</param>
        /// <param name="cacheStampFunc">Function to get the stamp the stored entries must match, or null to never discard entries by stamp. On the first access after the stamp changes, every entry in the folder is deleted.</param>
        public CacheManager(Func<string> cachePathFunc, bool hashFileNames, Func<string> cacheStampFunc)
        {
            getCachePath = cachePathFunc;
            this.hashFileNames = hashFileNames;
            getCacheStamp = cacheStampFunc;
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
                DiscardEntriesOnStampMismatch();
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
                DiscardEntriesOnStampMismatch();
                var resolved = ResolveExistingFileName(fileName);
                var file = Path.Combine(CachePath, resolved);
                var data = File.ReadAllText(file);
                File.SetLastAccessTimeUtc(file, DateTime.UtcNow);
                return data;
            }
        }

        /// <summary>
        /// Save binary data to cache, writing it straight into the cache file instead of buffering the whole
        /// entry in memory first.
        /// </summary>
        /// <param name="fileName">File name to save.</param>
        /// <param name="writeAction">Action writing the entry to the given (still open) stream.</param>
        internal void SaveBinary(string fileName, Action<Stream> writeAction)
        {
            lock (lockObject)
            {
                DiscardEntriesOnStampMismatch();
                Directory.CreateDirectory(CachePath);
                var target = MapFileNameForSave(fileName);
                using (var stream = File.Create(Path.Combine(CachePath, target)))
                {
                    writeAction(stream);
                }
            }
        }

        /// <summary>
        /// Load binary data from cache, reading it straight from the cache file instead of buffering the whole
        /// entry in memory first.
        /// </summary>
        /// <typeparam name="T">Type read from the entry.</typeparam>
        /// <param name="fileName">File name to load.</param>
        /// <param name="readAction">Function reading the entry from the given (still open) stream.</param>
        /// <returns>Value returned by <paramref name="readAction"/>.</returns>
        internal T LoadBinary<T>(string fileName, Func<Stream, T> readAction)
        {
            lock (lockObject)
            {
                DiscardEntriesOnStampMismatch();
                var resolved = ResolveExistingFileName(fileName);
                var file = Path.Combine(CachePath, resolved);
                T result;
                using (var stream = File.OpenRead(file))
                {
                    result = readAction(stream);
                }
                File.SetLastAccessTimeUtc(file, DateTime.UtcNow);
                return result;
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
                DiscardEntriesOnStampMismatch();
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
                DiscardEntriesOnStampMismatch();
                var resolved = ResolveExistingFileName(fileName);
                var file = Path.Combine(CachePath, resolved);
                File.Copy(file, destPath, true);
                File.SetLastAccessTimeUtc(file, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Check whether the file exists in cache.
        /// </summary>
        /// <param name="fileName">File name to check.</param>
        /// <returns>true when the file exists.</returns>
        internal bool Exists(string fileName)
        {
            lock (lockObject)
            {
                DiscardEntriesOnStampMismatch();
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
        }

        /// <summary>
        /// Clear cache.
        /// </summary>
        internal void Clear()
        {
            lock (lockObject)
            {
                DiscardEntriesOnStampMismatch();
                foreach (var file in GetEntryFiles())
                {
                    file.Delete();
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
            lock (lockObject)
            {
                DiscardEntriesOnStampMismatch();
                var files = GetEntryFiles()
                    .OrderByDescending(f => f.LastAccessTime)
                    .ToArray();
                ulong size = 0;
                foreach (var file in files)
                {
                    size += (ulong)file.Length;
                    if (size > totalSize)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            // Opportunistic maintenance, not something a conversion or a preview should fail
                            // over: an entry that cannot be deleted right now (e.g. locked by another process)
                            // is simply left for the next trim.
                            Logger.LogWarning($"Failed to evict cache file {file.Name}. {e.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the cache entry files, excluding the bookkeeping stamp file.
        /// </summary>
        /// <returns>Cache entry files, or an empty array when the cache folder does not exist yet.</returns>
        private FileInfo[] GetEntryFiles()
        {
            if (!Directory.Exists(CachePath))
            {
                return new FileInfo[0];
            }
            return new DirectoryInfo(CachePath).GetFiles()
                .Where(f => f.Name != StampFileName)
                .ToArray();
        }

        /// <summary>
        /// Deletes every entry in the cache folder when its stamp file does not match the current stamp, then
        /// records the current stamp. Runs at most once per instance (i.e. once per domain reload for
        /// <see cref="Texture"/>) and does nothing when the manager has no stamp function.
        /// Callers must hold <see cref="lockObject"/>.
        /// </summary>
        private void DiscardEntriesOnStampMismatch()
        {
            if (getCacheStamp == null || isStampVerified)
            {
                return;
            }

            // Set upfront so a failure below is not retried on every single cache access afterwards.
            isStampVerified = true;

            try
            {
                var expected = getCacheStamp();
                var stampPath = Path.Combine(CachePath, StampFileName);
                if (File.Exists(stampPath) && File.ReadAllText(stampPath) == expected)
                {
                    return;
                }

                var files = GetEntryFiles();
                var deleted = 0;
                foreach (var file in files)
                {
                    try
                    {
                        file.Delete();
                        deleted++;
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning($"Failed to delete outdated cache file {file.Name}. {e.Message}");
                    }
                }

                if (deleted > 0)
                {
                    Logger.Log($"Discarded {deleted} outdated cache file(s) in {CachePath} because they were written for a different version or format ({expected} expected).");
                }

                Directory.CreateDirectory(CachePath);
                File.WriteAllText(stampPath, expected);
            }
            catch (Exception e)
            {
                // Cache bookkeeping must never break the operation that happened to touch the cache first.
                Logger.LogException(e);
            }
        }

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
    }
}
