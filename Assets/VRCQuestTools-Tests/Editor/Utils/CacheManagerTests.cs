using System.IO;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Test <see cref="CacheManager"/>.
    /// </summary>
    public class CacheManagerTests
    {
        private string testCacheFolder;
        private CacheManager testCacheManager;

        /// <summary>
        /// Setup test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            testCacheFolder = Path.Combine(Path.GetTempPath(), "VRCQuestTools_Test_Cache");
            if (Directory.Exists(testCacheFolder))
            {
                Directory.Delete(testCacheFolder, true);
            }
            Directory.CreateDirectory(testCacheFolder);
            testCacheManager = new CacheManager(() => testCacheFolder);
        }

        /// <summary>
        /// Cleanup test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testCacheFolder))
            {
                Directory.Delete(testCacheFolder, true);
            }
        }

        /// <summary>
        /// Test that cache can save and load data.
        /// </summary>
        [Test]
        public void SaveAndLoadData()
        {
            var testFileName = "test.txt";
            var testData = "Hello, World!";

            testCacheManager.Save(testFileName, testData);
            Assert.IsTrue(testCacheManager.Exists(testFileName));

            var loadedData = testCacheManager.LoadString(testFileName);
            Assert.AreEqual(testData, loadedData);
        }

        /// <summary>
        /// Test that cache can clear all files.
        /// </summary>
        [Test]
        public void ClearCache()
        {
            var testFileName1 = "test1.txt";
            var testFileName2 = "test2.txt";

            testCacheManager.Save(testFileName1, "data1");
            testCacheManager.Save(testFileName2, "data2");

            Assert.IsTrue(testCacheManager.Exists(testFileName1));
            Assert.IsTrue(testCacheManager.Exists(testFileName2));

            testCacheManager.Clear();

            Assert.IsFalse(testCacheManager.Exists(testFileName1));
            Assert.IsFalse(testCacheManager.Exists(testFileName2));
        }

        /// <summary>
        /// Test that binary entries survive a save/load round trip without going through a string.
        /// </summary>
        [Test]
        public void SaveAndLoadBinary()
        {
            var testFileName = "binary.bin";
            var testData = new byte[] { 0x00, 0x01, 0xFE, 0xFF, 0x7F, 0x80 };

            testCacheManager.SaveBinary(testFileName, stream => stream.Write(testData, 0, testData.Length));
            Assert.IsTrue(testCacheManager.Exists(testFileName));

            var loaded = testCacheManager.LoadBinary(testFileName, stream =>
            {
                var buffer = new byte[testData.Length];
                var read = stream.Read(buffer, 0, buffer.Length);
                Assert.AreEqual(testData.Length, read);
                Assert.AreEqual(-1, stream.ReadByte(), "Entry should contain exactly the written bytes.");
                return buffer;
            });

            Assert.AreEqual(testData, loaded);
        }

        /// <summary>
        /// Test that eviction removes the least recently accessed entries until the total fits the given size.
        /// </summary>
        [Test]
        public void ClearToFitSizeEvictsLeastRecentlyAccessed()
        {
            var data = new string('x', 1000);
            var names = new[] { "oldest.txt", "middle.txt", "newest.txt" };
            for (int i = 0; i < names.Length; i++)
            {
                testCacheManager.Save(names[i], data);
                File.SetLastAccessTimeUtc(Path.Combine(testCacheFolder, names[i]), new System.DateTime(2020, 1, 1 + i, 0, 0, 0, System.DateTimeKind.Utc));
            }

            // Fits the two most recently accessed entries (2000 bytes), but not the third.
            testCacheManager.Clear(2500);

            Assert.IsFalse(testCacheManager.Exists("oldest.txt"), "The least recently accessed entry should be evicted.");
            Assert.IsTrue(testCacheManager.Exists("middle.txt"));
            Assert.IsTrue(testCacheManager.Exists("newest.txt"));
        }

        /// <summary>
        /// Test that entries written under a different stamp (e.g. by another tool version, or in another cache
        /// entry format) are discarded on first access, since their file names are hashed and cannot be
        /// recognized individually.
        /// </summary>
        [Test]
        public void StampMismatchDiscardsEntries()
        {
            var name = "stamped.bin";
            var oldStampManager = new CacheManager(() => testCacheFolder, true, () => "stamp-a");
            oldStampManager.Save(name, "data");
            Assert.IsTrue(oldStampManager.Exists(name));

            // A fresh instance stands in for the next domain reload, this time expecting another stamp.
            var newStampManager = new CacheManager(() => testCacheFolder, true, () => "stamp-b");
            Assert.IsFalse(newStampManager.Exists(name), "Entries written under the previous stamp should be discarded.");
        }

        /// <summary>
        /// Test that entries are kept when the recorded stamp still matches, and that clearing the cache does
        /// not lose the recorded stamp (which would make the next access discard perfectly valid entries).
        /// </summary>
        [Test]
        public void StampMatchKeepsEntries()
        {
            var name = "stamped.bin";
            var manager = new CacheManager(() => testCacheFolder, true, () => "stamp-a");
            manager.Save(name, "data");

            var sameStampManager = new CacheManager(() => testCacheFolder, true, () => "stamp-a");
            Assert.IsTrue(sameStampManager.Exists(name), "Entries written under the same stamp should be kept.");

            sameStampManager.Clear();
            sameStampManager.Save(name, "data again");

            var afterClearManager = new CacheManager(() => testCacheFolder, true, () => "stamp-a");
            Assert.IsTrue(afterClearManager.Exists(name), "Clearing the cache should not lose the recorded stamp.");
        }

        /// <summary>
        /// Test that cache uses lock for thread-safety.
        /// </summary>
        [Test]
        public void LockIsThreadSafe()
        {
            var testFileName = "thread_test.txt";
            var writeCount = 10;
            var threads = new Thread[writeCount];

            for (int i = 0; i < writeCount; i++)
            {
                var index = i;
                threads[i] = new Thread(() =>
                {
                    testCacheManager.Save(testFileName, $"data_{index}");
                });
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            // File should exist and contain one of the written values
            Assert.IsTrue(testCacheManager.Exists(testFileName));
            var data = testCacheManager.LoadString(testFileName);
            Assert.IsTrue(data.StartsWith("data_"));
        }

        /// <summary>
        /// Test that texture cache manager exists and is accessible.
        /// </summary>
        [Test]
        public void TextureCacheManagerExists()
        {
            Assert.IsNotNull(CacheManager.Texture);
        }

        /// <summary>
        /// Test that saving under hashing stores the file under a hashed filename on disk, while Save/LoadString/Exists accept the original cache key.
        /// </summary>
        [Test]
        public void SaveAndLoadData_WithHashing()
        {
            var longName = "very/long/path/that/should/be/hashed_by_the_cache_manager_for_test_purposes.txt";
            var testData = "HashedData";
            var hashedManager = new CacheManager(() => testCacheFolder, true);

            hashedManager.Save(longName, testData);

            var expectedHashed = KRT.VRCQuestTools.Utils.CacheUtility.HashFileName(longName);
            Assert.IsTrue(File.Exists(Path.Combine(testCacheFolder, expectedHashed)), "Hashed file should exist in cache folder.");
            Assert.IsTrue(hashedManager.Exists(longName));

            var loaded = hashedManager.LoadString(longName);
            Assert.AreEqual(testData, loaded);
        }

        /// <summary>
        /// Test fallback: if an original non-hashed file exists (older cache), the loader should still find and load it.
        /// </summary>
        [Test]
        public void LoadFallsBackToOriginalIfHashedMissing()
        {
            var name = "legacy_cache_name.json";
            var legacyData = "legacy";
            // Create original file manually to simulate old cache.
            File.WriteAllText(Path.Combine(testCacheFolder, name), legacyData);

            var hashedManager = new CacheManager(() => testCacheFolder, true);
            Assert.IsTrue(hashedManager.Exists(name));
            var loaded = hashedManager.LoadString(name);
            Assert.AreEqual(legacyData, loaded);
        }

        /// <summary>
        /// Test that CopyToCache stores the file under a hashed filename and CopyFromCache retrieves it using the original cache key.
        /// </summary>
        [Test]
        public void CopyToCacheAndFromCache_WithHashing()
        {
            var cacheKey = "copy/round/trip/test_file.png";
            var srcContent = "source file content";
            var srcPath = Path.Combine(Path.GetTempPath(), "vqt_copy_src_test.txt");
            var destPath = Path.Combine(Path.GetTempPath(), "vqt_copy_dest_test.txt");

            try
            {
                File.WriteAllText(srcPath, srcContent);

                var hashedManager = new CacheManager(() => testCacheFolder, true);
                hashedManager.CopyToCache(srcPath, cacheKey);

                var expectedHashed = CacheUtility.HashFileName(cacheKey);
                Assert.IsTrue(File.Exists(Path.Combine(testCacheFolder, expectedHashed)), "Hashed file should exist in cache folder.");
                Assert.IsTrue(hashedManager.Exists(cacheKey));

                hashedManager.CopyFromCache(cacheKey, destPath);
                Assert.IsTrue(File.Exists(destPath));
                Assert.AreEqual(srcContent, File.ReadAllText(destPath));
            }
            finally
            {
                if (File.Exists(srcPath))
                {
                    File.Delete(srcPath);
                }
                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }
            }
        }

        /// <summary>
        /// Test fallback: CopyFromCache falls back to the original non-hashed filename when the hashed file is missing.
        /// </summary>
        [Test]
        public void CopyFromCacheFallsBackToOriginalIfHashedMissing()
        {
            var name = "legacy_copy_file.png";
            var legacyContent = "legacy copy content";
            var destPath = Path.Combine(Path.GetTempPath(), "vqt_legacy_copy_dest.txt");

            try
            {
                // Create original file manually to simulate old cache.
                File.WriteAllText(Path.Combine(testCacheFolder, name), legacyContent);

                var hashedManager = new CacheManager(() => testCacheFolder, true);
                Assert.IsTrue(hashedManager.Exists(name));

                hashedManager.CopyFromCache(name, destPath);
                Assert.IsTrue(File.Exists(destPath));
                Assert.AreEqual(legacyContent, File.ReadAllText(destPath));
            }
            finally
            {
                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }
            }
        }

        /// <summary>
        /// Test that a texture cache entry survives a binary write/read round trip with its attributes and its
        /// raw bytes intact.
        /// </summary>
        [Test]
        public void TextureCache_BinaryRoundTrip()
        {
            Texture2D source = null;
            Texture2D restored = null;
            try
            {
                source = new Texture2D(8, 8, TextureFormat.RGBA32, false, false);
                var pixels = new Color32[8 * 8];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32((byte)i, (byte)(255 - i), (byte)(i * 2), 255);
                }
                source.SetPixels32(pixels);
                source.Apply();

                var expectedBytes = source.GetRawTextureData();

                CacheUtility.TextureCache readBack;
                using (var stream = new MemoryStream())
                {
                    new CacheUtility.TextureCache(source, false, false, BuildTarget.Android).WriteTo(stream);
                    stream.Position = 0;
                    readBack = CacheUtility.TextureCache.ReadFrom(stream);
                }

                restored = readBack.ToTexture2D();
                Assert.AreEqual(source.width, restored.width);
                Assert.AreEqual(source.height, restored.height);
                Assert.AreEqual(source.format, restored.format);
                Assert.AreEqual(source.mipmapCount, restored.mipmapCount);
                Assert.AreEqual(expectedBytes, restored.GetRawTextureData());
            }
            finally
            {
                if (source != null)
                {
                    Object.DestroyImmediate(source);
                }
                if (restored != null)
                {
                    Object.DestroyImmediate(restored);
                }
            }
        }

        /// <summary>
        /// Test that a file which is not a texture cache entry is rejected instead of being interpreted as one.
        /// </summary>
        [Test]
        public void TextureCache_ReadFromRejectsForeignData()
        {
            using (var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
            {
                Assert.Throws<InvalidDataException>(() => CacheUtility.TextureCache.ReadFrom(stream));
            }
        }

        /// <summary>
        /// Test that TextureCache.ToTexture2D falls back to building the normal map container directly
        /// (instead of throwing) when the pre-baked blank normal map asset used as a container doesn't match
        /// the texture attributes recorded at cache-save time.
        ///
        /// This reproduces the Linux CI failure (NormalMapPreviewRenderingTests): the blank normal map
        /// asset's .meta only overrides the import format to ASTC for the Android/iOS platforms, so on a
        /// non-mobile active build target the asset is imported with a different format and its raw byte
        /// layout no longer matches what was recorded, which used to make LoadRawTextureData
        /// overread/underread the stored buffer.
        ///
        /// The mismatch is reproduced here via mip layout instead of format, which keeps the test independent
        /// of the running editor/CI's active build target: the blank asset always has mipmaps generated (its
        /// .meta has enableMipMap: 1, a generic, platform-independent import setting), so caching a
        /// non-mipmapped source texture always mismatches it, regardless of which format the asset actually
        /// imported as.
        /// </summary>
        [Test]
        public void TextureCache_NormalMap_FallsBackWhenContainerAssetMismatches()
        {
            var folder = AssetDatabase.GUIDToAssetPath("17d9dbede49f19943a367a284154f9d4"); // Package/-/Assets/BlankNormalMaps
            var assetPath = Path.Combine(folder, "VQT_Normal_256px_ASTC_6x6.png").Replace('\\', '/');
            var blankAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Assert.IsNotNull(blankAsset, "Precondition: blank normal map asset should exist for this test to exercise the mismatch path.");
            Assert.IsTrue(blankAsset.mipmapCount > 1, "Precondition: blank normal map asset should have mipmaps so it mismatches a no-mip cached texture.");

            Texture2D source = null;
            Texture2D restored = null;
            try
            {
                source = new Texture2D(256, 256, TextureFormat.ASTC_6x6, false, true);
                var cache = new CacheUtility.TextureCache(source, true, true, BuildTarget.Android);

                Assert.DoesNotThrow(() => restored = cache.ToTexture2D(), "Restoring from cache should fall back to building the container directly instead of throwing when the blank asset container doesn't match.");
                Assert.IsNotNull(restored);
                Assert.AreEqual(256, restored.width);
                Assert.AreEqual(256, restored.height);
                Assert.AreEqual(TextureFormat.ASTC_6x6, restored.format);
                Assert.AreEqual(1, restored.mipmapCount, "Restored texture should follow the cached (no-mip) layout, proving the mismatched blank asset was not used as the container.");
            }
            finally
            {
                if (source != null)
                {
                    Object.DestroyImmediate(source);
                }
                if (restored != null)
                {
                    Object.DestroyImmediate(restored);
                }
            }
        }
    }
}
