using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Test <see cref="VRCQuestToolsSettings"/>.
    /// </summary>
    public class VRCQuestToolsSettingsTests
    {
        private static void ResetI18nField()
        {
            var field = typeof(VRCQuestToolsSettings).GetField("i18n", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, null);
        }
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
            var originalSize = VRCQuestToolsSettings.TextureCacheSize;

            try
            {
                // Reset to defaults
                VRCQuestToolsSettings.ResetPreferences();

                var expectedSize = 128UL * 1024 * 1024;
                Assert.AreEqual(expectedSize, VRCQuestToolsSettings.TextureCacheSize);
            }
            finally
            {
                VRCQuestToolsSettings.TextureCacheSize = originalSize;
            }
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

        [Test]
        public void I18nResource_ReturnsNonNull()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            Assert.IsNotNull(i18n);
        }

        [Test]
        public void DisplayLanguage_GetReturnsValidEnum()
        {
            var language = VRCQuestToolsSettings.DisplayLanguage;
            Assert.IsTrue(System.Enum.IsDefined(typeof(DisplayLanguage), language));
        }

        [Test]
        public void DisplayLanguage_SetAndGet_RoundTrips()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.English;
                Assert.AreEqual(DisplayLanguage.English, VRCQuestToolsSettings.DisplayLanguage);

                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Japanese;
                Assert.AreEqual(DisplayLanguage.Japanese, VRCQuestToolsSettings.DisplayLanguage);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
            }
        }

        [Test]
        public void IsShowUnitySettingsWindowOnLoadEnabled_GetReturnsValue()
        {
            var value = VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled;
            Assert.IsInstanceOf<bool>(value);
        }

        [Test]
        public void LatestVersionCache_GetReturnsNonNull()
        {
            var version = VRCQuestToolsSettings.LatestVersionCache;
            Assert.IsNotNull(version);
        }

        [Test]
        public void LatestVersionCache_SetAndGet_RoundTrips()
        {
            var original = VRCQuestToolsSettings.LatestVersionCache;
            try
            {
                var version = new SemVer("1.2.3");
                VRCQuestToolsSettings.LatestVersionCache = version;
                var cached = VRCQuestToolsSettings.LatestVersionCache;
                Assert.AreEqual(version.ToString(), cached.ToString());
            }
            finally
            {
                VRCQuestToolsSettings.LatestVersionCache = original;
            }
        }

        [Test]
        public void SkippedVersion_GetReturnsNonNull()
        {
            var version = VRCQuestToolsSettings.SkippedVersion;
            Assert.IsNotNull(version);
        }

        [Test]
        public void SkippedVersion_SetAndGet_RoundTrips()
        {
            var original = VRCQuestToolsSettings.SkippedVersion;
            try
            {
                var version = new SemVer("2.0.0");
                VRCQuestToolsSettings.SkippedVersion = version;
                var skipped = VRCQuestToolsSettings.SkippedVersion;
                Assert.AreEqual(version.ToString(), skipped.ToString());
            }
            finally
            {
                VRCQuestToolsSettings.SkippedVersion = original;
            }
        }

        [Test]
        public void LastVersionCheckDateTime_SetAndGet_RoundTrips()
        {
            var original = VRCQuestToolsSettings.LastVersionCheckDateTime;
            try
            {
                var now = System.DateTime.UtcNow;
                VRCQuestToolsSettings.LastVersionCheckDateTime = now;
                var stored = VRCQuestToolsSettings.LastVersionCheckDateTime;
                Assert.That((stored - now).TotalSeconds, Is.LessThan(1.0).And.GreaterThan(-1.0));
            }
            finally
            {
                VRCQuestToolsSettings.LastVersionCheckDateTime = original;
            }
        }

        [Test]
        public void IsValidationAutomatorEnabled_GetReturnsValue()
        {
            var value = VRCQuestToolsSettings.IsValidationAutomatorEnabled;
            Assert.IsInstanceOf<bool>(value);
        }

        [Test]
        public void IsValidationAutomatorEnabled_SetAndGet_RoundTrips()
        {
            var original = VRCQuestToolsSettings.IsValidationAutomatorEnabled;
            try
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsValidationAutomatorEnabled);

                VRCQuestToolsSettings.IsValidationAutomatorEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsValidationAutomatorEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = original;
            }
        }

        [Test]
        public void IsCheckTextureFormatOnStandaloneEnabled_GetReturnsValue()
        {
            var value = VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled;
            Assert.IsInstanceOf<bool>(value);
        }

        [Test]
        public void IsCheckTextureFormatOnStandaloneEnabled_SetAndGet_RoundTrips()
        {
            var original = VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled;
            try
            {
                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled);

                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = original;
            }
        }

        [Test]
        public void ResetPreferences_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCQuestToolsSettings.ResetPreferences());
        }

        [Test]
        public void I18nResource_WhenFieldIsNull_InitializesFromGetI18n()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.English;
                ResetI18nField();
                var i18n = VRCQuestToolsSettings.I18nResource;
                Assert.IsNotNull(i18n);
                Assert.IsInstanceOf<I18n.I18nEnglish>(i18n);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
                ResetI18nField();
            }
        }

        [Test]
        public void I18nResource_Japanese_ReturnsI18nJapanese()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Japanese;
                ResetI18nField();
                var i18n = VRCQuestToolsSettings.I18nResource;
                Assert.IsInstanceOf<I18n.I18nJapanese>(i18n);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
                ResetI18nField();
            }
        }

        [Test]
        public void I18nResource_Russian_ReturnsI18nRussian()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Russian;
                ResetI18nField();
                var i18n = VRCQuestToolsSettings.I18nResource;
                Assert.IsInstanceOf<I18n.I18nRussian>(i18n);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
                ResetI18nField();
            }
        }

        [Test]
        public void I18nResource_Auto_ReturnsNonNull()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Auto;
                ResetI18nField();
                var i18n = VRCQuestToolsSettings.I18nResource;
                Assert.IsNotNull(i18n);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
                ResetI18nField();
            }
        }
    }
}
