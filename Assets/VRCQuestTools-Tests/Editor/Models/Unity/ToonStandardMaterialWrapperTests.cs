// Tests for ToonStandardMaterialWrapper

using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using KRT.VRCQuestTools.Models.Unity;

namespace KRT.VRCQuestTools.Models.Unity.Tests
{
    [TestFixture]
    public class ToonStandardMaterialWrapperTests
    {
        private Material material;
        private ToonStandardMaterialWrapper wrapper;

        [SetUp]
        public void SetUp()
        {
            // Use a known shader; "VRChat/Mobile/Toon Standard" may not be available
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            material = new Material(shader);
            wrapper = new ToonStandardMaterialWrapper(material);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(material);
        }

        [Test]
        public void Name_GetSet()
        {
            wrapper.Name = "TestMaterial";
            Assert.AreEqual("TestMaterial", wrapper.Name);
        }

        [Test]
        public void MainTexture_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.MainTexture = tex;
                Assert.AreEqual(tex, wrapper.MainTexture);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MainTextureScale_GetSet()
        {
            var scale = new Vector2(2f, 3f);
            wrapper.MainTextureScale = scale;
            Assert.AreEqual(scale, wrapper.MainTextureScale);
        }

        [Test]
        public void MainTextureOffset_GetSet()
        {
            var offset = new Vector2(0.5f, 0.25f);
            wrapper.MainTextureOffset = offset;
            Assert.AreEqual(offset, wrapper.MainTextureOffset);
        }

        [Test]
        public void MainColor_GetSet()
        {
            var color = new Color(0.1f, 0.2f, 0.3f, 1f);
            wrapper.MainColor = color;
            var result = wrapper.MainColor;
            Assert.AreEqual(color.r, result.r, 0.01f);
            Assert.AreEqual(color.g, result.g, 0.01f);
            Assert.AreEqual(color.b, result.b, 0.01f);
            Assert.AreEqual(color.a, result.a, 0.01f);
        }

        [Test]
        public void UseNormalMap_GetSet()
        {
            wrapper.UseNormalMap = true;
            Assert.IsTrue(wrapper.UseNormalMap);
            wrapper.UseNormalMap = false;
            Assert.IsFalse(wrapper.UseNormalMap);
        }

        [Test]
        public void NormalMap_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.NormalMap = tex;
                Assert.AreEqual(tex, wrapper.NormalMap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void NormalMapTextureScale_GetSet()
        {
            var scale = new Vector2(2f, 2f);
            wrapper.NormalMapTextureScale = scale;
            Assert.AreEqual(scale, wrapper.NormalMapTextureScale);
        }

        [Test]
        public void NormalMapTextureOffset_GetSet()
        {
            var offset = new Vector2(0.3f, 0.7f);
            wrapper.NormalMapTextureOffset = offset;
            Assert.AreEqual(offset, wrapper.NormalMapTextureOffset);
        }

        [Test]
        public void NormalMapScale_GetSet()
        {
            wrapper.NormalMapScale = 1.5f;
            Assert.AreEqual(1.5f, wrapper.NormalMapScale, 0.001f);
        }

        [Test]
        public void Culling_GetSet()
        {
            wrapper.Culling = CullMode.Front;
            Assert.AreEqual(CullMode.Front, wrapper.Culling);
            wrapper.Culling = CullMode.Back;
            Assert.AreEqual(CullMode.Back, wrapper.Culling);
        }

        [Test]
        public void ShadowRamp_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.ShadowRamp = tex;
                Assert.AreEqual(tex, wrapper.ShadowRamp);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void ShadowBoost_GetSet()
        {
            wrapper.ShadowBoost = 0.75f;
            Assert.AreEqual(0.75f, wrapper.ShadowBoost, 0.001f);
        }

        [Test]
        public void ShadowTint_GetSet()
        {
            wrapper.ShadowTint = 0.5f;
            Assert.AreEqual(0.5f, wrapper.ShadowTint, 0.001f);
        }

        [Test]
        public void MinBrightness_GetSet()
        {
            wrapper.MinBrightness = 0.1f;
            Assert.AreEqual(0.1f, wrapper.MinBrightness, 0.001f);
        }

        [Test]
        public void EmissionMap_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.EmissionMap = tex;
                Assert.AreEqual(tex, wrapper.EmissionMap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void EmissionMapTextureScale_GetSet()
        {
            var scale = new Vector2(3f, 3f);
            wrapper.EmissionMapTextureScale = scale;
            Assert.AreEqual(scale, wrapper.EmissionMapTextureScale);
        }

        [Test]
        public void EmissionMapTextureOffset_GetSet()
        {
            var offset = new Vector2(0.1f, 0.9f);
            wrapper.EmissionMapTextureOffset = offset;
            Assert.AreEqual(offset, wrapper.EmissionMapTextureOffset);
        }

        [Test]
        public void EmissionColor_GetSet()
        {
            var color = Color.red;
            wrapper.EmissionColor = color;
            Assert.AreEqual(color, wrapper.EmissionColor);
        }

        [Test]
        public void EmissionUVMap_GetSet()
        {
            wrapper.EmissionUVMap = ToonStandardMaterialWrapper.UVMapMode.UV1;
            Assert.AreEqual(ToonStandardMaterialWrapper.UVMapMode.UV1, wrapper.EmissionUVMap);
        }

        [Test]
        public void EmissionStrength_GetSet()
        {
            wrapper.EmissionStrength = 2.0f;
            Assert.AreEqual(2.0f, wrapper.EmissionStrength, 0.001f);
        }

        [Test]
        public void UseOcclusion_GetSet()
        {
            wrapper.UseOcclusion = true;
            Assert.IsTrue(wrapper.UseOcclusion);
            wrapper.UseOcclusion = false;
            Assert.IsFalse(wrapper.UseOcclusion);
        }

        [Test]
        public void OcclusionMap_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.OcclusionMap = tex;
                Assert.AreEqual(tex, wrapper.OcclusionMap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void OcclusionMapTextureScale_GetSet()
        {
            var scale = new Vector2(1.5f, 1.5f);
            wrapper.OcclusionMapTextureScale = scale;
            Assert.AreEqual(scale, wrapper.OcclusionMapTextureScale);
        }

        [Test]
        public void OcclusionMapTextureOffset_GetSet()
        {
            var offset = new Vector2(0.2f, 0.4f);
            wrapper.OcclusionMapTextureOffset = offset;
            Assert.AreEqual(offset, wrapper.OcclusionMapTextureOffset);
        }

        [Test]
        public void OcclusionMapChannel_GetSet()
        {
            wrapper.OcclusionMapChannel = ToonStandardMaterialWrapper.MaskChannel.G;
            Assert.AreEqual(ToonStandardMaterialWrapper.MaskChannel.G, wrapper.OcclusionMapChannel);
        }

        [Test]
        public void OcclusionStrength_GetSet()
        {
            wrapper.OcclusionStrength = 0.8f;
            Assert.AreEqual(0.8f, wrapper.OcclusionStrength, 0.001f);
        }

        [Test]
        public void UseDetail_GetSet()
        {
            wrapper.UseDetail = true;
            Assert.IsTrue(wrapper.UseDetail);
            wrapper.UseDetail = false;
            Assert.IsFalse(wrapper.UseDetail);
        }

        [Test]
        public void DetailMode_GetSet()
        {
            wrapper.DetailMode = ToonStandardMaterialWrapper.DetailMapMode.Multiply;
            Assert.AreEqual(ToonStandardMaterialWrapper.DetailMapMode.Multiply, wrapper.DetailMode);
        }

        [Test]
        public void DetailMask_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.DetailMask = tex;
                Assert.AreEqual(tex, wrapper.DetailMask);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DetailMaskChannel_GetSet()
        {
            wrapper.DetailMaskChannel = ToonStandardMaterialWrapper.MaskChannel.B;
            Assert.AreEqual(ToonStandardMaterialWrapper.MaskChannel.B, wrapper.DetailMaskChannel);
        }

        [Test]
        public void DetailTexture_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.DetailTexture = tex;
                Assert.AreEqual(tex, wrapper.DetailTexture);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DetailNormalMap_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.DetailNormalMap = tex;
                Assert.AreEqual(tex, wrapper.DetailNormalMap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DetailNormalMapScale_GetSet()
        {
            wrapper.DetailNormalMapScale = 0.5f;
            Assert.AreEqual(0.5f, wrapper.DetailNormalMapScale, 0.001f);
        }

        [Test]
        public void DetailUVMap_GetSet()
        {
            wrapper.DetailUVMap = ToonStandardMaterialWrapper.UVMapMode.UV1;
            Assert.AreEqual(ToonStandardMaterialWrapper.UVMapMode.UV1, wrapper.DetailUVMap);
        }

        [Test]
        public void UseSpecular_GetSet()
        {
            wrapper.UseSpecular = true;
            Assert.IsTrue(wrapper.UseSpecular);
            wrapper.UseSpecular = false;
            Assert.IsFalse(wrapper.UseSpecular);
        }

        [Test]
        public void MetallicMap_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.MetallicMap = tex;
                Assert.AreEqual(tex, wrapper.MetallicMap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MetallicMapTextureScale_GetSet()
        {
            var scale = new Vector2(4f, 4f);
            wrapper.MetallicMapTextureScale = scale;
            Assert.AreEqual(scale, wrapper.MetallicMapTextureScale);
        }

        [Test]
        public void MetallicMapTextureOffset_GetSet()
        {
            var offset = new Vector2(0.6f, 0.7f);
            wrapper.MetallicMapTextureOffset = offset;
            Assert.AreEqual(offset, wrapper.MetallicMapTextureOffset);
        }

        [Test]
        public void MetallicMapChannel_GetSet()
        {
            wrapper.MetallicMapChannel = ToonStandardMaterialWrapper.MaskChannel.A;
            Assert.AreEqual(ToonStandardMaterialWrapper.MaskChannel.A, wrapper.MetallicMapChannel);
        }

        [Test]
        public void MetallicStrength_GetSet()
        {
            wrapper.MetallicStrength = 0.9f;
            Assert.AreEqual(0.9f, wrapper.MetallicStrength, 0.001f);
        }

        [Test]
        public void GlossMap_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.GlossMap = tex;
                Assert.AreEqual(tex, wrapper.GlossMap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GlossMapTextureScale_GetSet()
        {
            var scale = new Vector2(2f, 3f);
            wrapper.GlossMapTextureScale = scale;
            Assert.AreEqual(scale, wrapper.GlossMapTextureScale);
        }

        [Test]
        public void GlossMapTextureOffset_GetSet()
        {
            var offset = new Vector2(0.1f, 0.2f);
            wrapper.GlossMapTextureOffset = offset;
            Assert.AreEqual(offset, wrapper.GlossMapTextureOffset);
        }

        [Test]
        public void GlossMapChannel_GetSet()
        {
            wrapper.GlossMapChannel = ToonStandardMaterialWrapper.MaskChannel.R;
            Assert.AreEqual(ToonStandardMaterialWrapper.MaskChannel.R, wrapper.GlossMapChannel);
        }

        [Test]
        public void GlossStrength_GetSet()
        {
            wrapper.GlossStrength = 0.7f;
            Assert.AreEqual(0.7f, wrapper.GlossStrength, 0.001f);
        }

        [Test]
        public void Sharpness_GetSet()
        {
            wrapper.Sharpness = 0.3f;
            Assert.AreEqual(0.3f, wrapper.Sharpness, 0.001f);
        }

        [Test]
        public void Reflectance_GetSet()
        {
            wrapper.Reflectance = 0.6f;
            Assert.AreEqual(0.6f, wrapper.Reflectance, 0.001f);
        }

        [Test]
        public void UseMatcap_GetSet()
        {
            wrapper.UseMatcap = true;
            Assert.IsTrue(wrapper.UseMatcap);
            wrapper.UseMatcap = false;
            Assert.IsFalse(wrapper.UseMatcap);
        }

        [Test]
        public void Matcap_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.Matcap = tex;
                Assert.AreEqual(tex, wrapper.Matcap);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MatcapMask_GetSet()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                wrapper.MatcapMask = tex;
                Assert.AreEqual(tex, wrapper.MatcapMask);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MatcapMaskTextureScale_GetSet()
        {
            var scale = new Vector2(1.5f, 2.5f);
            wrapper.MatcapMaskTextureScale = scale;
            Assert.AreEqual(scale, wrapper.MatcapMaskTextureScale);
        }

        [Test]
        public void MatcapMaskTextureOffset_GetSet()
        {
            var offset = new Vector2(0.3f, 0.6f);
            wrapper.MatcapMaskTextureOffset = offset;
            Assert.AreEqual(offset, wrapper.MatcapMaskTextureOffset);
        }

        [Test]
        public void MatcapMaskChannel_GetSet()
        {
            wrapper.MatcapMaskChannel = ToonStandardMaterialWrapper.MaskChannel.G;
            Assert.AreEqual(ToonStandardMaterialWrapper.MaskChannel.G, wrapper.MatcapMaskChannel);
        }

        [Test]
        public void MatcapStrength_GetSet()
        {
            wrapper.MatcapStrength = 0.4f;
            Assert.AreEqual(0.4f, wrapper.MatcapStrength, 0.001f);
        }

        [Test]
        public void MatcapType_GetSet()
        {
            wrapper.MatcapType = ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative;
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, wrapper.MatcapType);
        }

        [Test]
        public void UseRimLighting_GetSet()
        {
            wrapper.UseRimLighting = true;
            Assert.IsTrue(wrapper.UseRimLighting);
            wrapper.UseRimLighting = false;
            Assert.IsFalse(wrapper.UseRimLighting);
        }

        [Test]
        public void RimColor_GetSet()
        {
            var color = Color.cyan;
            wrapper.RimColor = color;
            Assert.AreEqual(color, wrapper.RimColor);
        }

        [Test]
        public void RimAlbedoTint_GetSet()
        {
            wrapper.RimAlbedoTint = 0.5f;
            Assert.AreEqual(0.5f, wrapper.RimAlbedoTint, 0.001f);
        }

        [Test]
        public void RimIntensity_GetSet()
        {
            wrapper.RimIntensity = 1.5f;
            Assert.AreEqual(1.5f, wrapper.RimIntensity, 0.001f);
        }

        [Test]
        public void RimRange_GetSet()
        {
            wrapper.RimRange = 0.8f;
            Assert.AreEqual(0.8f, wrapper.RimRange, 0.001f);
        }

        [Test]
        public void RimSoftness_GetSet()
        {
            wrapper.RimSoftness = 0.6f;
            Assert.AreEqual(0.6f, wrapper.RimSoftness, 0.001f);
        }

        [Test]
        public void RimEnvironmental_GetSet()
        {
            wrapper.RimEnvironmental = true;
            Assert.IsTrue(wrapper.RimEnvironmental);
            wrapper.RimEnvironmental = false;
            Assert.IsFalse(wrapper.RimEnvironmental);
        }

        [Test]
        public void ImplicitConversionToMaterial()
        {
            Material m = wrapper;
            Assert.AreEqual(material, m);
        }

        [Test]
        public void DefaultConstructor_CreatesMaterial()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null)
            {
                Assert.Ignore("Toon Standard shader not available");
                return;
            }
            var defaultWrapper = new ToonStandardMaterialWrapper();
            Material m = defaultWrapper;
            Assert.IsNotNull(m);
            Object.DestroyImmediate(m);
        }
    }
}
