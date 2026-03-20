// Tests for AvatarConverterPassUtility testable static methods.

using System;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Tests.NDMF
{
    [TestFixture]
    internal class AvatarConverterPassUtilityTests
    {
        private static readonly Type passUtilityType;
        private static readonly MethodInfo hasMaterialOperatorMethod;
        private static readonly MethodInfo resolvePhaseMethod;

        static AvatarConverterPassUtilityTests()
        {
            var ndmfAssembly = NdmfTestHelper.NdmfAssembly;
            if (ndmfAssembly != null)
            {
                passUtilityType = ndmfAssembly.GetType("KRT.VRCQuestTools.Ndmf.AvatarConverterPassUtility");
            }
            if (passUtilityType != null)
            {
                hasMaterialOperatorMethod = passUtilityType.GetMethod("HasMaterialOperatorComponents",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                resolvePhaseMethod = passUtilityType.GetMethod("ResolveAvatarConverterNdmfPhase",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            }
        }

        [SetUp]
        public void SetUp()
        {
            if (passUtilityType == null)
            {
                Assert.Ignore("AvatarConverterPassUtility type not found.");
            }
        }

        [Test]
        public void HasMaterialOperatorComponents_NoComponents_ReturnsFalse()
        {
            if (hasMaterialOperatorMethod == null) Assert.Ignore("Method not found.");

            var go = new GameObject("TestAvatar");
            go.AddComponent<VRCAvatarDescriptor>();
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
        public void HasMaterialOperatorComponents_WithMaterialSwap_ReturnsTrue()
        {
            if (hasMaterialOperatorMethod == null) Assert.Ignore("Method not found.");

            var go = new GameObject("TestAvatar");
            go.AddComponent<VRCAvatarDescriptor>();
            go.AddComponent<MaterialSwap>();
            try
            {
                var result = (bool)hasMaterialOperatorMethod.Invoke(null, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMaterialOperatorComponents_WithAvatarConverterSettings_ReturnsTrue()
        {
            if (hasMaterialOperatorMethod == null) Assert.Ignore("Method not found.");

            var go = new GameObject("TestAvatar");
            go.AddComponent<VRCAvatarDescriptor>();
            go.AddComponent<AvatarConverterSettings>();
            try
            {
                var result = (bool)hasMaterialOperatorMethod.Invoke(null, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMaterialOperatorComponents_WithMaterialConversionSettings_ReturnsTrue()
        {
            if (hasMaterialOperatorMethod == null) Assert.Ignore("Method not found.");

            var go = new GameObject("TestAvatar");
            go.AddComponent<VRCAvatarDescriptor>();
            go.AddComponent<MaterialConversionSettings>();
            try
            {
                var result = (bool)hasMaterialOperatorMethod.Invoke(null, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMaterialOperatorComponents_ChildComponent_ReturnsTrue()
        {
            if (hasMaterialOperatorMethod == null) Assert.Ignore("Method not found.");

            var go = new GameObject("TestAvatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Child");
            child.transform.parent = go.transform;
            child.AddComponent<MaterialSwap>();
            try
            {
                var result = (bool)hasMaterialOperatorMethod.Invoke(null, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResolveAvatarConverterNdmfPhase_NoComponents_ReturnsResolved()
        {
            if (resolvePhaseMethod == null) Assert.Ignore("Method not found.");

            var go = new GameObject("TestAvatar");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var result = resolvePhaseMethod.Invoke(null, new object[] { go });
                Assert.IsNotNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResolveAvatarConverterNdmfPhase_WithAvatarConverterSettings_ReturnsPhase()
        {
            if (resolvePhaseMethod == null) Assert.Ignore("Method not found.");

            var go = new GameObject("TestAvatar");
            go.AddComponent<VRCAvatarDescriptor>();
            go.AddComponent<AvatarConverterSettings>();
            try
            {
                var result = resolvePhaseMethod.Invoke(null, new object[] { go });
                Assert.IsNotNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResolveAvatarConverterNdmfPhase_WithMaterialConversionSettings_ReturnsPhase()
        {
            if (resolvePhaseMethod == null) Assert.Ignore("Method not found.");

            var go = new GameObject("TestAvatar");
            go.AddComponent<VRCAvatarDescriptor>();
            go.AddComponent<MaterialConversionSettings>();
            try
            {
                var result = resolvePhaseMethod.Invoke(null, new object[] { go });
                Assert.IsNotNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
