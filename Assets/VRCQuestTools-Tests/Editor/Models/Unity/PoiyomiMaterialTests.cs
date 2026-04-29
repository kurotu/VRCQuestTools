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
    }
}
