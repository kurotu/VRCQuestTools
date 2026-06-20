using System.IO;
using System.Threading;
using NUnit.Framework;

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
    }
}
