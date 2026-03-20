// Tests for NdmfHelper.ResolveBuildTarget and NdmfPluginUtility.SetBuildTarget.

using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.NDMF
{
    [TestFixture]
    internal class NdmfHelperResolveBuildTargetTests
    {
        private Type helperType;
        private MethodInfo resolveBuildTargetMethod;

        [SetUp]
        public void SetUp()
        {
            helperType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfHelper");
            if (helperType == null)
                Assert.Ignore("NdmfHelper type not found.");

            resolveBuildTargetMethod = helperType.GetMethod(
                "ResolveBuildTarget",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(GameObject) },
                null);
            if (resolveBuildTargetMethod == null)
                Assert.Ignore("ResolveBuildTarget(GameObject) method not found.");
        }

        [Test]
        public void ResolveBuildTarget_WithoutPlatformTargetSettings_ReturnsNonAuto()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var result = resolveBuildTargetMethod.Invoke(null, new object[] { go });
                Assert.IsNotNull(result);
                // Without PlatformTargetSettings, it should resolve Auto to PC or Android based on EditorUserBuildSettings
                var buildTarget = (int)result;
                Assert.AreNotEqual(0, buildTarget, "Should not return Auto (0) after resolution.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResolveBuildTarget_WithPlatformTargetSettings_SetToPC_ReturnsPC()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var comp = go.AddComponent<KRT.VRCQuestTools.Components.PlatformTargetSettings>();
                comp.buildTarget = KRT.VRCQuestTools.Models.BuildTarget.PC;

                var result = resolveBuildTargetMethod.Invoke(null, new object[] { go });
                Assert.AreEqual(KRT.VRCQuestTools.Models.BuildTarget.PC, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResolveBuildTarget_WithPlatformTargetSettings_SetToAndroid_ReturnsAndroid()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var comp = go.AddComponent<KRT.VRCQuestTools.Components.PlatformTargetSettings>();
                comp.buildTarget = KRT.VRCQuestTools.Models.BuildTarget.Android;

                var result = resolveBuildTargetMethod.Invoke(null, new object[] { go });
                Assert.AreEqual(KRT.VRCQuestTools.Models.BuildTarget.Android, result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResolveBuildTarget_WithPlatformTargetSettings_SetToAuto_ReturnsNonAuto()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var comp = go.AddComponent<KRT.VRCQuestTools.Components.PlatformTargetSettings>();
                comp.buildTarget = KRT.VRCQuestTools.Models.BuildTarget.Auto;

                var result = resolveBuildTargetMethod.Invoke(null, new object[] { go });
                Assert.IsNotNull(result);
                var buildTarget = (int)result;
                Assert.AreNotEqual(0, buildTarget, "Auto should be resolved to PC or Android.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    internal class NdmfSessionStateInitTests
    {
        private Type sessionStateType;

        [SetUp]
        public void SetUp()
        {
            sessionStateType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfSessionState");
            if (sessionStateType == null)
                Assert.Ignore("NdmfSessionState type not found.");
        }

        [Test]
        public void BuildTarget_SetAndGet_Android()
        {
            var prop = sessionStateType.GetProperty("BuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null) Assert.Ignore("BuildTarget property not found.");

            try
            {
                var android = Enum.ToObject(prop.PropertyType, 2);
                prop.SetValue(null, android);
                var result = prop.GetValue(null);
                Assert.AreEqual(android, result);
            }
            finally
            {
                // Restore Auto
                prop.SetValue(null, Enum.ToObject(prop.PropertyType, 0));
            }
        }

        [Test]
        public void BuildTarget_SetAndGet_PC()
        {
            var prop = sessionStateType.GetProperty("BuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null) Assert.Ignore("BuildTarget property not found.");

            try
            {
                var pc = Enum.ToObject(prop.PropertyType, 1);
                prop.SetValue(null, pc);
                var result = prop.GetValue(null);
                Assert.AreEqual(pc, result);
            }
            finally
            {
                prop.SetValue(null, Enum.ToObject(prop.PropertyType, 0));
            }
        }

        [Test]
        public void Init_ResetsBuildTargetToAuto()
        {
            var prop = sessionStateType.GetProperty("BuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null) Assert.Ignore("BuildTarget property not found.");

            var initMethod = sessionStateType.GetMethod("Init",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (initMethod == null) Assert.Ignore("Init method not found.");

            try
            {
                var android = Enum.ToObject(prop.PropertyType, 2);
                prop.SetValue(null, android);
                initMethod.Invoke(null, null);
                var result = prop.GetValue(null);
                var auto = Enum.ToObject(prop.PropertyType, 0);
                Assert.AreEqual(auto, result, "Init should reset BuildTarget to Auto.");
            }
            finally
            {
                prop.SetValue(null, Enum.ToObject(prop.PropertyType, 0));
            }
        }
    }

    [TestFixture]
    internal class NdmfPluginUtilitySetBuildTargetTests
    {
        private Type utilityType;

        [SetUp]
        public void SetUp()
        {
            utilityType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfPluginUtility");
            if (utilityType == null)
                Assert.Ignore("NdmfPluginUtility type not found.");
        }

        [Test]
        public void SetBuildTarget_Android_SetsSessionState()
        {
            var method = utilityType.GetMethod("SetBuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null) Assert.Ignore("SetBuildTarget method not found.");

            var sessionStateType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfSessionState");
            if (sessionStateType == null) Assert.Ignore("NdmfSessionState type not found.");

            var btProp = sessionStateType.GetProperty("BuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (btProp == null) Assert.Ignore("BuildTarget property not found.");

            try
            {
                var androidValue = Enum.ToObject(btProp.PropertyType, 2);
                method.Invoke(null, new object[] { androidValue });
                var result = btProp.GetValue(null);
                Assert.AreEqual(androidValue, result);
            }
            finally
            {
                btProp.SetValue(null, Enum.ToObject(btProp.PropertyType, 0));
            }
        }

        [Test]
        public void SetBuildTarget_PC_SetsSessionState()
        {
            var method = utilityType.GetMethod("SetBuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null) Assert.Ignore("SetBuildTarget method not found.");

            var sessionStateType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfSessionState");
            if (sessionStateType == null) Assert.Ignore("NdmfSessionState type not found.");

            var btProp = sessionStateType.GetProperty("BuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (btProp == null) Assert.Ignore("BuildTarget property not found.");

            try
            {
                var pcValue = Enum.ToObject(btProp.PropertyType, 1);
                method.Invoke(null, new object[] { pcValue });
                var result = btProp.GetValue(null);
                Assert.AreEqual(pcValue, result);
            }
            finally
            {
                btProp.SetValue(null, Enum.ToObject(btProp.PropertyType, 0));
            }
        }
    }
}
