#if VQT_HAS_NDMF
// Tests for VRCQuestTools-Editor-Ndmf assembly classes (using reflection since types are internal).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Helper to resolve types from VRCQuestTools-Editor-Ndmf assembly via reflection.
    /// </summary>
    internal static class NdmfTestHelper
    {
        private static Assembly ndmfAssembly;

        internal static Assembly NdmfAssembly
        {
            get
            {
                if (ndmfAssembly == null)
                {
                    ndmfAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "VRCQuestTools-Editor-Ndmf");
                }
                return ndmfAssembly;
            }
        }

        internal static Type GetNdmfType(string fullName)
        {
            return NdmfAssembly?.GetType(fullName);
        }

        internal static object CreateInstance(Type type, params object[] args)
        {
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Length == args.Length);
            if (ctor == null) throw new InvalidOperationException($"No matching constructor found for {type.Name} with {args.Length} args");
            return ctor.Invoke(args);
        }

        internal static object GetProperty(object instance, string propertyName)
        {
            var prop = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return prop?.GetValue(instance);
        }

        internal static object GetStaticProperty(Type type, string propertyName)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return prop?.GetValue(null);
        }

        internal static object GetField(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(instance);
        }

        internal static object GetStaticField(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(null);
        }

        internal static void SetStaticProperty(Type type, string propertyName, object value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            prop?.SetValue(null, value);
        }

        internal static object InvokeMethod(object instance, string methodName, params object[] args)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return method?.Invoke(instance, args);
        }

        internal static object InvokeStaticMethod(Type type, string methodName, params object[] args)
        {
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var method = methods.FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == args.Length);
            return method?.Invoke(null, args);
        }
    }

    [TestFixture]
    internal class SimpleStringErrorReflectionTests
    {
        private Type errorType;

        [SetUp]
        public void SetUp()
        {
            errorType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.SimpleStringError");
            if (errorType == null) Assert.Ignore("SimpleStringError type not found (NDMF not installed)");
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var error = NdmfTestHelper.CreateInstance(errorType, "Title", "Detail", "Hint", ErrorSeverity.Error);
            Assert.AreEqual("Title", NdmfTestHelper.GetProperty(error, "TitleKey"));
            Assert.AreEqual("Detail", NdmfTestHelper.GetProperty(error, "DetailsKey"));
            Assert.AreEqual("Hint", NdmfTestHelper.GetProperty(error, "HintKey"));
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void TitleSubst_ContainsTitle()
        {
            var error = NdmfTestHelper.CreateInstance(errorType, "MyTitle", "d", "h", ErrorSeverity.NonFatal);
            var subst = (string[])NdmfTestHelper.GetProperty(error, "TitleSubst");
            Assert.AreEqual(1, subst.Length);
            Assert.AreEqual("MyTitle", subst[0]);
        }

        [Test]
        public void DetailsSubst_ContainsDetail()
        {
            var error = NdmfTestHelper.CreateInstance(errorType, "t", "MyDetail", "h", ErrorSeverity.NonFatal);
            var subst = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual(1, subst.Length);
            Assert.AreEqual("MyDetail", subst[0]);
        }

        [Test]
        public void HintSubst_ContainsHint()
        {
            var error = NdmfTestHelper.CreateInstance(errorType, "t", "d", "MyHint", ErrorSeverity.NonFatal);
            var subst = (string[])NdmfTestHelper.GetProperty(error, "HintSubst");
            Assert.AreEqual(1, subst.Length);
            Assert.AreEqual("MyHint", subst[0]);
        }

        [Test]
        public void Localizer_IsNotNull()
        {
            var error = NdmfTestHelper.CreateInstance(errorType, "t", "d", "h", ErrorSeverity.Error);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(error, "Localizer"));
        }

        [TestCase(ErrorSeverity.Error)]
        [TestCase(ErrorSeverity.NonFatal)]
        [TestCase(ErrorSeverity.Information)]
        public void Severity_ReturnsCorrectValue(ErrorSeverity expected)
        {
            var error = NdmfTestHelper.CreateInstance(errorType, "t", "d", "h", expected);
            Assert.AreEqual(expected, NdmfTestHelper.GetProperty(error, "Severity"));
        }
    }

    [TestFixture]
    internal class NdmfStateReflectionTests
    {
        private Type stateType;

        [SetUp]
        public void SetUp()
        {
            stateType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfState");
            if (stateType == null) Assert.Ignore("NdmfState type not found");
        }

        [Test]
        public void CompressExpressionsMenuIcons_DefaultFalse()
        {
            var state = Activator.CreateInstance(stateType);
            var field = stateType.GetField("compressExpressionsMenuIcons", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsFalse((bool)field.GetValue(state));
        }

        [Test]
        public void CompressExpressionsMenuIcons_CanSetTrue()
        {
            var state = Activator.CreateInstance(stateType);
            var field = stateType.GetField("compressExpressionsMenuIcons", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            field.SetValue(state, true);
            Assert.IsTrue((bool)field.GetValue(state));
        }

        [Test]
        public void CompressExpressionsMenuIcons_CanToggle()
        {
            var state = Activator.CreateInstance(stateType);
            var field = stateType.GetField("compressExpressionsMenuIcons", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            field.SetValue(state, true);
            field.SetValue(state, false);
            Assert.IsFalse((bool)field.GetValue(state));
        }
    }

    [TestFixture]
    internal class NdmfSessionStateReflectionTests
    {
        private Type sessionStateType;
        private object originalBuildTarget;

        [SetUp]
        public void SetUp()
        {
            sessionStateType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfSessionState");
            if (sessionStateType == null) Assert.Ignore("NdmfSessionState type not found");
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
        public void BuildTarget_CanSetAndroid()
        {
            NdmfTestHelper.SetStaticProperty(sessionStateType, "BuildTarget", Models.BuildTarget.Android);
            Assert.AreEqual(Models.BuildTarget.Android, NdmfTestHelper.GetStaticProperty(sessionStateType, "BuildTarget"));
        }

        [Test]
        public void BuildTarget_CanSetPC()
        {
            NdmfTestHelper.SetStaticProperty(sessionStateType, "BuildTarget", Models.BuildTarget.PC);
            Assert.AreEqual(Models.BuildTarget.PC, NdmfTestHelper.GetStaticProperty(sessionStateType, "BuildTarget"));
        }

        [Test]
        public void BuildTarget_CanSetAuto()
        {
            NdmfTestHelper.SetStaticProperty(sessionStateType, "BuildTarget", Models.BuildTarget.Auto);
            Assert.AreEqual(Models.BuildTarget.Auto, NdmfTestHelper.GetStaticProperty(sessionStateType, "BuildTarget"));
        }

        [Test]
        public void LastActualPerformanceRating_IsNotNull()
        {
            var rating = NdmfTestHelper.GetStaticField(sessionStateType, "LastActualPerformanceRating");
            Assert.IsNotNull(rating);
        }
    }

    [TestFixture]
    internal class NdmfHelperReflectionTests
    {
        private Type helperType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            helperType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfHelper");
            if (helperType == null) Assert.Ignore("NdmfHelper type not found");
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
        public void ResolveBuildTarget_NoPlatformTargetSettings_ReturnsNonAuto()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var result = NdmfTestHelper.InvokeStaticMethod(helperType, "ResolveBuildTarget", go);
            Assert.IsNotNull(result);
            Assert.AreNotEqual(Models.BuildTarget.Auto, result);
        }

        [Test]
        public void ResolveBuildTarget_WithPlatformTargetSettings_Android()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var pts = go.AddComponent<PlatformTargetSettings>();
            pts.buildTarget = Models.BuildTarget.Android;
            var result = NdmfTestHelper.InvokeStaticMethod(helperType, "ResolveBuildTarget", go);
            Assert.AreEqual(Models.BuildTarget.Android, result);
        }

        [Test]
        public void ResolveBuildTarget_WithPlatformTargetSettings_PC()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var pts = go.AddComponent<PlatformTargetSettings>();
            pts.buildTarget = Models.BuildTarget.PC;
            var result = NdmfTestHelper.InvokeStaticMethod(helperType, "ResolveBuildTarget", go);
            Assert.AreEqual(Models.BuildTarget.PC, result);
        }

        [Test]
        public void ResolveBuildTarget_WithPlatformTargetSettings_Auto()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var pts = go.AddComponent<PlatformTargetSettings>();
            pts.buildTarget = Models.BuildTarget.Auto;
            var result = NdmfTestHelper.InvokeStaticMethod(helperType, "ResolveBuildTarget", go);
            Assert.AreNotEqual(Models.BuildTarget.Auto, result);
        }
    }

    [TestFixture]
    internal class NdmfLocalizerReflectionTests
    {
        private Type localizerType;

        [SetUp]
        public void SetUp()
        {
            localizerType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfLocalizer");
            if (localizerType == null) Assert.Ignore("NdmfLocalizer type not found");
        }

        [Test]
        public void Instance_IsNotNull()
        {
            var instance = NdmfTestHelper.GetStaticField(localizerType, "Instance");
            Assert.IsNotNull(instance);
        }

        [Test]
        public void AllConstants_AreNotNullOrEmpty()
        {
            var fields = localizerType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string));

            int count = 0;
            foreach (var field in fields)
            {
                var value = (string)field.GetValue(null);
                Assert.IsFalse(string.IsNullOrEmpty(value), $"Constant {field.Name} should not be null or empty");
                count++;
            }
            Assert.IsTrue(count > 10, $"Expected at least 10 constants, found {count}");
        }

        [Test]
        public void AllConstants_HaveNdmfPrefix()
        {
            var fields = localizerType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string));

            foreach (var field in fields)
            {
                var value = (string)field.GetValue(null);
                Assert.IsTrue(value.StartsWith("NDMF:"), $"{field.Name} = '{value}' should start with 'NDMF:'");
            }
        }
    }

    [TestFixture]
    internal class MissingNetworkIDAssignerWarningReflectionTests
    {
        private Type warningType;

        [SetUp]
        public void SetUp()
        {
            warningType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MissingNetworkIDAssignerWarning");
            if (warningType == null) Assert.Ignore("MissingNetworkIDAssignerWarning type not found");
        }

        [Test]
        public void Severity_IsNonFatal()
        {
            var instance = Activator.CreateInstance(warningType);
            Assert.AreEqual(ErrorSeverity.NonFatal, NdmfTestHelper.GetProperty(instance, "Severity"));
        }

        [Test]
        public void Localizer_IsNotNull()
        {
            var instance = Activator.CreateInstance(warningType);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(instance, "Localizer"));
        }

        [Test]
        public void TitleKey_IsNotNull()
        {
            var instance = Activator.CreateInstance(warningType);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(instance, "TitleKey"));
        }

        [Test]
        public void DetailsKey_IsNotNull()
        {
            var instance = Activator.CreateInstance(warningType);
            Assert.IsNotNull(NdmfTestHelper.GetProperty(instance, "DetailsKey"));
        }
    }

    [TestFixture]
    internal class NdmfComponentRemoverWarningReflectionTests
    {
        private Type warningType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            warningType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfComponentRemoverWarning");
            if (warningType == null) Assert.Ignore("NdmfComponentRemoverWarning type not found");
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
        public void Constructor_SetsProperties()
        {
            var go = new GameObject("TestObject");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var objRef = ObjectRegistry.GetReference(component);

            var warning = NdmfTestHelper.CreateInstance(warningType, typeof(AudioSource), objRef);
            Assert.AreEqual(ErrorSeverity.Information, NdmfTestHelper.GetProperty(warning, "Severity"));
        }

        [Test]
        public void DetailsSubst_ContainsTypeNameAndObjectName()
        {
            var go = new GameObject("MyGameObject");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var objRef = ObjectRegistry.GetReference(component);

            var warning = NdmfTestHelper.CreateInstance(warningType, typeof(AudioSource), objRef);
            var details = (string[])NdmfTestHelper.GetProperty(warning, "DetailsSubst");
            Assert.AreEqual(2, details.Length);
            Assert.AreEqual("AudioSource", details[0]);
            Assert.AreEqual("MyGameObject", details[1]);
        }
    }

    [TestFixture]
    internal class NdmfObjectRegistryReflectionTests
    {
        private Type registryType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            registryType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfObjectRegistry");
            if (registryType == null) Assert.Ignore("NdmfObjectRegistry type not found");
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
        public void GetReference_ReturnsObjectReference()
        {
            var go = new GameObject("TestObj");
            objectsToCleanup.Add(go);
            var result = NdmfTestHelper.InvokeStaticMethod(registryType, "GetReference", (UnityEngine.Object)go);
            Assert.IsNotNull(result);
            var objRef = (ObjectReference)result;
            Assert.AreEqual(go, objRef.Object);
        }

        [Test]
        public void RegisterReplacedObject_RegistersSuccessfully()
        {
            var registry = Activator.CreateInstance(registryType);
            var original = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(original);
            var replaced = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(replaced);

            NdmfTestHelper.InvokeMethod(registry, "RegisterReplacedObject", original, (UnityEngine.Object)replaced);
        }

        [Test]
        public void RegisterReplacedObject_DuplicateRegistration_LogsWarning()
        {
            var registry = Activator.CreateInstance(registryType);
            var original = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(original);
            var replaced = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(replaced);

            NdmfTestHelper.InvokeMethod(registry, "RegisterReplacedObject", original, (UnityEngine.Object)replaced);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already registered"));
            NdmfTestHelper.InvokeMethod(registry, "RegisterReplacedObject", original, (UnityEngine.Object)replaced);
        }
    }

    [TestFixture]
    internal class PackageCompatibilityErrorReflectionTests
    {
        private Type errorType;

        [SetUp]
        public void SetUp()
        {
            errorType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.PackageCompatibilityError");
            if (errorType == null) Assert.Ignore("PackageCompatibilityError type not found");
        }

        [Test]
        public void Severity_IsError()
        {
            var exception = new LegacyPackageException("TestPackage", "1.0.0");
            var error = NdmfTestHelper.CreateInstance(errorType, exception);
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void TitleSubst_ContainsPackageName()
        {
            var exception = new LegacyPackageException("TestPackage", "1.0.0");
            var error = NdmfTestHelper.CreateInstance(errorType, exception);
            var subst = (string[])NdmfTestHelper.GetProperty(error, "TitleSubst");
            Assert.AreEqual("TestPackage", subst[0]);
        }

        [Test]
        public void BreakingPackageException_AlsoWorks()
        {
            var exception = new BreakingPackageException("AnotherPkg", "3.0.0");
            var error = NdmfTestHelper.CreateInstance(errorType, exception);
            var subst = (string[])NdmfTestHelper.GetProperty(error, "TitleSubst");
            Assert.AreEqual("AnotherPkg", subst[0]);
        }
    }

    [TestFixture]
    internal class TargetMaterialNullErrorReflectionTests
    {
        private Type errorType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            errorType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.TargetMaterialNullError");
            if (errorType == null) Assert.Ignore("TargetMaterialNullError type not found");
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
        public void Severity_IsError()
        {
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var error = NdmfTestHelper.CreateInstance(errorType, (Component)component);
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void DetailsSubst_ContainsTypeName()
        {
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var error = NdmfTestHelper.CreateInstance(errorType, (Component)component);
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual("AudioSource", details[0]);
        }
    }

    [TestFixture]
    internal class TextureFormatErrorReflectionTests
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

        [Test]
        public void UnsupportedTextureFormatError_Severity_IsNonFatal()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnsupportedTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4, TextureFormat.ASTC_4x4, false);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.ASTC_4x4, UnityEditor.BuildTarget.Android, new Texture[] { tex });
            Assert.AreEqual(ErrorSeverity.NonFatal, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void UnsupportedTextureFormatError_DetailsSubst_ContainsFormatAndTarget()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnsupportedTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4, TextureFormat.ASTC_4x4, false);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.ASTC_4x4, UnityEditor.BuildTarget.Android, new Texture[] { tex });
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual("ASTC_4x4", details[0]);
            Assert.AreEqual("Android", details[1]);
        }

        [Test]
        public void UnknownTextureFormatError_Severity_IsNonFatal()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnknownTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.RGBA32, UnityEditor.BuildTarget.StandaloneWindows64, new Texture[] { tex });
            Assert.AreEqual(ErrorSeverity.NonFatal, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void UnknownTextureFormatError_DetailsSubst_ContainsFormatAndTarget()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnknownTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            objectsToCleanup.Add(tex);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.RGBA32, UnityEditor.BuildTarget.StandaloneWindows64, new Texture[] { tex });
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual("RGBA32", details[0]);
            Assert.AreEqual("StandaloneWindows64", details[1]);
        }

        [Test]
        public void MultipleTextures_Tracked()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.UnsupportedTextureFormatError");
            if (type == null) Assert.Ignore("Type not found");
            var tex1 = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            objectsToCleanup.Add(tex1);
            var tex2 = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            objectsToCleanup.Add(tex2);
            var error = NdmfTestHelper.CreateInstance(type, TextureFormat.RGBA32, UnityEditor.BuildTarget.Android, new Texture[] { tex1, tex2 });
            Assert.IsNotNull(error);
        }
    }

    [TestFixture]
    internal class MaterialConversionErrorReflectionTests
    {
        private Type errorType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            errorType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionError");
            if (errorType == null) Assert.Ignore("MaterialConversionError type not found");
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
        public void Severity_IsError()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMaterial";
            objectsToCleanup.Add(mat);
            var objRef = ObjectRegistry.GetReference(mat);
            InvalidOperationException exception;
            try { throw new InvalidOperationException("Test error"); }
            catch (InvalidOperationException e) { exception = e; }

            var error = NdmfTestHelper.CreateInstance(errorType, objRef, (Exception)exception);
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void DetailsSubst_ContainsMaterialAndShaderInfo()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "MyMaterial";
            objectsToCleanup.Add(mat);
            var objRef = ObjectRegistry.GetReference(mat);
            InvalidOperationException exception;
            try { throw new InvalidOperationException("Something went wrong"); }
            catch (InvalidOperationException e) { exception = e; }

            var error = NdmfTestHelper.CreateInstance(errorType, objRef, (Exception)exception);
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.IsTrue(details.Length >= 4);
            Assert.AreEqual("MyMaterial", details[0]);
            Assert.AreEqual("Standard", details[1]);
            Assert.AreEqual("InvalidOperationException", details[2]);
        }
    }

    [TestFixture]
    internal class ObjectConversionErrorReflectionTests
    {
        private Type errorType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            errorType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.ObjectConversionError");
            if (errorType == null) Assert.Ignore("ObjectConversionError type not found");
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
        public void Severity_IsError()
        {
            var go = new GameObject("TestObj");
            objectsToCleanup.Add(go);
            var objRef = ObjectRegistry.GetReference(go);
            InvalidOperationException exception;
            try { throw new InvalidOperationException("Test"); }
            catch (InvalidOperationException e) { exception = e; }

            var error = NdmfTestHelper.CreateInstance(errorType, objRef, (Exception)exception);
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void DetailsSubst_ContainsObjectInfo()
        {
            var go = new GameObject("MyObject");
            objectsToCleanup.Add(go);
            var objRef = ObjectRegistry.GetReference(go);
            InvalidOperationException exception;
            try { throw new InvalidOperationException("Obj conversion failed"); }
            catch (InvalidOperationException e) { exception = e; }

            var error = NdmfTestHelper.CreateInstance(errorType, objRef, (Exception)exception);
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.IsTrue(details.Length >= 4);
            Assert.AreEqual("GameObject", details[0]);
            Assert.AreEqual("MyObject", details[1]);
        }
    }

    [TestFixture]
    internal class ReplacementMaterialErrorReflectionTests
    {
        private Type errorType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            errorType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.ReplacementMaterialError");
            if (errorType == null) Assert.Ignore("ReplacementMaterialError type not found");
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
        public void Severity_IsError()
        {
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var mat = new Material(Shader.Find("Standard"));
            objectsToCleanup.Add(mat);
            var error = NdmfTestHelper.CreateInstance(errorType, (Component)component, mat);
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void DetailsSubst_ContainsMaterialAndShaderInfo()
        {
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var component = go.AddComponent<AudioSource>();
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "MyReplacementMat";
            objectsToCleanup.Add(mat);
            var error = NdmfTestHelper.CreateInstance(errorType, (Component)component, mat);
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual("MyReplacementMat", details[0]);
            Assert.AreEqual("Standard", details[1]);
        }
    }

    [TestFixture]
    internal class MeshFlipperErrorReflectionTests
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

        [Test]
        public void MeshFlipperMaskMissingError_Severity_IsError()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperMaskMissingError");
            if (type == null) Assert.Ignore("Type not found");
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var mf = go.AddComponent<MeshFlipper>();
            var error = NdmfTestHelper.CreateInstance(type, mf);
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void MeshFlipperMaskNotReadableError_Severity_IsError()
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
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }
    }

    [TestFixture]
    internal class MaterialSwapNullErrorReflectionTests
    {
        private Type errorType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            errorType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialSwapNullError");
            if (errorType == null) Assert.Ignore("MaterialSwapNullError type not found");
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
        public void Severity_IsError()
        {
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var swap = go.AddComponent<MaterialSwap>();
            var mapping = new MaterialSwap.MaterialMapping { originalMaterial = null, replacementMaterial = null };
            var error = NdmfTestHelper.CreateInstance(errorType, swap, mapping);
            Assert.AreEqual(ErrorSeverity.Error, NdmfTestHelper.GetProperty(error, "Severity"));
        }

        [Test]
        public void DetailsSubst_NullMaterials_ShowsNone()
        {
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var swap = go.AddComponent<MaterialSwap>();
            var mapping = new MaterialSwap.MaterialMapping { originalMaterial = null, replacementMaterial = null };
            var error = NdmfTestHelper.CreateInstance(errorType, swap, mapping);
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual("None", details[0]);
            Assert.AreEqual("None", details[1]);
        }

        [Test]
        public void DetailsSubst_WithMaterials_ShowsNames()
        {
            var go = new GameObject("Test");
            objectsToCleanup.Add(go);
            var swap = go.AddComponent<MaterialSwap>();
            var origMat = new Material(Shader.Find("Standard"));
            origMat.name = "OrigMat";
            objectsToCleanup.Add(origMat);
            var replMat = new Material(Shader.Find("Standard"));
            replMat.name = "ReplMat";
            objectsToCleanup.Add(replMat);
            var mapping = new MaterialSwap.MaterialMapping { originalMaterial = origMat, replacementMaterial = replMat };
            var error = NdmfTestHelper.CreateInstance(errorType, swap, mapping);
            var details = (string[])NdmfTestHelper.GetProperty(error, "DetailsSubst");
            Assert.AreEqual("OrigMat", details[0]);
            Assert.AreEqual("ReplMat", details[1]);
        }
    }

    [TestFixture]
    internal class VRCQuestToolsNdmfPluginReflectionTests
    {
        private Type pluginType;

        [SetUp]
        public void SetUp()
        {
            pluginType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.VRCQuestToolsNdmfPlugin");
            if (pluginType == null) Assert.Ignore("VRCQuestToolsNdmfPlugin type not found");
        }

        [Test]
        public void Type_Exists()
        {
            Assert.IsNotNull(pluginType);
        }

        [Test]
        public void DisplayName_IsVRCQuestTools()
        {
            var instance = Activator.CreateInstance(pluginType);
            Assert.AreEqual("VRCQuestTools", NdmfTestHelper.GetProperty(instance, "DisplayName"));
        }

        [Test]
        public void QualifiedName_IsCorrect()
        {
            var instance = Activator.CreateInstance(pluginType);
            Assert.AreEqual("com.github.kurotu.vrc-quest-tools", NdmfTestHelper.GetProperty(instance, "QualifiedName"));
        }
    }

    [TestFixture]
    internal class AvatarConverterPassUtilityReflectionTests
    {
        private Type utilType;
        private List<UnityEngine.Object> objectsToCleanup = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            utilType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterPassUtility");
            if (utilType == null) Assert.Ignore("AvatarConverterPassUtility type not found");
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
        public void HasMaterialOperatorComponents_NoComponents_ReturnsFalse()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var result = NdmfTestHelper.InvokeStaticMethod(utilType, "HasMaterialOperatorComponents", go);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void HasMaterialOperatorComponents_WithAvatarConverterSettings_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            go.AddComponent<AvatarConverterSettings>();
            var result = NdmfTestHelper.InvokeStaticMethod(utilType, "HasMaterialOperatorComponents", go);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void HasMaterialOperatorComponents_WithChildComponent_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            objectsToCleanup.Add(go);
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<MaterialConversionSettings>();
            var result = NdmfTestHelper.InvokeStaticMethod(utilType, "HasMaterialOperatorComponents", go);
            Assert.AreEqual(true, result);
        }
    }

    [TestFixture]
    internal class NdmfPassTypeReflectionTests
    {
        [TestCase("KRT.VRCQuestTools.Ndmf.BuildTargetConfigurationPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.AvatarConverterResolvingPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.AvatarConverterTransformingPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.AvatarConverterOptimizingPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.PlatformGameObjectRemoverPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.PlatformComponentRemoverPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.AssignNetworkIDsPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.MeshFlipperPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.MeshFlipperAfterPolygonReductionPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.RemoveVertexColorPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.CheckTextureFormatPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.MenuIconResizerPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.RemoveUnsupportedComponentsPass")]
        [TestCase("KRT.VRCQuestTools.Ndmf.RemoveVRCQuestToolsComponentsPass")]
        public void PassType_Exists(string fullName)
        {
            var type = NdmfTestHelper.GetNdmfType(fullName);
            Assert.IsNotNull(type, $"Pass type {fullName} should exist");
        }

        [Test]
        public void AllPasses_HaveDisplayName()
        {
            var passNames = new[]
            {
                "BuildTargetConfigurationPass", "AvatarConverterResolvingPass",
                "AvatarConverterTransformingPass", "AvatarConverterOptimizingPass",
                "PlatformGameObjectRemoverPass", "PlatformComponentRemoverPass",
                "AssignNetworkIDsPass", "MeshFlipperPass", "MeshFlipperAfterPolygonReductionPass",
                "RemoveVertexColorPass", "CheckTextureFormatPass", "MenuIconResizerPass",
                "RemoveUnsupportedComponentsPass", "RemoveVRCQuestToolsComponentsPass",
            };

            foreach (var name in passNames)
            {
                var type = NdmfTestHelper.GetNdmfType($"KRT.VRCQuestTools.Ndmf.{name}");
                if (type == null) continue;
                var displayNameProp = type.GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(displayNameProp, $"{name} should have DisplayName property");
            }
        }
    }

    [TestFixture]
    internal class VRCQuestToolsActualPerformanceCallbackReflectionTests
    {
        private Type callbackType;

        [SetUp]
        public void SetUp()
        {
            callbackType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.VRCQuestToolsActualPerformanceCallback");
            if (callbackType == null) Assert.Ignore("VRCQuestToolsActualPerformanceCallback type not found");
        }

        [Test]
        public void CallbackOrder_IsMaxValue()
        {
            var instance = Activator.CreateInstance(callbackType);
            var order = NdmfTestHelper.GetProperty(instance, "callbackOrder");
            Assert.AreEqual(int.MaxValue, order);
        }
    }

    [TestFixture]
    internal class NdmfPreviewFilterReflectionTests
    {
        [Test]
        public void MaterialConversionFilter_Exists()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionFilter");
            Assert.IsNotNull(type, "MaterialConversionFilter should exist");
        }

        [Test]
        public void MeshFlipperFilter_Exists()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperFilter");
            Assert.IsNotNull(type, "MeshFlipperFilter should exist");
        }

        [Test]
        public void VertexColorRemoverFilter_Exists()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.VertexColorRemoverFilter");
            Assert.IsNotNull(type, "VertexColorRemoverFilter should exist");
        }

        [Test]
        public void AllFilters_ImplementIRenderFilter()
        {
            var irfType = typeof(nadena.dev.ndmf.preview.IRenderFilter);
            foreach (var name in new[] { "MaterialConversionFilter", "MeshFlipperFilter", "VertexColorRemoverFilter" })
            {
                var type = NdmfTestHelper.GetNdmfType($"KRT.VRCQuestTools.Ndmf.{name}");
                if (type == null) continue;
                Assert.IsTrue(irfType.IsAssignableFrom(type), $"{name} should implement IRenderFilter");
            }
        }
    }

    [TestFixture]
    internal class NdmfErrorReportReflectionTests
    {
        [Test]
        public void Type_Exists()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfErrorReport");
            Assert.IsNotNull(type);
        }

        [Test]
        public void ReportError_MethodExists()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfErrorReport");
            if (type == null) Assert.Ignore("Type not found");
            var method = type.GetMethod("ReportError", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "ReportError method should exist");
        }
    }

    [TestFixture]
    internal class NdmfAssemblyReflectionTests
    {
        [Test]
        public void NdmfAssembly_IsLoaded()
        {
            Assert.IsNotNull(NdmfTestHelper.NdmfAssembly, "VRCQuestTools-Editor-Ndmf assembly should be loaded");
        }

        [Test]
        public void NdmfAssembly_ContainsExpectedTypes()
        {
            var assembly = NdmfTestHelper.NdmfAssembly;
            if (assembly == null) Assert.Ignore("NDMF assembly not loaded");

            var expectedTypes = new[]
            {
                "KRT.VRCQuestTools.Ndmf.VRCQuestToolsNdmfPlugin",
                "KRT.VRCQuestTools.Ndmf.NdmfHelper",
                "KRT.VRCQuestTools.Ndmf.NdmfState",
                "KRT.VRCQuestTools.Ndmf.NdmfSessionState",
                "KRT.VRCQuestTools.Ndmf.NdmfLocalizer",
                "KRT.VRCQuestTools.Ndmf.NdmfObjectRegistry",
                "KRT.VRCQuestTools.Ndmf.NdmfErrorReport",
                "KRT.VRCQuestTools.Ndmf.SimpleStringError",
            };

            foreach (var typeName in expectedTypes)
            {
                Assert.IsNotNull(assembly.GetType(typeName), $"Type {typeName} should exist in NDMF assembly");
            }
        }
    }
}
#endif
