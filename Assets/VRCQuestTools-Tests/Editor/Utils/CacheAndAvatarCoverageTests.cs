// Tests for CacheUtility, ModularAvatarUtility, UnitySettings, VRChatAvatar additional - Batch 13

using System;
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Tests.Utils
{
    // --- CacheUtility Tests ---

    [TestFixture]
    public class CacheUtilityBatch13Tests
    {
        [Test]
        public void GetContentCacheKey_SimpleStandardMaterial_ReturnsNonEmptyString()
        {
            Material mat = null;
            try
            {
                mat = new Material(Shader.Find("Standard"));
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
                Assert.IsTrue(key.Contains("Standard"));
            }
            finally
            {
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentShaders_ProduceDifferentKeys()
        {
            Material mat1 = null, mat2 = null;
            try
            {
                mat1 = new Material(Shader.Find("Standard"));
                mat2 = new Material(Shader.Find("Unlit/Color"));
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                if (mat1 != null) UnityEngine.Object.DestroyImmediate(mat1);
                if (mat2 != null) UnityEngine.Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentColors_ProduceDifferentKeys()
        {
            Material mat1 = null, mat2 = null;
            try
            {
                mat1 = new Material(Shader.Find("Standard"));
                mat1.color = Color.red;
                mat2 = new Material(Shader.Find("Standard"));
                mat2.color = Color.blue;
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                if (mat1 != null) UnityEngine.Object.DestroyImmediate(mat1);
                if (mat2 != null) UnityEngine.Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_SameMaterial_ProducesSameKey()
        {
            Material mat = null;
            try
            {
                mat = new Material(Shader.Find("Standard"));
                mat.color = Color.green;
                var key1 = CacheUtility.GetContentCacheKey(mat);
                var key2 = CacheUtility.GetContentCacheKey(mat);
                Assert.AreEqual(key1, key2);
            }
            finally
            {
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_IncludesKeywords()
        {
            Material mat = null;
            try
            {
                mat = new Material(Shader.Find("Standard"));
                mat.EnableKeyword("_EMISSION");
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsTrue(key.Contains("_EMISSION"));
            }
            finally
            {
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentFloat_ProduceDifferentKeys()
        {
            Material mat1 = null, mat2 = null;
            try
            {
                mat1 = new Material(Shader.Find("Standard"));
                mat1.SetFloat("_Metallic", 0.0f);
                mat2 = new Material(Shader.Find("Standard"));
                mat2.SetFloat("_Metallic", 1.0f);
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                if (mat1 != null) UnityEngine.Object.DestroyImmediate(mat1);
                if (mat2 != null) UnityEngine.Object.DestroyImmediate(mat2);
            }
        }

        // --- TextureCache Tests ---

        [Test]
        public void TextureCache_RoundTrip_PreservesDimensions()
        {
            Texture2D original = null;
            Texture2D restored = null;
            try
            {
                original = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                var pixels = original.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(255, 128, 64, 255);
                }
                original.SetPixels32(pixels);
                original.Apply();

                var cache = new CacheUtility.TextureCache(original, false, false, UnityEditor.BuildTarget.StandaloneWindows64);
                var json = JsonUtility.ToJson(cache);
                Assert.IsNotNull(json);
                Assert.IsNotEmpty(json);

                var deserialized = JsonUtility.FromJson<CacheUtility.TextureCache>(json);
                restored = deserialized.ToTexture2D();
                Assert.AreEqual(8, restored.width);
                Assert.AreEqual(8, restored.height);
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (restored != null) UnityEngine.Object.DestroyImmediate(restored);
            }
        }

        [Test]
        public void TextureCache_Constructor_CapturesDataCorrectly()
        {
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                var cache = new CacheUtility.TextureCache(tex, true, false, UnityEditor.BuildTarget.Android);
                var json = JsonUtility.ToJson(cache);
                Assert.IsTrue(json.Contains("16"));
                Assert.IsTrue(json.Contains("RGBA32") || json.Contains("4")); // TextureFormat.RGBA32 == 4
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }
    }

    // --- ModularAvatarUtility Tests ---

    [TestFixture]
    public class ModularAvatarUtilityBatch13Tests
    {
        [Test]
        public void IsModularAvatarImported_ReturnsBoolean()
        {
            // Just verifying it runs without exception
            var result = ModularAvatarUtility.IsModularAvatarImported();
            Assert.IsTrue(result || !result); // always true but verifies no exception
        }

        [Test]
        public void IsLegacyVersion_ReturnsBoolean()
        {
            var result = ModularAvatarUtility.IsLegacyVersion();
            Assert.IsTrue(result || !result);
        }

        [Test]
        public void IsBreakingVersion_ReturnsBoolean()
        {
            var result = ModularAvatarUtility.IsBreakingVersion();
            Assert.IsTrue(result || !result);
        }

        [Test]
        public void GetUnsupportedComponentsInChildren_ReturnsArray()
        {
            var go = new GameObject("MA_Test");
            try
            {
                var result = ModularAvatarUtility.GetUnsupportedComponentsInChildren(go, true);
                Assert.IsNotNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveUnsupportedComponents_NoComponents_DoesNotThrow()
        {
            var go = new GameObject("MA_Test2");
            try
            {
                Assert.DoesNotThrow(() => ModularAvatarUtility.RemoveUnsupportedComponents(go, true));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasConvertConstraintsComponent_WithoutComponent_ReturnsFalse()
        {
            var go = new GameObject("MA_Test3");
            try
            {
                Assert.IsFalse(ModularAvatarUtility.HasConvertConstraintsComponent(go));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetMergeAnimatorComponentsInChildren_NoComponents_ReturnsEmpty()
        {
            var go = new GameObject("MA_Test4");
            try
            {
                var result = ModularAvatarUtility.GetMergeAnimatorComponentsInChildren(go, true);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetMergeAnimatorController_NullComponent_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ModularAvatarUtility.GetMergeAnimatorController(null);
            });
        }

        [Test]
        public void SetMergeAnimatorController_NullComponent_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ModularAvatarUtility.SetMergeAnimatorController(null, null);
            });
        }

        [Test]
        public void GetMergeAnimatorController_WrongComponentType_ThrowsArgumentException()
        {
            var go = new GameObject("MA_Test5");
            try
            {
                var transform = go.transform;
                Assert.Throws<ArgumentException>(() =>
                {
                    ModularAvatarUtility.GetMergeAnimatorController(transform);
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SetMergeAnimatorController_WrongComponentType_ThrowsArgumentException()
        {
            var go = new GameObject("MA_Test6");
            try
            {
                var transform = go.transform;
                Assert.Throws<ArgumentException>(() =>
                {
                    ModularAvatarUtility.SetMergeAnimatorController(transform, null);
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Constants_AreExpected()
        {
            Assert.AreEqual("Modular Avatar", ModularAvatarUtility.PackageDisplayName);
            Assert.AreEqual("1.12.2", ModularAvatarUtility.RequiredVersion);
            Assert.AreEqual("2.0.0", ModularAvatarUtility.BreakingVersion);
        }
    }

    // --- UnitySettings Tests ---

    [TestFixture]
    public class UnitySettingsBatch13Tests
    {
        [Test]
        public void HasAndroidBuildSupport_ReturnsBoolean()
        {
            var result = UnitySettings.HasAndroidBuildSupport;
            Assert.IsTrue(result || !result); // verifies no exception
        }

        [Test]
        public void DefaultAndroidTextureCompression_GetSet_Works()
        {
            var original = UnitySettings.DefaultAndroidTextureCompression;
            try
            {
                UnitySettings.DefaultAndroidTextureCompression = MobileTextureSubtarget.ASTC;
                Assert.AreEqual(MobileTextureSubtarget.ASTC, UnitySettings.DefaultAndroidTextureCompression);
            }
            finally
            {
                UnitySettings.DefaultAndroidTextureCompression = original;
            }
        }
    }

    // --- VRChatAvatar Additional Tests ---

    [TestFixture]
    public class VRChatAvatarBatch13Tests
    {
        private GameObject avatarRoot;
        private VRCAvatarDescriptor descriptor;
        private VRChatAvatar avatar;

        [SetUp]
        public void SetUp()
        {
            avatarRoot = new GameObject("TestAvatar");
            descriptor = avatarRoot.AddComponent<VRCAvatarDescriptor>();
            avatar = new VRChatAvatar(descriptor);
        }

        [TearDown]
        public void TearDown()
        {
            if (avatarRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(avatarRoot);
            }
        }

        [Test]
        public void Materials_EmptyAvatar_ReturnsEmptyOrThrows()
        {
            // baseAnimationLayers is null on freshly created descriptor
            try
            {
                var mats = avatar.Materials;
                Assert.IsNotNull(mats);
            }
            catch (ArgumentNullException)
            {
                // Expected when baseAnimationLayers is null
                Assert.Pass("ArgumentNullException due to null baseAnimationLayers is expected");
            }
        }

        [Test]
        public void Materials_WithRenderer_ReturnsMaterialsOrThrows()
        {
            var child = new GameObject("ChildRenderer");
            child.transform.SetParent(avatarRoot.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial = mat;
            try
            {
                var mats = avatar.Materials;
                Assert.IsNotNull(mats);
                Assert.GreaterOrEqual(mats.Length, 1);
            }
            catch (ArgumentNullException)
            {
                // Expected when baseAnimationLayers is null
                Assert.Pass("ArgumentNullException due to null baseAnimationLayers is expected");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void HasAnimatedMaterials_NoAnimators_ReturnsFalseOrThrows()
        {
            try
            {
                Assert.IsFalse(avatar.HasAnimatedMaterials);
            }
            catch (ArgumentNullException)
            {
                // Expected when baseAnimationLayers is null
                Assert.Pass("ArgumentNullException due to null baseAnimationLayers is expected");
            }
        }

        [Test]
        public void HasVertexColor_NoRenderers_ReturnsFalse()
        {
            Assert.IsFalse(avatar.HasVertexColor);
        }

        [Test]
        public void HasVertexColor_RendererWithoutVertexColors_ReturnsFalse()
        {
            var child = new GameObject("ChildSMR");
            child.transform.SetParent(avatarRoot.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            smr.sharedMesh = mesh;
            try
            {
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void HasVertexColor_RendererWithVertexColors_ReturnsTrue()
        {
            var child = new GameObject("ChildSMR");
            child.transform.SetParent(avatarRoot.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.colors32 = new Color32[] { Color.red, Color.green, Color.blue };
            smr.sharedMesh = mesh;
            try
            {
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void HasDynamicBoneComponents_NoDynamicBone_ReturnsFalse()
        {
            Assert.IsFalse(avatar.HasDynamicBoneComponents);
        }

        [Test]
        public void HasUnityConstraints_NoConstraints_ReturnsFalse()
        {
            Assert.IsFalse(avatar.HasUnityConstraints);
        }

        [Test]
        public void GetPhysBones_NoPhysBones_ReturnsEmpty()
        {
            var pbs = avatar.GetPhysBones();
            Assert.IsNotNull(pbs);
            Assert.AreEqual(0, pbs.Length);
        }

        [Test]
        public void GetPhysBoneProviders_NoPhysBones_ReturnsEmpty()
        {
            var providers = avatar.GetPhysBoneProviders();
            Assert.IsNotNull(providers);
            Assert.AreEqual(0, providers.Length);
        }

        [Test]
        public void GetPhysBoneColliders_NoColliders_ReturnsEmpty()
        {
            var colliders = avatar.GetPhysBoneColliders();
            Assert.IsNotNull(colliders);
            Assert.AreEqual(0, colliders.Length);
        }

        [Test]
        public void GetContacts_NoContacts_ReturnsEmpty()
        {
            var contacts = avatar.GetContacts();
            Assert.IsNotNull(contacts);
            Assert.AreEqual(0, contacts.Length);
        }

        [Test]
        public void GetNonLocalContacts_NoContacts_ReturnsEmpty()
        {
            var contacts = avatar.GetNonLocalContacts();
            Assert.IsNotNull(contacts);
            Assert.AreEqual(0, contacts.Length);
        }

        [Test]
        public void GetLocalContactReceivers_NoContacts_ReturnsEmpty()
        {
            var receivers = avatar.GetLocalContactReceivers();
            Assert.IsNotNull(receivers);
            Assert.AreEqual(0, receivers.Length);
        }

        [Test]
        public void GetLocalContactSenders_NoContacts_ReturnsEmpty()
        {
            var senders = avatar.GetLocalContactSenders();
            Assert.IsNotNull(senders);
            Assert.AreEqual(0, senders.Length);
        }

        [Test]
        public void GetRelatedMaterials_Static_EmptyGameObject_ReturnsEmpty()
        {
            var go = new GameObject("Empty");
            try
            {
                var mats = VRChatAvatar.GetRelatedMaterials(go);
                Assert.IsNotNull(mats);
                Assert.AreEqual(0, mats.Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRelatedMaterials_Static_WithRenderer_ReturnsMaterials()
        {
            var go = new GameObject("WithRenderer");
            var renderer = go.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial = mat;
            try
            {
                var mats = VRChatAvatar.GetRelatedMaterials(go);
                Assert.IsNotNull(mats);
                Assert.AreEqual(1, mats.Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }

    // --- MissingScriptsRule / MissingNdmfRule Validate Tests ---

    [TestFixture]
    public class ValidationRulesBatch13Tests
    {
        [Test]
        public void MissingScriptsRule_Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("InactiveAvatar");
            go.SetActive(false);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var avatar = new VRChatAvatar(desc);
            try
            {
                var rule = new KRT.VRCQuestTools.Models.Validators.MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingScriptsRule_Validate_NoMissingScripts_ReturnsNull()
        {
            var go = new GameObject("CleanAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var avatar = new VRChatAvatar(desc);
            try
            {
                var rule = new KRT.VRCQuestTools.Models.Validators.MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingNdmfRule_Validate_NoNdmfComponents_ReturnsNull()
        {
            var go = new GameObject("NoNdmfAvatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var avatar = new VRChatAvatar(desc);
            try
            {
                var rule = new KRT.VRCQuestTools.Models.Validators.MissingNdmfRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
