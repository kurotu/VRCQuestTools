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
            testCacheManager = new CacheManager("Local\\VRCQuestTools-Test-Mutex", () => testCacheFolder);
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
        /// Test that cache uses mutex for thread-safety.
        /// </summary>
        [Test]
        public void MutexIsThreadSafe()
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
        /// Test that texture cache manager uses project-specific mutex.
        /// </summary>
        [Test]
        public void TextureCacheManagerUsesLocalMutex()
        {
            var mutex = CacheManager.Texture.CreateMutex();
            Assert.IsNotNull(mutex);
            
            // The mutex should be creatable without throwing an exception
            mutex.Dispose();
        }
    }
}
