// <copyright file="LilToonToonStandardGeneratorTests.cs" company="kurotu">
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
    /// Tests for main texture UV tiling preservation in ToonStandard conversion.
    /// </summary>
    public class LilToonToonStandardGeneratorTests
    {
        /// <summary>
        /// Test that ToonStandardMaterialWrapper correctly stores and retrieves MainTextureScale.
        /// </summary>
        [Test]
        public void ToonStandardMaterialWrapper_MainTextureScale_RoundTrips()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            using var material = DisposableObject.New(new Material(shader));
            var wrapper = new ToonStandardMaterialWrapper(material.Object);
            var expected = new Vector2(2.5f, 3.0f);
            wrapper.MainTextureScale = expected;
            Assert.AreEqual(expected, wrapper.MainTextureScale);
        }

        /// <summary>
        /// Test that ToonStandardMaterialWrapper correctly stores and retrieves MainTextureOffset.
        /// </summary>
        [Test]
        public void ToonStandardMaterialWrapper_MainTextureOffset_RoundTrips()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            using var material = DisposableObject.New(new Material(shader));
            var wrapper = new ToonStandardMaterialWrapper(material.Object);
            var expected = new Vector2(0.25f, 0.75f);
            wrapper.MainTextureOffset = expected;
            Assert.AreEqual(expected, wrapper.MainTextureOffset);
        }

        /// <summary>
        /// Test that ToonStandardMaterialWrapper defaults to identity scale and zero offset.
        /// </summary>
        [Test]
        public void ToonStandardMaterialWrapper_MainTextureST_DefaultValues()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            using var material = DisposableObject.New(new Material(shader));
            var wrapper = new ToonStandardMaterialWrapper(material.Object);
            Assert.AreEqual(Vector2.one, wrapper.MainTextureScale, "Default scale should be (1,1).");
            Assert.AreEqual(Vector2.zero, wrapper.MainTextureOffset, "Default offset should be (0,0).");
        }

        /// <summary>
        /// Test that ConvertToToonStandard preserves main texture UV scale and offset from lilToon material.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_PreservesMainTextureScaleAndOffset()
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

            var toonStandardShader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (toonStandardShader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            using var sourceMaterial = DisposableObject.New(new Material(lilToonShader));
            var expectedScale = new Vector2(2.0f, 3.0f);
            var expectedOffset = new Vector2(0.5f, 0.25f);
            sourceMaterial.Object.SetTextureScale("_MainTex", expectedScale);
            sourceMaterial.Object.SetTextureOffset("_MainTex", expectedOffset);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };

            var lilMat = new LilToonMaterial(sourceMaterial.Object);
            var generator = new LilToonToonStandardGenerator(lilMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
            {
                resultMat = mat;
            }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");

            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.AreEqual(expectedScale, resultWrapper.MainTextureScale, "Main texture scale should be preserved.");
            Assert.AreEqual(expectedOffset, resultWrapper.MainTextureOffset, "Main texture offset should be preserved.");
        }

        /// <summary>
        /// Test that ConvertToToonStandard preserves default (identity) UV when not modified.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_PreservesDefaultUV()
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

            var toonStandardShader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (toonStandardShader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            using var sourceMaterial = DisposableObject.New(new Material(lilToonShader));
            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
            };

            var lilMat = new LilToonMaterial(sourceMaterial.Object);
            var generator = new LilToonToonStandardGenerator(lilMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
            {
                resultMat = mat;
            }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");

            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.AreEqual(Vector2.one, resultWrapper.MainTextureScale, "Default scale should be (1,1).");
            Assert.AreEqual(Vector2.zero, resultWrapper.MainTextureOffset, "Default offset should be (0,0).");
        }

        /// <summary>
        /// Test that GenerateMaterial with texture baking preserves main texture UV scale and offset.
        /// </summary>
        [Test]
        public void GenerateMaterial_WithBake_PreservesMainTextureST()
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

            var toonStandardShader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (toonStandardShader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            using var sourceMaterial = DisposableObject.New(new Material(lilToonShader));
            using var dummyTexture = DisposableObject.New(new Texture2D(4, 4));
            var expectedScale = new Vector2(2.0f, 3.0f);
            var expectedOffset = new Vector2(0.5f, 0.25f);
            sourceMaterial.Object.mainTexture = dummyTexture.Object;
            sourceMaterial.Object.SetTextureScale("_MainTex", expectedScale);
            sourceMaterial.Object.SetTextureOffset("_MainTex", expectedOffset);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = true,
            };
            settings.SetAllFeatures(false);

            var lilMat = new LilToonMaterial(sourceMaterial.Object);
            var generator = new LilToonToonStandardGenerator(lilMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
            {
                resultMat = mat;
            }).WaitForCompletion();
            using var resultMainTex = DisposableObject.New(resultMat != null ? resultMat.mainTexture : null);
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");

            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.AreEqual(expectedScale, resultWrapper.MainTextureScale, "Main texture scale should be preserved after baking.");
            Assert.AreEqual(expectedOffset, resultWrapper.MainTextureOffset, "Main texture offset should be preserved after baking.");
        }

        /// <summary>
        /// Test that LilToonMaterial correctly reads MainTextureScale and MainTextureOffset.
        /// </summary>
        [Test]
        public void LilToonMaterial_MainTextureST_ReadsCorrectly()
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

            using var sourceMaterial = DisposableObject.New(new Material(lilToonShader));
            var expectedScale = new Vector2(4.0f, 5.0f);
            var expectedOffset = new Vector2(-0.5f, 1.5f);
            sourceMaterial.Object.SetTextureScale("_MainTex", expectedScale);
            sourceMaterial.Object.SetTextureOffset("_MainTex", expectedOffset);

            var lilMat = new LilToonMaterial(sourceMaterial.Object);
            Assert.AreEqual(expectedScale, lilMat.MainTextureScale, "LilToonMaterial should read correct scale.");
            Assert.AreEqual(expectedOffset, lilMat.MainTextureOffset, "LilToonMaterial should read correct offset.");
        }

        /// <summary>
        /// Test that LilToonMaterial correctly reads EmissionMapTextureScale and EmissionMapTextureOffset.
        /// </summary>
        [Test]
        public void LilToonMaterial_EmissionMapST_ReadsCorrectly()
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

            using var sourceMaterial = DisposableObject.New(new Material(lilToonShader));
            var expectedScale = new Vector2(3.0f, 4.0f);
            var expectedOffset = new Vector2(0.1f, 0.2f);
            sourceMaterial.Object.SetTextureScale("_EmissionMap", expectedScale);
            sourceMaterial.Object.SetTextureOffset("_EmissionMap", expectedOffset);

            var lilMat = new LilToonMaterial(sourceMaterial.Object);
            Assert.AreEqual(expectedScale, lilMat.EmissionMapTextureScale, "LilToonMaterial should read correct emission scale.");
            Assert.AreEqual(expectedOffset, lilMat.EmissionMapTextureOffset, "LilToonMaterial should read correct emission offset.");
        }

        /// <summary>
        /// Test that ToonStandardMaterialWrapper correctly stores and retrieves EmissionMapTextureScale and EmissionMapTextureOffset.
        /// </summary>
        [Test]
        public void ToonStandardMaterialWrapper_EmissionMapST_RoundTrips()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            using var material = DisposableObject.New(new Material(shader));
            var wrapper = new ToonStandardMaterialWrapper(material.Object);
            var expectedScale = new Vector2(2.0f, 3.0f);
            var expectedOffset = new Vector2(0.1f, 0.5f);
            wrapper.EmissionMapTextureScale = expectedScale;
            wrapper.EmissionMapTextureOffset = expectedOffset;
            Assert.AreEqual(expectedScale, wrapper.EmissionMapTextureScale, "Emission map scale should round-trip.");
            Assert.AreEqual(expectedOffset, wrapper.EmissionMapTextureOffset, "Emission map offset should round-trip.");
        }

        /// <summary>
        /// Test that ConvertToToonStandard preserves emission map UV scale and offset when main and emission have different UV tiling.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_PreservesEmissionMapST_WhenDifferentFromMainUV()
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

            var toonStandardShader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (toonStandardShader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            using var sourceMaterial = DisposableObject.New(new Material(lilToonShader));

            // Set main texture UV tiling different from emission UV tiling
            var mainScale = new Vector2(2.0f, 2.0f);
            var mainOffset = new Vector2(0.0f, 0.0f);
            sourceMaterial.Object.SetTextureScale("_MainTex", mainScale);
            sourceMaterial.Object.SetTextureOffset("_MainTex", mainOffset);

            var emissionScale = new Vector2(3.0f, 4.0f);
            var emissionOffset = new Vector2(0.1f, 0.2f);
            sourceMaterial.Object.SetTextureScale("_EmissionMap", emissionScale);
            sourceMaterial.Object.SetTextureOffset("_EmissionMap", emissionOffset);
            sourceMaterial.Object.SetFloat("_UseEmission", 1);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useEmission = true,
            };

            var lilMat = new LilToonMaterial(sourceMaterial.Object);
            var generator = new LilToonToonStandardGenerator(lilMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
            {
                resultMat = mat;
            }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");

            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.AreEqual(mainScale, resultWrapper.MainTextureScale, "Main texture scale should be preserved.");
            Assert.AreEqual(mainOffset, resultWrapper.MainTextureOffset, "Main texture offset should be preserved.");
            Assert.AreEqual(emissionScale, resultWrapper.EmissionMapTextureScale, "Emission map scale should be preserved independently.");
            Assert.AreEqual(emissionOffset, resultWrapper.EmissionMapTextureOffset, "Emission map offset should be preserved independently.");
        }

        /// <summary>
        /// Test that LilToonMaterial.GenerateToonLitImage does NOT bake UV tiling into the output texture.
        /// The ltsother_baker shader operates in UV space (0-1) and ignores _MainTex_ST by design.
        /// UV tiling is a real-time rendering concept; for ToonStandard it is preserved on the output
        /// material via GetMainTextureST(), not baked into the texture pixels.
        /// </summary>
        [Test]
        public void LilToonMaterial_GenerateToonLitImage_MainUVTilingDoesNotAffectBakedOutput()
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
            // MainBake creates a fresh uncompressed copy each time without destroying the
            // original, allowing both bake calls to use the same source texture.
            var mainTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(TestUtils.TexturesFolder + "/albedo_1024px_png.png");
            if (mainTexture == null)
            {
                Assert.Ignore("albedo_1024px_png.png fixture not found.");
                return;
            }

            var settings = new ToonLitConvertSettings
            {
                mainTextureBrightness = 1.0f,
            };

            using var mat11 = DisposableObject.New(new Material(lilToonShader));
            using var mat22 = DisposableObject.New(new Material(lilToonShader));
            mat11.Object.mainTexture = mainTexture;
            mat11.Object.SetTextureScale("_MainTex", Vector2.one);
            mat11.Object.SetTextureOffset("_MainTex", Vector2.zero);

            mat22.Object.mainTexture = mainTexture;
            mat22.Object.SetTextureScale("_MainTex", new Vector2(2f, 2f));
            mat22.Object.SetTextureOffset("_MainTex", Vector2.zero);

            Texture2D result11 = null;
            new LilToonMaterial(mat11.Object).GenerateToonLitImage(settings, (t) => { result11 = t; }).WaitForCompletion();
            using var result11Disposable = DisposableObject.New(result11);

            Texture2D result22 = null;
            new LilToonMaterial(mat22.Object).GenerateToonLitImage(settings, (t) => { result22 = t; }).WaitForCompletion();
            using var result22Disposable = DisposableObject.New(result22);

            Assert.IsNotNull(result11, "result for scale (1,1) should not be null.");
            Assert.IsNotNull(result22, "result for scale (2,2) should not be null.");

            var diff = TestUtils.MaxDifference(result11, result22);
            Assert.Less(diff, 0.01f, "UV tiling should NOT affect the baked texture content. The ltsother_baker shader ignores _MainTex_ST by design and operates in UV space (0-1).");
        }

        /// <summary>
        /// Test that the ToonStandard bake path (GenerateMainTexture) does NOT bake UV tiling
        /// into the texture, preventing double-tiling. UV tiling is applied separately to the
        /// output material via GetMainTextureST(). Two materials with different UV tiling should
        /// produce baked textures with the same pixel content.
        /// </summary>
        [Test]
        public void GenerateMaterial_WithBake_ToonStandardBakesTextureWithoutUVTiling()
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

            var toonStandardShader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (toonStandardShader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            // Use an AssetDatabase asset texture so that DestroyNonAssetTexture inside
            // MainBake creates a fresh uncompressed copy each time without destroying the
            // original, allowing both bake calls to use the same source texture.
            var mainTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(TestUtils.TexturesFolder + "/albedo_1024px_png.png");
            if (mainTexture == null)
            {
                Assert.Ignore("albedo_1024px_png.png fixture not found.");
                return;
            }

            using var mat11 = DisposableObject.New(new Material(lilToonShader));
            using var mat22 = DisposableObject.New(new Material(lilToonShader));
            mat11.Object.mainTexture = mainTexture;
            mat11.Object.SetTextureScale("_MainTex", Vector2.one);
            mat11.Object.SetTextureOffset("_MainTex", Vector2.zero);

            mat22.Object.mainTexture = mainTexture;
            mat22.Object.SetTextureScale("_MainTex", new Vector2(2f, 2f));
            mat22.Object.SetTextureOffset("_MainTex", Vector2.zero);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = true,
            };
            settings.SetAllFeatures(false);

            var lilMat11 = new LilToonMaterial(mat11.Object);
            var gen11 = new LilToonToonStandardGenerator(lilMat11, settings, null, false);
            var lilMat22 = new LilToonMaterial(mat22.Object);
            var gen22 = new LilToonToonStandardGenerator(lilMat22, settings, null, false);

            Material resultMat11 = null;
            gen11.GenerateMaterial(lilMat11, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat11 = mat; }).WaitForCompletion();
            using var result11Tex = DisposableObject.New(resultMat11 != null ? resultMat11.mainTexture : null);
            using var result11 = DisposableObject.New(resultMat11);

            Material resultMat22 = null;
            gen22.GenerateMaterial(lilMat22, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) => { resultMat22 = mat; }).WaitForCompletion();
            using var result22Tex = DisposableObject.New(resultMat22 != null ? resultMat22.mainTexture : null);
            using var result22 = DisposableObject.New(resultMat22);

            Assert.IsNotNull(resultMat11, "ToonStandard result for scale (1,1) should not be null.");
            Assert.IsNotNull(resultMat22, "ToonStandard result for scale (2,2) should not be null.");

            var wrapper11 = new ToonStandardMaterialWrapper(resultMat11);
            var wrapper22 = new ToonStandardMaterialWrapper(resultMat22);

            Assert.AreEqual(Vector2.one, wrapper11.MainTextureScale, "Scale (1,1) source should produce (1,1) on output material.");
            Assert.AreEqual(new Vector2(2f, 2f), wrapper22.MainTextureScale, "Scale (2,2) source should produce (2,2) on output material.");

            var bakedTex11 = resultMat11.mainTexture as Texture2D;
            var bakedTex22 = resultMat22.mainTexture as Texture2D;

            if (bakedTex11 != null && bakedTex22 != null)
            {
                var diff = TestUtils.MaxDifference(bakedTex11, bakedTex22);
                Assert.Less(diff, 0.01f, "Baked textures should be nearly identical: UV tiling must not be baked into the texture to avoid double-tiling.");
            }
            else
            {
                Assert.Ignore("Baked textures are not available as Texture2D for comparison (may be RenderTexture on this platform).");
            }
        }

        /// <summary>
        /// Test that ConvertToToonStandard preserves occlusion map (AOMap) UV scale and offset.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_PreservesOcclusionMapST()
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

            var toonStandardShader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (toonStandardShader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return;
            }

            var lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return;
            }

            using var sourceMaterial = DisposableObject.New(new Material(lilToonShader));
            var expectedScale = new Vector2(3.0f, 2.0f);
            var expectedOffset = new Vector2(0.1f, 0.3f);
            sourceMaterial.Object.SetTextureScale("_ShadowBorderMask", expectedScale);
            sourceMaterial.Object.SetTextureOffset("_ShadowBorderMask", expectedOffset);
            sourceMaterial.Object.SetFloat("_UseShadow", 1);

            // A non-null texture is needed so AOMap != null check passes
            var dummyTex = Texture2D.whiteTexture;
            sourceMaterial.Object.SetTexture("_ShadowBorderMask", dummyTex);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = false,
                useOcclusion = true,
            };

            var lilMat = new LilToonMaterial(sourceMaterial.Object);
            var generator = new LilToonToonStandardGenerator(lilMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
            {
                resultMat = mat;
            }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");

            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.IsTrue(resultWrapper.UseOcclusion, "UseOcclusion should be enabled.");
            Assert.AreEqual(expectedScale, resultWrapper.OcclusionMapTextureScale, "Occlusion map texture scale should be preserved.");
            Assert.AreEqual(expectedOffset, resultWrapper.OcclusionMapTextureOffset, "Occlusion map texture offset should be preserved.");
        }

        /// <summary>
        /// Test that NoLimit for both maxTextureSize and maskMaxTextureSize does not produce zero-sized packed mask.
        /// </summary>
        [Test]
        public void GenerateMaterial_WithNoLimitMaskSizes_DoesNotCreateZeroSizedMaskTexture()
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

            using var sourceMaterial = DisposableObject.New(new Material(lilToonShader));
            using var mainTexture = DisposableObject.New(new Texture2D(64, 64));
            using var occlusionMask = DisposableObject.New(new Texture2D(32, 32));
            sourceMaterial.Object.mainTexture = mainTexture.Object;
            sourceMaterial.Object.SetFloat("_UseShadow", 1);
            sourceMaterial.Object.SetTexture("_ShadowBorderMask", occlusionMask.Object);

            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = true,
                maxTextureSize = TextureSizeLimit.NoLimit,
                maskMaxTextureSize = TextureSizeLimit.NoLimit,
                useOcclusion = true,
            };
            settings.SetAllFeatures(false);
            settings.useOcclusion = true;

            var lilMat = new LilToonMaterial(sourceMaterial.Object);
            var generator = new LilToonToonStandardGenerator(lilMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
            {
                resultMat = mat;
            }).WaitForCompletion();
            using var resultMaterial = DisposableObject.New(resultMat);

            Assert.IsNotNull(resultMat, "Generated material should not be null.");
            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.IsTrue(resultWrapper.UseOcclusion, "UseOcclusion should be enabled.");
            Assert.IsNotNull(resultWrapper.OcclusionMap, "Occlusion map should be generated.");

            if (resultWrapper.OcclusionMap is Texture2D texture2D)
            {
                Assert.Greater(texture2D.width, 0, "Generated occlusion texture width should be greater than zero.");
                Assert.Greater(texture2D.height, 0, "Generated occlusion texture height should be greater than zero.");
            }
        }

        /// <summary>
        /// Test that ConvertToToonStandard derives OcclusionStrength from the shadow color floor luminance.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_SetsOcclusionStrengthFromShadowColor()
        {
            if (!IsLilToonTestEnvironmentAvailable())
            {
                return;
            }

            var shadowColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            using var sourceMaterial = DisposableObject.New(CreateLilToonMaterialWithAOMap());
            sourceMaterial.Object.SetColor("_ShadowColor", shadowColor);
            sourceMaterial.Object.SetColor("_Shadow2ndColor", new Color(0, 0, 0, 0));
            sourceMaterial.Object.SetColor("_Shadow3rdColor", new Color(0, 0, 0, 0));
            sourceMaterial.Object.SetFloat("_ShadowStrength", 1.0f);

            var strength = GetGeneratedOcclusionStrength(sourceMaterial.Object, false);

            var floor = Color.Lerp(Color.white, Color.white * shadowColor.linear, shadowColor.a);
            var expected = 1.0f - Utils.ColorUtility.GetRec709Grayscale(floor);
            Assert.AreEqual(expected, strength, 0.001f, "OcclusionStrength should match 1 - luminance of the linear shadow floor color.");
            Assert.Less(strength, 1.0f, "OcclusionStrength should be less than the shader default 1.0.");
        }

        /// <summary>
        /// Test that an enabled 2nd shadow color darkens the floor and increases OcclusionStrength.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_OcclusionStrengthIncreasesWithShadow2nd()
        {
            if (!IsLilToonTestEnvironmentAvailable())
            {
                return;
            }

            var shadowColor = new Color(0.7f, 0.65f, 0.72f, 1.0f);

            using var baseMaterial = DisposableObject.New(CreateLilToonMaterialWithAOMap());
            baseMaterial.Object.SetColor("_ShadowColor", shadowColor);
            baseMaterial.Object.SetColor("_Shadow2ndColor", new Color(0, 0, 0, 0));
            baseMaterial.Object.SetColor("_Shadow3rdColor", new Color(0, 0, 0, 0));
            baseMaterial.Object.SetFloat("_ShadowStrength", 1.0f);
            var strength1 = GetGeneratedOcclusionStrength(baseMaterial.Object, false);

            using var secondShadowMaterial = DisposableObject.New(CreateLilToonMaterialWithAOMap());
            secondShadowMaterial.Object.SetColor("_ShadowColor", shadowColor);
            secondShadowMaterial.Object.SetColor("_Shadow2ndColor", new Color(0.2f, 0.2f, 0.2f, 1.0f));
            secondShadowMaterial.Object.SetColor("_Shadow3rdColor", new Color(0, 0, 0, 0));
            secondShadowMaterial.Object.SetFloat("_ShadowStrength", 1.0f);
            var strength2 = GetGeneratedOcclusionStrength(secondShadowMaterial.Object, false);

            Assert.Greater(strength2, strength1, "OcclusionStrength should increase when the 2nd shadow color darkens the floor.");
        }

        /// <summary>
        /// Test that OcclusionStrength becomes zero when shadow strength is zero.
        /// </summary>
        [Test]
        public void ConvertToToonStandard_OcclusionStrengthZeroWhenShadowStrengthZero()
        {
            if (!IsLilToonTestEnvironmentAvailable())
            {
                return;
            }

            using var sourceMaterial = DisposableObject.New(CreateLilToonMaterialWithAOMap());
            sourceMaterial.Object.SetColor("_ShadowColor", new Color(0.5f, 0.5f, 0.5f, 1.0f));
            sourceMaterial.Object.SetColor("_Shadow2ndColor", new Color(0, 0, 0, 0));
            sourceMaterial.Object.SetColor("_Shadow3rdColor", new Color(0, 0, 0, 0));
            sourceMaterial.Object.SetFloat("_ShadowStrength", 0.0f);

            var strength = GetGeneratedOcclusionStrength(sourceMaterial.Object, false);

            Assert.AreEqual(0.0f, strength, 0.001f, "OcclusionStrength should be zero when shadow strength is zero.");
        }

        /// <summary>
        /// Test that the Quest texture generation path also sets OcclusionStrength.
        /// </summary>
        [Test]
        public void GenerateMaterial_QuestTextures_SetsOcclusionStrength()
        {
            if (!IsLilToonTestEnvironmentAvailable())
            {
                return;
            }

            var shadowColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            using var aoMap = DisposableObject.New(new Texture2D(32, 32));
            using var sourceMaterial = DisposableObject.New(CreateLilToonMaterialWithAOMap(aoMap.Object));
            sourceMaterial.Object.SetColor("_ShadowColor", shadowColor);
            sourceMaterial.Object.SetColor("_Shadow2ndColor", new Color(0, 0, 0, 0));
            sourceMaterial.Object.SetColor("_Shadow3rdColor", new Color(0, 0, 0, 0));
            sourceMaterial.Object.SetFloat("_ShadowStrength", 1.0f);

            var strength = GetGeneratedOcclusionStrength(sourceMaterial.Object, true);

            var floor = Color.Lerp(Color.white, Color.white * shadowColor.linear, shadowColor.a);
            var expected = 1.0f - Utils.ColorUtility.GetRec709Grayscale(floor);
            Assert.AreEqual(expected, strength, 0.001f, "OcclusionStrength should be set on the Quest texture generation path.");
        }

        /// <summary>
        /// Test that the _ShadowAOShift remap is baked into the generated occlusion mask.
        /// </summary>
        [Test]
        public void GenerateMaterial_QuestTextures_BakesShadowAOShiftIntoOcclusionMask()
        {
            if (!IsLilToonTestEnvironmentAvailable())
            {
                return;
            }

            using var aoMap = DisposableObject.New(new Texture2D(4, 4));
            var pixels = new Color[4 * 4];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0.25f, 0.25f, 0.25f, 1.0f);
            }
            aoMap.Object.SetPixels(pixels);
            aoMap.Object.Apply();

            using var sourceMaterial = DisposableObject.New(CreateLilToonMaterialWithAOMap(aoMap.Object));

            // Scale 0 and offset 1 remap every AO value to white regardless of color space conversion.
            sourceMaterial.Object.SetVector("_ShadowAOShift", new Vector4(0.0f, 1.0f, 1.0f, 0.0f));
            sourceMaterial.Object.SetColor("_Shadow2ndColor", new Color(0, 0, 0, 0));
            sourceMaterial.Object.SetColor("_Shadow3rdColor", new Color(0, 0, 0, 0));

            var resultMat = GenerateMaterial(sourceMaterial.Object, true);
            using var resultMaterial = DisposableObject.New(resultMat);
            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);

            Assert.IsTrue(resultWrapper.UseOcclusion, "UseOcclusion should be enabled.");
            var occlusionMap = resultWrapper.OcclusionMap as Texture2D;
            Assert.IsNotNull(occlusionMap, "Occlusion map should be generated as Texture2D.");

            var sample = SampleCenterPixel(occlusionMap);
            Assert.GreaterOrEqual(sample.g, 0.99f, "AO values remapped by _ShadowAOShift should be baked as white.");
        }

        /// <summary>
        /// Test that Metallic/Gloss maps derive their ReflectionColor scalar from the
        /// sRGB-decoded (linear) luminance, not the raw Inspector (gamma) value.
        /// </summary>
        [Test]
        public void GenerateMaterial_QuestTextures_MetallicAndGlossMapsUseLinearReflectionColorLuminance()
        {
            if (!IsLilToonTestEnvironmentAvailable())
            {
                return;
            }

            // Equal RGB components make the expected luminance equal to the (decoded) component value,
            // and alpha != 1 exercises that the alpha multiplier itself is left untouched.
            var reflectionColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);

            using var sourceMaterial = DisposableObject.New(new Material(Shader.Find("lilToon")));
            sourceMaterial.Object.SetFloat("_UseReflection", 1);
            sourceMaterial.Object.SetColor("_ReflectionColor", reflectionColor);

            var resultMat = GenerateMaterial(sourceMaterial.Object, true, useSpecular: true);
            using var resultMaterial = DisposableObject.New(resultMat);
            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);

            Assert.IsTrue(resultWrapper.UseSpecular, "UseSpecular should be enabled.");
            var metallicTex = resultWrapper.MetallicMap as Texture2D;
            var glossTex = resultWrapper.GlossMap as Texture2D;
            Assert.IsNotNull(metallicTex, "Metallic map should be generated as Texture2D.");
            Assert.IsNotNull(glossTex, "Gloss map should be generated as Texture2D.");

            // Correct value: linearize the sRGB Inspector color before taking luminance.
            var expected = Utils.ColorUtility.GetRec709Grayscale(reflectionColor.linear) * reflectionColor.a;

            var metallicSample = SampleCenterPixel(metallicTex);
            var glossSample = SampleCenterPixel(glossTex);
            Assert.AreEqual(expected, metallicSample.r, 0.05f, "Metallic map R should equal linear-space Rec.709 luminance of ReflectionColor.");
            Assert.AreEqual(expected, glossSample.a, 0.05f, "Gloss map A should equal linear-space Rec.709 luminance of ReflectionColor.");

            // Sanity check the fixture actually discriminates linear vs. gamma-space math.
            var naiveGammaSpaceValue = Utils.ColorUtility.GetRec709Grayscale(reflectionColor) * reflectionColor.a;
            Assert.Greater(Mathf.Abs(expected - naiveGammaSpaceValue), 0.1f, "Fixture should meaningfully separate linear vs. gamma-space results.");
        }

        private static bool IsLilToonTestEnvironmentAvailable()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon is not installed.");
                return false;
            }

            var lilToonVersion = AssetUtility.LilToonVersion;
            var requiredVersion = new SemVer(1, 10, 0);
            var breakingVersion = new SemVer(3, 0, 0);
            if (lilToonVersion < requiredVersion || lilToonVersion >= breakingVersion)
            {
                Assert.Ignore($"lilToon version {lilToonVersion} is not supported.");
                return false;
            }

            if (Shader.Find("VRChat/Mobile/Toon Standard") == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Standard shader not available.");
                return false;
            }

            if (Shader.Find("lilToon") == null)
            {
                Assert.Ignore("lilToon shader not available.");
                return false;
            }

            return true;
        }

        private static Material CreateLilToonMaterialWithAOMap(Texture2D aoMap = null)
        {
            var material = new Material(Shader.Find("lilToon"));
            material.SetFloat("_UseShadow", 1);
            material.SetTexture("_ShadowBorderMask", aoMap != null ? aoMap : Texture2D.whiteTexture);
            return material;
        }

        private static Material GenerateMaterial(Material sourceMaterial, bool generateQuestTextures, bool useSpecular = false)
        {
            var settings = new ToonStandardConvertSettings
            {
                generateQuestTextures = generateQuestTextures,
                maxTextureSize = TextureSizeLimit.NoLimit,
                maskMaxTextureSize = TextureSizeLimit.NoLimit,
            };
            settings.SetAllFeatures(false);
            settings.useOcclusion = true;
            settings.useSpecular = useSpecular;

            var lilMat = new LilToonMaterial(sourceMaterial);
            var generator = new LilToonToonStandardGenerator(lilMat, settings, null, false);

            Material resultMat = null;
            generator.GenerateMaterial(lilMat, UnityEditor.BuildTarget.Android, false, string.Empty, (mat) =>
            {
                resultMat = mat;
            }).WaitForCompletion();

            Assert.IsNotNull(resultMat, "Generated material should not be null.");
            return resultMat;
        }

        private static Color SampleCenterPixel(Texture2D texture)
        {
            // The baked texture may be non-readable, so read pixels back through a temporary RenderTexture.
            var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;
                var readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false, true);
                try
                {
                    readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    readable.Apply(false, false);
                    return readable.GetPixel(texture.width / 2, texture.height / 2);
                }
                finally
                {
                    Object.DestroyImmediate(readable);
                }
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static float GetGeneratedOcclusionStrength(Material sourceMaterial, bool generateQuestTextures)
        {
            var resultMat = GenerateMaterial(sourceMaterial, generateQuestTextures);
            using var resultMaterial = DisposableObject.New(resultMat);
            var resultWrapper = new ToonStandardMaterialWrapper(resultMat);
            Assert.IsTrue(resultWrapper.UseOcclusion, "UseOcclusion should be enabled.");
            return resultWrapper.OcclusionStrength;
        }
    }
}
