// Tests for LilToonMaterial boolean and texture properties not yet covered:
// UseShadow, UseEmission (get/set), EmissionMap, EmissionColor (get/set),
// EmissionBlendMask, UseEmission2nd (get/set), Emission2ndColor (get/set),
// Emission2ndBlendMask, UseReflection, UseMatCap, MatCapTex,
// UseRimLight, AOMap, UseNormalMap, NormalMap.

using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Tests;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    [TestFixture]
    public class LilToonMaterialBoolAndTexturePropertyTests
    {
        private static Shader lilToonShader;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LilToonTestHelper.SkipIfNotImported();

            var lilToonVersion = AssetUtility.LilToonVersion;
            if (lilToonVersion < new SemVer(1, 10, 0) || lilToonVersion >= new SemVer(3, 0, 0))
            {
                Assert.Ignore($"lilToon {lilToonVersion} is not supported.");
            }

            lilToonShader = Shader.Find("lilToon");
            if (lilToonShader == null)
            {
                Assert.Ignore("lilToon shader not available.");
            }
        }

        // --- UseShadow ---

        [Test]
        public void UseShadow_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetFloat("_UseShadow", 1f);
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
            try
            {
                mat.SetFloat("_UseShadow", 0f);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseShadow);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- UseEmission ---

        [Test]
        public void UseEmission_GetSet_True()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                lilMat.UseEmission = true;
                Assert.IsTrue(lilMat.UseEmission);
                Assert.That(mat.GetFloat("_UseEmission"), Is.GreaterThan(0.5f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseEmission_GetSet_False()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                lilMat.UseEmission = false;
                Assert.IsFalse(lilMat.UseEmission);
                Assert.That(mat.GetFloat("_UseEmission"), Is.LessThanOrEqualTo(0.5f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- EmissionMap ---

        [Test]
        public void EmissionMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            try
            {
                mat.SetTexture("_EmissionMap", tex);
                var lilMat = new LilToonMaterial(mat);
                Assert.That(lilMat.EmissionMap, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void EmissionMap_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetTexture("_EmissionMap", null);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsNull(lilMat.EmissionMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- EmissionColor ---

        [Test]
        public void EmissionColor_GetSet()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                var expected = new Color(0.5f, 0.3f, 0.8f, 1.0f);
                lilMat.EmissionColor = expected;
                Assert.That(lilMat.EmissionColor.r, Is.EqualTo(expected.r).Within(0.01f));
                Assert.That(lilMat.EmissionColor.g, Is.EqualTo(expected.g).Within(0.01f));
                Assert.That(lilMat.EmissionColor.b, Is.EqualTo(expected.b).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- EmissionBlendMask ---

        [Test]
        public void EmissionBlendMask_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            try
            {
                mat.SetTexture("_EmissionBlendMask", tex);
                var lilMat = new LilToonMaterial(mat);
                Assert.That(lilMat.EmissionBlendMask, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void EmissionBlendMask_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetTexture("_EmissionBlendMask", null);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsNull(lilMat.EmissionBlendMask);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- UseEmission2nd ---

        [Test]
        public void UseEmission2nd_GetSet_True()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                lilMat.UseEmission2nd = true;
                Assert.IsTrue(lilMat.UseEmission2nd);
                Assert.That(mat.GetFloat("_UseEmission2nd"), Is.GreaterThan(0.5f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseEmission2nd_GetSet_False()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                lilMat.UseEmission2nd = false;
                Assert.IsFalse(lilMat.UseEmission2nd);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- Emission2ndColor ---

        [Test]
        public void Emission2ndColor_GetSet()
        {
            var mat = new Material(lilToonShader);
            try
            {
                var lilMat = new LilToonMaterial(mat);
                var expected = new Color(0.2f, 0.7f, 0.4f, 1.0f);
                lilMat.Emission2ndColor = expected;
                Assert.That(lilMat.Emission2ndColor.r, Is.EqualTo(expected.r).Within(0.01f));
                Assert.That(lilMat.Emission2ndColor.g, Is.EqualTo(expected.g).Within(0.01f));
                Assert.That(lilMat.Emission2ndColor.b, Is.EqualTo(expected.b).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- Emission2ndBlendMask ---

        [Test]
        public void Emission2ndBlendMask_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            try
            {
                mat.SetTexture("_Emission2ndBlendMask", tex);
                var lilMat = new LilToonMaterial(mat);
                Assert.That(lilMat.Emission2ndBlendMask, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        // --- UseReflection ---

        [Test]
        public void UseReflection_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetFloat("_UseReflection", 1f);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseReflection);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseReflection_Disabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetFloat("_UseReflection", 0f);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseReflection);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- UseMatCap ---

        [Test]
        public void UseMatCap_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetFloat("_UseMatCap", 1f);
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
            try
            {
                mat.SetFloat("_UseMatCap", 0f);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseMatCap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- MatCapTex ---

        [Test]
        public void MatCapTex_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            try
            {
                mat.SetTexture("_MatCapTex", tex);
                var lilMat = new LilToonMaterial(mat);
                Assert.That(lilMat.MatCapTex, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void MatCapTex_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetTexture("_MatCapTex", null);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsNull(lilMat.MatCapTex);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- UseRimLight ---

        [Test]
        public void UseRimLight_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetFloat("_UseRim", 1f);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsTrue(lilMat.UseRimLight);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void UseRimLight_Disabled_ReturnsFalse()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetFloat("_UseRim", 0f);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseRimLight);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- AOMap ---

        [Test]
        public void AOMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            try
            {
                mat.SetTexture("_ShadowBorderMask", tex);
                var lilMat = new LilToonMaterial(mat);
                Assert.That(lilMat.AOMap, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void AOMap_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetTexture("_ShadowBorderMask", null);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsNull(lilMat.AOMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- UseNormalMap ---

        [Test]
        public void UseNormalMap_Enabled_ReturnsTrue()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetFloat("_UseBumpMap", 1f);
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
            try
            {
                mat.SetFloat("_UseBumpMap", 0f);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsFalse(lilMat.UseNormalMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        // --- NormalMap ---

        [Test]
        public void NormalMap_WithTexture_ReturnsTexture()
        {
            var mat = new Material(lilToonShader);
            var tex = new Texture2D(4, 4);
            try
            {
                mat.SetTexture("_BumpMap", tex);
                var lilMat = new LilToonMaterial(mat);
                Assert.That(lilMat.NormalMap, Is.EqualTo(tex));
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void NormalMap_NoTexture_ReturnsNull()
        {
            var mat = new Material(lilToonShader);
            try
            {
                mat.SetTexture("_BumpMap", null);
                var lilMat = new LilToonMaterial(mat);
                Assert.IsNull(lilMat.NormalMap);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
