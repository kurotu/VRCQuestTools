// <copyright file="LilToonToonLitGeneratorTests.cs" company="kurotu">
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
    /// Tests for main texture UV tiling preservation in ToonLit conversion.
    /// </summary>
    public class LilToonToonLitGeneratorTests
    {
        /// <summary>
        /// Test that ToonLitGenerator without baking preserves main texture UV scale and offset from lilToon source.
        /// </summary>
        [Test]
        public void ConvertToToonLit_NonBake_PreservesMainTextureScaleAndOffset()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon is not installed.");
                return;
            }

            var lilToonVersion = AssetUtility.LilToonVersion;
            var requiredVersion = new SemVer(1, 10, 0);
            var breakingVersion = new SemVer(3, 0, 0);
            if (lilToonVersion < requiredVersion || lilToonVersion >= breakingVersion)
            {
                Assert.Ignore($"lilToon version {lilToonVersion} is not supported.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            var sourceMaterial = new Material(lilToonShader);
            try
            {
                var expectedScale = new Vector2(2.5f, 3.0f);
                var expectedOffset = new Vector2(0.1f, 0.2f);
                sourceMaterial.SetTextureScale("_MainTex", expectedScale);
                sourceMaterial.SetTextureOffset("_MainTex", expectedOffset);

                var settings = new ToonLitConvertSettings
                {
                    generateQuestTextures = false,
                };

                var lilMat = new LilToonMaterial(sourceMaterial);
                var generator = new ToonLitGenerator(settings);

                Material resultMat = null;
                generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
                {
                    resultMat = mat;
                }).WaitForCompletion();

                Assert.IsNotNull(resultMat, "Generated material should not be null.");
                Assert.AreEqual(expectedScale, resultMat.mainTextureScale, "Main texture scale should be preserved.");
                Assert.AreEqual(expectedOffset, resultMat.mainTextureOffset, "Main texture offset should be preserved.");
            }
            finally
            {
                Object.DestroyImmediate(resultMat);
                Object.DestroyImmediate(sourceMaterial);
            }
        }

        /// <summary>
        /// Test that ToonLitGenerator without baking preserves the default (identity) main texture UV.
        /// </summary>
        [Test]
        public void ConvertToToonLit_NonBake_PreservesDefaultMainTextureST()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon is not installed.");
                return;
            }

            var lilToonVersion = AssetUtility.LilToonVersion;
            var requiredVersion = new SemVer(1, 10, 0);
            var breakingVersion = new SemVer(3, 0, 0);
            if (lilToonVersion < requiredVersion || lilToonVersion >= breakingVersion)
            {
                Assert.Ignore($"lilToon version {lilToonVersion} is not supported.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            var sourceMaterial = new Material(lilToonShader);
            try
            {
                var settings = new ToonLitConvertSettings
                {
                    generateQuestTextures = false,
                };

                var lilMat = new LilToonMaterial(sourceMaterial);
                var generator = new ToonLitGenerator(settings);

                Material resultMat = null;
                generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
                {
                    resultMat = mat;
                }).WaitForCompletion();

                Assert.IsNotNull(resultMat, "Generated material should not be null.");
                Assert.AreEqual(Vector2.one, resultMat.mainTextureScale, "Default main texture scale should be (1,1).");
                Assert.AreEqual(Vector2.zero, resultMat.mainTextureOffset, "Default main texture offset should be (0,0).");
            }
            finally
            {
                Object.DestroyImmediate(resultMat);
                Object.DestroyImmediate(sourceMaterial);
            }
        }

        /// <summary>
        /// Test that ToonLitGenerator with baking preserves main texture UV scale and offset on the output material.
        /// The baked texture replaces mainTexture but the UV tiling remains on the material for runtime rendering.
        /// </summary>
        [Test]
        public void ConvertToToonLit_WithBake_PreservesMainTextureScaleAndOffset()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon is not installed.");
                return;
            }

            var lilToonVersion = AssetUtility.LilToonVersion;
            var requiredVersion = new SemVer(1, 10, 0);
            var breakingVersion = new SemVer(3, 0, 0);
            if (lilToonVersion < requiredVersion || lilToonVersion >= breakingVersion)
            {
                Assert.Ignore($"lilToon version {lilToonVersion} is not supported.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            // Use an AssetDatabase asset texture so that DestroyNonAssetTexture inside
            // baking creates a fresh uncompressed copy without destroying the original.
            var mainTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(TestUtils.TexturesFolder + "/albedo_1024px_png.png");
            if (mainTexture == null)
            {
                Assert.Ignore("albedo_1024px_png.png fixture not found.");
                return;
            }

            var sourceMaterial = new Material(lilToonShader);
            try
            {
                var expectedScale = new Vector2(2.5f, 3.0f);
                var expectedOffset = new Vector2(0.1f, 0.2f);
                sourceMaterial.mainTexture = mainTexture;
                sourceMaterial.SetTextureScale("_MainTex", expectedScale);
                sourceMaterial.SetTextureOffset("_MainTex", expectedOffset);

                var settings = new ToonLitConvertSettings
                {
                    generateQuestTextures = true,
                };

                var lilMat = new LilToonMaterial(sourceMaterial);
                var generator = new ToonLitGenerator(settings);

                Material resultMat = null;
                generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
                {
                    resultMat = mat;
                }).WaitForCompletion();

                Assert.IsNotNull(resultMat, "Generated material should not be null.");
                Assert.AreEqual(expectedScale, resultMat.mainTextureScale, "Main texture scale should be preserved after baking.");
                Assert.AreEqual(expectedOffset, resultMat.mainTextureOffset, "Main texture offset should be preserved after baking.");
            }
            finally
            {
                if (resultMat != null)
                {
                    Object.DestroyImmediate(resultMat.mainTexture);
                    Object.DestroyImmediate(resultMat);
                }

                Object.DestroyImmediate(sourceMaterial);
            }
        }

        /// <summary>
        /// Test that ToonLitGenerator with baking does NOT embed UV tiling into the baked texture.
        /// Two materials with the same source texture but different UV tiling should produce nearly
        /// identical baked textures. UV tiling is applied at runtime by the Toon Lit material via
        /// mainTextureScale, not embedded in the texture pixels.
        /// </summary>
        [Test]
        public void ConvertToToonLit_WithBake_BakedTextureDoesNotEmbedUVTiling()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon is not installed.");
                return;
            }

            var lilToonVersion = AssetUtility.LilToonVersion;
            var requiredVersion = new SemVer(1, 10, 0);
            var breakingVersion = new SemVer(3, 0, 0);
            if (lilToonVersion < requiredVersion || lilToonVersion >= breakingVersion)
            {
                Assert.Ignore($"lilToon version {lilToonVersion} is not supported.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            // Use an AssetDatabase asset texture so that DestroyNonAssetTexture inside
            // baking creates a fresh uncompressed copy each time without destroying the original.
            var mainTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(TestUtils.TexturesFolder + "/albedo_1024px_png.png");
            if (mainTexture == null)
            {
                Assert.Ignore("albedo_1024px_png.png fixture not found.");
                return;
            }

            var mat11 = new Material(lilToonShader);
            var mat22 = new Material(lilToonShader);
            try
            {
                mat11.mainTexture = mainTexture;
                mat11.SetTextureScale("_MainTex", Vector2.one);
                mat11.SetTextureOffset("_MainTex", Vector2.zero);

                mat22.mainTexture = mainTexture;
                mat22.SetTextureScale("_MainTex", new Vector2(2f, 2f));
                mat22.SetTextureOffset("_MainTex", Vector2.zero);

                var settings = new ToonLitConvertSettings
                {
                    generateQuestTextures = true,
                };

                var lilMat11 = new LilToonMaterial(mat11);
                var gen11 = new ToonLitGenerator(settings);
                var lilMat22 = new LilToonMaterial(mat22);
                var gen22 = new ToonLitGenerator(settings);

                Material resultMat11 = null;
                Material resultMat22 = null;

                gen11.GenerateMaterial(lilMat11, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat11 = mat; }).WaitForCompletion();
                gen22.GenerateMaterial(lilMat22, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat22 = mat; }).WaitForCompletion();

                Assert.IsNotNull(resultMat11, "ToonLit result for scale (1,1) should not be null.");
                Assert.IsNotNull(resultMat22, "ToonLit result for scale (2,2) should not be null.");

                Assert.AreEqual(Vector2.one, resultMat11.mainTextureScale, "Scale (1,1) source should produce (1,1) on output material.");
                Assert.AreEqual(new Vector2(2f, 2f), resultMat22.mainTextureScale, "Scale (2,2) source should produce (2,2) on output material.");

                var bakedTex11 = resultMat11.mainTexture as Texture2D;
                var bakedTex22 = resultMat22.mainTexture as Texture2D;

                if (bakedTex11 != null && bakedTex22 != null)
                {
                    var diff = TestUtils.MaxDifference(bakedTex11, bakedTex22);
                    Assert.Less(diff, 0.01f, "Baked textures should be nearly identical regardless of UV tiling. UV tiling must not be embedded into the baked texture to prevent double-tiling at runtime.");
                }
                else
                {
                    Assert.Ignore("Baked textures are not available as Texture2D for comparison (may be RenderTexture on this platform).");
                }
            }
            finally
            {
                if (resultMat11 != null)
                {
                    Object.DestroyImmediate(resultMat11.mainTexture);
                    Object.DestroyImmediate(resultMat11);
                }

                if (resultMat22 != null)
                {
                    Object.DestroyImmediate(resultMat22.mainTexture);
                    Object.DestroyImmediate(resultMat22);
                }

                Object.DestroyImmediate(mat11);
                Object.DestroyImmediate(mat22);
            }
        }

        /// <summary>
        /// Test that emission UV tiling is correctly adjusted relative to main UV tiling during baking.
        /// Two materials with the same emission-to-main UV ratio (es/ms) must produce identical baked textures:
        /// mat1 has main scale (1,1) and emission scale (0.5,0.5) => adjusted emission bake scale = (0.5, 0.5).
        /// mat2 has main scale (2,2) and emission scale (1,1)     => adjusted emission bake scale = (0.5, 0.5).
        /// After the fix both baked textures must be nearly identical.
        /// </summary>
        [Test]
        public void ConvertToToonLit_WithBake_EmissionUVIsAdjustedRelativeToMainUV()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon is not installed.");
                return;
            }

            var lilToonVersion = AssetUtility.LilToonVersion;
            var requiredVersion = new SemVer(1, 10, 0);
            var breakingVersion = new SemVer(3, 0, 0);
            if (lilToonVersion < requiredVersion || lilToonVersion >= breakingVersion)
            {
                Assert.Ignore($"lilToon version {lilToonVersion} is not supported.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            // Use an AssetDatabase asset so LoadUncompressedTexture creates a fresh copy each time,
            // preventing the shared emission texture from being destroyed during the first bake.
            var emissionTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(TestUtils.TexturesFolder + "/albedo_1024px_png.png");
            if (emissionTexture == null)
            {
                Assert.Ignore("albedo_1024px_png.png fixture not found.");
                return;
            }

            var mat1 = new Material(lilToonShader);
            var mat2 = new Material(lilToonShader);
            try
            {
                // mat1: main (1,1), emission (0.5,0.5) => adjusted emission bake scale = 0.5/1 = (0.5, 0.5)
                mat1.SetTextureScale("_MainTex", Vector2.one);
                mat1.SetTextureOffset("_MainTex", Vector2.zero);
                mat1.SetFloat("_UseEmission", 1f);
                mat1.SetTexture("_EmissionMap", emissionTexture);
                mat1.SetTextureScale("_EmissionMap", new Vector2(0.5f, 0.5f));
                mat1.SetTextureOffset("_EmissionMap", Vector2.zero);
                mat1.SetColor("_EmissionColor", Color.white);
                mat1.SetFloat("_EmissionBlend", 1f);
                mat1.SetFloat("_EmissionMainStrength", 0f);
                mat1.SetFloat("_EmissionBlendMode", 1f);
                mat1.SetFloat("_UseEmission2nd", 0f);

                // mat2: main (2,2), emission (1,1) => adjusted emission bake scale = 1/2 = (0.5, 0.5) (same as mat1)
                mat2.SetTextureScale("_MainTex", new Vector2(2f, 2f));
                mat2.SetTextureOffset("_MainTex", Vector2.zero);
                mat2.SetFloat("_UseEmission", 1f);
                mat2.SetTexture("_EmissionMap", emissionTexture);
                mat2.SetTextureScale("_EmissionMap", Vector2.one);
                mat2.SetTextureOffset("_EmissionMap", Vector2.zero);
                mat2.SetColor("_EmissionColor", Color.white);
                mat2.SetFloat("_EmissionBlend", 1f);
                mat2.SetFloat("_EmissionMainStrength", 0f);
                mat2.SetFloat("_EmissionBlendMode", 1f);
                mat2.SetFloat("_UseEmission2nd", 0f);

                var settings = new ToonLitConvertSettings { generateQuestTextures = true };
                var lilMat1 = new LilToonMaterial(mat1);
                var gen1 = new ToonLitGenerator(settings);
                var lilMat2 = new LilToonMaterial(mat2);
                var gen2 = new ToonLitGenerator(settings);

                Material resultMat1 = null;
                Material resultMat2 = null;
                gen1.GenerateMaterial(lilMat1, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat1 = mat; }).WaitForCompletion();
                gen2.GenerateMaterial(lilMat2, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat2 = mat; }).WaitForCompletion();

                Assert.IsNotNull(resultMat1, "Baked result for mat1 should not be null.");
                Assert.IsNotNull(resultMat2, "Baked result for mat2 should not be null.");

                var bakedTex1 = resultMat1.mainTexture as Texture2D;
                var bakedTex2 = resultMat2.mainTexture as Texture2D;

                if (bakedTex1 == null || bakedTex2 == null)
                {
                    Assert.Ignore("Baked textures are not available as Texture2D on this platform.");
                    return;
                }

                var diff = TestUtils.MaxDifference(bakedTex1, bakedTex2);
                Assert.Less(diff, 0.01f,
                    $"Baked textures for materials with the same es/ms emission ratio should be nearly identical. " +
                    $"Actual max difference: {diff:F4}. " +
                    $"This indicates emission UV is not being adjusted relative to main UV tiling during baking.");
            }
            finally
            {
                if (resultMat1 != null)
                {
                    Object.DestroyImmediate(resultMat1.mainTexture);
                    Object.DestroyImmediate(resultMat1);
                }

                if (resultMat2 != null)
                {
                    Object.DestroyImmediate(resultMat2.mainTexture);
                    Object.DestroyImmediate(resultMat2);
                }

                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }
    }
}
