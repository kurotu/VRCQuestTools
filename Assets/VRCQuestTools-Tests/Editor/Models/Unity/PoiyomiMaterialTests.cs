// <copyright file="PoiyomiMaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Tests for PoiyomiMaterial.
    /// </summary>
    public class PoiyomiMaterialTests
    {
        private const string PoiyomiShaderName = ".poiyomi/Poiyomi Toon";

        /// <summary>
        /// Test that MainTextureScale reads from _MainTex_ST Vector4 property.
        /// </summary>
        [Test]
        public void MainTextureScale_ReadsFromMainTexST()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found");
            var material = new Material(shader);
            try
            {
                var expectedScale = new Vector2(2.0f, 3.0f);
                var expectedOffset = new Vector2(0.5f, 0.25f);
                material.SetVector("_MainTex_ST", new Vector4(expectedScale.x, expectedScale.y, expectedOffset.x, expectedOffset.y));

                var poiMaterial = new PoiyomiMaterial(material);

                Assert.AreEqual(expectedScale, poiMaterial.MainTextureScale);
                Assert.AreEqual(expectedOffset, poiMaterial.MainTextureOffset);
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        /// <summary>
        /// Test that ConvertToToonLit preserves UV tiling from _MainTex_ST.
        /// </summary>
        [Test]
        public void ConvertToToonLit_PreservesUVTiling()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            var toonLitShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (toonLitShader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Lit shader not found");
            }

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found");
            var material = new Material(shader);
            try
            {
                var expectedScale = new Vector2(2.0f, 3.0f);
                var expectedOffset = new Vector2(0.5f, 0.25f);
                material.SetVector("_MainTex_ST", new Vector4(expectedScale.x, expectedScale.y, expectedOffset.x, expectedOffset.y));

                var poiMaterial = new PoiyomiMaterial(material);
                var toonLitMaterial = poiMaterial.ConvertToToonLit();
                try
                {
                    Assert.AreEqual(expectedScale, toonLitMaterial.mainTextureScale);
                    Assert.AreEqual(expectedOffset, toonLitMaterial.mainTextureOffset);
                }
                finally
                {
                    Object.DestroyImmediate(toonLitMaterial);
                }
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        /// <summary>
        /// Test that ConvertToToonLit preserves default (identity) UV when _MainTex_ST is not set.
        /// </summary>
        [Test]
        public void ConvertToToonLit_PreservesDefaultUVTiling()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            var toonLitShader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (toonLitShader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Lit shader not found");
            }

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found");
            var material = new Material(shader);
            try
            {
                var poiMaterial = new PoiyomiMaterial(material);
                var toonLitMaterial = poiMaterial.ConvertToToonLit();
                try
                {
                    Assert.AreEqual(Vector2.one, toonLitMaterial.mainTextureScale, "Default scale should be (1,1).");
                    Assert.AreEqual(Vector2.zero, toonLitMaterial.mainTextureOffset, "Default offset should be (0,0).");
                }
                finally
                {
                    Object.DestroyImmediate(toonLitMaterial);
                }
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        /// <summary>
        /// Test that non-uniform UV tiling values are correctly extracted.
        /// </summary>
        [Test]
        public void MainTextureScale_NonUniformTiling()
        {
            if (!AssetUtility.IsPoiyomiImported())
            {
                Assert.Ignore("Poiyomi is not installed.");
                return;
            }

            var shader = Shader.Find(PoiyomiShaderName);
            Assert.NotNull(shader, $"{PoiyomiShaderName} shader not found");
            var material = new Material(shader);
            try
            {
                material.SetVector("_MainTex_ST", new Vector4(4.0f, 0.5f, -0.1f, 0.75f));

                var poiMaterial = new PoiyomiMaterial(material);

                Assert.AreEqual(new Vector2(4.0f, 0.5f), poiMaterial.MainTextureScale);
                Assert.AreEqual(new Vector2(-0.1f, 0.75f), poiMaterial.MainTextureOffset);
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        /// <summary>
        /// Verify that feature toggles return false when both the toggle and the fallback value
        /// property are missing (simulating a locked Poiyomi shader with that feature disabled).
        /// </summary>
        [Test]
        public void FeatureToggles_ReturnFalse_WhenBothToggleAndFallbackMissing()
        {
            // Standard shader has no Poiyomi-specific properties such as _EmissionStrength,
            // _LightingMode, _Matcap, _RimStyle, _MochieMetallicMaps, or _MainVertexColoring,
            // so it simulates a locked Poiyomi shader where a feature has been disabled and stripped.
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                Assert.Ignore("Standard shader not found.");
                return;
            }

            using var material = DisposableObject.New(new Material(shader));
            var poiMat = new PoiyomiMaterial(material.Object);

            Assert.IsFalse(poiMat.UseShadow, "UseShadow should be false when _ShadingEnabled and _LightingMode are missing.");
            Assert.IsFalse(poiMat.EnableEmission0, "EnableEmission0 should be false when _EnableEmission and _EmissionStrength are missing.");
            Assert.IsFalse(poiMat.EnableEmission1, "EnableEmission1 should be false when _EnableEmission1 and _EmissionStrength1 are missing.");
            Assert.IsFalse(poiMat.EnableEmission2, "EnableEmission2 should be false when _EnableEmission2 and _EmissionStrength2 are missing.");
            Assert.IsFalse(poiMat.EnableEmission3, "EnableEmission3 should be false when _EnableEmission3 and _EmissionStrength3 are missing.");
            Assert.IsFalse(poiMat.UseEmission, "UseEmission should be false when all channel toggles and fallbacks are missing.");
            Assert.IsFalse(poiMat.UseMatcap, "UseMatcap should be false when _MatcapEnable and _Matcap are missing.");
            Assert.IsFalse(poiMat.UseRimLighting, "UseRimLighting should be false when _EnableRimLighting and _RimStyle are missing.");
            Assert.IsFalse(poiMat.UseSpecular, "UseSpecular should be false when _MochieBRDF and _MochieMetallicMaps are missing.");
            Assert.IsFalse(poiMat.UseVertexColor, "UseVertexColor should be false when vertex color properties are missing.");
        }

        /// <summary>
        /// Verify that feature toggles return true based on the fallback value property when the
        /// toggle is absent (simulating a locked Poiyomi shader with that feature enabled).
        /// A dedicated test shader has _EmissionStrength (and siblings) but no _EnableEmission,
        /// which perfectly models the locked-and-enabled scenario.
        /// </summary>
        [Test]
        public void EnableEmission_ReturnTrue_WhenToggleMissingButFallbackExists()
        {
            // Hidden/VRCQuestTools/Test/EmissionStrengthOnly defines _EmissionStrength but NOT _EnableEmission,
            // matching a locked Poiyomi shader where the emission toggle was baked in as permanently on.
            var shader = Shader.Find("Hidden/VRCQuestTools/Test/EmissionStrengthOnly");
            if (shader == null)
            {
                Assert.Ignore("Test emission-strength-only shader not found.");
                return;
            }

            using var material = DisposableObject.New(new Material(shader));
            var poiMat = new PoiyomiMaterial(material.Object);

            Assert.IsTrue(poiMat.EnableEmission0, "EnableEmission0 should be true when _EmissionStrength exists but _EnableEmission is absent.");
            Assert.IsTrue(poiMat.EnableEmission1, "EnableEmission1 should be true when _EmissionStrength1 exists but _EnableEmission1 is absent.");
            Assert.IsTrue(poiMat.EnableEmission2, "EnableEmission2 should be true when _EmissionStrength2 exists but _EnableEmission2 is absent.");
            Assert.IsTrue(poiMat.EnableEmission3, "EnableEmission3 should be true when _EmissionStrength3 exists but _EnableEmission3 is absent.");
            Assert.IsTrue(poiMat.UseEmission, "UseEmission should be true when at least one channel fallback is present.");
        }

        /// <summary>
        /// Verify that setting emission channel toggles does not throw when the underlying shader
        /// properties are absent (simulating a locked Poiyomi shader).
        /// </summary>
        [Test]
        public void SetEnableEmission_DoesNotThrow_WhenPropertyMissing()
        {
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                Assert.Ignore("Standard shader not found.");
                return;
            }

            using var material = DisposableObject.New(new Material(shader));
            var poiMat = new PoiyomiMaterial(material.Object);

            Assert.DoesNotThrow(() => { poiMat.EnableEmission0 = false; }, "Setting EnableEmission0 on missing property should not throw.");
            Assert.DoesNotThrow(() => { poiMat.EnableEmission1 = false; }, "Setting EnableEmission1 on missing property should not throw.");
            Assert.DoesNotThrow(() => { poiMat.EnableEmission2 = false; }, "Setting EnableEmission2 on missing property should not throw.");
            Assert.DoesNotThrow(() => { poiMat.EnableEmission3 = false; }, "Setting EnableEmission3 on missing property should not throw.");
        }
    }
}
