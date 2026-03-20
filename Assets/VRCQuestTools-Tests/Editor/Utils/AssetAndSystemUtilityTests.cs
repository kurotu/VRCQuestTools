// Tests for AssetUtility, ComponentRemover, SystemUtility, MSMapGenViewModel, VirtualLensUtility - Batch 12

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    internal class AssetUtilityAdditionalTests
    {
        [Test]
        public void IsDynamicBoneImported_ReturnsConsistentWithType()
        {
            var result = AssetUtility.IsDynamicBoneImported();
            Assert.AreEqual(AssetUtility.DynamicBoneType != null, result);
        }

        [Test]
        public void IsLilToonImported_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => AssetUtility.IsLilToonImported());
        }

        [Test]
        public void CanLilToonBakeShadowRamp_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => AssetUtility.CanLilToonBakeShadowRamp());
        }

        [Test]
        public void GetLilToon2Ramp_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => AssetUtility.GetLilToon2Ramp());
        }

        [Test]
        public void LilToonVersion_IsNotNull()
        {
            Assert.IsNotNull(AssetUtility.LilToonVersion);
        }

        [Test]
        public void GetAllObjectReferences_Material_ReturnsReferences()
        {
            var mat = new Material(Shader.Find("Standard"));
            var tex = new Texture2D(4, 4);
            mat.mainTexture = tex;
            try
            {
                var refs = AssetUtility.GetAllObjectReferences(mat);
                Assert.IsNotNull(refs);
                // Should find at least the texture
                Assert.IsTrue(refs.Length > 0);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetAllObjectReferences_EmptyGameObject_ReturnsReferences()
        {
            var go = new GameObject("Test");
            try
            {
                var refs = AssetUtility.GetAllObjectReferences(go);
                Assert.IsNotNull(refs);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void LoadAssetByGUID_InvalidGUID_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("Failed to get asset path by GUID"));
            var result = AssetUtility.LoadAssetByGUID<Material>("00000000000000000000000000000000");
            Assert.IsNull(result);
        }

        [Test]
        public void LoadAssetByGUID_EmptyGUID_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("Failed to get asset path by GUID"));
            var result = AssetUtility.LoadAssetByGUID<Material>("");
            Assert.IsNull(result);
        }
    }

    [TestFixture]
    internal class ComponentRemoverAdditionalTests
    {
        [Test]
        public void RemoveUnsupportedComponentsInChildren_WithAllowedComponents_SkipsAllowed()
        {
            var go = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            var audioSource = child.AddComponent<AudioSource>();
            var light = child.AddComponent<Light>();

            var remover = new ComponentRemover();
            try
            {
                // Allow AudioSource, should only remove Light
                remover.RemoveUnsupportedComponentsInChildren(go, true, false, new System.Type[] { typeof(AudioSource) });
                Assert.IsNotNull(child.GetComponent<AudioSource>(), "AudioSource should be kept");
                Assert.IsNull(child.GetComponent<Light>(), "Light should be removed");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveUnsupportedComponentsInChildren_NoAllowed_RemovesAll()
        {
            var go = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<AudioSource>();
            child.AddComponent<Light>();

            var remover = new ComponentRemover();
            try
            {
                remover.RemoveUnsupportedComponentsInChildren(go, true, false);
                Assert.IsNull(child.GetComponent<AudioSource>(), "AudioSource should be removed");
                Assert.IsNull(child.GetComponent<Light>(), "Light should be removed");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetUnsupportedComponentsInChildren_WithUnsupported_ReturnsComponents()
        {
            var go = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<AudioSource>();
            child.AddComponent<Light>();

            var remover = new ComponentRemover();
            try
            {
                var components = remover.GetUnsupportedComponentsInChildren(go, true);
                Assert.IsTrue(components.Length >= 2);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetUnsupportedComponentsInChildren_CleanHierarchy_ReturnsEmpty()
        {
            var go = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);

            var remover = new ComponentRemover();
            try
            {
                var components = remover.GetUnsupportedComponentsInChildren(go, true);
                Assert.AreEqual(0, components.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsUnsupportedComponent_Cloth_ReturnsTrue()
        {
            var go = new GameObject("Test");
            go.AddComponent<SkinnedMeshRenderer>(); // Cloth needs renderer
            var cloth = go.AddComponent<Cloth>();
            var remover = new ComponentRemover();
            try
            {
                Assert.IsTrue(remover.IsUnsupportedComponent(cloth));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsUnsupportedComponent_Transform_ReturnsFalse()
        {
            var go = new GameObject("Test");
            var remover = new ComponentRemover();
            try
            {
                Assert.IsFalse(remover.IsUnsupportedComponent(go.transform));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    internal class SystemUtilityAdditionalTests
    {
        [Test]
        public void OpenFolder_NonExistentPath_ThrowsDirectoryNotFound()
        {
            Assert.Throws<System.IO.DirectoryNotFoundException>(() =>
            {
                SystemUtility.OpenFolder(@"C:\nonexistent_path_that_does_not_exist_12345");
            });
        }

        [Test]
        public void GetAppLocalCachePath_ReturnsNonEmpty()
        {
            var path = SystemUtility.GetAppLocalCachePath("TestApp");
            Assert.IsNotNull(path);
            Assert.IsTrue(path.Length > 0);
            Assert.IsTrue(path.Contains("TestApp"));
        }

        [Test]
        public void GetTypeByName_ExistingType_ReturnsType()
        {
            var type = SystemUtility.GetTypeByName("UnityEngine.GameObject");
            Assert.IsNotNull(type);
            Assert.AreEqual(typeof(GameObject), type);
        }

        [Test]
        public void GetTypeByName_NonExistingType_ReturnsNull()
        {
            var type = SystemUtility.GetTypeByName("NonExistent.Type.That.Does.Not.Exist");
            Assert.IsNull(type);
        }
    }

    [TestFixture]
    internal class MSMapGenViewModelAdditionalTests
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
        public void DisableGenerateButton_WithMetallic_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            var vm = new MSMapGenViewModel();
            vm.metallicMap = tex;
            vm.smoothnessMap = null;
            try
            {
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_WithSmoothness_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            var vm = new MSMapGenViewModel();
            vm.metallicMap = null;
            vm.smoothnessMap = tex;
            try
            {
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
            var vm = new MSMapGenViewModel();
            vm.metallicMap = tex1;
            vm.smoothnessMap = tex2;
            try
            {
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex1);
                Object.DestroyImmediate(tex2);
            }
        }
    }

    [TestFixture]
    internal class VirtualLensUtilityAdditionalTests
    {
        [Test]
        public void VirtualLensSettingsType_IsNullWhenNotInstalled()
        {
            // VirtualLens2 is not installed in the test project, so the type should be null
            Assert.IsNull(VirtualLensUtility.VirtualLensSettingsType);
        }

        [Test]
        public void VirtualLensSettingsProxy_ConstructorAcceptsComponent()
        {
            var go = new GameObject("Test");
            var transform = go.transform;
            try
            {
                var proxy = new VirtualLensUtility.VirtualLensSettingsProxy(transform);
                Assert.AreEqual(transform, proxy.Component);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void VirtualLensSettingsProxy_SetRemoteOnlyMode_WhenTypeNull_LogsWarning()
        {
            // Since VirtualLens2 is not installed, VirtualLensSettingsType is null,
            // RemoteOnlyModeField is null, setting the property should log warning
            var go = new GameObject("Test");
            try
            {
                var proxy = new VirtualLensUtility.VirtualLensSettingsProxy(go.transform);
                // Should not throw, just log warning
                Assert.DoesNotThrow(() => proxy.remoteOnlyMode = VirtualLensUtility.RemoteOnlyMode.ForceEnable);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoteOnlyMode_HasExpectedValues()
        {
            Assert.AreEqual(0, (int)VirtualLensUtility.RemoteOnlyMode.ForceDisable);
            Assert.AreEqual(1, (int)VirtualLensUtility.RemoteOnlyMode.ForceEnable);
            Assert.AreEqual(2, (int)VirtualLensUtility.RemoteOnlyMode.MobileOnly);
        }
    }
}
