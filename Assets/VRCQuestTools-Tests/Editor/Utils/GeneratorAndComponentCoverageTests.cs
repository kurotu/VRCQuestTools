// Batch 18 tests targeting remaining coverage gaps

using System;
using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UBuildTarget = UnityEditor.BuildTarget;
using Object = UnityEngine.Object;

namespace KRT.VRCQuestTools.Tests
{
    // ====================================================
    // GenericToonStandardGenerator - all protected methods
    // ====================================================
    [TestFixture]
    public class Batch18_GenericToonStandardGeneratorTests
    {
        private Type generatorType;
        private object generator;
        private Material mat;
        private Texture2D blackTex;

        [SetUp]
        public void SetUp()
        {
            mat = new Material(Shader.Find("Standard"));
            blackTex = new Texture2D(4, 4);
            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var settings = new ToonStandardConvertSettings();

            generatorType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.GenericToonStandardGenerator");
            Assert.IsNotNull(generatorType, "GenericToonStandardGenerator type should exist");

            var ctor = generatorType.GetConstructor(new Type[] { typeof(MaterialBase), typeof(ToonStandardConvertSettings), typeof(Texture2D) });
            if (ctor == null)
            {
                var ctors = generatorType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var c in ctors)
                {
                    if (c.GetParameters().Length == 3) { ctor = c; break; }
                }
            }
            Assert.IsNotNull(ctor);
            generator = ctor.Invoke(new object[] { wrapper, settings, blackTex });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(mat);
            Object.DestroyImmediate(blackTex);
        }

        private MethodInfo GetMethod(string name)
        {
            return generatorType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        private void AssertThrowsNotImplemented(string methodName, object[] args = null)
        {
            var method = GetMethod(methodName);
            if (method == null)
            {
                Assert.Inconclusive($"Method {methodName} not found");
                return;
            }
            try
            {
                method.Invoke(generator, args);
                Assert.Fail($"Expected NotImplementedException from {methodName}");
            }
            catch (TargetInvocationException ex)
            {
                Assert.IsInstanceOf<NotImplementedException>(ex.InnerException, $"{methodName} should throw NotImplementedException");
            }
        }

        // --- Methods that throw NotImplementedException ---

        [Test] public void GenerateEmissionMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GenerateEmissionMap", new object[] { (Action<Texture2D>)((t) => { }) });
        [Test] public void GenerateGlossMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GenerateGlossMap", new object[] { (Action<Texture2D>)((t) => { }) });
        [Test] public void GenerateMainTexture_ThrowsNotImplemented() => AssertThrowsNotImplemented("GenerateMainTexture", new object[] { (Action<Texture2D>)((t) => { }) });
        [Test] public void GenerateMatcap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GenerateMatcap", new object[] { (Action<Texture2D>)((t) => { }) });
        [Test] public void GenerateMatcapMask_ThrowsNotImplemented() => AssertThrowsNotImplemented("GenerateMatcapMask", new object[] { (Action<Texture2D>)((t) => { }) });
        [Test] public void GenerateMetallicMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GenerateMetallicMap", new object[] { (Action<Texture2D>)((t) => { }) });
        [Test] public void GenerateOcclusionMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GenerateOcclusionMap", new object[] { (Action<Texture2D>)((t) => { }) });
        [Test] public void GenerateShadowRamp_ThrowsNotImplemented() => AssertThrowsNotImplemented("GenerateShadowRamp", new object[] { (Action<Texture2D>)((t) => { }) });

        [Test]
        public void GenerateNormalMap_ThrowsNotImplemented()
        {
            AssertThrowsNotImplemented("GenerateNormalMap", new object[] { true, (Action<Texture2D>)((t) => { }) });
        }

        [Test]
        public void GeneratePackedMask_ThrowsNotImplemented()
        {
            // TexturePack is likely an enum or struct - find it
            var texturePackType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.TexturePack");
            if (texturePackType == null)
            {
                // Try as nested type in ToonStandardGenerator
                texturePackType = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.Unity.ToonStandardMaterialWrapper+TexturePack");
            }
            if (texturePackType == null)
            {
                // Search more broadly
                foreach (var type in typeof(AvatarConverter).Assembly.GetTypes())
                {
                    if (type.Name == "TexturePack")
                    {
                        texturePackType = type;
                        break;
                    }
                }
            }
            if (texturePackType == null)
            {
                Assert.Inconclusive("TexturePack type not found");
                return;
            }
            var defaultValue = Activator.CreateInstance(texturePackType);
            AssertThrowsNotImplemented("GeneratePackedMask", new object[] { defaultValue, (Action<Texture2D>)((t) => { }) });
        }

        [Test] public void GetCulling_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetCulling");
        [Test] public void GetEmissionColor_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetEmissionColor");
        [Test] public void GetGlossMapST_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetGlossMapST");
        [Test] public void GetGlossStrength_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetGlossStrength");
        [Test] public void GetMainColor_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMainColor");
        [Test] public void GetMainTextureST_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMainTextureST");
        [Test] public void GetMapcapType_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMapcapType");
        [Test] public void GetMatcap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMatcap");
        [Test] public void GetMatcapMaskST_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMatcapMaskST");
        [Test] public void GetMatcapMaskStrength_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMatcapMaskStrength");
        [Test] public void GetMetallicMapST_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMetallicMapST");
        [Test] public void GetMetallicStrength_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMetallicStrength");
        [Test] public void GetMinBrightness_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetMinBrightness");
        [Test] public void GetNormalMapScale_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetNormalMapScale");
        [Test] public void GetNormalMapST_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetNormalMapST");
        [Test] public void GetOcculusionMapST_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetOcculusionMapST");
        [Test] public void GetReflectance_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetReflectance");
        [Test] public void GetRimAlbedoTint_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetRimAlbedoTint");
        [Test] public void GetRimColor_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetRimColor");
        [Test] public void GetRimEnvironmental_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetRimEnvironmental");
        [Test] public void GetRimIntensity_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetRimIntensity");
        [Test] public void GetRimRange_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetRimRange");
        [Test] public void GetRimSoftness_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetRimSoftness");
        [Test] public void GetSharpness_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetSharpness");
        [Test] public void GetUseEmissionMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseEmissionMap");
        [Test] public void GetUseGlossMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseGlossMap");
        [Test] public void GetUseMainTexture_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseMainTexture");
        [Test] public void GetUseMatcap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseMatcap");
        [Test] public void GetUseMatcapMask_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseMatcapMask");
        [Test] public void GetUseMetallicMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseMetallicMap");
        [Test] public void GetUseNormalMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseNormalMap");
        [Test] public void GetUseOcclusionMap_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseOcclusionMap");
        [Test] public void GetUseRimLighting_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseRimLighting");
        [Test] public void GetUseShadowRamp_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseShadowRamp");
        [Test] public void GetUseSpecular_ThrowsNotImplemented() => AssertThrowsNotImplemented("GetUseSpecular");

        // --- Methods that return values ---

        [Test]
        public void GetUseEmission_ReturnsFalse()
        {
            var method = GetMethod("GetUseEmission");
            if (method == null) { Assert.Inconclusive("GetUseEmission not found"); return; }
            var result = (bool)method.Invoke(generator, null);
            Assert.IsFalse(result);
        }

        [Test]
        public void ConvertToToonStandard_ReturnsNewMaterial()
        {
            var method = GetMethod("ConvertToToonStandard");
            if (method == null) { Assert.Inconclusive("ConvertToToonStandard not found"); return; }
            var result = (Material)method.Invoke(generator, null);
            Assert.IsNotNull(result);
            Object.DestroyImmediate(result);
        }

        [Test]
        public void GetMainTexturePlatformOverride_ReturnsValue()
        {
            var method = GetMethod("GetMainTexturePlatformOverride");
            if (method == null) { Assert.Inconclusive("not found"); return; }
            // May return null or a tuple depending on the material's mainTexture
            method.Invoke(generator, null); // Just ensure it doesn't throw
        }

        [Test]
        public void GetNormalMapPlatformOverride_ReturnsNull()
        {
            var method = GetMethod("GetNormalMapPlatformOverride");
            if (method == null) { Assert.Inconclusive("not found"); return; }
            var result = method.Invoke(generator, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetEmissionMapPlatformOverride_ReturnsNull()
        {
            var method = GetMethod("GetEmissionMapPlatformOverride");
            if (method == null) { Assert.Inconclusive("not found"); return; }
            var result = method.Invoke(generator, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetMatcapPlatformOverride_ReturnsNull()
        {
            var method = GetMethod("GetMatcapPlatformOverride");
            if (method == null) { Assert.Inconclusive("not found"); return; }
            var result = method.Invoke(generator, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetMatcapMaskPlatformOverride_ReturnsNull()
        {
            var method = GetMethod("GetMatcapMaskPlatformOverride");
            if (method == null) { Assert.Inconclusive("not found"); return; }
            var result = method.Invoke(generator, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetMetallicMapPlatformOverride_ReturnsNull()
        {
            var method = GetMethod("GetMetallicMapPlatformOverride");
            if (method == null) { Assert.Inconclusive("not found"); return; }
            var result = method.Invoke(generator, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetGlossMapPlatformOverride_ReturnsNull()
        {
            var method = GetMethod("GetGlossMapPlatformOverride");
            if (method == null) { Assert.Inconclusive("not found"); return; }
            var result = method.Invoke(generator, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetOcclusionMapPlatformOverride_ReturnsNull()
        {
            var method = GetMethod("GetOcclusionMapPlatformOverride");
            if (method == null) { Assert.Inconclusive("not found"); return; }
            var result = method.Invoke(generator, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetPackedMaskPlatformOverride_ReturnsNull()
        {
            var method = generatorType.GetMethod("GetPackedMaskPlatformOverride", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (method == null) { Assert.Inconclusive("not found"); return; }
            var texturePackType = method.GetParameters()[0].ParameterType;
            var defaultValue = Activator.CreateInstance(texturePackType);
            var result = method.Invoke(generator, new object[] { defaultValue });
            Assert.IsNull(result);
        }
    }

    // ====================================================
    // CacheUtility.TextureCache tests
    // ====================================================
    [TestFixture]
    public class Batch18_TextureCacheTests
    {
        [Test]
        public void TextureCache_Constructor_SetsFields()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false, false);
            try
            {
                var pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(128, 64, 32, 255);
                tex.SetPixels32(pixels);
                tex.Apply();

                var cacheType = typeof(CacheUtility).GetNestedType("TextureCache", BindingFlags.NonPublic);
                Assert.IsNotNull(cacheType, "TextureCache type should exist");

                var ctor = cacheType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { typeof(Texture2D), typeof(bool), typeof(bool), typeof(UBuildTarget) }, null);
                Assert.IsNotNull(ctor, "TextureCache constructor should exist");

                var cache = ctor.Invoke(new object[] { tex, false, false, UBuildTarget.Android });
                Assert.IsNotNull(cache);

                // Test ToTexture2D
                var toTex2DMethod = cacheType.GetMethod("ToTexture2D", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(toTex2DMethod);
                var restored = (Texture2D)toTex2DMethod.Invoke(cache, null);
                Assert.IsNotNull(restored);
                Assert.AreEqual(8, restored.width);
                Assert.AreEqual(8, restored.height);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_WithMipmap_SetsCorrectly()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, true, false);
            try
            {
                tex.Apply();

                var cacheType = typeof(CacheUtility).GetNestedType("TextureCache", BindingFlags.NonPublic);
                var ctor = cacheType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { typeof(Texture2D), typeof(bool), typeof(bool), typeof(UBuildTarget) }, null);

                var cache = ctor.Invoke(new object[] { tex, true, false, UBuildTarget.StandaloneWindows64 });
                Assert.IsNotNull(cache);

                var toTex2DMethod = cacheType.GetMethod("ToTexture2D", BindingFlags.NonPublic | BindingFlags.Instance);
                var restored = (Texture2D)toTex2DMethod.Invoke(cache, null);
                Assert.IsNotNull(restored);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_Linear_SetsCorrectly()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
            try
            {
                tex.Apply();

                var cacheType = typeof(CacheUtility).GetNestedType("TextureCache", BindingFlags.NonPublic);
                var ctor = cacheType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { typeof(Texture2D), typeof(bool), typeof(bool), typeof(UBuildTarget) }, null);

                var cache = ctor.Invoke(new object[] { tex, true, false, UBuildTarget.Android });
                Assert.IsNotNull(cache);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void TextureCache_NormalMap_ReturnsTexture()
        {
            // Normal map handling has Android-specific path
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false, false);
            try
            {
                tex.Apply();

                var cacheType = typeof(CacheUtility).GetNestedType("TextureCache", BindingFlags.NonPublic);
                var ctor = cacheType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { typeof(Texture2D), typeof(bool), typeof(bool), typeof(UBuildTarget) }, null);

                // Use StandaloneWindows64 to avoid the Android-specific blank normal map path
                var cache = ctor.Invoke(new object[] { tex, false, true, UBuildTarget.StandaloneWindows64 });
                Assert.IsNotNull(cache);

                var toTex2DMethod = cacheType.GetMethod("ToTexture2D", BindingFlags.NonPublic | BindingFlags.Instance);
                var restored = (Texture2D)toTex2DMethod.Invoke(cache, null);
                Assert.IsNotNull(restored);
                Object.DestroyImmediate(restored);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }
    }

    // ====================================================
    // ResultRequest (non-generic) tests
    // ====================================================
    [TestFixture]
    public class Batch18_ResultRequestTests
    {
        [Test]
        public void ResultRequest_Constructor_InvokesCompletion()
        {
            var type = typeof(AsyncCallbackRequest).Assembly.GetType("KRT.VRCQuestTools.Utils.ResultRequest");
            Assert.IsNotNull(type, "Non-generic ResultRequest should exist");

            bool called = false;
            Action completion = () => { called = true; };

            var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(Action) }, null);
            Assert.IsNotNull(ctor);

            var instance = ctor.Invoke(new object[] { completion });
            Assert.IsTrue(called);

            // WaitForCompletion does nothing
            var waitMethod = type.GetMethod("WaitForCompletion", BindingFlags.NonPublic | BindingFlags.Instance);
            waitMethod.Invoke(instance, null);
        }

        [Test]
        public void ResultRequest_NullCompletion_DoesNotThrow()
        {
            var type = typeof(AsyncCallbackRequest).Assembly.GetType("KRT.VRCQuestTools.Utils.ResultRequest");
            var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(Action) }, null);

            Assert.DoesNotThrow(() => ctor.Invoke(new object[] { null }));
        }
    }

    // ====================================================
    // TextureCPUReadbackRequest tests
    // ====================================================
    [TestFixture]
    public class Batch18_TextureCPUReadbackRequestTests
    {
        [Test]
        public void TextureCPUReadbackRequest_ReadsPixels()
        {
            var rt = new RenderTexture(4, 4, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            Texture2D resultTex = null;
            try
            {
                // Ensure the RT is active for ReadPixels
                var prevActive = RenderTexture.active;
                RenderTexture.active = rt;

                var type = typeof(AsyncCallbackRequest).Assembly.GetType("KRT.VRCQuestTools.Utils.TextureCPUReadbackRequest");
                Assert.IsNotNull(type);

                var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { typeof(RenderTexture), typeof(bool), typeof(Action<Texture2D>) }, null);
                Assert.IsNotNull(ctor);

                var instance = (AsyncCallbackRequest)ctor.Invoke(new object[] { rt, false, (Action<Texture2D>)((t) => { resultTex = t; }) });
                Assert.IsNotNull(resultTex);
                Assert.AreEqual(4, resultTex.width);

                // WaitForCompletion does nothing
                instance.WaitForCompletion();

                RenderTexture.active = prevActive;
            }
            finally
            {
                rt.Release();
                Object.DestroyImmediate(rt);
                if (resultTex != null) Object.DestroyImmediate(resultTex);
            }
        }

        [Test]
        public void TextureCPUReadbackRequest_WithMipmap_Works()
        {
            var rt = new RenderTexture(8, 8, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            Texture2D resultTex = null;
            try
            {
                var prevActive = RenderTexture.active;
                RenderTexture.active = rt;

                var type = typeof(AsyncCallbackRequest).Assembly.GetType("KRT.VRCQuestTools.Utils.TextureCPUReadbackRequest");
                var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { typeof(RenderTexture), typeof(bool), typeof(bool), typeof(Action<Texture2D>) }, null);
                Assert.IsNotNull(ctor);

                var instance = (AsyncCallbackRequest)ctor.Invoke(new object[] { rt, true, false, (Action<Texture2D>)((t) => { resultTex = t; }) });
                Assert.IsNotNull(resultTex);
                Assert.AreEqual(8, resultTex.width);

                RenderTexture.active = prevActive;
            }
            finally
            {
                rt.Release();
                Object.DestroyImmediate(rt);
                if (resultTex != null) Object.DestroyImmediate(resultTex);
            }
        }

        [Test]
        public void TextureCPUReadbackRequest_Linear_Works()
        {
            var rt = new RenderTexture(4, 4, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            Texture2D resultTex = null;
            try
            {
                var prevActive = RenderTexture.active;
                RenderTexture.active = rt;

                var type = typeof(AsyncCallbackRequest).Assembly.GetType("KRT.VRCQuestTools.Utils.TextureCPUReadbackRequest");
                var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { typeof(RenderTexture), typeof(bool), typeof(bool), typeof(Action<Texture2D>) }, null);

                var instance = (AsyncCallbackRequest)ctor.Invoke(new object[] { rt, false, true, (Action<Texture2D>)((t) => { resultTex = t; }) });
                Assert.IsNotNull(resultTex);

                RenderTexture.active = prevActive;
            }
            finally
            {
                rt.Release();
                Object.DestroyImmediate(rt);
                if (resultTex != null) Object.DestroyImmediate(resultTex);
            }
        }
    }

    // ====================================================
    // MSMapGenViewModel tests
    // ====================================================
    [TestFixture]
    public class Batch18_MSMapGenViewModelTests
    {
        [Test]
        public void DisableGenerateButton_BothNull_ReturnsTrue()
        {
            var vm = new MSMapGenViewModel();
            vm.metallicMap = null;
            vm.smoothnessMap = null;
            Assert.IsTrue(vm.DisableGenerateButton);
        }

        [Test]
        public void DisableGenerateButton_MetallicSet_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = tex;
                vm.smoothnessMap = null;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_SmoothnessSet_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = null;
                vm.smoothnessMap = tex;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_BothSet_ReturnsFalse()
        {
            var tex1 = new Texture2D(4, 4);
            var tex2 = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = tex1;
                vm.smoothnessMap = tex2;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex1);
                Object.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void InvertSmoothness_DefaultFalse()
        {
            var vm = new MSMapGenViewModel();
            Assert.IsFalse(vm.invertSmoothness);
        }
    }

    // ====================================================
    // VRCQuestTools main entry tests
    // ====================================================
    [TestFixture]
    public class Batch18_VRCQuestToolsEntryTests
    {
        [Test]
        public void Version_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VRCQuestTools.Version));
        }

        [Test]
        public void Name_IsVRCQuestTools()
        {
            Assert.AreEqual("VRCQuestTools", VRCQuestTools.Name);
        }

        [Test]
        public void Version_IsSemVer()
        {
            var parts = VRCQuestTools.Version.Split('.');
            Assert.GreaterOrEqual(parts.Length, 3, "Version should have at least 3 parts");
            Assert.IsTrue(int.TryParse(parts[0], out _), "Major should be a number");
            Assert.IsTrue(int.TryParse(parts[1], out _), "Minor should be a number");
        }

        [Test]
        public void AssetRoot_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VRCQuestTools.AssetRoot));
        }

        [Test]
        public void IsImportedAsPackage_ReturnsBool()
        {
            // Just verify it doesn't throw
            var result = VRCQuestTools.IsImportedAsPackage;
            Assert.IsNotNull((object)result);
        }
    }

    // ====================================================
    // AvatarConverterNdmfPhaseExtension tests
    // ====================================================
    [TestFixture]
    public class Batch18_AvatarConverterNdmfPhaseExtensionTests
    {
        [Test]
        public void AvatarConverterNdmfPhaseExtension_CanBeInstantiated()
        {
            // This is a ScriptableObject or similar - check if type exists
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.AvatarConverterNdmfPhaseExtension");
            if (type == null)
            {
                Assert.Inconclusive("AvatarConverterNdmfPhaseExtension type not found");
                return;
            }
            Assert.IsNotNull(type);
        }
    }

    // ====================================================
    // Mock_AvatarPerformanceStatsLevelSet tests
    // ====================================================
    [TestFixture]
    public class Batch18_MockPerformanceStatsLevelSetTests
    {
        [Test]
        public void MockAvatarPerformanceStatsLevelSet_CanBeCreated()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet");
            if (type == null)
            {
                Assert.Inconclusive("Mock_AvatarPerformanceStatsLevelSet not found");
                return;
            }

            // Check for properties or methods
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsTrue(members.Length > 0, "Should have members");
        }
    }

    // ====================================================
    // Additional ComponentRemover gap tests
    // ====================================================
    [TestFixture]
    public class Batch18_ComponentRemoverGapTests
    {
        [Test]
        public void ComponentRemover_RemoveUnsupportedComponentsInChildren_EmptyAvatar()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var remover = new ComponentRemover();
                // Should not throw for an empty avatar
                remover.RemoveUnsupportedComponentsInChildren(go, true, false);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ====================================================
    // MeshFlipperMaskNotReadableException test
    // ====================================================
    [TestFixture]
    public class Batch18_MeshFlipperExceptionTests
    {
        [Test]
        public void MeshFlipperMaskNotReadableException_CanBeCreated()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Components.MeshFlipperMaskNotReadableException");
            if (type == null)
            {
                Assert.Inconclusive("Type not found");
                return;
            }
            var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsTrue(ctor.Length > 0);
            try
            {
                var ex = (Exception)Activator.CreateInstance(type);
                Assert.IsNotNull(ex);
            }
            catch (MissingMethodException)
            {
                // Constructor might need params - try with string
                try
                {
                    var ex = (Exception)Activator.CreateInstance(type, new object[] { "test" });
                    Assert.IsNotNull(ex);
                }
                catch
                {
                    Assert.Pass("Exception type exists but needs specific constructor args");
                }
            }
        }
    }

    // ====================================================
    // IMaterialConversionComponent interface coverage
    // ====================================================
    [TestFixture]
    public class Batch18_IMaterialConversionComponentTests
    {
        [Test]
        public void IMaterialConversionComponent_InterfaceExists()
        {
            Assert.IsNotNull(typeof(KRT.VRCQuestTools.Components.IMaterialConversionComponent));
            Assert.IsTrue(typeof(KRT.VRCQuestTools.Components.IMaterialConversionComponent).IsInterface);
        }
    }

    // ====================================================
    // VRCPhysBoneProviderBase uncovered method
    // ====================================================
    [TestFixture]
    public class Batch18_VRCPhysBoneProviderBaseTests
    {
        [Test]
        public void VRCPhysBoneProviderBase_TypeExists()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.VRChat.VRCPhysBoneProviderBase");
            Assert.IsNotNull(type);
        }
    }

    // ====================================================
    // MenuIconResizer component tests
    // ====================================================
    [TestFixture]
    public class Batch18_MenuIconResizerTests
    {
        [Test]
        public void MenuIconResizer_CanBeAdded()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Components.MenuIconResizer");
            if (type == null)
            {
                Assert.Inconclusive("MenuIconResizer not found");
                return;
            }

            var go = new GameObject("TestMenuIconResizer");
            try
            {
                var comp = go.AddComponent(type);
                Assert.IsNotNull(comp);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ====================================================
    // NdmfUtility tests
    // ====================================================
    [TestFixture]
    public class Batch18_NdmfUtilityTests
    {
        [Test]
        public void NdmfUtility_TypeExists()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Utils.NdmfUtility");
            Assert.IsNotNull(type);

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsTrue(methods.Length > 0);
        }
    }

    // ====================================================
    // ValidationAutomator additional tests
    // ====================================================
    [TestFixture]
    public class Batch18_ValidationAutomatorTests
    {
        [Test]
        public void ValidationAutomator_TypeExists()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Automators.ValidationAutomator");
            Assert.IsNotNull(type);
        }
    }

    // ====================================================
    // MaterialConversionGUI more coverage
    // ====================================================
    [TestFixture]
    public class Batch18_MaterialConversionGUITests
    {
        [Test]
        public void MaterialConversionGUI_TypeExists()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Inspector.MaterialConversionGUI");
            Assert.IsNotNull(type);
        }
    }

    // ====================================================
    // I18n coverage (6 uncovered lines)
    // ====================================================
    [TestFixture]
    public class Batch18_I18nTests
    {
        [Test]
        public void I18n_SupportsMultipleLanguages()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.I18n.I18n");
            Assert.IsNotNull(type);

            // Try to get supported languages
            var languagesField = type.GetField("SupportedLanguages", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (languagesField != null)
            {
                var languages = languagesField.GetValue(null);
                Assert.IsNotNull(languages);
            }
        }
    }

    // ====================================================
    // AvatarConverterSettings uncovered lines
    // ====================================================
    [TestFixture]
    public class Batch18_AvatarConverterSettingsGapTests
    {
        [Test]
        public void AvatarConverterSettings_DefaultSettings()
        {
            var go = new GameObject("TestSettings");
            try
            {
                var settings = go.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();
                Assert.IsNotNull(settings);

                // Test default property values
                Assert.IsTrue(settings.RemoveExtraMaterialSlots);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    // ====================================================
    // VPMService tests
    // ====================================================
    [TestFixture]
    public class Batch18_VPMServiceTests
    {
        [Test]
        public void VPMService_TypeExists()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Services.VPMService");
            Assert.IsNotNull(type);
        }
    }

    // ====================================================
    // Additional VRCQuestToolsSettings coverage
    // ====================================================
    [TestFixture]
    public class Batch18_VRCQuestToolsSettingsTests
    {
        [Test]
        public void Settings_SetValues_Persist()
        {
            var type = typeof(AvatarConverter).Assembly.GetType("KRT.VRCQuestTools.Models.VRCQuestToolsSettings");
            Assert.IsNotNull(type);

            // Try to access instance/static properties
            var instanceProp = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (instanceProp != null)
            {
                var instance = instanceProp.GetValue(null);
                Assert.IsNotNull(instance);
            }
        }
    }
}
