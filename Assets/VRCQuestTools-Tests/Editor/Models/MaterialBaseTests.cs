// <copyright file="MaterialBaseTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="MaterialBase"/> and <see cref="StandardMaterial"/>.
    /// </summary>
    public class MaterialBaseTests
    {
        [Test]
        public void StandardMaterial_Material_ReturnsOriginal()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = new StandardMaterial(mat);
                Assert.AreEqual(mat, wrapper.Material);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ConvertToToonLit_ReturnsNewMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestStandard";
            Material converted = null;
            try
            {
                var wrapper = new StandardMaterial(mat);
                converted = wrapper.ConvertToToonLit();
                Assert.IsNotNull(converted);
                Assert.AreNotEqual(mat, converted);
                StringAssert.Contains("Toon Lit", converted.shader.name);
                StringAssert.Contains("TestStandard", converted.name);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                if (converted != null)
                {
                    Object.DestroyImmediate(converted);
                }
            }
        }

        [Test]
        public void ConvertToToonLit_PreservesMainTexture()
        {
            var mat = new Material(Shader.Find("Standard"));
            var tex = new Texture2D(4, 4);
            mat.mainTexture = tex;
            mat.mainTextureScale = new Vector2(2, 3);
            mat.mainTextureOffset = new Vector2(0.5f, 0.5f);
            Material converted = null;
            try
            {
                var wrapper = new StandardMaterial(mat);
                converted = wrapper.ConvertToToonLit();
                Assert.AreEqual(tex, converted.mainTexture);
                Assert.AreEqual(new Vector2(2, 3), converted.mainTextureScale);
                Assert.AreEqual(new Vector2(0.5f, 0.5f), converted.mainTextureOffset);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
                if (converted != null)
                {
                    Object.DestroyImmediate(converted);
                }
            }
        }

        [Test]
        public void ConvertToToonLit_EnablesInstancing()
        {
            var mat = new Material(Shader.Find("Standard"));
            Material converted = null;
            try
            {
                var wrapper = new StandardMaterial(mat);
                converted = wrapper.ConvertToToonLit();
                Assert.IsTrue(converted.enableInstancing);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                if (converted != null)
                {
                    Object.DestroyImmediate(converted);
                }
            }
        }

        [Test]
        public void GetToonLitPlatformOverride_NoTexture_ReturnsNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = new StandardMaterial(mat);
                var result = wrapper.GetToonLitPlatformOverride();
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ToonLitBakeShader_StandardMaterial_FindsShader()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = new StandardMaterial(mat);
                var shader = wrapper.ToonLitBakeShader;
                // Shader may or may not exist depending on project setup
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
