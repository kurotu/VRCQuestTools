// Tests for NDMF Pass classes - DisplayName properties and testable methods.

using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    /// <summary>
    /// Tests for all NDMF pass DisplayName properties.
    /// </summary>
    [TestFixture]
    internal class NdmfPassDisplayNameTests
    {
        private static readonly (string TypeName, string ExpectedDisplayName)[] PassDisplayNames = new[]
        {
            ("KRT.VRCQuestTools.Ndmf.RemoveVertexColorPass", "Remove vertex color"),
            ("KRT.VRCQuestTools.Ndmf.MenuIconResizerPass", "Resize menu icons"),
            ("KRT.VRCQuestTools.Ndmf.PlatformComponentRemoverPass", "Remove platform components"),
            ("KRT.VRCQuestTools.Ndmf.PlatformGameObjectRemoverPass", "Remove platform game objects"),
            ("KRT.VRCQuestTools.Ndmf.MeshFlipperPass", "Flip meshes before polygon reduction"),
            ("KRT.VRCQuestTools.Ndmf.MeshFlipperAfterPolygonReductionPass", "Flip meshes after polygon reduction"),
            ("KRT.VRCQuestTools.Ndmf.BuildTargetConfigurationPass", "Configure build target platform"),
            ("KRT.VRCQuestTools.Ndmf.AvatarConverterTransformingPass", "Convert avatar for Mobile in transforming phase"),
            ("KRT.VRCQuestTools.Ndmf.AvatarConverterOptimizingPass", "Convert avatar for Mobile in optimizing phase"),
            ("KRT.VRCQuestTools.Ndmf.AssignNetworkIDsPass", "Assign network IDs"),
            ("KRT.VRCQuestTools.Ndmf.RemoveVRCQuestToolsComponentsPass", "Remove VRCQuestTools components"),
            ("KRT.VRCQuestTools.Ndmf.RemoveUnsupportedComponentsPass", "Remove unsupported components"),
            ("KRT.VRCQuestTools.Ndmf.CheckTextureFormatPass", "Check texture format"),
            ("KRT.VRCQuestTools.Ndmf.AvatarConverterResolvingPass", "Prepare converter and build target"),
        };

        [Test]
        public void AllPassDisplayNames_AreCorrect([Values(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)] int index)
        {
            var (typeName, expectedDisplayName) = PassDisplayNames[index];
            var type = NdmfTestHelper.GetNdmfType(typeName);
            if (type == null)
            {
                Assert.Ignore($"{typeName} not found");
                return;
            }

            var instance = Activator.CreateInstance(type);
            var displayName = NdmfTestHelper.GetProperty(instance, "DisplayName");
            Assert.AreEqual(expectedDisplayName, displayName, $"DisplayName mismatch for {typeName}");
        }
    }

    /// <summary>
    /// Tests for PlatformComponentRemoverPass.IsDecendant method.
    /// </summary>
    [TestFixture]
    internal class PlatformComponentRemoverPassTests
    {
        private Type passType;
        private MethodInfo isDecendantMethod;

        [SetUp]
        public void SetUp()
        {
            passType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.PlatformComponentRemoverPass");
            if (passType == null)
            {
                Assert.Ignore("PlatformComponentRemoverPass not found");
                return;
            }

            isDecendantMethod = passType.GetMethod("IsDecendant", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isDecendantMethod == null)
            {
                Assert.Ignore("IsDecendant method not found");
            }
        }

        private bool InvokeIsDecendant(object instance, GameObject descendant, GameObject root)
        {
            return (bool)isDecendantMethod.Invoke(instance, new object[] { descendant, root });
        }

        [Test]
        public void IsDecendant_NullDescendant_ReturnsFalse()
        {
            var instance = Activator.CreateInstance(passType);
            var root = new GameObject("Root");
            try
            {
                Assert.IsFalse(InvokeIsDecendant(instance, null, root));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void IsDecendant_NullRoot_ReturnsFalse()
        {
            var instance = Activator.CreateInstance(passType);
            var go = new GameObject("Child");
            try
            {
                Assert.IsFalse(InvokeIsDecendant(instance, go, null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsDecendant_SameObject_ReturnsTrue()
        {
            var instance = Activator.CreateInstance(passType);
            var go = new GameObject("Same");
            try
            {
                Assert.IsTrue(InvokeIsDecendant(instance, go, go));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsDecendant_DirectChild_ReturnsTrue()
        {
            var instance = Activator.CreateInstance(passType);
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                Assert.IsTrue(InvokeIsDecendant(instance, child, root));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void IsDecendant_DeepDescendant_ReturnsTrue()
        {
            var instance = Activator.CreateInstance(passType);
            var root = new GameObject("Root");
            var child1 = new GameObject("Child1");
            child1.transform.SetParent(root.transform);
            var child2 = new GameObject("Child2");
            child2.transform.SetParent(child1.transform);
            var child3 = new GameObject("Child3");
            child3.transform.SetParent(child2.transform);
            try
            {
                Assert.IsTrue(InvokeIsDecendant(instance, child3, root));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void IsDecendant_UnrelatedObject_ReturnsFalse()
        {
            var instance = Activator.CreateInstance(passType);
            var root = new GameObject("Root");
            var unrelated = new GameObject("Unrelated");
            try
            {
                Assert.IsFalse(InvokeIsDecendant(instance, unrelated, root));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(unrelated);
            }
        }

        [Test]
        public void IsDecendant_ParentIsNotDescendant_ReturnsFalse()
        {
            var instance = Activator.CreateInstance(passType);
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                Assert.IsFalse(InvokeIsDecendant(instance, root, child));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void IsDecendant_SiblingNotDescendant_ReturnsFalse()
        {
            var instance = Activator.CreateInstance(passType);
            var root = new GameObject("Root");
            var child1 = new GameObject("Child1");
            child1.transform.SetParent(root.transform);
            var child2 = new GameObject("Child2");
            child2.transform.SetParent(root.transform);
            try
            {
                // child2 is not descendant of child1
                Assert.IsFalse(InvokeIsDecendant(instance, child2, child1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    /// <summary>
    /// Tests for NDMF state and utility types.
    /// </summary>
    [TestFixture]
    internal class NdmfStateTests
    {
        [Test]
        public void NdmfState_CanBeCreated()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfState");
            if (type == null) Assert.Ignore("NdmfState not found");
            var instance = Activator.CreateInstance(type);
            Assert.IsNotNull(instance);
        }

        [Test]
        public void NdmfState_CompressExpressionsMenuIcons_DefaultFalse()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfState");
            if (type == null) Assert.Ignore("NdmfState not found");
            var instance = Activator.CreateInstance(type);
            var field = type.GetField("compressExpressionsMenuIcons", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) Assert.Ignore("compressExpressionsMenuIcons field not found");
            var value = field.GetValue(instance);
            Assert.AreEqual(false, value);
        }

        [Test]
        public void NdmfState_CompressExpressionsMenuIcons_CanBeSet()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfState");
            if (type == null) Assert.Ignore("NdmfState not found");
            var instance = Activator.CreateInstance(type);
            var field = type.GetField("compressExpressionsMenuIcons", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) Assert.Ignore("compressExpressionsMenuIcons field not found");
            field.SetValue(instance, true);
            Assert.AreEqual(true, field.GetValue(instance));
        }
    }

    /// <summary>
    /// Tests for NdmfObjectRegistry.
    /// </summary>
    [TestFixture]
    internal class NdmfObjectRegistryTests
    {
        [Test]
        public void NdmfObjectRegistry_CanBeCreated()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfObjectRegistry");
            if (type == null) Assert.Ignore("NdmfObjectRegistry not found");
            var instance = Activator.CreateInstance(type);
            Assert.IsNotNull(instance);
        }

        [Test]
        public void NdmfObjectRegistry_RegisterReplacedObject_CanRegisterMaterials()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfObjectRegistry");
            if (type == null) Assert.Ignore("NdmfObjectRegistry not found");
            var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("RegisterReplacedObject", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) Assert.Ignore("RegisterReplacedObject not found");

            var original = new Material(Shader.Find("Standard"));
            var replaced = new Material(Shader.Find("Standard"));
            try
            {
                Assert.DoesNotThrow(() => method.Invoke(instance, new object[] { original, replaced }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(original);
                UnityEngine.Object.DestroyImmediate(replaced);
            }
        }
    }

    /// <summary>
    /// Tests for AvatarConverterNdmfPhase enum.
    /// </summary>
    [TestFixture]
    internal class AvatarConverterNdmfPhaseTests
    {
        [Test]
        public void AvatarConverterNdmfPhase_HasExpectedValues()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterNdmfPhase");
            if (type == null) Assert.Ignore("AvatarConverterNdmfPhase not found");
            Assert.IsTrue(type.IsEnum);
            var values = Enum.GetNames(type);
            Assert.Contains("Optimizing", values);
            Assert.Contains("Transforming", values);
        }

        [Test]
        public void AvatarConverterNdmfPhase_AutoValue_Exists()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterNdmfPhase");
            if (type == null) Assert.Ignore("AvatarConverterNdmfPhase not found");
            var values = Enum.GetNames(type);
            Assert.Contains("Auto", values);
        }
    }

    /// <summary>
    /// Tests for MeshFlipperProcessingPhase enum.
    /// </summary>
    [TestFixture]
    internal class MeshFlipperProcessingPhaseTests
    {
        [Test]
        public void MeshFlipperProcessingPhase_HasExpectedValues()
        {
            var type = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperProcessingPhase");
            if (type == null) Assert.Ignore("MeshFlipperProcessingPhase not found");
            Assert.IsTrue(type.IsEnum);
            var values = Enum.GetNames(type);
            Assert.Contains("BeforePolygonReduction", values);
        }
    }
}
