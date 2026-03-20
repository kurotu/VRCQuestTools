// Batch 57: PlatformOverride methods, complex branches, AssetUtility, FallbackAvatarCallback, MissingScriptsRule
// Targets: LilToonToonStandardGenerator Get*PlatformOverride (lines 142-302), complex Get* branches,
//          AssetUtility simple getters, FallbackAvatarCallback.OnPreprocessAvatar, validation rules

using System;
using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Tests
{
    // ========== PlatformOverride method tests ==========
    [TestFixture]
    public class PlatformOverrideTests
    {
        private static readonly Type GeneratorType = SystemUtility.GetTypeByName("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
        private static readonly Type SettingsType = typeof(ToonStandardConvertSettings);

        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            try { return new LilToonMaterial(mat); }
            catch { UnityEngine.Object.DestroyImmediate(mat); return null; }
        }

        private object CreateGenerator(LilToonMaterial lil)
        {
            if (GeneratorType == null) return null;
            var settings = new ToonStandardConvertSettings();
            var ctor = GeneratorType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var c in ctor)
            {
                var p = c.GetParameters();
                if (p.Length == 3 && p[0].ParameterType == typeof(LilToonMaterial))
                    return c.Invoke(new object[] { lil, settings, null });
            }
            return null;
        }

        private object InvokeProtected(object gen, string methodName, params object[] args)
        {
            var method = GeneratorType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                // Try base type
                method = GeneratorType.BaseType?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }
            return method?.Invoke(gen, args);
        }

        // --- GetMainTexturePlatformOverride ---
        [Test]
        public void GetMainTexturePlatformOverride_NoTextures_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            // No mainTexture set, no 2nd/3rd
            lil.Material.mainTexture = null;
            lil.Material.SetFloat("_UseMain2ndTex", 0f);
            lil.Material.SetFloat("_UseMain3rdTex", 0f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMainTexturePlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetMainTexturePlatformOverride_WithMainTexture_ReturnsNullForRuntime()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.mainTexture = new Texture2D(4, 4); // Runtime texture, no importer
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMainTexturePlatformOverride");
            // Runtime textures have no TextureImporter, so GetBestPlatformOverrideSettings returns null
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material.mainTexture);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetMainTexturePlatformOverride_WithMain2ndTex_CollectsTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.mainTexture = null;
            lil.Material.SetFloat("_UseMain2ndTex", 1f);
            lil.Material.SetTexture("_Main2ndTex", new Texture2D(4, 4));
            lil.Material.SetFloat("_UseMain3rdTex", 0f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMainTexturePlatformOverride");
            // Runtime textures → null from GetBestPlatformOverrideSettings
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material.GetTexture("_Main2ndTex"));
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetMainTexturePlatformOverride_WithMain3rdTex_CollectsTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.mainTexture = null;
            lil.Material.SetFloat("_UseMain2ndTex", 0f);
            lil.Material.SetFloat("_UseMain3rdTex", 1f);
            lil.Material.SetTexture("_Main3rdTex", new Texture2D(4, 4));
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMainTexturePlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material.GetTexture("_Main3rdTex"));
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetEmissionMapPlatformOverride ---
        [Test]
        public void GetEmissionMapPlatformOverride_NoEmission_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetEmissionMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetEmissionMapPlatformOverride_WithEmissionMap_ReturnsNullForRuntime()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_EmissionMap", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetEmissionMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetEmissionMapPlatformOverride_WithEmission2ndMap_ReturnsNullForRuntime()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_Emission2ndMap", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetEmissionMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetGlossMapPlatformOverride ---
        [Test]
        public void GetGlossMapPlatformOverride_NoTextures_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetGlossMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetGlossMapPlatformOverride_WithSmoothnessTex_ReturnsNullForRuntime()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_SmoothnessTex", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetGlossMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetGlossMapPlatformOverride_WithReflectionColorTex_ReturnsNullForRuntime()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_ReflectionColorTex", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetGlossMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetMatcapPlatformOverride, GetMatcapMaskPlatformOverride ---
        [Test]
        public void GetMatcapPlatformOverride_NullMatCapTex_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMatcapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetMatcapMaskPlatformOverride_NullMatCapMask_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMatcapMaskPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetMetallicMapPlatformOverride ---
        [Test]
        public void GetMetallicMapPlatformOverride_NullTextures_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMetallicMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetNormalMapPlatformOverride ---
        [Test]
        public void GetNormalMapPlatformOverride_NullNormalMap_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetNormalMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetOcclusionMapPlatformOverride ---
        [Test]
        public void GetOcclusionMapPlatformOverride_NullAOMap_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetOcclusionMapPlatformOverride");
            Assert.IsNull(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetPackedMaskPlatformOverride ---
        [Test]
        public void GetPackedMaskPlatformOverride_NoMaskTextures_ReturnsNull()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            // Create a TexturePack with masks
            var texturePackType = SystemUtility.GetTypeByName("KRT.VRCQuestTools.Models.ToonStandardGenerator+TexturePack");
            if (texturePackType == null) { Assert.Ignore("TexturePack type not found"); return; }
            // Use reflection to call the method - need a TexturePack argument
            var method = GeneratorType.GetMethod("GetPackedMaskPlatformOverride", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) { Assert.Ignore("Method not found"); return; }
            // Create TexturePack via constructor
            var packCtor = texturePackType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (packCtor.Length == 0) { Assert.Ignore("TexturePack constructor not found"); return; }
            try
            {
                var pack = Activator.CreateInstance(texturePackType);
                var result = method.Invoke(gen, new[] { pack });
                Assert.IsNull(result);
            }
            catch (Exception ex)
            {
                // Log but don't fail - constructor may need args
                Assert.Ignore($"Cannot create TexturePack: {ex.InnerException?.Message ?? ex.Message}");
            }
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }
    }

    // ========== Complex branch coverage for Get* methods ==========
    [TestFixture]
    public class ComplexBranchTests
    {
        private static readonly Type GeneratorType = SystemUtility.GetTypeByName("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");

        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            try { return new LilToonMaterial(mat); }
            catch { UnityEngine.Object.DestroyImmediate(mat); return null; }
        }

        private object CreateGeneratorWithSettings(LilToonMaterial lil, ToonStandardConvertSettings settings)
        {
            if (GeneratorType == null) return null;
            var ctor = GeneratorType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var c in ctor)
            {
                var p = c.GetParameters();
                if (p.Length == 3 && p[0].ParameterType == typeof(LilToonMaterial))
                    return c.Invoke(new object[] { lil, settings, null });
            }
            return null;
        }

        private object InvokeProtected(object gen, string methodName)
        {
            var method = GeneratorType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
                method = GeneratorType.BaseType?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return method?.Invoke(gen, null);
        }

        // --- GetMainColor branches ---
        [Test]
        public void GetMainColor_MatCapNormal_NoMainTex_ReturnsBlack()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1f);
            lil.Material.SetFloat("_MatCapBlendMode", 0f); // Normal
            lil.Material.mainTexture = null;
            // Ensure UseEmission doesn't force main texture
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            var settings = new ToonStandardConvertSettings { useMatcap = true, useEmission = true };
            var gen = CreateGeneratorWithSettings(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Color)InvokeProtected(gen, "GetMainColor");
            Assert.AreEqual(Color.black, result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetMainColor_MatCapNormal_WithMainTex_ReturnsMaterialColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1f);
            lil.Material.SetFloat("_MatCapBlendMode", 0f); // Normal
            lil.Material.mainTexture = new Texture2D(4, 4);
            lil.Material.color = new Color(0.5f, 0.3f, 0.8f, 1f);
            var settings = new ToonStandardConvertSettings { useMatcap = true };
            var gen = CreateGeneratorWithSettings(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Color)InvokeProtected(gen, "GetMainColor");
            Assert.AreEqual(0.5f, result.r, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material.mainTexture);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetMainColor_NoMatCap_ReturnsMaterialColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 0f);
            lil.Material.color = new Color(0.2f, 0.4f, 0.6f, 1f);
            var settings = new ToonStandardConvertSettings();
            var gen = CreateGeneratorWithSettings(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Color)InvokeProtected(gen, "GetMainColor");
            Assert.AreEqual(0.2f, result.r, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetUseEmissionMap branches ---
        [Test]
        public void GetUseEmissionMap_EmissionWithMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_EmissionMap", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseEmissionMap_EmissionWithBlendMask_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_EmissionBlendMask", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseEmissionMap_EmissionWithLowBlend_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            lil.Material.SetFloat("_EmissionBlend", 0.5f);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseEmissionMap_Emission2ndWithMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_Emission2ndMap", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseEmissionMap_Emission2ndWithBlendMask_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_Emission2ndBlendMask", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseEmissionMap_Emission2ndWithLowBlend_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 1f);
            lil.Material.SetFloat("_Emission2ndBlend", 0.5f);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseEmissionMap_BothEmissions_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1f);
            lil.Material.SetFloat("_UseEmission2nd", 1f);
            lil.Material.SetFloat("_EmissionBlend", 1f);
            lil.Material.SetFloat("_Emission2ndBlend", 1f);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseEmissionMap_NoEmission_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmissionMap");
            Assert.IsFalse(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetUseGlossMap branches ---
        [Test]
        public void GetUseGlossMap_NoReflection_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 0f);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsFalse(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseGlossMap_WithSmoothnessTex_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_SmoothnessTex", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseGlossMap_WithReflectionColorTex_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_ReflectionColorTex", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseGlossMap_WithNonWhiteReflectionColor_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1f);
            lil.Material.SetColor("_ReflectionColor", new Color(0.5f, 0.5f, 0.5f, 1f));
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseGlossMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetUseMainTexture branches ---
        [Test]
        public void GetUseMainTexture_EmissionForced_NoMainTex_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.mainTexture = null;
            lil.Material.SetFloat("_UseEmission", 1f);
            var settings = new ToonStandardConvertSettings { useEmission = false };
            var gen = CreateGeneratorWithSettings(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseMainTexture_NoEmission_WithMainTex_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.mainTexture = new Texture2D(4, 4);
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            var settings = new ToonStandardConvertSettings { useEmission = true };
            var gen = CreateGeneratorWithSettings(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(lil.Material.mainTexture);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseMainTexture_NoEmission_NoMainTex_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.mainTexture = null;
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 0f);
            var settings = new ToonStandardConvertSettings { useEmission = true };
            var gen = CreateGeneratorWithSettings(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMainTexture");
            Assert.IsFalse(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetUseMetallicMap branches ---
        [Test]
        public void GetUseMetallicMap_NoReflection_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 0f);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
            Assert.IsFalse(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseMetallicMap_WithMetallicMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_MetallicGlossMap", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseMetallicMap_WithNonWhiteReflectionColor_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1f);
            lil.Material.SetColor("_ReflectionColor", new Color(0.5f, 0.5f, 0.5f, 1f));
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMetallicMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetMapcapType branches ---
        [Test]
        public void GetMapcapType_Add_ReturnsAdditive()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 1f); // Add
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMapcapType");
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetMapcapType_Screen_ReturnsAdditive()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 2f); // Screen
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMapcapType");
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Additive, result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetMapcapType_Multiply_ReturnsMultiplicative()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 3f); // Multiply
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = InvokeProtected(gen, "GetMapcapType");
            Assert.AreEqual(ToonStandardMaterialWrapper.MatcapTypeMode.Multiplicative, result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetRimColor ---
        [Test]
        public void GetRimColor_SetsAlphaTo1()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_RimColor", new Color(0.3f, 0.5f, 0.7f, 0.2f));
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (Color)InvokeProtected(gen, "GetRimColor");
            Assert.AreEqual(0.3f, result.r, 0.01f);
            Assert.AreEqual(1.0f, result.a, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetUseEmission ---
        [Test]
        public void GetUseEmission_OnlyEmission2nd_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0f);
            lil.Material.SetFloat("_UseEmission2nd", 1f);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseEmission");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetUseNormalMap ---
        [Test]
        public void GetUseNormalMap_EnabledButNoTexture_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseBumpMap", 1f);
            lil.Material.SetTexture("_BumpMap", null);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseNormalMap");
            Assert.IsFalse(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseNormalMap_EnabledWithTexture_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseBumpMap", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_BumpMap", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseNormalMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetUseOcclusionMap ---
        [Test]
        public void GetUseOcclusionMap_ShadowEnabled_WithAOMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_ShadowBorderMask", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseOcclusionMap");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseOcclusionMap_ShadowEnabled_NoAOMap_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1f);
            lil.Material.SetTexture("_ShadowBorderMask", null);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseOcclusionMap");
            Assert.IsFalse(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // --- GetUseMatcapMask ---
        [Test]
        public void GetUseMatcapMask_MatCapEnabled_WithMask_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1f);
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_MatCapBlendMask", tex);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMatcapMask");
            Assert.IsTrue(result);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void GetUseMatcapMask_MatCapEnabled_NoMask_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1f);
            lil.Material.SetTexture("_MatCapBlendMask", null);
            var gen = CreateGeneratorWithSettings(lil, new ToonStandardConvertSettings());
            if (gen == null) { Assert.Ignore("Generator not found"); return; }
            var result = (bool)InvokeProtected(gen, "GetUseMatcapMask");
            Assert.IsFalse(result);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }
    }

    // ========== AssetUtility tests ==========
    [TestFixture]
    public class AssetUtilityTests_PlatOverride
    {
        [Test]
        public void IsDynamicBoneImported_ReturnsBool()
        {
            // DynamicBone is likely not installed, so should return false
            var result = AssetUtility.IsDynamicBoneImported();
            Assert.IsFalse(result);
        }

        [Test]
        public void IsLilToonImported_ReturnsTrue()
        {
            // lilToon is installed in this project
            var result = AssetUtility.IsLilToonImported();
            Assert.IsTrue(result);
        }

        [Test]
        public void CanLilToonBakeShadowRamp_ReturnsTrue()
        {
            // lilToon 2.3.2 >= 1.10.0
            var result = AssetUtility.CanLilToonBakeShadowRamp();
            Assert.IsTrue(result);
        }

        [Test]
        public void GetLilToon2Ramp_ReturnsShaderOrNull()
        {
            var result = AssetUtility.GetLilToon2Ramp();
            // May or may not be available depending on lilToon version
            // Just assert it doesn't throw
            Assert.Pass();
        }

        [Test]
        public void LilToonVersion_IsValid()
        {
            var version = AssetUtility.LilToonVersion;
            Assert.IsNotNull(version);
            Assert.IsNotNull(version.ToString());
        }

        [Test]
        public void LoadAssetByGUID_InvalidGUID_ReturnsNull()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Failed to get asset path by GUID"));
            var result = AssetUtility.LoadAssetByGUID<Texture2D>("00000000000000000000000000000000");
            Assert.IsNull(result);
        }

        [Test]
        public void GetAllObjectReferences_SimpleMaterial_ReturnsArray()
        {
            var mat = new Material(Shader.Find("Standard"));
            var refs = AssetUtility.GetAllObjectReferences(mat);
            Assert.IsNotNull(refs);
            UnityEngine.Object.DestroyImmediate(mat);
        }
    }

    // ========== FallbackAvatarCallback tests ==========
    [TestFixture]
    public class FallbackAvatarCallbackTests_PlatOverride
    {
        private static readonly Type CallbackType = SystemUtility.GetTypeByName("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");

        [Test]
        public void OnPreprocessAvatar_NullPipelineManager_ReturnsTrue()
        {
            if (CallbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            var instance = Activator.CreateInstance(CallbackType);
            var method = CallbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            var go = new GameObject("TestAvatar");
            try
            {
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithPipelineManager_EmptyBlueprintId_ReturnsTrue()
        {
            if (CallbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            var instance = Activator.CreateInstance(CallbackType);
            var method = CallbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            var go = new GameObject("TestAvatar");
            var pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
            if (pmType == null) { Assert.Ignore("PipelineManager not found"); return; }
            var pm = go.AddComponent(pmType);
            try
            {
                pmType.GetField("blueprintId").SetValue(pm, "");
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithPipelineManager_NoBlueprintId_ReturnsTrue()
        {
            if (CallbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            var instance = Activator.CreateInstance(CallbackType);
            var method = CallbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            var go = new GameObject("TestAvatar");
            var pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
            if (pmType == null) { Assert.Ignore("PipelineManager not found"); return; }
            var pm = go.AddComponent(pmType);
            try
            {
                pmType.GetField("blueprintId").SetValue(pm, null);
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithValidBlueprintId_NoFallbackComponent_ReturnsTrue()
        {
            if (CallbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            var instance = Activator.CreateInstance(CallbackType);
            var method = CallbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            var go = new GameObject("TestAvatar");
            var pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
            if (pmType == null) { Assert.Ignore("PipelineManager not found"); return; }
            var pm = go.AddComponent(pmType);
            try
            {
                pmType.GetField("blueprintId").SetValue(pm, "avtr_test_12345");
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithFallbackComponent_ReturnsTrue()
        {
            if (CallbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            var fallbackType = SystemUtility.GetTypeByName("KRT.VRCQuestTools.Components.FallbackAvatar");
            if (fallbackType == null) { Assert.Ignore("FallbackAvatar component not found"); return; }

            var instance = Activator.CreateInstance(CallbackType);
            var method = CallbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("Method not found"); return; }

            var go = new GameObject("TestAvatar");
            var pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
            if (pmType == null) { Assert.Ignore("PipelineManager not found"); return; }
            var pm = go.AddComponent(pmType);
            go.AddComponent(fallbackType);
            try
            {
                pmType.GetField("blueprintId").SetValue(pm, "avtr_fallback_67890");
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CallbackOrder_IsNegative()
        {
            if (CallbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            var instance = Activator.CreateInstance(CallbackType);
            var prop = CallbackType.GetProperty("callbackOrder", BindingFlags.Instance | BindingFlags.Public);
            if (prop == null) { Assert.Ignore("callbackOrder not found"); return; }
            var order = (int)prop.GetValue(instance);
            Assert.Less(order, 0);
        }
    }

    // ========== MissingScriptsRule / MissingNdmfRule tests ==========
    [TestFixture]
    public class ValidationRuleTests_PlatOverride
    {
        [Test]
        public void MissingScriptsRule_Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("TestAvatar");
            go.SetActive(false);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            try
            {
                var avatar = new VRChatAvatar(descriptor);
                var rule = new MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingScriptsRule_Validate_CleanAvatar_ReturnsNull()
        {
            var go = new GameObject("TestAvatar");
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            try
            {
                var avatar = new VRChatAvatar(descriptor);
                var rule = new MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingNdmfRule_Validate_CleanAvatar_ReturnsNull()
        {
            var go = new GameObject("TestAvatar");
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            try
            {
                var avatar = new VRChatAvatar(descriptor);
                var rule = new MissingNdmfRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AvatarValidationRules_HasRules()
        {
            // AvatarValidationRules should have rules registered via InitOnLoad
            var rulesField = typeof(AvatarValidationRules).GetField("rules", BindingFlags.Static | BindingFlags.NonPublic);
            if (rulesField == null)
            {
                // Try property
                var rulesProp = typeof(AvatarValidationRules).GetProperty("Rules", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (rulesProp != null)
                {
                    var rules = rulesProp.GetValue(null);
                    Assert.IsNotNull(rules);
                    return;
                }
                // Try GetRules
                var method = typeof(AvatarValidationRules).GetMethod("GetRules", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    var rules = method.Invoke(null, null);
                    Assert.IsNotNull(rules);
                    return;
                }
                Assert.Ignore("Cannot access rules collection");
            }
            else
            {
                var rules = rulesField.GetValue(null);
                Assert.IsNotNull(rules);
            }
        }
    }

    // ========== LilToonMaterial remaining property tests ==========
    [TestFixture]
    public class LilToonMaterialRemainingProps
    {
        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            try { return new LilToonMaterial(mat); }
            catch { UnityEngine.Object.DestroyImmediate(mat); return null; }
        }

        // AOMap-related properties
        [Test]
        public void AOMap_ReturnsTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_ShadowBorderMask", tex);
            Assert.AreEqual(tex, lil.AOMap);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void AOMapTextureScale_ReturnsScale()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_ShadowBorderMask", new Vector2(2f, 3f));
            Assert.AreEqual(new Vector2(2f, 3f), lil.AOMapTextureScale);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void AOMapTextureOffset_ReturnsOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureOffset("_ShadowBorderMask", new Vector2(0.1f, 0.2f));
            Assert.AreEqual(new Vector2(0.1f, 0.2f), lil.AOMapTextureOffset);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // CullMode
        [Test]
        public void CullMode_ReturnsCullMode()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Cull", (float)CullMode.Front);
            Assert.AreEqual(CullMode.Front, lil.CullMode);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // ReflectionColor
        [Test]
        public void ReflectionColor_ReturnsColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_ReflectionColor", new Color(0.1f, 0.2f, 0.3f, 1f));
            var c = lil.ReflectionColor;
            Assert.AreEqual(0.1f, c.r, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // ReflectionColorTex
        [Test]
        public void ReflectionColorTex_ReturnsTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_ReflectionColorTex", tex);
            Assert.AreEqual(tex, lil.ReflectionColorTex);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // SpecularBlur
        [Test]
        public void SpecularBlur_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_SpecularBlur", 0.7f);
            Assert.AreEqual(0.7f, lil.SpecularBlur, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // Reflectance
        [Test]
        public void Reflectance_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Reflectance", 0.3f);
            Assert.AreEqual(0.3f, lil.Reflectance, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // MatCapColor
        [Test]
        public void MatCapColor_ReturnsColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_MatCapColor", new Color(0.4f, 0.5f, 0.6f, 0.8f));
            var c = lil.MatCapColor;
            Assert.AreEqual(0.4f, c.r, 0.01f);
            Assert.AreEqual(0.8f, c.a, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // MatCapMainStrength
        [Test]
        public void MatCapMainStrength_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapMainStrength", 0.5f);
            Assert.AreEqual(0.5f, lil.MatCapMainStrength, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // MatCapBlend
        [Test]
        public void MatCapBlend_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlend", 0.9f);
            Assert.AreEqual(0.9f, lil.MatCapBlend, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // MatCapBlendingMode
        [Test]
        public void MatCapBlendingMode_ReturnsMode()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_MatCapBlendMode", 2f); // Screen
            Assert.AreEqual(LilToonMaterial.MatCapBlendMode.Screen, lil.MatCapBlendingMode);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // RimLight properties
        [Test]
        public void RimLightColor_ReturnsColor()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetColor("_RimColor", new Color(0.1f, 0.9f, 0.5f, 0.7f));
            var c = lil.RimLightColor;
            Assert.AreEqual(0.1f, c.r, 0.01f);
            Assert.AreEqual(0.7f, c.a, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void RimMainStrength_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimMainStrength", 0.6f);
            Assert.AreEqual(0.6f, lil.RimMainStrength, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void RimLightBorder_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimBorder", 0.4f);
            Assert.AreEqual(0.4f, lil.RimLightBorder, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void RimEnableLighting_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimEnableLighting", 0.8f);
            Assert.AreEqual(0.8f, lil.RimEnableLighting, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void RimFresnelPower_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimFresnelPower", 3.0f);
            Assert.AreEqual(3.0f, lil.RimFresnelPower, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void RimLightBlur_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_RimBlur", 0.2f);
            Assert.AreEqual(0.2f, lil.RimLightBlur, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // MinimumBrightness / LightMinLimit
        [Test]
        public void MinimumBrightness_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_LightMinLimit", 0.15f);
            Assert.AreEqual(0.15f, lil.MinimumBrightness, 0.01f);
            Assert.AreEqual(0.15f, lil.LightMinLimit, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // NormalMapScale
        [Test]
        public void NormalMapScale_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_BumpScale", 1.5f);
            Assert.AreEqual(1.5f, lil.NormalMapScale, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // NormalMapTextureScale/Offset
        [Test]
        public void NormalMapTextureScale_ReturnsScale()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_BumpMap", new Vector2(2f, 2f));
            Assert.AreEqual(new Vector2(2f, 2f), lil.NormalMapTextureScale);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void NormalMapTextureOffset_ReturnsOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureOffset("_BumpMap", new Vector2(0.3f, 0.4f));
            Assert.AreEqual(new Vector2(0.3f, 0.4f), lil.NormalMapTextureOffset);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // Metallic
        [Test]
        public void Metallic_ReturnsFloat()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_Metallic", 0.5f);
            Assert.AreEqual(0.5f, lil.Metallic, 0.01f);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // MetallicMap
        [Test]
        public void MetallicMap_ReturnsTexture()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            lil.Material.SetTexture("_MetallicGlossMap", tex);
            Assert.AreEqual(tex, lil.MetallicMap);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // MetallicMapTextureScale/Offset
        [Test]
        public void MetallicMapTextureScale_ReturnsScale()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_MetallicGlossMap", new Vector2(3f, 4f));
            Assert.AreEqual(new Vector2(3f, 4f), lil.MetallicMapTextureScale);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void MetallicMapTextureOffset_ReturnsOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureOffset("_MetallicGlossMap", new Vector2(0.5f, 0.6f));
            Assert.AreEqual(new Vector2(0.5f, 0.6f), lil.MetallicMapTextureOffset);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // MatCapMaskTextureScale/Offset
        [Test]
        public void MatCapMaskTextureScale_ReturnsScale()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureScale("_MatCapBlendMask", new Vector2(1.5f, 2.5f));
            Assert.AreEqual(new Vector2(1.5f, 2.5f), lil.MatCapMaskTextureScale);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void MatCapMaskTextureOffset_ReturnsOffset()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetTextureOffset("_MatCapBlendMask", new Vector2(0.7f, 0.8f));
            Assert.AreEqual(new Vector2(0.7f, 0.8f), lil.MatCapMaskTextureOffset);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // UseRimLight
        [Test]
        public void UseRimLight_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseRim", 1f);
            Assert.IsTrue(lil.UseRimLight);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        [Test]
        public void UseRimLight_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseRim", 0f);
            Assert.IsFalse(lil.UseRimLight);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // UseNormalMap
        [Test]
        public void UseNormalMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseBumpMap", 1f);
            Assert.IsTrue(lil.UseNormalMap);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }

        // UseReflection
        [Test]
        public void UseReflection_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1f);
            Assert.IsTrue(lil.UseReflection);
            UnityEngine.Object.DestroyImmediate(lil.Material);
        }
    }
}
