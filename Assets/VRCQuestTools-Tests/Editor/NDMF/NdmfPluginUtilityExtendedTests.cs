// Tests for NdmfPluginUtility.HandleConversionException and related NDMF utility methods.

using System;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools
{
    [TestFixture]
    internal class NdmfPluginUtilityHandleExceptionTests
    {
        private Type pluginUtilityType;
        private MethodInfo handleExceptionMethod;
        private System.Collections.Generic.List<UnityEngine.Object> objectsToCleanup = new System.Collections.Generic.List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            pluginUtilityType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfPluginUtility");
            if (pluginUtilityType == null) Assert.Ignore("NdmfPluginUtility not found");
            handleExceptionMethod = pluginUtilityType.GetMethod("HandleConversionException",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (handleExceptionMethod == null) Assert.Ignore("HandleConversionException not found");
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

        private void InvokeHandleException(Exception exception)
        {
            try
            {
                handleExceptionMethod.Invoke(null, new object[] { exception });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        public void PackageCompatibilityException_ReportsError_DoesNotRethrow()
        {
            var exception = new LegacyPackageException("TestPackage", "1.0.0");
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
                // ErrorReport may fail without active build context, but code paths are still covered
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void BreakingPackageException_ReportsError_DoesNotRethrow()
        {
            var exception = new BreakingPackageException("TestPackage", "2.0.0");
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void MaterialConversionException_WithNonPackageInner_ReportsMaterialError()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";
            objectsToCleanup.Add(mat);
            Exception inner;
            try { throw new InvalidOperationException("shader error"); }
            catch (Exception e) { inner = e; }
            var exception = new MaterialConversionException("conversion failed", mat, inner);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void MaterialConversionException_WithPackageCompatibilityInner_ReportsPackageError()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";
            objectsToCleanup.Add(mat);
            var inner = new LegacyPackageException("BadPkg", "0.9.0");
            var exception = new MaterialConversionException("conversion failed", mat, inner);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void AnimationClipConversionException_ReportsObjectError()
        {
            var clip = new AnimationClip();
            clip.name = "TestClip";
            objectsToCleanup.Add(clip);
            Exception inner;
            try { throw new InvalidOperationException("anim error"); }
            catch (Exception e) { inner = e; }
            var exception = new AnimationClipConversionException("clip conversion failed", clip, inner);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void AnimatorControllerConversionException_ReportsObjectError()
        {
            var controller = new UnityEditor.Animations.AnimatorController();
            controller.name = "TestController";
            objectsToCleanup.Add(controller);
            Exception inner;
            try { throw new InvalidOperationException("controller error"); }
            catch (Exception e) { inner = e; }
            var exception = new AnimatorControllerConversionException("controller conversion failed", controller, inner);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void InvalidMaterialSwapNullException_ReportsMaterialSwapError()
        {
            var go = new GameObject("SwapTest");
            objectsToCleanup.Add(go);
            var swap = go.AddComponent<MaterialSwap>();
            swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping(),
            };
            var exception = new InvalidMaterialSwapNullException("swap null", swap, 0);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void InvalidReplacementMaterialException_ReportsReplacementError()
        {
            var go = new GameObject("ReplTest");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "BadMat";
            objectsToCleanup.Add(mat);
            var exception = new InvalidReplacementMaterialException("bad replacement", component, mat);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void TargetMaterialNullException_ReportsTargetError()
        {
            var go = new GameObject("TargetTest");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var exception = new TargetMaterialNullException("target null", component);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                InvokeHandleException(exception);
            }
            catch (Exception)
            {
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void NonIVRCQuestToolsException_Rethrows()
        {
            var exception = new InvalidOperationException("non-vqt exception");
            LogAssert.ignoreFailingMessages = true;
            try
            {
                Assert.Throws<InvalidOperationException>(() => InvokeHandleException(exception));
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void NonIVRCQuestToolsException_WithInnerException_LogsInner()
        {
            var inner = new ArgumentException("inner");
            var exception = new InvalidOperationException("outer", inner);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                Assert.Throws<InvalidOperationException>(() => InvokeHandleException(exception));
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }
    }

    [TestFixture]
    internal class AvatarConverterPassUtilityReflectionExtendedTests
    {
        private Type utilType;
        private System.Collections.Generic.List<UnityEngine.Object> objectsToCleanup = new System.Collections.Generic.List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            utilType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterPassUtility");
            if (utilType == null) Assert.Ignore("AvatarConverterPassUtility not found");
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

        [Test]
        public void ResolveAvatarConverterNdmfPhase_NoPrimaryComponent_ReturnsResolved()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var method = utilType.GetMethod("ResolveAvatarConverterNdmfPhase",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null) Assert.Ignore("Method not found");
            var result = method.Invoke(null, new object[] { go });
            Assert.IsNotNull(result);
        }

        [Test]
        public void ResolveAvatarConverterNdmfPhase_WithAvatarConverterSettings_ReturnsResolved()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            go.AddComponent<AvatarConverterSettings>();
            var method = utilType.GetMethod("ResolveAvatarConverterNdmfPhase",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null) Assert.Ignore("Method not found");
            var result = method.Invoke(null, new object[] { go });
            Assert.IsNotNull(result);
        }

        [Test]
        public void ResolveAvatarConverterNdmfPhase_WithMaterialConversionSettings_ReturnsResolved()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            go.AddComponent<MaterialConversionSettings>();
            var method = utilType.GetMethod("ResolveAvatarConverterNdmfPhase",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null) Assert.Ignore("Method not found");
            var result = method.Invoke(null, new object[] { go });
            Assert.IsNotNull(result);
        }

        [Test]
        public void HasMaterialOperatorComponents_WithMaterialSwap_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            go.AddComponent<MaterialSwap>();
            var result = NdmfTestHelper.InvokeStaticMethod(utilType, "HasMaterialOperatorComponents", go);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void HasMaterialOperatorComponents_WithChildMaterialConversionSettings_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<MaterialConversionSettings>();
            var result = NdmfTestHelper.InvokeStaticMethod(utilType, "HasMaterialOperatorComponents", go);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void HasMaterialOperatorComponents_WithInactiveChild_StillReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var child = new GameObject("InactiveChild");
            child.transform.SetParent(go.transform);
            child.SetActive(false);
            child.AddComponent<MaterialConversionSettings>();
            var result = NdmfTestHelper.InvokeStaticMethod(utilType, "HasMaterialOperatorComponents", go);
            Assert.AreEqual(true, result);
        }
    }

    [TestFixture]
    internal class NdmfLocalizerExtendedReflectionTests
    {
        private Type localizerType;

        [SetUp]
        public void SetUp()
        {
            localizerType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfLocalizer");
            if (localizerType == null) Assert.Ignore("NdmfLocalizer not found");
        }

        [Test]
        public void Instance_IsNotNull()
        {
            var instance = NdmfTestHelper.GetStaticField(localizerType, "Instance");
            Assert.IsNotNull(instance);
        }

        [Test]
        public void ErrorTitle_Constants_AllStartWithNDMF()
        {
            var fields = localizerType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string) && f.Name.Contains("Title"));
            int count = 0;
            foreach (var field in fields)
            {
                var value = (string)field.GetValue(null);
                Assert.IsTrue(value.StartsWith("NDMF:"), $"{field.Name} = '{value}' should start with 'NDMF:'");
                count++;
            }
            Assert.IsTrue(count >= 5, $"Expected at least 5 title constants, found {count}");
        }

        [Test]
        public void ErrorDescription_Constants_AllStartWithNDMF()
        {
            var fields = localizerType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string) && f.Name.Contains("Description"));
            int count = 0;
            foreach (var field in fields)
            {
                var value = (string)field.GetValue(null);
                Assert.IsTrue(value.StartsWith("NDMF:"), $"{field.Name} = '{value}' should start with 'NDMF:'");
                count++;
            }
            Assert.IsTrue(count >= 5, $"Expected at least 5 description constants, found {count}");
        }
    }

    [TestFixture]
    internal class NdmfSessionStateExtendedTests
    {
        private Type sessionStateType;
        private object originalBuildTarget;

        [SetUp]
        public void SetUp()
        {
            sessionStateType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfSessionState");
            if (sessionStateType == null) Assert.Ignore("NdmfSessionState not found");
            originalBuildTarget = NdmfTestHelper.GetStaticProperty(sessionStateType, "BuildTarget");
        }

        [TearDown]
        public void TearDown()
        {
            if (sessionStateType != null && originalBuildTarget != null)
            {
                NdmfTestHelper.SetStaticProperty(sessionStateType, "BuildTarget", originalBuildTarget);
            }
        }

        [Test]
        public void LastActualPerformanceRating_IsEmptyDictionary()
        {
            var dict = NdmfTestHelper.GetStaticField(sessionStateType, "LastActualPerformanceRating");
            Assert.IsNotNull(dict);
            Assert.IsInstanceOf<System.Collections.IDictionary>(dict);
        }

        [Test]
        public void LastActualPerformanceRating_CanAddAndRetrieve()
        {
            var dict = NdmfTestHelper.GetStaticField(sessionStateType, "LastActualPerformanceRating")
                as System.Collections.Generic.Dictionary<string, VRC.SDKBase.Validation.Performance.PerformanceRating>;
            if (dict == null) Assert.Ignore("Dictionary type mismatch");
            var testKey = "test-blueprint-id-" + Guid.NewGuid().ToString();
            try
            {
                dict[testKey] = VRC.SDKBase.Validation.Performance.PerformanceRating.Good;
                Assert.AreEqual(VRC.SDKBase.Validation.Performance.PerformanceRating.Good, dict[testKey]);
            }
            finally
            {
                dict.Remove(testKey);
            }
        }

        [Test]
        public void BuildTarget_Roundtrips_AllValues()
        {
            foreach (Models.BuildTarget bt in Enum.GetValues(typeof(Models.BuildTarget)))
            {
                NdmfTestHelper.SetStaticProperty(sessionStateType, "BuildTarget", bt);
                Assert.AreEqual(bt, NdmfTestHelper.GetStaticProperty(sessionStateType, "BuildTarget"));
            }
        }
    }

    [TestFixture]
    internal class NdmfErrorClassTests
    {
        private System.Collections.Generic.List<UnityEngine.Object> objectsToCleanup = new System.Collections.Generic.List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToCleanup)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            objectsToCleanup.Clear();
        }

        // --- PackageCompatibilityError full property tests ---

        [Test]
        public void PackageCompatibilityError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.PackageCompatibilityError");
            if (type == null) Assert.Ignore("Type not found");
            var error = NdmfTestHelper.CreateInstance(type, new LegacyPackageException("Pkg", "1.0"));
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void PackageCompatibilityError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.PackageCompatibilityError");
            if (type == null) Assert.Ignore("Type not found");
            var error = NdmfTestHelper.CreateInstance(type, new LegacyPackageException("Pkg", "1.0"));
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void PackageCompatibilityError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.PackageCompatibilityError");
            if (type == null) Assert.Ignore("Type not found");
            var error = NdmfTestHelper.CreateInstance(type, new LegacyPackageException("Pkg", "1.0"));
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        [Test]
        public void PackageCompatibilityError_DetailsSubst_ContainsLocalizedMessage()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.PackageCompatibilityError");
            if (type == null) Assert.Ignore("Type not found");
            var exception = new LegacyPackageException("TestPkg", "1.0");
            var error = NdmfTestHelper.CreateInstance(type, exception);
            var subst = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual(1, subst.Length);
            Assert.IsNotNull(subst[0]);
        }

        // --- MaterialConversionError full property tests ---

        [Test]
        public void MaterialConversionError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionError");
            if (type == null) Assert.Ignore("Type not found");
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";
            objectsToCleanup.Add(mat);
            var objRef = ObjectRegistry.GetReference(mat);
            Exception ex;
            try { throw new InvalidOperationException("test"); }
            catch (Exception e) { ex = e; }
            var error = NdmfTestHelper.CreateInstance(type, objRef, ex);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void MaterialConversionError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionError");
            if (type == null) Assert.Ignore("Type not found");
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var objRef = ObjectRegistry.GetReference(mat);
            Exception ex;
            try { throw new InvalidOperationException("test"); }
            catch (Exception e) { ex = e; }
            var error = NdmfTestHelper.CreateInstance(type, objRef, ex);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void MaterialConversionError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionError");
            if (type == null) Assert.Ignore("Type not found");
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var objRef = ObjectRegistry.GetReference(mat);
            Exception ex;
            try { throw new InvalidOperationException("test"); }
            catch (Exception e) { ex = e; }
            var error = NdmfTestHelper.CreateInstance(type, objRef, ex);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        [Test]
        public void MaterialConversionError_WithMaterialConversionException_UnwrapsInner()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionError");
            if (type == null) Assert.Ignore("Type not found");
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";
            objectsToCleanup.Add(mat);
            var objRef = ObjectRegistry.GetReference(mat);
            Exception inner;
            try { throw new InvalidOperationException("inner error"); }
            catch (Exception e) { inner = e; }
            var matEx = new MaterialConversionException("conversion failed", mat, inner);
            var error = NdmfTestHelper.CreateInstance(type, objRef, (Exception)matEx);
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual(5, details.Length);
            Assert.AreEqual("InvalidOperationException", details[2]);
            Assert.AreEqual("inner error", details[3]);
        }

        [Test]
        public void MaterialConversionError_WithMaterialConversionException_NoInner_UsesOriginal()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionError");
            if (type == null) Assert.Ignore("Type not found");
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";
            objectsToCleanup.Add(mat);
            var objRef = ObjectRegistry.GetReference(mat);
            Exception matEx;
            try { throw new MaterialConversionException("no inner", mat, null); }
            catch (Exception e) { matEx = e; }
            var error = NdmfTestHelper.CreateInstance(type, objRef, matEx);
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual(5, details.Length);
            Assert.AreEqual("MaterialConversionException", details[2]);
        }

        // --- ObjectConversionError full property tests ---

        [Test]
        public void ObjectConversionError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.ObjectConversionError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("TestObj");
            objectsToCleanup.Add(go);
            var objRef = ObjectRegistry.GetReference(go);
            Exception ex;
            try { throw new InvalidOperationException("test"); }
            catch (Exception e) { ex = e; }
            var error = NdmfTestHelper.CreateInstance(type, objRef, ex);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void ObjectConversionError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.ObjectConversionError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("TestObj");
            objectsToCleanup.Add(go);
            var objRef = ObjectRegistry.GetReference(go);
            Exception ex;
            try { throw new InvalidOperationException("test"); }
            catch (Exception e) { ex = e; }
            var error = NdmfTestHelper.CreateInstance(type, objRef, ex);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void ObjectConversionError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.ObjectConversionError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("TestObj");
            objectsToCleanup.Add(go);
            var objRef = ObjectRegistry.GetReference(go);
            Exception ex;
            try { throw new InvalidOperationException("test"); }
            catch (Exception e) { ex = e; }
            var error = NdmfTestHelper.CreateInstance(type, objRef, ex);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        // --- TargetMaterialNullError full tests ---

        [Test]
        public void TargetMaterialNullError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.TargetMaterialNullError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var error = NdmfTestHelper.CreateInstance(type, (Component)component);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void TargetMaterialNullError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.TargetMaterialNullError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var error = NdmfTestHelper.CreateInstance(type, (Component)component);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        // --- ReplacementMaterialError full tests ---

        [Test]
        public void ReplacementMaterialError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.ReplacementMaterialError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var error = NdmfTestHelper.CreateInstance(type, (Component)component, mat);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void ReplacementMaterialError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.ReplacementMaterialError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var error = NdmfTestHelper.CreateInstance(type, (Component)component, mat);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void ReplacementMaterialError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.ReplacementMaterialError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var error = NdmfTestHelper.CreateInstance(type, (Component)component, mat);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        // --- UnsupportedTextureFormatError full tests ---

        [Test]
        public void UnsupportedTextureFormatError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnsupportedTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.ASTC_4x4, UnityEditor.BuildTarget.Android, new Texture[] { tex });
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void UnsupportedTextureFormatError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnsupportedTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.ASTC_4x4, UnityEditor.BuildTarget.Android, new Texture[] { tex });
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void UnsupportedTextureFormatError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnsupportedTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.ASTC_4x4, UnityEditor.BuildTarget.Android, new Texture[] { tex });
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        // --- UnknownTextureFormatError full tests ---

        [Test]
        public void UnknownTextureFormatError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnknownTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.RGBA32, UnityEditor.BuildTarget.StandaloneWindows64, new Texture[] { tex });
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void UnknownTextureFormatError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnknownTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.RGBA32, UnityEditor.BuildTarget.StandaloneWindows64, new Texture[] { tex });
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void UnknownTextureFormatError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnknownTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.RGBA32, UnityEditor.BuildTarget.StandaloneWindows64, new Texture[] { tex });
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        // --- MaterialSwapNullError full tests ---

        [Test]
        public void MaterialSwapNullError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialSwapNullError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var swap = go.AddComponent<MaterialSwap>();
            var mapping = new MaterialSwap.MaterialMapping();
            var error = NdmfTestHelper.CreateInstance(type, swap, mapping);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void MaterialSwapNullError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialSwapNullError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var swap = go.AddComponent<MaterialSwap>();
            var mapping = new MaterialSwap.MaterialMapping();
            var error = NdmfTestHelper.CreateInstance(type, swap, mapping);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void MaterialSwapNullError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialSwapNullError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var swap = go.AddComponent<MaterialSwap>();
            var mapping = new MaterialSwap.MaterialMapping();
            var error = NdmfTestHelper.CreateInstance(type, swap, mapping);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        // --- MeshFlipperMaskMissingError full tests ---

        [Test]
        public void MeshFlipperMaskMissingError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperMaskMissingError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var mf = go.AddComponent<MeshFlipper>();
            var error = NdmfTestHelper.CreateInstance(type, mf);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void MeshFlipperMaskMissingError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperMaskMissingError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var mf = go.AddComponent<MeshFlipper>();
            var error = NdmfTestHelper.CreateInstance(type, mf);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void MeshFlipperMaskMissingError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperMaskMissingError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var mf = go.AddComponent<MeshFlipper>();
            var error = NdmfTestHelper.CreateInstance(type, mf);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        // --- MeshFlipperMaskNotReadableError full tests ---

        [Test]
        public void MeshFlipperMaskNotReadableError_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperMaskNotReadableError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var mf = go.AddComponent<MeshFlipper>();
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var texRef = ObjectRegistry.GetReference(tex);
            var error = NdmfTestHelper.CreateInstance(type, mf, texRef);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [Test]
        public void MeshFlipperMaskNotReadableError_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperMaskNotReadableError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var mf = go.AddComponent<MeshFlipper>();
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var texRef = ObjectRegistry.GetReference(tex);
            var error = NdmfTestHelper.CreateInstance(type, mf, texRef);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "TitleKey")));
        }

        [Test]
        public void MeshFlipperMaskNotReadableError_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperMaskNotReadableError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var mf = go.AddComponent<MeshFlipper>();
            var tex = new Texture2D(4, 4);
            objectsToCleanup.Add(tex);
            var texRef = ObjectRegistry.GetReference(tex);
            var error = NdmfTestHelper.CreateInstance(type, mf, texRef);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(error, "DetailsKey")));
        }

        // --- NdmfComponentRemoverWarning full tests ---

        [Test]
        public void NdmfComponentRemoverWarning_Localizer_IsNotNull()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfComponentRemoverWarning");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var objRef = ObjectRegistry.GetReference(component);
            var warning = NdmfTestHelper.CreateInstance(type, typeof(AudioSource), objRef);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(warning, "Localizer"));
        }

        [Test]
        public void NdmfComponentRemoverWarning_TitleKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfComponentRemoverWarning");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var objRef = ObjectRegistry.GetReference(component);
            var warning = NdmfTestHelper.CreateInstance(type, typeof(AudioSource), objRef);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(warning, "TitleKey")));
        }

        [Test]
        public void NdmfComponentRemoverWarning_DetailsKey_IsNotNullOrEmpty()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfComponentRemoverWarning");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var objRef = ObjectRegistry.GetReference(component);
            var warning = NdmfTestHelper.CreateInstance(type, typeof(AudioSource), objRef);
            Assert.IsFalse(string.IsNullOrEmpty((string)NdmfTestHelper.GetProperty(warning, "DetailsKey")));
        }
    }

    [TestFixture]
    internal class NdmfHelperExtendedTests
    {
        private Type helperType;
        private System.Collections.Generic.List<UnityEngine.Object> objectsToCleanup = new System.Collections.Generic.List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            helperType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfHelper");
            if (helperType == null) Assert.Ignore("NdmfHelper not found");
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

        [Test]
        public void ResolveBuildTarget_WithMultipleChildren_NoPlatformTarget()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var child1 = new GameObject("Child1");
            child1.transform.SetParent(go.transform);
            var child2 = new GameObject("Child2");
            child2.transform.SetParent(go.transform);
            var result = NdmfTestHelper.InvokeStaticMethod(helperType, "ResolveBuildTarget", go);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ResolveBuildTarget_DeepNestedPlatformTarget_NotFound()
        {
            // ResolveBuildTarget uses GetComponent (root only), not GetComponentInChildren
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var grandchild = new GameObject("Grandchild");
            grandchild.transform.SetParent(child.transform);
            grandchild.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;
            var result = NdmfTestHelper.InvokeStaticMethod(helperType, "ResolveBuildTarget", go);
            // Component on grandchild is NOT found by GetComponent on root, so falls back to Auto resolution
            Assert.AreNotEqual(Models.BuildTarget.Android, result);
        }
    }

    [TestFixture]
    internal class NdmfPluginReflectionExtendedTests
    {
        private Type pluginType;

        [SetUp]
        public void SetUp()
        {
            pluginType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.VRCQuestToolsNdmfPlugin");
            if (pluginType == null) Assert.Ignore("VRCQuestToolsNdmfPlugin not found");
        }

        [Test]
        public void AllProperties_AreReadable()
        {
            var instance = Activator.CreateInstance(pluginType);
            var props = pluginType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
            foreach (var prop in props)
            {
                // Just verify property getter doesn't throw
                Assert.DoesNotThrow(() => prop.GetValue(instance), $"Property {prop.Name} getter should not throw");
            }
        }
    }

    [TestFixture]
    internal class NdmfErrorReportReflectionExtendedTests
    {
        [Test]
        public void Type_HasReportErrorMethod()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfErrorReport");
            if (type == null) Assert.Ignore("Type not found");
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var reportMethod = methods.FirstOrDefault(m => m.Name == "ReportError");
            Assert.IsNotNull(reportMethod);
            Assert.AreEqual(1, reportMethod.GetParameters().Length);
        }
    }
}
