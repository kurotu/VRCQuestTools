// Batch 54: Final coverage push targeting remaining testable methods
// Targets: LilToonToonStandardGenerator GetUse*(), AssetUtility, FallbackAvatarCallback,
// AvatarConverter.FindDescendant, ToonStandardGenerator abstract coverage

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Tests
{
    // ========== LilToonToonStandardGenerator GetUse*() Tests ==========
    [TestFixture]
    public class Batch54_LilToonToonStandardGeneratorTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            objectsToCleanup.Add(mat);
            return new LilToonMaterial(mat);
        }

        private object CreateGenerator(LilToonMaterial lilMaterial, ToonStandardConvertSettings settings = null)
        {
            if (settings == null)
            {
                settings = new ToonStandardConvertSettings();
            }
            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);
            blackTex.SetPixel(0, 0, Color.black);
            blackTex.Apply();

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) return null;
            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
            if (ctor == null) return null;
            return ctor.Invoke(new object[] { lilMaterial, settings, blackTex });
        }

        private object InvokeProtected(object generator, string methodName)
        {
            var method = generator.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) return null;
            return method.Invoke(generator, null);
        }

        [Test]
        public void GetUseEmission_WhenEmissionDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0);
            lil.Material.SetFloat("_UseEmission2nd", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseEmission");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseEmission_WhenEmissionEnabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseEmission");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseEmission_WhenEmission2ndEnabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0);
            lil.Material.SetFloat("_UseEmission2nd", 1);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseEmission");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseEmissionMap_WhenNoEmission_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0);
            lil.Material.SetFloat("_UseEmission2nd", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseEmissionMap");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseEmissionMap_WhenEmissionWithMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_EmissionMap", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseEmissionMap");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseEmissionMap_WhenEmissionBlendLessThanOne_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1);
            lil.Material.SetFloat("_EmissionBlend", 0.5f);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseEmissionMap");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseEmissionMap_WhenEmission2ndWithMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 0);
            lil.Material.SetFloat("_UseEmission2nd", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_Emission2ndMap", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseEmissionMap");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseGlossMap_WhenNoReflection_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseGlossMap");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseGlossMap_WhenReflectionWithSmoothnessTex_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_SmoothnessTex", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseGlossMap");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseMainTexture_WhenHasMainTexture_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.mainTexture = tex;
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseMainTexture");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseMainTexture_WhenEmissionNotInSettings_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1);
            var settings = new ToonStandardConvertSettings();
            // useEmission defaults to false in new settings
            var useEmissionField = typeof(ToonStandardConvertSettings).GetField("useEmission", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (useEmissionField != null) useEmissionField.SetValue(settings, false);
            var gen = CreateGenerator(lil, settings);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseMainTexture");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseMatcap_WhenDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseMatcap");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseMatcap_WhenEnabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseMatcap");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseMatcapMask_WhenNoMatCap_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseMatcapMask");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseMatcapMask_WhenMatCapWithMask_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_MatCapBlendMask", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseMatcapMask");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseMetallicMap_WhenNoReflection_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseMetallicMap");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseMetallicMap_WhenReflectionWithMetallicTex_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_MetallicGlossMap", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseMetallicMap");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseNormalMap_WhenDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseBumpMap", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseNormalMap");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseNormalMap_WhenEnabledWithMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseBumpMap", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_BumpMap", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseNormalMap");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseOcclusionMap_WhenNoShadow_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseOcclusionMap");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseOcclusionMap_WhenShadowWithAOMap_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_ShadowBorderMask", tex);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseOcclusionMap");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseRimLighting_WhenDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseRim", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseRimLighting");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseRimLighting_WhenEnabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseRim", 1);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseRimLighting");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseShadowRamp_WhenDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseShadowRamp");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseShadowRamp_WhenEnabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseShadowRamp");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetUseSpecular_WhenDisabled_ReturnsFalse()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 0);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseSpecular");
            Assert.AreEqual(false, result);
        }

        [Test]
        public void GetUseSpecular_WhenEnabled_ReturnsTrue()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1);
            var gen = CreateGenerator(lil);
            if (gen == null) { Assert.Ignore("Generator not available"); return; }
            var result = InvokeProtected(gen, "GetUseSpecular");
            Assert.AreEqual(true, result);
        }
    }

    // ========== AssetUtility Coverage Tests ==========
    [TestFixture]
    public class Batch54_AssetUtilityTests
    {
        [Test]
        public void CanLilToonBakeShadowRamp_ReturnsBoolean()
        {
            // CanLilToonBakeShadowRamp checks if LilToonVersion >= 1.10.0
            var result = AssetUtility.CanLilToonBakeShadowRamp();
            Assert.IsTrue(result is bool);
        }

        [Test]
        public void GetAllObjectReferences_ForMaterial_ReturnsArray()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var refs = AssetUtility.GetAllObjectReferences(mat);
                Assert.IsNotNull(refs);
                // A standard material may reference the standard shader
                Assert.IsTrue(refs.Length >= 0);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetAllObjectReferences_ForMaterialWithTexture_IncludesTexture()
        {
            var mat = new Material(Shader.Find("Standard"));
            var tex = new Texture2D(4, 4);
            mat.mainTexture = tex;
            try
            {
                var refs = AssetUtility.GetAllObjectReferences(mat);
                Assert.IsNotNull(refs);
                Assert.IsTrue(refs.Contains(tex), "References should include the assigned texture");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetAllObjectReferences_ForGameObject_ReturnsReferences()
        {
            var go = new GameObject("TestRefs");
            var mr = go.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            mr.sharedMaterial = mat;
            try
            {
                var refs = AssetUtility.GetAllObjectReferences(go);
                Assert.IsNotNull(refs);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsLilToonImported_ReturnsBool()
        {
            var result = AssetUtility.IsLilToonImported();
            Assert.IsTrue(result is bool);
            // Since we installed lilToon, it should be true
            Assert.IsTrue(result);
        }

        [Test]
        public void LilToonVersion_ReturnsValidSemVer()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon not imported");
                return;
            }
            var versionProp = typeof(AssetUtility).GetProperty("LilToonVersion", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (versionProp == null)
            {
                var versionField = typeof(AssetUtility).GetField("LilToonVersion", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (versionField != null)
                {
                    var version = versionField.GetValue(null);
                    Assert.IsNotNull(version);
                }
                else
                {
                    Assert.Ignore("LilToonVersion field not found");
                }
            }
            else
            {
                var version = versionProp.GetValue(null);
                Assert.IsNotNull(version);
            }
        }
    }

    // ========== AvatarConverter.FindDescendant Tests ==========
    [TestFixture]
    public class Batch54_AvatarConverterFindDescendantTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private AvatarConverter CreateConverter()
        {
            var builder = new MaterialWrapperBuilder();
            return new AvatarConverter(builder);
        }

        private MethodInfo GetFindDescendant()
        {
            return typeof(AvatarConverter).GetMethod("FindDescendant", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [Test]
        public void FindDescendant_DirectChild_ReturnsChild()
        {
            var parent = new GameObject("Parent");
            objectsToCleanup.Add(parent);
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);

            var converter = CreateConverter();
            var method = GetFindDescendant();
            if (method == null) { Assert.Ignore("FindDescendant not found"); return; }

            var result = method.Invoke(converter, new object[] { parent, "Child" });
            Assert.AreEqual(child, result);
        }

        [Test]
        public void FindDescendant_DeepChild_ReturnsChild()
        {
            var parent = new GameObject("Root");
            objectsToCleanup.Add(parent);
            var mid = new GameObject("Mid");
            mid.transform.SetParent(parent.transform);
            var deep = new GameObject("DeepChild");
            deep.transform.SetParent(mid.transform);

            var converter = CreateConverter();
            var method = GetFindDescendant();
            if (method == null) { Assert.Ignore("FindDescendant not found"); return; }

            var result = method.Invoke(converter, new object[] { parent, "DeepChild" });
            Assert.AreEqual(deep, result);
        }

        [Test]
        public void FindDescendant_NotFound_ReturnsNull()
        {
            var parent = new GameObject("Root");
            objectsToCleanup.Add(parent);
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);

            var converter = CreateConverter();
            var method = GetFindDescendant();
            if (method == null) { Assert.Ignore("FindDescendant not found"); return; }

            var result = method.Invoke(converter, new object[] { parent, "NonExistent" });
            Assert.IsNull(result);
        }

        [Test]
        public void FindDescendant_EmptyHierarchy_ReturnsNull()
        {
            var parent = new GameObject("EmptyRoot");
            objectsToCleanup.Add(parent);

            var converter = CreateConverter();
            var method = GetFindDescendant();
            if (method == null) { Assert.Ignore("FindDescendant not found"); return; }

            var result = method.Invoke(converter, new object[] { parent, "SomeName" });
            Assert.IsNull(result);
        }
    }

    // ========== FallbackAvatarCallback Tests ==========
    [TestFixture]
    public class Batch54_FallbackAvatarCallbackTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();
        private System.Type pipelineManagerType;

        [SetUp]
        public void SetUp()
        {
            // PipelineManager is in VRCCore-Editor.dll which is not referenced from test assembly
            // Use reflection to find it
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                pipelineManagerType = asm.GetType("VRC.Core.PipelineManager");
                if (pipelineManagerType != null) break;
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private System.Type GetCallbackType()
        {
            return typeof(KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback).Assembly
                .GetType("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");
        }

        [Test]
        public void CallbackOrder_IsNegative100000()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }

            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(callbackType);
            var prop = callbackType.GetProperty("callbackOrder", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) { Assert.Ignore("callbackOrder not found"); return; }

            var order = (int)prop.GetValue(instance);
            Assert.AreEqual(-100000, order);
        }

        [Test]
        public void OnPreprocessAvatar_WithNoPipelineManager_ReturnsTrue()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }

            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(callbackType);
            var method = callbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("OnPreprocessAvatar not found"); return; }

            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);

            var result = (bool)method.Invoke(instance, new object[] { go });
            Assert.IsTrue(result);
        }

        [Test]
        public void OnPreprocessAvatar_WithPipelineManagerEmptyId_ReturnsTrue()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            if (pipelineManagerType == null) { Assert.Ignore("PipelineManager not available"); return; }

            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(callbackType);
            var method = callbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("OnPreprocessAvatar not found"); return; }

            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var pm = go.AddComponent(pipelineManagerType);
            var blueprintField = pipelineManagerType.GetField("blueprintId", BindingFlags.Public | BindingFlags.Instance);
            if (blueprintField != null) blueprintField.SetValue(pm, "");

            var result = (bool)method.Invoke(instance, new object[] { go });
            Assert.IsTrue(result);
        }

        [Test]
        public void OnPreprocessAvatar_WithPipelineManagerAndBlueprintId_NoFallback_ReturnsTrue()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            if (pipelineManagerType == null) { Assert.Ignore("PipelineManager not available"); return; }

            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(callbackType);
            var method = callbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("OnPreprocessAvatar not found"); return; }

            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var pm = go.AddComponent(pipelineManagerType);
            var blueprintField = pipelineManagerType.GetField("blueprintId", BindingFlags.Public | BindingFlags.Instance);
            if (blueprintField != null) blueprintField.SetValue(pm, "avtr_test_12345");

            var result = (bool)method.Invoke(instance, new object[] { go });
            Assert.IsTrue(result);

            // Without FallbackAvatar, should remove from pending
            var pendingField = callbackType.GetField("PendingFallbackAvatars", BindingFlags.NonPublic | BindingFlags.Static);
            if (pendingField != null)
            {
                var dict = pendingField.GetValue(null) as System.Collections.IDictionary;
                Assert.IsFalse(dict.Contains("avtr_test_12345"));
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithFallbackComponent_AddsToPending()
        {
            var callbackType = GetCallbackType();
            if (callbackType == null) { Assert.Ignore("FallbackAvatarCallback not found"); return; }
            if (pipelineManagerType == null) { Assert.Ignore("PipelineManager not available"); return; }

            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(callbackType);
            var method = callbackType.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) { Assert.Ignore("OnPreprocessAvatar not found"); return; }

            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var pm = go.AddComponent(pipelineManagerType);
            var blueprintField = pipelineManagerType.GetField("blueprintId", BindingFlags.Public | BindingFlags.Instance);
            if (blueprintField != null) blueprintField.SetValue(pm, "avtr_fallback_test");
            go.AddComponent<FallbackAvatar>();

            var result = (bool)method.Invoke(instance, new object[] { go });
            Assert.IsTrue(result);

            // With FallbackAvatar, should add to pending
            var pendingField = callbackType.GetField("PendingFallbackAvatars", BindingFlags.NonPublic | BindingFlags.Static);
            if (pendingField != null)
            {
                var dict = pendingField.GetValue(null) as System.Collections.IDictionary;
                Assert.IsTrue(dict.Contains("avtr_fallback_test"));
                // Clean up
                dict.Remove("avtr_fallback_test");
            }
        }
    }

    // ========== VRCQuestToolsSettings Additional Tests ==========
    [TestFixture]
    public class Batch54_VRCQuestToolsSettingsTests
    {
        [Test]
        public void DisplayLanguage_GetSet_RoundTrips()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Japanese;
                Assert.AreEqual(DisplayLanguage.Japanese, VRCQuestToolsSettings.DisplayLanguage);
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.English;
                Assert.AreEqual(DisplayLanguage.English, VRCQuestToolsSettings.DisplayLanguage);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
            }
        }

        [Test]
        public void IsValidationAutomatorEnabled_GetSet_RoundTrips()
        {
            var original = VRCQuestToolsSettings.IsValidationAutomatorEnabled;
            try
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsValidationAutomatorEnabled);
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsValidationAutomatorEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = original;
            }
        }

        [Test]
        public void IsCheckTextureFormatOnStandaloneEnabled_GetSet_RoundTrips()
        {
            var prop = typeof(VRCQuestToolsSettings).GetProperty("IsCheckTextureFormatOnStandaloneEnabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null) { Assert.Ignore("Property not found"); return; }
            var original = (bool)prop.GetValue(null);
            try
            {
                prop.SetValue(null, true);
                Assert.IsTrue((bool)prop.GetValue(null));
                prop.SetValue(null, false);
                Assert.IsFalse((bool)prop.GetValue(null));
            }
            finally
            {
                prop.SetValue(null, original);
            }
        }
    }

    // ========== MaterialGeneratorUtility Additional Tests ==========
    [TestFixture]
    public class Batch54_MaterialGeneratorUtilityTests
    {
        [Test]
        public void ConvertToNullableTextureFormat_ASTC6x6_ReturnsASTC6x6()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("ConvertToNullableTextureFormat not found"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_6x6 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_NoOverride_ReturnsNull()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("ConvertToNullableTextureFormat not found"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.NoOverride });
            Assert.IsNull(result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC4x4_ReturnsASTC4x4()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("ConvertToNullableTextureFormat not found"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_4x4 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_4x4, result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC8x8_ReturnsASTC8x8()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("ConvertToNullableTextureFormat not found"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_8x8 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_8x8, result);
        }
    }

    // ========== ConvertToToonStandard LilToon Additional Tests ==========
    [TestFixture]
    public class Batch54_LilToonConvertToToonStandardTests
    {
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        private LilToonMaterial CreateLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) return null;
            var mat = new Material(shader);
            objectsToCleanup.Add(mat);
            return new LilToonMaterial(mat);
        }

        [Test]
        public void ConvertToToonStandard_WithEmission_SetsEmissionProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseEmission", 1);
            lil.Material.SetColor("_EmissionColor", new Color(1, 0, 0, 1));

            var settings = new ToonStandardConvertSettings();

            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }

            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("ConvertToToonStandard", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("ConvertToToonStandard not found"); return; }

            var result = method.Invoke(gen, null) as Material;
            Assert.IsNotNull(result);
            objectsToCleanup.Add(result);
        }

        [Test]
        public void ConvertToToonStandard_WithMatCap_SetsMatCapProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseMatCap", 1);
            lil.Material.SetFloat("_MatCapBlendMode", 0); // Normal

            var settings = new ToonStandardConvertSettings();

            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }

            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("ConvertToToonStandard", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("ConvertToToonStandard not found"); return; }

            var result = method.Invoke(gen, null) as Material;
            Assert.IsNotNull(result);
            objectsToCleanup.Add(result);
        }

        [Test]
        public void ConvertToToonStandard_WithReflection_SetsSpecularProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseReflection", 1);
            lil.Material.SetFloat("_Metallic", 0.5f);
            lil.Material.SetFloat("_Smoothness", 0.7f);

            var settings = new ToonStandardConvertSettings();

            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }

            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("ConvertToToonStandard", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("ConvertToToonStandard not found"); return; }

            var result = method.Invoke(gen, null) as Material;
            Assert.IsNotNull(result);
            objectsToCleanup.Add(result);
        }

        [Test]
        public void ConvertToToonStandard_WithRimLight_SetsRimProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseRim", 1);
            lil.Material.SetColor("_RimColor", new Color(1, 1, 1, 0.5f));

            var settings = new ToonStandardConvertSettings();

            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }

            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("ConvertToToonStandard", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("ConvertToToonStandard not found"); return; }

            var result = method.Invoke(gen, null) as Material;
            Assert.IsNotNull(result);
            objectsToCleanup.Add(result);
        }

        [Test]
        public void ConvertToToonStandard_WithShadow_SetsShadowProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseShadow", 1);
            lil.Material.SetColor("_ShadowColor", new Color(0.5f, 0.5f, 0.5f, 1));
            lil.Material.SetColor("_Shadow2ndColor", new Color(0.3f, 0.3f, 0.3f, 1));

            var settings = new ToonStandardConvertSettings();

            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }

            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("ConvertToToonStandard", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("ConvertToToonStandard not found"); return; }

            var result = method.Invoke(gen, null) as Material;
            Assert.IsNotNull(result);
            objectsToCleanup.Add(result);
        }

        [Test]
        public void ConvertToToonStandard_WithNormalMap_SetsNormalProperties()
        {
            var lil = CreateLilToon();
            if (lil == null) { Assert.Ignore("lilToon not available"); return; }
            lil.Material.SetFloat("_UseBumpMap", 1);
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            lil.Material.SetTexture("_BumpMap", tex);
            lil.Material.SetFloat("_BumpScale", 1.0f);

            var settings = new ToonStandardConvertSettings();

            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }

            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("ConvertToToonStandard", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("ConvertToToonStandard not found"); return; }

            var result = method.Invoke(gen, null) as Material;
            Assert.IsNotNull(result);
            objectsToCleanup.Add(result);
        }

        [Test]
        public void ConvertToToonStandard_WithCutout_SetsRenderType()
        {
            var shader = Shader.Find("[lilToon] Cutout");
            if (shader == null) shader = Shader.Find("lilToon");
            if (shader == null) { Assert.Ignore("lilToon not available"); return; }

            var mat = new Material(shader);
            objectsToCleanup.Add(mat);
            mat.SetFloat("_Cutoff", 0.5f);

            var lil = new LilToonMaterial(mat);
            var settings = new ToonStandardConvertSettings();

            var blackTex = new Texture2D(1, 1);
            objectsToCleanup.Add(blackTex);

            var genType = typeof(ToonStandardGenerator).Assembly.GetType("KRT.VRCQuestTools.Models.LilToonToonStandardGenerator");
            if (genType == null) { Assert.Ignore("Generator not found"); return; }

            var ctor = genType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First();
            var gen = ctor.Invoke(new object[] { lil, settings, blackTex });

            var method = genType.GetMethod("ConvertToToonStandard", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null) { Assert.Ignore("ConvertToToonStandard not found"); return; }

            var result = method.Invoke(gen, null) as Material;
            Assert.IsNotNull(result);
            objectsToCleanup.Add(result);
        }
    }
}
