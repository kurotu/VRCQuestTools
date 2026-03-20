// Tests for NdmfPluginUtility.HandleConversionException via reflection.

using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using VQTBuildTarget = KRT.VRCQuestTools.Models.BuildTarget;

namespace KRT.VRCQuestTools.Tests.NDMF
{
    [TestFixture]
    internal class NdmfPluginUtilityHandleExceptionTests
    {
        private Type utilityType;
        private MethodInfo handleMethod;

        [SetUp]
        public void SetUp()
        {
            utilityType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfPluginUtility");
            if (utilityType == null)
            {
                Assert.Ignore("NdmfPluginUtility not found");
                return;
            }
            handleMethod = utilityType.GetMethod("HandleConversionException", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (handleMethod == null)
            {
                Assert.Ignore("HandleConversionException method not found");
            }
        }

        [Test]
        public void HandleConversionException_NonVQTException_Rethrows()
        {
            var exception = new InvalidOperationException("Test non-VQT exception");
            var ex = Assert.Throws<TargetInvocationException>(() =>
            {
                handleMethod.Invoke(null, new object[] { exception });
            });
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void HandleConversionException_ArgumentException_Rethrows()
        {
            var exception = new ArgumentException("Test argument exception");
            var ex = Assert.Throws<TargetInvocationException>(() =>
            {
                handleMethod.Invoke(null, new object[] { exception });
            });
            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
        }

        [Test]
        public void HandleConversionException_NullReferenceException_Rethrows()
        {
            var exception = new NullReferenceException("Test null ref");
            var ex = Assert.Throws<TargetInvocationException>(() =>
            {
                handleMethod.Invoke(null, new object[] { exception });
            });
            Assert.That(ex.InnerException, Is.TypeOf<NullReferenceException>());
        }
    }

    [TestFixture]
    internal class NdmfPluginUtilitySetBuildTargetTests2
    {
        [Test]
        public void SetBuildTarget_CanSetAndroid()
        {
            var utilityType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfPluginUtility");
            if (utilityType == null)
            {
                Assert.Ignore("NdmfPluginUtility not found");
                return;
            }

            var setBuildTarget = utilityType.GetMethod("SetBuildTarget", BindingFlags.Static | BindingFlags.NonPublic);
            if (setBuildTarget == null)
            {
                Assert.Ignore("SetBuildTarget not found");
                return;
            }

            var sessionStateType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfSessionState");
            if (sessionStateType == null)
            {
                Assert.Ignore("NdmfSessionState not found");
                return;
            }

            var buildTargetProp = sessionStateType.GetProperty("BuildTarget", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (buildTargetProp == null)
            {
                Assert.Ignore("BuildTarget property not found");
                return;
            }

            try
            {
                setBuildTarget.Invoke(null, new object[] { VQTBuildTarget.Android });
                var value = (VQTBuildTarget)buildTargetProp.GetValue(null);
                Assert.AreEqual(VQTBuildTarget.Android, value);
            }
            finally
            {
                setBuildTarget.Invoke(null, new object[] { VQTBuildTarget.Auto });
            }
        }
    }

    [TestFixture]
    internal class AvatarConverterPassUtilityHasMaterialOperatorTests
    {
        private Type passUtilityType;
        private MethodInfo hasMaterialOperatorMethod;

        [SetUp]
        public void SetUp()
        {
            passUtilityType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterPassUtility");
            if (passUtilityType == null)
            {
                Assert.Ignore("AvatarConverterPassUtility not found");
                return;
            }
            hasMaterialOperatorMethod = passUtilityType.GetMethod("HasMaterialOperatorComponents",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (hasMaterialOperatorMethod == null)
            {
                Assert.Ignore("HasMaterialOperatorComponents not found");
            }
        }

        [Test]
        public void HasMaterialOperatorComponents_EmptyGameObject_ReturnsFalse()
        {
            var go = new GameObject("EmptyAvatar");
            try
            {
                var result = (bool)hasMaterialOperatorMethod.Invoke(null, new object[] { go });
                Assert.IsFalse(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMaterialOperatorComponents_WithChildButNoOperator_ReturnsFalse()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            child.AddComponent<MeshRenderer>();
            try
            {
                var result = (bool)hasMaterialOperatorMethod.Invoke(null, new object[] { root });
                Assert.IsFalse(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    [TestFixture]
    internal class AvatarConverterPassUtilityResolvePhaseTests
    {
        [Test]
        public void ResolveAvatarConverterNdmfPhase_NoSettings_ReturnsNone()
        {
            var passUtilityType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterPassUtility");
            if (passUtilityType == null)
            {
                Assert.Ignore("AvatarConverterPassUtility not found");
                return;
            }

            var method = passUtilityType.GetMethod("ResolveAvatarConverterNdmfPhase",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                Assert.Ignore("ResolveAvatarConverterNdmfPhase not found");
                return;
            }

            var go = new GameObject("NoSettings");
            try
            {
                var result = method.Invoke(null, new object[] { go });
                Assert.IsNotNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
