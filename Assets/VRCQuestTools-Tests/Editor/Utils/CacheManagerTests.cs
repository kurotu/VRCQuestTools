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
        /// Test CopyToCache copies file into cache.
        /// </summary>
        [Test]
        public void CopyToCache_CopiesFile()
        {
            var srcFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(srcFile, "source data");
                testCacheManager.CopyToCache(srcFile, "copied.txt");
                Assert.IsTrue(testCacheManager.Exists("copied.txt"));
                Assert.AreEqual("source data", testCacheManager.LoadString("copied.txt"));
            }
            finally
            {
                File.Delete(srcFile);
            }
        }

        /// <summary>
        /// Test CopyFromCache copies file from cache.
        /// </summary>
        [Test]
        public void CopyFromCache_CopiesFile()
        {
            testCacheManager.Save("cached.txt", "cached data");
            var destFile = Path.GetTempFileName();
            try
            {
                testCacheManager.CopyFromCache("cached.txt", destFile);
                Assert.AreEqual("cached data", File.ReadAllText(destFile));
            }
            finally
            {
                File.Delete(destFile);
            }
        }

        /// <summary>
        /// Test Clear with size limit removes old files.
        /// </summary>
        [Test]
        public void ClearWithSize_KeepsFilesWithinLimit()
        {
            testCacheManager.Save("small1.txt", new string('a', 100));
            testCacheManager.Save("small2.txt", new string('b', 100));
            testCacheManager.Save("small3.txt", new string('c', 100));

            testCacheManager.Clear(200UL);

            int existCount = 0;
            if (testCacheManager.Exists("small1.txt")) existCount++;
            if (testCacheManager.Exists("small2.txt")) existCount++;
            if (testCacheManager.Exists("small3.txt")) existCount++;
            Assert.Less(existCount, 3);
        }

        /// <summary>
        /// Test Clear when directory does not exist.
        /// </summary>
        [Test]
        public void Clear_WhenDirectoryDoesNotExist_DoesNotThrow()
        {
            var noDir = Path.Combine(Path.GetTempPath(), "nonexistent_" + System.Guid.NewGuid().ToString("N"));
            var mgr = new CacheManager(() => noDir);
            Assert.DoesNotThrow(() => mgr.Clear());
        }

        /// <summary>
        /// Test Clear with size when directory does not exist.
        /// </summary>
        [Test]
        public void ClearWithSize_WhenDirectoryDoesNotExist_DoesNotThrow()
        {
            var noDir = Path.Combine(Path.GetTempPath(), "nonexistent_" + System.Guid.NewGuid().ToString("N"));
            var mgr = new CacheManager(() => noDir);
            Assert.DoesNotThrow(() => mgr.Clear(1024UL));
        }

        /// <summary>
        /// Test Exists returns false for nonexistent file.
        /// </summary>
        [Test]
        public void Exists_WhenNotSaved_ReturnsFalse()
        {
            Assert.IsFalse(testCacheManager.Exists("nonexistent.txt"));
        }
    }
}
