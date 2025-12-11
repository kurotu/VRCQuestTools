using System.IO;
using NUnit.Framework;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Test <see cref="VRCQuestToolsSettings"/>.
    /// </summary>
    public class VRCQuestToolsSettingsTests
    {
        /// <summary>
        /// Test that texture cache folder is in Library directory.
        /// </summary>
        [Test]
        public void TextureCacheFolderIsInLibrary()
        {
            var cacheFolder = VRCQuestToolsSettings.TextureCacheFolder;
            Assert.IsTrue(cacheFolder.StartsWith("Library"), "Cache folder should be in Library directory");
            Assert.IsTrue(cacheFolder.Contains("VRCQuestTools"), "Cache folder should contain VRCQuestTools");
            Assert.IsTrue(cacheFolder.Contains("TextureCache"), "Cache folder should contain TextureCache");
        }

        /// <summary>
        /// Test that texture cache size can be set and retrieved from project settings.
        /// </summary>
        [Test]
        public void TextureCacheSizeCanBeSetAndRetrieved()
        {
            var originalSize = VRCQuestToolsSettings.TextureCacheSize;
            
            // Set a custom size
            var testSize = 256UL * 1024 * 1024; // 256MB
            VRCQuestToolsSettings.TextureCacheSize = testSize;
            
            // Verify it was set
            Assert.AreEqual(testSize, VRCQuestToolsSettings.TextureCacheSize);
            
            // Restore original size
            VRCQuestToolsSettings.TextureCacheSize = originalSize;
        }

        /// <summary>
        /// Test that default texture cache size is 128MB.
        /// </summary>
        [Test]
        public void DefaultTextureCacheSizeIs128MB()
        {
            // Reset to defaults
            VRCQuestToolsSettings.ResetPreferences();
            
            var expectedSize = 128UL * 1024 * 1024;
            Assert.AreEqual(expectedSize, VRCQuestToolsSettings.TextureCacheSize);
        }

        /// <summary>
        /// Test that texture cache folder path is read-only (no setter).
        /// </summary>
        [Test]
        public void TextureCacheFolderIsReadOnly()
        {
            var folder1 = VRCQuestToolsSettings.TextureCacheFolder;
            var folder2 = VRCQuestToolsSettings.TextureCacheFolder;
            
            // Should always return the same path
            Assert.AreEqual(folder1, folder2);
            
            // Should be a relative path
            Assert.IsFalse(Path.IsPathRooted(folder1), "Cache folder should be a relative path");
        }
    }
}
