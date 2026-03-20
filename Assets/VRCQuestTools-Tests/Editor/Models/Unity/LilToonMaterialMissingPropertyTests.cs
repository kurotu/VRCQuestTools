// Tests for LilToonMaterial properties that are not yet tested:
// UseShadow, UseNormalMap, NormalMap, UseEmission, EmissionMap, EmissionColor,
// EmissionBlendMask, UseEmission2nd, Emission2ndColor, Emission2ndBlendMask,
// UseMatCap, MatCapTex.

using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    [TestFixture]
    public class LilToonMaterialMissingPropertyTests
    {
        private static bool isLilToonAvailable;
        private static Shader lilToonShader;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                return;
            }

            var lilToonVersion = AssetUtility.LilToonVersion;
            if (lilToonVersion < new SemVer(1, 10, 0) || lilToonVersion >= new SemVer(3, 0, 0))
            {
                return;
            }

            lilToonShader = Shader.Find("lilToon");
            isLilToonAvailable = lilToonShader != null;
        }

        [SetUp]
        public void SetUp()
        {
            if (!isLilToonAvailable)
            {
                Assert.Ignore("lilToon shader not available.");
            }
        }

        [Test]
        public void UseShadow_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 1.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseShadow);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseShadow_Disabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseShadow", 0.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseShadow);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseNormalMap_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseBumpMap", 1.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseNormalMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseNormalMap_Disabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseBumpMap", 0.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseNormalMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void NormalMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_BumpMap", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.NormalMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void NormalMap_WithoutTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsNull(lilMat.NormalMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseEmission_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 1.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseEmission);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseEmission_Disabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseEmission);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseEmission_SetTrue_UpdatesMaterial()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission", 0.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                lilMat.UseEmission = true;
                Assert.AreEqual(1.0f, mat.GetFloat("_UseEmission"), 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void EmissionMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_EmissionMap", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.EmissionMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void EmissionColor_GetAndSet()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                var color = new Color(1.0f, 0.5f, 0.25f, 1.0f);
                lilMat.EmissionColor = color;
                Assert.AreEqual(color.r, lilMat.EmissionColor.r, 0.01f);
                Assert.AreEqual(color.g, lilMat.EmissionColor.g, 0.01f);
                Assert.AreEqual(color.b, lilMat.EmissionColor.b, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void EmissionBlendMask_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_EmissionBlendMask", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.EmissionBlendMask);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void UseEmission2nd_GetAndSet()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseEmission2nd", 0.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseEmission2nd);
                lilMat.UseEmission2nd = true;
                Assert.IsTrue(lilMat.UseEmission2nd);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Emission2ndColor_GetAndSet()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                var color = new Color(0.2f, 0.8f, 0.4f, 1.0f);
                lilMat.Emission2ndColor = color;
                Assert.AreEqual(color.r, lilMat.Emission2ndColor.r, 0.01f);
                Assert.AreEqual(color.g, lilMat.Emission2ndColor.g, 0.01f);
                Assert.AreEqual(color.b, lilMat.Emission2ndColor.b, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Emission2ndBlendMask_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_Emission2ndBlendMask", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.Emission2ndBlendMask);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void UseMatCap_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 1.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseMatCap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseMatCap_Disabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseMatCap", 0.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseMatCap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void MatCapTex_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_MatCapTex", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.MatCapTex);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MatCapTex_WithoutTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsNull(lilMat.MatCapTex);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void AOMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            mat.SetTexture("_ShadowBorderMask", tex);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.AreEqual(tex, lilMat.AOMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void UseRimLight_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseRim", 1.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseRimLight);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseReflection_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            mat.SetFloat("_UseReflection", 1.0f);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseReflection);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
