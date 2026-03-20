// <copyright file="MaterialWrapperBuilderTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="MaterialWrapperBuilder"/>.
    /// </summary>
    public class MaterialWrapperBuilderTests
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
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, category);
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
                Assert.Ignore("Standard (Specular setup) shader not found");
                return;
            }

            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, category);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_Unlit_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                Assert.Ignore("Unlit/Texture shader not found");
                return;
            }

            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, category);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_VRChatMobile_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Standard Lite");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Standard Lite shader not found");
                return;
            }

            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, category);
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
            var shader = Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                Assert.Ignore("Unlit/Texture shader not found");
                return;
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
    }
}
