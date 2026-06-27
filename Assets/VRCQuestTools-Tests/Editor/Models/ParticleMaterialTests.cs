// <copyright file="ParticleMaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BlendMode = UnityEngine.Rendering.BlendMode;
using BlendOp = UnityEngine.Rendering.BlendOp;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for ParticleMaterial.
    /// </summary>
    public class ParticleMaterialTests
    {
        private const string ToonLit = "VRChat/Mobile/Toon Lit";
        private const string Additive = "VRChat/Mobile/Particles/Additive";
        private const string Multiply = "VRChat/Mobile/Particles/Multiply";

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleMaterialTests"/> class.
        /// </summary>
        public ParticleMaterialTests()
        {
            CacheManager.Texture.Clear();
        }

        /// <summary>
        /// Standard particle shader is detected as ParticleMaterial.
        /// </summary>
        [Test]
        public void DetectsStandardParticleAsParticleMaterial()
        {
            using (var mat = NewMaterial("Particles/Standard Unlit"))
            {
                var wrapper = new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(typeof(ParticleMaterial), wrapper.GetType());
            }
        }

        /// <summary>
        /// Blend state One/Zero is opaque and converts to Toon Lit.
        /// </summary>
        [Test]
        public void OpaqueConvertsToToonLit()
        {
            AssertDestinationByBlendState(BlendMode.One, BlendMode.Zero, false, ParticleMaterial.ParticleBlend.Opaque, ToonLit);
        }

        /// <summary>
        /// Blend state One/Zero with alpha test is cutout and converts to Additive.
        /// </summary>
        [Test]
        public void CutoutConvertsToAdditive()
        {
            AssertDestinationByBlendState(BlendMode.One, BlendMode.Zero, true, ParticleMaterial.ParticleBlend.Cutout, Additive);
        }

        /// <summary>
        /// Blend state SrcAlpha/OneMinusSrcAlpha is fade and converts to Additive.
        /// </summary>
        [Test]
        public void FadeConvertsToAdditive()
        {
            AssertDestinationByBlendState(BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, false, ParticleMaterial.ParticleBlend.Fade, Additive);
        }

        /// <summary>
        /// Blend state One/OneMinusSrcAlpha is premultiplied transparent and converts to Additive.
        /// </summary>
        [Test]
        public void TransparentConvertsToAdditive()
        {
            AssertDestinationByBlendState(BlendMode.One, BlendMode.OneMinusSrcAlpha, false, ParticleMaterial.ParticleBlend.Transparent, Additive);
        }

        /// <summary>
        /// Blend state SrcAlpha/One is additive and converts to Additive.
        /// </summary>
        [Test]
        public void AdditiveConvertsToAdditive()
        {
            AssertDestinationByBlendState(BlendMode.SrcAlpha, BlendMode.One, false, ParticleMaterial.ParticleBlend.Additive, Additive);
        }

        /// <summary>
        /// Blend state Zero/OneMinusSrcColor is subtractive and converts to Multiply with inverted RGB.
        /// </summary>
        [Test]
        public void SubtractiveConvertsToMultiplyInverted()
        {
            using (var mat = NewParticleWithBlend(BlendMode.Zero, BlendMode.OneMinusSrcColor, false))
            {
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(ParticleMaterial.ParticleBlend.Subtractive, particle.Blend);
                Assert.AreEqual(Multiply, particle.DestinationShaderName);
                Assert.IsTrue(particle.Invert);
            }
        }

        /// <summary>
        /// Blend state DstColor/Zero is modulate and converts to Multiply.
        /// </summary>
        [Test]
        public void ModulateConvertsToMultiply()
        {
            using (var mat = NewParticleWithBlend(BlendMode.DstColor, BlendMode.Zero, false))
            {
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(ParticleMaterial.ParticleBlend.Modulate, particle.Blend);
                Assert.AreEqual(Multiply, particle.DestinationShaderName);
                Assert.IsFalse(particle.Invert);
            }
        }

        /// <summary>
        /// The real Standard Particles "Modulate" blend state (DstColor/OneMinusSrcAlpha, set by Unity's
        /// shader GUI) converts to Multiply.
        /// </summary>
        [Test]
        public void StandardModulateBlendConvertsToMultiply()
        {
            AssertDestinationByBlendState(BlendMode.DstColor, BlendMode.OneMinusSrcAlpha, false, ParticleMaterial.ParticleBlend.Modulate, Multiply);
        }

        /// <summary>
        /// The real Standard Particles "Subtractive" blend state (SrcAlpha/One with ReverseSubtract op,
        /// set by Unity's shader GUI) converts to Multiply.
        /// </summary>
        [Test]
        public void StandardSubtractiveBlendConvertsToMultiply()
        {
            using (var mat = NewParticleWithBlend(BlendMode.SrcAlpha, BlendMode.One, BlendOp.ReverseSubtract, false))
            {
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(ParticleMaterial.ParticleBlend.Subtractive, particle.Blend);
                Assert.AreEqual(Multiply, particle.DestinationShaderName);
                Assert.IsTrue(particle.Invert);
            }
        }

        /// <summary>
        /// VRChat/Mobile/Particles/Additive passes through to Additive.
        /// </summary>
        [Test]
        public void VRChatMobileAdditivePassesThrough()
        {
            AssertDestinationByShaderName(Additive, ParticleMaterial.ParticleBlend.Additive, Additive);
        }

        /// <summary>
        /// VRChat/Mobile/Particles/Multiply passes through to Multiply.
        /// </summary>
        [Test]
        public void VRChatMobileMultiplyPassesThrough()
        {
            AssertDestinationByShaderName(Multiply, ParticleMaterial.ParticleBlend.Modulate, Multiply);
        }

        /// <summary>
        /// VRChat/Mobile/Particles/Alpha Blended falls back to Additive.
        /// </summary>
        [Test]
        public void VRChatMobileAlphaBlendedFallsBackToAdditive()
        {
            AssertDestinationByShaderName("VRChat/Mobile/Particles/Alpha Blended", ParticleMaterial.ParticleBlend.Fade, Additive);
        }

        /// <summary>
        /// Legacy additive particle shader converts to Additive.
        /// </summary>
        [Test]
        public void LegacyAdditiveConvertsToAdditive()
        {
            AssertDestinationByShaderName("Legacy Shaders/Particles/Additive", ParticleMaterial.ParticleBlend.Additive, Additive);
        }

        /// <summary>
        /// Legacy multiply particle shader converts to Multiply.
        /// </summary>
        [Test]
        public void LegacyMultiplyConvertsToMultiply()
        {
            AssertDestinationByShaderName("Legacy Shaders/Particles/Multiply", ParticleMaterial.ParticleBlend.Modulate, Multiply);
        }

        /// <summary>
        /// Converted material uses the destination shader and enables instancing.
        /// </summary>
        [Test]
        public void CreateConvertedMaterialUsesDestinationShader()
        {
            TestUtils.AssertIgnoreOnMissingShader(Additive);
            using (var mat = NewParticleWithBlend(BlendMode.SrcAlpha, BlendMode.One, false))
            {
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                using (var converted = DisposableObject.New(particle.CreateConvertedMaterial()))
                {
                    Assert.AreEqual(Additive, converted.Object.shader.name);
                    Assert.IsTrue(converted.Object.enableInstancing);
                }
            }
        }

        /// <summary>
        /// The original main texture is reused for a no-op bake on a unity_builtin_extra texture.
        /// </summary>
        [Test]
        public void SkipsBakeForBuiltinTexture()
        {
            TestUtils.AssertIgnoreOnMissingShader(Additive);
            var builtin = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
            if (builtin == null || AssetDatabase.GetAssetPath(builtin) != "Resources/unity_builtin_extra")
            {
                Assert.Ignore("Default-Particle builtin texture is not available.");
            }

            using (var mat = NewParticleWithBlend(BlendMode.SrcAlpha, BlendMode.One, false))
            {
                mat.Object.mainTexture = builtin;
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.IsTrue(particle.ShouldUseOriginalMainTexture);
            }
        }

        /// <summary>
        /// A project texture is not skipped even when texture processing is logically a no-op.
        /// </summary>
        [Test]
        public void DoesNotSkipBakeForProjectTexture()
        {
            using (var mat = NewParticleWithBlend(BlendMode.SrcAlpha, BlendMode.One, false))
            {
                mat.Object.mainTexture = TestUtils.LoadFixtureAssetAtPath<Texture2D>("Textures/albedo_1024px_png.png");
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.IsFalse(particle.ShouldUseOriginalMainTexture);
            }
        }

        /// <summary>
        /// A non-white tint is not skipped.
        /// </summary>
        [Test]
        public void DoesNotSkipBakeForTintedMaterial()
        {
            var builtin = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
            if (builtin == null || AssetDatabase.GetAssetPath(builtin) != "Resources/unity_builtin_extra")
            {
                Assert.Ignore("Default-Particle builtin texture is not available.");
            }

            using (var mat = NewParticleWithBlend(BlendMode.SrcAlpha, BlendMode.One, false))
            {
                mat.Object.mainTexture = builtin;
                mat.Object.SetColor("_Color", Color.red);
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.IsFalse(particle.ShouldUseOriginalMainTexture);
            }
        }

        /// <summary>
        /// Cutout baking punches alpha to binary {0, 1} by threshold.
        /// </summary>
        [Test]
        public void CutoutBakesBinaryAlpha()
        {
            using (var mat = NewParticleWithBlend(BlendMode.One, BlendMode.Zero, true))
            {
                mat.Object.mainTexture = TestUtils.LoadFixtureAssetAtPath<Texture2D>("Textures/alpha_test.png");
                mat.Object.SetFloat("_Cutoff", 0.5f);
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(ParticleMaterial.ParticleBlend.Cutout, particle.Blend);

                Texture2D texObj = null;
                particle.GenerateParticleImage(0, (t) => { texObj = t; }).WaitForCompletion();
                using (var tex = DisposableObject.New(texObj))
                {
                    var pixels = TestUtils.CopyTextureAsReadable(tex.Object).GetPixels32();
                    Assert.IsTrue(pixels.All(p => p.a == 0 || p.a == 255), "Cutout alpha must be binary.");
                }
            }
        }

        /// <summary>
        /// Transparent (premultiplied) baking forces alpha to 1.
        /// </summary>
        [Test]
        public void TransparentBakesOpaqueAlpha()
        {
            using (var mat = NewParticleWithBlend(BlendMode.One, BlendMode.OneMinusSrcAlpha, false))
            {
                mat.Object.mainTexture = TestUtils.LoadFixtureAssetAtPath<Texture2D>("Textures/alpha_test.png");
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(ParticleMaterial.ParticleBlend.Transparent, particle.Blend);

                Texture2D texObj = null;
                particle.GenerateParticleImage(0, (t) => { texObj = t; }).WaitForCompletion();
                using (var tex = DisposableObject.New(texObj))
                {
                    var pixels = TestUtils.CopyTextureAsReadable(tex.Object).GetPixels32();
                    Assert.IsTrue(pixels.All(p => p.a == 255), "Transparent bake must force alpha to 1.");
                }
            }
        }

        /// <summary>
        /// Baking a non-readable built-in texture (e.g. Default-Particle) does not throw or log an error.
        /// </summary>
        [Test]
        public void BakesNonReadableBuiltinTextureWithoutError()
        {
            var builtin = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
            if (builtin == null)
            {
                Assert.Ignore("Default-Particle builtin texture is not available.");
            }

            // Cutout requires baking (alpha threshold), so the original texture cannot be reused.
            using (var mat = NewParticleWithBlend(BlendMode.One, BlendMode.Zero, true))
            {
                mat.Object.mainTexture = builtin;
                mat.Object.SetFloat("_Cutoff", 0.5f);
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(ParticleMaterial.ParticleBlend.Cutout, particle.Blend);
                Assert.IsFalse(particle.ShouldUseOriginalMainTexture);

                Texture2D texObj = null;
                Assert.DoesNotThrow(() => particle.GenerateParticleImage(0, (t) => { texObj = t; }).WaitForCompletion());
                using (var tex = DisposableObject.New(texObj))
                {
                    Assert.NotNull(tex.Object);
                }
            }
        }

        /// <summary>
        /// A ParticleSystem material with a non-particle shader is wrapped as RenderedParticleMaterial and converts to Additive.
        /// </summary>
        [Test]
        public void ParticleSystemMaterialWithNonParticleShaderBecomesRendered()
        {
            TestUtils.AssertIgnoreOnMissingShader("Unlit/Transparent");
            using (var mat = DisposableObject.New(new Material(Shader.Find("Unlit/Transparent"))))
            {
                var wrapper = new MaterialWrapperBuilder().Build(mat.Object, true);
                Assert.AreEqual(typeof(RenderedParticleMaterial), wrapper.GetType());
                var particle = (ParticleMaterial)wrapper;
                Assert.AreEqual(Additive, particle.DestinationShaderName);
                Assert.IsFalse(particle.ShouldUseOriginalMainTexture);
            }
        }

        /// <summary>
        /// A ParticleSystem material with a recognized particle shader stays a normal ParticleMaterial.
        /// </summary>
        [Test]
        public void ParticleSystemMaterialWithParticleShaderStaysParticleMaterial()
        {
            using (var mat = NewParticleWithBlend(BlendMode.SrcAlpha, BlendMode.One, false))
            {
                var wrapper = new MaterialWrapperBuilder().Build(mat.Object, true);
                Assert.AreEqual(typeof(ParticleMaterial), wrapper.GetType());
            }
        }

        /// <summary>
        /// A non-particle shader that is not used by a ParticleSystem is not treated as a particle material.
        /// </summary>
        [Test]
        public void NonParticleShaderNotRenderedWhenNotOnParticleSystem()
        {
            TestUtils.AssertIgnoreOnMissingShader("Unlit/Transparent");
            using (var mat = DisposableObject.New(new Material(Shader.Find("Unlit/Transparent"))))
            {
                var wrapper = new MaterialWrapperBuilder().Build(mat.Object, false);
                Assert.IsFalse(wrapper is ParticleMaterial);
            }
        }

        /// <summary>
        /// Rendering the material's own shader bakes a texture that reflects transparency (alpha).
        /// </summary>
        [Test]
        public void RenderedParticleBakeReflectsTransparency()
        {
            TestUtils.AssertIgnoreOnMissingShader("Unlit/Transparent");
            using (var mat = DisposableObject.New(new Material(Shader.Find("Unlit/Transparent"))))
            {
                mat.Object.mainTexture = TestUtils.LoadFixtureAssetAtPath<Texture2D>("Textures/alpha_test.png");
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object, true);

                Texture2D texObj = null;
                Assert.DoesNotThrow(() => particle.GenerateParticleImage(0, (t) => { texObj = t; }).WaitForCompletion());
                using (var tex = DisposableObject.New(texObj))
                {
                    Assert.NotNull(tex.Object);
                    var pixels = TestUtils.CopyTextureAsReadable(tex.Object).GetPixels32();
                    Assert.IsTrue(pixels.Any(p => p.a < 255), "Rendered result must reflect transparency.");
                }
            }
        }

        /// <summary>
        /// BuildIgnoringParticleCategory treats a recognized particle shader as a generic material so that an
        /// explicit per-material convert setting is honored instead of automatic particle conversion.
        /// </summary>
        [Test]
        public void BuildIgnoringParticleCategoryTreatsParticleShaderAsGeneric()
        {
            using (var mat = NewParticleWithBlend(BlendMode.SrcAlpha, BlendMode.One, false))
            {
                var wrapper = new MaterialWrapperBuilder().BuildIgnoringParticleCategory(mat.Object);
                Assert.IsFalse(wrapper is ParticleMaterial, "Explicit settings must bypass particle conversion.");
                Assert.AreEqual(typeof(StandardMaterial), wrapper.GetType());
            }
        }

        /// <summary>
        /// BuildIgnoringParticleCategory does not render a ParticleSystem material with a non-particle shader.
        /// </summary>
        [Test]
        public void BuildIgnoringParticleCategoryDoesNotRenderNonParticleShader()
        {
            TestUtils.AssertIgnoreOnMissingShader("Unlit/Transparent");
            using (var mat = DisposableObject.New(new Material(Shader.Find("Unlit/Transparent"))))
            {
                var wrapper = new MaterialWrapperBuilder().BuildIgnoringParticleCategory(mat.Object);
                Assert.IsFalse(wrapper is ParticleMaterial);
            }
        }

        private void AssertDestinationByBlendState(BlendMode src, BlendMode dst, bool alphaTest, ParticleMaterial.ParticleBlend expectedBlend, string expectedShader)
        {
            using (var mat = NewParticleWithBlend(src, dst, alphaTest))
            {
                var particle = (ParticleMaterial)new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(expectedBlend, particle.Blend);
                Assert.AreEqual(expectedShader, particle.DestinationShaderName);
            }
        }

        private void AssertDestinationByShaderName(string shaderName, ParticleMaterial.ParticleBlend expectedBlend, string expectedShader)
        {
            TestUtils.AssertIgnoreOnMissingShader(shaderName);
            using (var mat = NewMaterial(shaderName))
            {
                var wrapper = new MaterialWrapperBuilder().Build(mat.Object);
                Assert.AreEqual(typeof(ParticleMaterial), wrapper.GetType());
                var particle = (ParticleMaterial)wrapper;
                Assert.AreEqual(expectedBlend, particle.Blend);
                Assert.AreEqual(expectedShader, particle.DestinationShaderName);
            }
        }

        private DisposableObject<Material> NewMaterial(string shaderName)
        {
            TestUtils.AssertIgnoreOnMissingShader(shaderName);
            return DisposableObject.New(new Material(Shader.Find(shaderName)));
        }

        private DisposableObject<Material> NewParticleWithBlend(BlendMode src, BlendMode dst, bool alphaTest)
        {
            return NewParticleWithBlend(src, dst, BlendOp.Add, alphaTest);
        }

        private DisposableObject<Material> NewParticleWithBlend(BlendMode src, BlendMode dst, BlendOp op, bool alphaTest)
        {
            const string shaderName = "Particles/Standard Unlit";
            TestUtils.AssertIgnoreOnMissingShader(shaderName);
            var mat = DisposableObject.New(new Material(Shader.Find(shaderName)));
            mat.Object.SetFloat("_SrcBlend", (int)src);
            mat.Object.SetFloat("_DstBlend", (int)dst);
            mat.Object.SetFloat("_BlendOp", (int)op);
            if (alphaTest)
            {
                mat.Object.EnableKeyword("_ALPHATEST_ON");
            }
            return mat;
        }
    }
}
