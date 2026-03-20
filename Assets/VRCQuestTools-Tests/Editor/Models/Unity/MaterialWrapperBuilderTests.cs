// <copyright file="MaterialWrapperBuilderTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.Models.Unity
{
    [TestFixture]
    internal class MaterialWrapperBuilderTests
    {
        private MaterialWrapperBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MaterialWrapperBuilder();
        }

        [Test]
        public void DetectShaderCategory_Standard_ReturnsStandard()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_StandardSpecular_ReturnsStandard()
        {
            var shader = Shader.Find("Standard (Specular setup)");
            if (shader == null)
            {
                Assert.Ignore("Standard (Specular setup) shader not available");
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_Unlit_ReturnsUnlit()
        {
            var mat = new Material(Shader.Find("Unlit/Color"));
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_VRCMobile_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Lit shader not available");
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnknownShader_ReturnsUnverified()
        {
            // Use a shader that doesn't match any known category
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Assert.Ignore("Hidden/Internal-Colored shader not available");
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unverified, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Standard_ReturnsStandardMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Unlit_ReturnsStandardMaterial()
        {
            var mat = new Material(Shader.Find("Unlit/Color"));
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_QuestShader_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Lit shader not available");
            }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Unverified_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Assert.Ignore("Hidden/Internal-Colored shader not available");
            }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ShaderCategory_AllValuesAreDefined()
        {
            var values = System.Enum.GetValues(typeof(MaterialWrapperBuilder.ShaderCategory));
            Assert.IsTrue(values.Length >= 11, "Expected at least 11 shader categories");
        }

        [Test]
        public void Build_LilToon_ReturnsLilToonMaterial()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null)
            {
                Assert.Ignore("lilToon shader not available");
            }

            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsInstanceOf<LilToonMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_LilToon_ReturnsLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null)
            {
                Assert.Ignore("lilToon shader not available");
            }

            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.LilToon, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnlitTexture_ReturnsUnlit()
        {
            var mat = new Material(Shader.Find("Unlit/Texture"));
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_Poiyomi_ReturnsPoiyomi()
        {
            var shader = Shader.Find(".poiyomi/Poiyomi Toon");
            if (shader == null)
            {
                Assert.Ignore("Poiyomi shader not available");
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Poiyomi, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_UTS2_ReturnsUTS2Material()
        {
            var testBuilder = new TestableShaderCategoryBuilder(MaterialWrapperBuilder.ShaderCategory.UTS2);
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = testBuilder.Build(mat);
                Assert.IsInstanceOf<UTS2Material>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Arktoon_ReturnsArktoonMaterial()
        {
            var testBuilder = new TestableShaderCategoryBuilder(MaterialWrapperBuilder.ShaderCategory.Arktoon);
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = testBuilder.Build(mat);
                Assert.IsInstanceOf<ArktoonMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_AXCS_ReturnsArktoonMaterial()
        {
            var testBuilder = new TestableShaderCategoryBuilder(MaterialWrapperBuilder.ShaderCategory.AXCS);
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = testBuilder.Build(mat);
                Assert.IsInstanceOf<ArktoonMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Sunao_ReturnsSunaoMaterial()
        {
            var testBuilder = new TestableShaderCategoryBuilder(MaterialWrapperBuilder.ShaderCategory.Sunao);
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = testBuilder.Build(mat);
                Assert.IsInstanceOf<SunaoMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Poiyomi_ReturnsPoiyomiMaterial()
        {
            var testBuilder = new TestableShaderCategoryBuilder(MaterialWrapperBuilder.ShaderCategory.Poiyomi);
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = testBuilder.Build(mat);
                Assert.IsInstanceOf<PoiyomiMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_VirtualLens2_ReturnsVirtualLens2Material()
        {
            var testBuilder = new TestableShaderCategoryBuilder(MaterialWrapperBuilder.ShaderCategory.VirtualLens2);
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = testBuilder.Build(mat);
                Assert.IsInstanceOf<VirtualLens2Material>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        /// <summary>
        /// Subclass to force specific ShaderCategory for Build() branch testing.
        /// </summary>
        private class TestableShaderCategoryBuilder : MaterialWrapperBuilder
        {
            private readonly ShaderCategory category;

            internal TestableShaderCategoryBuilder(ShaderCategory category)
            {
                this.category = category;
            }

            internal override ShaderCategory DetectShaderCategory(Material material) => category;
        }
    }
}
