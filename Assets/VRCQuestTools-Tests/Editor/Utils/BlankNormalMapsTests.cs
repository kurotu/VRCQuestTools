using NUnit.Framework;
using UnityEditor;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for blank normal maps.
    /// </summary>
    public class BlankNormalMapsTests
    {
        private static readonly int[] Sizes = { 256, 512, 1024, 2048 };
        private static readonly TextureImporterFormat[] Formats = new[]
        {
            TextureImporterFormat.ASTC_4x4,
            TextureImporterFormat.ASTC_5x5,
            TextureImporterFormat.ASTC_6x6,
            TextureImporterFormat.ASTC_8x8,
            TextureImporterFormat.ASTC_10x10,
            TextureImporterFormat.ASTC_12x12,
        };

        /// <summary>
        /// Tests that normal maps have the correct Android and iOS overrides for all combinations of size and format.
        /// </summary>
        /// <param name="size">テクスチャサイズ.</param>
        /// <param name="format">期待されるフォーマット.</param>
        [Test]
        [Combinatorial]
        public void NormalMap_Has_Correct_Android_And_iOS_Overrides(
            [ValueSource(nameof(Sizes))] int size,
            [ValueSource(nameof(Formats))] TextureImporterFormat format)
        {
            string suffix = format.ToString();
            string assetPath = $"Packages/com.github.kurotu.vrc-quest-tools/Assets/BlankNormalMaps/VQT_Normal_{size}px_{suffix}.png";
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Assert.Fail($"{assetPath} does not exist or could not be imported");
            }

            // Android
            var androidSettings = importer.GetPlatformTextureSettings("Android");
            Assert.IsTrue(androidSettings.overridden, $"{assetPath} Android override not set");
            Assert.AreEqual(size, androidSettings.maxTextureSize, $"{assetPath} Android maxTextureSize");
            Assert.AreEqual(format, androidSettings.format, $"{assetPath} Android format");
            Assert.IsTrue(importer.textureType == TextureImporterType.NormalMap, $"{assetPath} is not set as NormalMap");

            // iOS
            var iosSettings = importer.GetPlatformTextureSettings("iPhone");
            Assert.IsTrue(iosSettings.overridden, $"{assetPath} iOS override not set");
            Assert.AreEqual(size, iosSettings.maxTextureSize, $"{assetPath} iOS maxTextureSize");
            Assert.AreEqual(format, iosSettings.format, $"{assetPath} iOS format");
            Assert.IsTrue(importer.textureType == TextureImporterType.NormalMap, $"{assetPath} is not set as NormalMap");
        }
    }
}
