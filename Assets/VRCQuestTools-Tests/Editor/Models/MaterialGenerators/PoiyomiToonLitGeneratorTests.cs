// <copyright file="PoiyomiToonLitGeneratorTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for main texture UV tiling preservation in Poiyomi ToonLit conversion.
    /// Requires Poiyomi Toon (com.poiyomi.toon) to be installed; tests are skipped otherwise.
    /// </summary>
    public class PoiyomiToonLitGeneratorTests
    {
        private const string PoiyomiShaderName = ".poiyomi/Poiyomi Toon";

        /// <summary>
        /// Test that ToonLitGenerator without baking preserves main texture UV scale and offset
        /// from a Poiyomi material with non-default UV tiling stored in _MainTex_ST.
        /// </summary>
        [Test]
        public void ConvertToToonLit_NonBake_PreservesMainTextureScaleAndOffset()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Lit");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            using (var sourceMaterial = DisposableObject.New(new Material(shader)))
            {
                var expectedScale = new Vector2(2.5f, 3.0f);
                var expectedOffset = new Vector2(0.1f, 0.2f);
                sourceMaterial.Object.SetVector("_MainTex_ST", new Vector4(expectedScale.x, expectedScale.y, expectedOffset.x, expectedOffset.y));

                var settings = new ToonLitConvertSettings
                {
                    generateQuestTextures = false,
                };

                var poiMat = new PoiyomiMaterial(sourceMaterial.Object);
                var generator = new ToonLitGenerator(settings);

                Material resultMat = null;
                generator.GenerateMaterial(poiMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
                {
                    resultMat = mat;
                }).WaitForCompletion();
                using (var resultMaterial = DisposableObject.New(resultMat))
                {
                    Assert.IsNotNull(resultMat, "Generated material should not be null.");
                    Assert.AreEqual(expectedScale, resultMat.mainTextureScale, "Main texture scale should be preserved.");
                    Assert.AreEqual(expectedOffset, resultMat.mainTextureOffset, "Main texture offset should be preserved.");
                }
            }
        }

        /// <summary>
        /// Test that ToonLitGenerator without baking preserves the default (identity) main texture UV.
        /// </summary>
        [Test]
        public void ConvertToToonLit_NonBake_PreservesDefaultMainTextureST()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Lit");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            using (var sourceMaterial = DisposableObject.New(new Material(shader)))
            {
                var settings = new ToonLitConvertSettings
                {
                    generateQuestTextures = false,
                };

                var poiMat = new PoiyomiMaterial(sourceMaterial.Object);
                var generator = new ToonLitGenerator(settings);

                Material resultMat = null;
                generator.GenerateMaterial(poiMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
                {
                    resultMat = mat;
                }).WaitForCompletion();
                using (var resultMaterial = DisposableObject.New(resultMat))
                {
                    Assert.IsNotNull(resultMat, "Generated material should not be null.");
                    Assert.AreEqual(Vector2.one, resultMat.mainTextureScale, "Default main texture scale should be (1,1).");
                    Assert.AreEqual(Vector2.zero, resultMat.mainTextureOffset, "Default main texture offset should be (0,0).");
                }
            }
        }

        /// <summary>
        /// Test that ToonLitGenerator with baking preserves main texture UV scale and offset
        /// on the output material. The baked texture captures the raw texture appearance
        /// while the UV tiling is carried by the output material's mainTextureScale/Offset.
        /// </summary>
        [Test]
        public void ConvertToToonLit_WithBake_PreservesMainTextureScaleAndOffset()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Lit");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            var mainTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(TestUtils.TexturesFolder + "/albedo_1024px_png.png");
            if (mainTexture == null)
            {
                Assert.Ignore("albedo_1024px_png.png fixture not found.");
                return;
            }

            using (var sourceMaterial = DisposableObject.New(new Material(shader)))
            {
                var expectedScale = new Vector2(2.5f, 3.0f);
                var expectedOffset = new Vector2(0.1f, 0.2f);
                sourceMaterial.Object.mainTexture = mainTexture;
                sourceMaterial.Object.SetVector("_MainTex_ST", new Vector4(expectedScale.x, expectedScale.y, expectedOffset.x, expectedOffset.y));

                var settings = new ToonLitConvertSettings
                {
                    generateQuestTextures = true,
                };

                var poiMat = new PoiyomiMaterial(sourceMaterial.Object);
                var generator = new ToonLitGenerator(settings);

                Material resultMat = null;
                generator.GenerateMaterial(poiMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
                {
                    resultMat = mat;
                }).WaitForCompletion();
                using (var resultMainTex = DisposableObject.New(resultMat != null ? resultMat.mainTexture : null))
                using (var resultMaterial = DisposableObject.New(resultMat))
                {
                    Assert.IsNotNull(resultMat, "Generated material should not be null.");
                    Assert.AreEqual(expectedScale, resultMat.mainTextureScale, "Main texture scale should be preserved after baking.");
                    Assert.AreEqual(expectedOffset, resultMat.mainTextureOffset, "Main texture offset should be preserved after baking.");
                }
            }
        }

        /// <summary>
        /// Test that ToonLitGenerator with baking does NOT embed UV tiling into the baked texture.
        /// Two Poiyomi materials with the same source texture but different UV tiling (stored in
        /// _MainTex_ST) should produce nearly identical baked textures. UV tiling is applied at
        /// runtime by the Toon Lit material via mainTextureScale, not embedded in the texture pixels.
        /// Embedding tiling in both the texture AND mainTextureScale causes double-tiling at runtime.
        /// </summary>
        [Test]
        public void ConvertToToonLit_WithBake_BakedTextureDoesNotEmbedUVTiling()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            TestUtils.AssertIgnoreOnMissingShader("VRChat/Mobile/Toon Lit");

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found.");

            var mainTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(TestUtils.TexturesFolder + "/albedo_1024px_png.png");
            if (mainTexture == null)
            {
                Assert.Ignore("albedo_1024px_png.png fixture not found.");
                return;
            }

            using (var mat11 = DisposableObject.New(new Material(shader)))
            using (var mat22 = DisposableObject.New(new Material(shader)))
            {
                mat11.Object.mainTexture = mainTexture;
                mat11.Object.SetVector("_MainTex_ST", new Vector4(1f, 1f, 0f, 0f));

                mat22.Object.mainTexture = mainTexture;
                mat22.Object.SetVector("_MainTex_ST", new Vector4(2f, 2f, 0f, 0f));

                var settings = new ToonLitConvertSettings
                {
                    generateQuestTextures = true,
                };

                var poiMat11 = new PoiyomiMaterial(mat11.Object);
                var gen11 = new ToonLitGenerator(settings);
                var poiMat22 = new PoiyomiMaterial(mat22.Object);
                var gen22 = new ToonLitGenerator(settings);

                Material resultMat11 = null;
                gen11.GenerateMaterial(poiMat11, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat11 = mat; }).WaitForCompletion();
                Material resultMat22 = null;
                gen22.GenerateMaterial(poiMat22, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat22 = mat; }).WaitForCompletion();

                using (var result11Tex = DisposableObject.New(resultMat11 != null ? resultMat11.mainTexture : null))
                using (var result11 = DisposableObject.New(resultMat11))
                using (var result22Tex = DisposableObject.New(resultMat22 != null ? resultMat22.mainTexture : null))
                using (var result22 = DisposableObject.New(resultMat22))
                {
                    Assert.IsNotNull(resultMat11, "ToonLit result for scale (1,1) should not be null.");
                    Assert.IsNotNull(resultMat22, "ToonLit result for scale (2,2) should not be null.");

                    Assert.AreEqual(Vector2.one, resultMat11.mainTextureScale, "Scale (1,1) source should produce (1,1) on output material.");
                    Assert.AreEqual(new Vector2(2f, 2f), resultMat22.mainTextureScale, "Scale (2,2) source should produce (2,2) on output material.");

                    var bakedTex11 = resultMat11.mainTexture as Texture2D;
                    var bakedTex22 = resultMat22.mainTexture as Texture2D;

                    if (bakedTex11 != null && bakedTex22 != null)
                    {
                        var diff = TestUtils.MaxDifference(bakedTex11, bakedTex22);
                        var message = "Baked textures should be nearly identical regardless of UV tiling stored in _MainTex_ST. " +
                            "UV tiling must not be embedded in the baked texture to prevent double-tiling at runtime. " +
                            $"Actual max difference: {diff:F4}.";
                        Assert.Less(diff, 0.01f, message);
                    }
                    else
                    {
                        Assert.Ignore("Baked textures are not available as Texture2D for comparison (may be RenderTexture on this platform).");
                    }
                }
            }
        }
    }
}
