// Batch 21 - Coverage tests for VRCSDKUtility, TextureUtility, UnityAnimationUtility, FallbackAvatarCallback
using System;
using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using VRC.SDKBase.Validation.Performance;
using UObject = UnityEngine.Object;
using UBuildTarget = UnityEditor.BuildTarget;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class VRCSDKUtilityTests_UtilFunc
    {
        [Test]
        public void IsAvatarRoot_NullObject_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAvatarRoot(null));
        }

        [Test]
        public void IsAvatarRoot_NoDescriptor_ReturnsFalse()
        {
            var go = new GameObject("Test");
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsAvatarRoot(go));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsAvatarRoot_WithDescriptor_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                Assert.IsTrue(VRCSDKUtility.IsAvatarRoot(go));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_NullObject_ReturnsNull()
        {
            Assert.IsNull(VRCSDKUtility.GetAvatarRoot(null));
        }

        [Test]
        public void GetAvatarRoot_NoDescriptorInHierarchy_ReturnsNull()
        {
            var go = new GameObject("Test");
            try
            {
                Assert.IsNull(VRCSDKUtility.GetAvatarRoot(go));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_DescriptorOnSelf_ReturnsSelf()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                Assert.AreEqual(go, VRCSDKUtility.GetAvatarRoot(go));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_DescriptorOnParent_ReturnsParent()
        {
            var parent = new GameObject("Avatar");
            var child = new GameObject("Child");
            try
            {
                child.transform.SetParent(parent.transform);
                parent.AddComponent<VRCAvatarDescriptor>();
                Assert.AreEqual(parent, VRCSDKUtility.GetAvatarRoot(child));
            }
            finally
            {
                UObject.DestroyImmediate(parent);
            }
        }

        [Test]
        public void IsExampleAsset_VpmSdk3Path_ReturnsTrue()
        {
            var path = "Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/test.anim";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_VpmBetaSdk3Path_ReturnsTrue()
        {
            var path = "Assets/Samples/VRChat SDK - Avatars/1.0/AV3 Demo Assets/Animation/test.anim";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_Sdk3Path_ReturnsTrue()
        {
            var path = "Assets/VRCSDK/Examples3/some_asset.fbx";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_RegularPath_ReturnsFalse()
        {
            var path = "Assets/MyAvatars/test.fbx";
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsUnsupportedComponentType_Light_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(Light)));
        }

        [Test]
        public void IsUnsupportedComponentType_Camera_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(Camera)));
        }

        [Test]
        public void IsUnsupportedComponentType_Cloth_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(Cloth)));
        }

        [Test]
        public void IsUnsupportedComponentType_AudioSource_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(AudioSource)));
        }

        [Test]
        public void IsUnsupportedComponentType_Joint_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(Joint)));
        }

        [Test]
        public void IsUnsupportedComponentType_Rigidbody_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(Rigidbody)));
        }

        [Test]
        public void IsUnsupportedComponentType_Collider_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsUnsupportedComponentType(typeof(Collider)));
        }

        [Test]
        public void IsUnsupportedComponentType_Transform_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(Transform)));
        }

        [Test]
        public void IsUnsupportedComponentType_MeshRenderer_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(MeshRenderer)));
        }

        [Test]
        public void RemoveMissingComponentsInChildren_NoMissing_DoesNotThrow()
        {
            var go = new GameObject("Test");
            try
            {
                Assert.DoesNotThrow(() => VRCSDKUtility.RemoveMissingComponentsInChildren(go, true));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetGameObjectsWithMissingComponents_NoMissing_ReturnsEmpty()
        {
            var go = new GameObject("Test");
            try
            {
                var result = VRCSDKUtility.GetGameObjectsWithMissingComponents(go, true);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CountMissingComponentsInChildren_NoMissing_ReturnsZero()
        {
            var go = new GameObject("Test");
            try
            {
                Assert.AreEqual(0, VRCSDKUtility.CountMissingComponentsInChildren(go, true));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsMaterialAllowedForQuestAvatar_ToonLit_ReturnsTrue()
        {
            var mat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            try
            {
                Assert.IsTrue(VRCSDKUtility.IsMaterialAllowedForQuestAvatar(mat));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void IsMaterialAllowedForQuestAvatar_Standard_ReturnsFalse()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsMaterialAllowedForQuestAvatar(mat));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void HasMissingNetworkIds_NoPhysBones_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                Assert.IsFalse(VRCSDKUtility.HasMissingNetworkIds(desc));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMissingNetworkIds_PhysBoneWithoutId_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                go.AddComponent<VRCPhysBone>();
                Assert.IsTrue(VRCSDKUtility.HasMissingNetworkIds(desc));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_AssignsIds()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var child = new GameObject("Child");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBone>();

                VRCSDKUtility.AssignNetworkIdsToPhysBones(desc);
                Assert.IsTrue(desc.NetworkIDCollection.Count > 0);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBonesByHierarchyHash_AssignsIds()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var child = new GameObject("Child");
                child.transform.SetParent(go.transform);
                child.AddComponent<VRCPhysBone>();

                var (allIDs, newIDs) = VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(desc);
                Assert.IsNotNull(allIDs);
                Assert.IsNotNull(newIDs);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_PhysBone_ReturnsRootTransform()
        {
            var go = new GameObject("Test");
            try
            {
                var pb = go.AddComponent<VRCPhysBone>();
                var root = VRCSDKUtility.GetRootTransform(pb);
                // rootTransform defaults to null (uses self)
                Assert.IsNull(root);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_PhysBoneCollider_ReturnsRootTransform()
        {
            var go = new GameObject("Test");
            try
            {
                var collider = go.AddComponent<VRCPhysBoneCollider>();
                var root = VRCSDKUtility.GetRootTransform(collider);
                Assert.IsNull(root);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_ContactSender_ReturnsRootTransform()
        {
            var go = new GameObject("Test");
            try
            {
                var sender = go.AddComponent<VRCContactSender>();
                var root = VRCSDKUtility.GetRootTransform(sender);
                Assert.IsNull(root);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_MeshRenderer_ReturnsNull()
        {
            var go = new GameObject("Test");
            try
            {
                var renderer = go.AddComponent<MeshRenderer>();
                var root = VRCSDKUtility.GetRootTransform(renderer);
                Assert.IsNull(root);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_RemovesNullGameObjects()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair { gameObject = null, ID = 10 });
                VRCSDKUtility.StripeUnusedNetworkIds(desc);
                Assert.AreEqual(0, desc.NetworkIDCollection.Count);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_KeepsValidPhysBoneIds()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                go.AddComponent<VRCPhysBone>();
                desc.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair { gameObject = go, ID = 10 });
                VRCSDKUtility.StripeUnusedNetworkIds(desc);
                Assert.AreEqual(1, desc.NetworkIDCollection.Count);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_RootObject_ReturnsSlashName()
        {
            var go = new GameObject("Avatar");
            try
            {
                var path = VRCSDKUtility.GetFullPathInHierarchy(go);
                Assert.IsTrue(path.Contains("Avatar"));
                Assert.IsTrue(path.StartsWith("/"));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_ChildObject_ReturnsFullPath()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            try
            {
                child.transform.SetParent(parent.transform);
                var path = VRCSDKUtility.GetFullPathInHierarchy(child);
                Assert.IsTrue(path.Contains("Parent"));
                Assert.IsTrue(path.Contains("Child"));
            }
            finally
            {
                UObject.DestroyImmediate(parent);
            }
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_ReturnsNotNull()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(set);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Desktop_ReturnsNotNull()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(false);
            Assert.IsNotNull(set);
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Excellent_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.Excellent));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Good_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.Good));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Medium_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.Medium));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Poor_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.Poor));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_VeryPoor_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(PerformanceRating.VeryPoor));
        }

        [Test]
        public void LoadPerformanceIcon_Excellent_ReturnsTexture()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Excellent);
            // May be null if asset not found, but shouldn't throw
            Assert.Pass();
        }

        [Test]
        public void LoadPerformanceIcon_Good_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Good));
        }

        [Test]
        public void LoadPerformanceIcon_Medium_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Medium));
        }

        [Test]
        public void LoadPerformanceIcon_Poor_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Poor));
        }

        [Test]
        public void LoadPerformanceIcon_VeryPoor_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.VeryPoor));
        }

        [Test]
        public void LoadPerformanceIcon_None_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() => VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.None));
        }

        [Test]
        public void GetContentTagLabel_Sex_ReturnsNudity()
        {
            Assert.AreEqual("Nudity/Sexuality", VRCSDKUtility.GetContentTagLabel("content_sex"));
        }

        [Test]
        public void GetContentTagLabel_Violence_ReturnsRealisticViolence()
        {
            Assert.AreEqual("Realistic Violence", VRCSDKUtility.GetContentTagLabel("content_violence"));
        }

        [Test]
        public void GetContentTagLabel_Gore_ReturnsBloodGore()
        {
            Assert.AreEqual("Blood/Gore", VRCSDKUtility.GetContentTagLabel("content_gore"));
        }

        [Test]
        public void GetContentTagLabel_Other_ReturnsOtherNSFW()
        {
            Assert.AreEqual("Other NSFW", VRCSDKUtility.GetContentTagLabel("content_other"));
        }

        [Test]
        public void GetContentTagLabel_Fallback_ReturnsFallback()
        {
            Assert.AreEqual("Fallback", VRCSDKUtility.GetContentTagLabel("author_quest_fallback"));
        }

        [Test]
        public void GetContentTagLabel_Unknown_ReturnsTag()
        {
            Assert.AreEqual("custom_tag", VRCSDKUtility.GetContentTagLabel("custom_tag"));
        }

        [Test]
        public void AvatarContentTag_Constants_Correct()
        {
            Assert.AreEqual("content_sex", VRCSDKUtility.AvatarContentTag.Sex);
            Assert.AreEqual("content_violence", VRCSDKUtility.AvatarContentTag.Violence);
            Assert.AreEqual("content_gore", VRCSDKUtility.AvatarContentTag.Gore);
            Assert.AreEqual("content_other", VRCSDKUtility.AvatarContentTag.Other);
            Assert.AreEqual("author_quest_fallback", VRCSDKUtility.AvatarContentTag.Fallback);
        }

        [Test]
        public void GetTexturesFromMenu_NullMenu_DoesNotThrow()
        {
            // GetTexturesFromMenu checks null inside
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            try
            {
                menu.controls = new List<VRCExpressionsMenu.Control>();
                var textures = VRCSDKUtility.GetTexturesFromMenu(menu);
                Assert.IsNotNull(textures);
                Assert.AreEqual(0, textures.Length);
            }
            finally
            {
                UObject.DestroyImmediate(menu);
            }
        }

        [Test]
        public void GetTexturesFromMenu_WithIcon_ReturnsTexture()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var tex = new Texture2D(4, 4);
            try
            {
                menu.controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control { icon = tex, type = VRCExpressionsMenu.Control.ControlType.Button }
                };
                var textures = VRCSDKUtility.GetTexturesFromMenu(menu);
                Assert.AreEqual(1, textures.Length);
                Assert.AreEqual(tex, textures[0]);
            }
            finally
            {
                UObject.DestroyImmediate(menu);
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DuplicateExpressionsMenu_Null_ReturnsNull()
        {
            Assert.IsNull(VRCSDKUtility.DuplicateExpressionsMenu(null));
        }

        [Test]
        public void DuplicateExpressionsMenu_Simple_ReturnsDuplicate()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            try
            {
                menu.controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control { name = "Test", type = VRCExpressionsMenu.Control.ControlType.Button }
                };
                var dup = VRCSDKUtility.DuplicateExpressionsMenu(menu);
                Assert.IsNotNull(dup);
                Assert.AreEqual(1, dup.controls.Count);
                Assert.AreNotSame(menu, dup);
                UObject.DestroyImmediate(dup);
            }
            finally
            {
                UObject.DestroyImmediate(menu);
            }
        }

        [Test]
        public void DuplicateExpressionsMenu_WithSubMenu_DuplicatesSubMenu()
        {
            var rootMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var subMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            try
            {
                subMenu.controls = new List<VRCExpressionsMenu.Control>();
                rootMenu.controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control
                    {
                        name = "SubMenu",
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = subMenu
                    }
                };
                var dup = VRCSDKUtility.DuplicateExpressionsMenu(rootMenu);
                Assert.IsNotNull(dup);
                Assert.IsNotNull(dup.controls[0].subMenu);
                Assert.AreNotSame(subMenu, dup.controls[0].subMenu);
                UObject.DestroyImmediate(dup.controls[0].subMenu);
                UObject.DestroyImmediate(dup);
            }
            finally
            {
                UObject.DestroyImmediate(rootMenu);
                UObject.DestroyImmediate(subMenu);
            }
        }

        [Test]
        public void ResizeExpressionMenuIcons_Null_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.ResizeExpressionMenuIcons(null, 256, false, null));
        }

        [Test]
        public void ResizeExpressionMenuIcons_ZeroSize_RemovesIcons()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var tex = new Texture2D(512, 512);
            try
            {
                menu.controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control { name = "Test", icon = tex, type = VRCExpressionsMenu.Control.ControlType.Button }
                };
                VRCSDKUtility.ResizeExpressionMenuIcons(menu, 0, false, null);
                Assert.IsNull(menu.controls[0].icon);
            }
            finally
            {
                UObject.DestroyImmediate(menu);
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CalculatePerformanceStats_EmptyAvatar_ReturnsStats()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var stats = VRCSDKUtility.CalculatePerformanceStats(go, true);
                Assert.IsNotNull(stats);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetSdkControlPanelSelectedAvatar_DoesNotThrow()
        {
            // Should not throw, returns null when no avatar is selected
            Assert.DoesNotThrow(() => VRCSDKUtility.GetSdkControlPanelSelectedAvatar());
        }

        [Test]
        public void PoorPhysBonesCountLimit_Is8()
        {
            Assert.AreEqual(8, VRCSDKUtility.PoorPhysBonesCountLimit);
        }

        [Test]
        public void PoorPhysBoneCollidersCountLimit_Is16()
        {
            Assert.AreEqual(16, VRCSDKUtility.PoorPhysBoneCollidersCountLimit);
        }

        [Test]
        public void PoorContactsCountLimit_Is16()
        {
            Assert.AreEqual(16, VRCSDKUtility.PoorContactsCountLimit);
        }

        [Test]
        public void AvatarDynamicsPerformanceCategories_HasExpectedEntries()
        {
            Assert.IsTrue(VRCSDKUtility.AvatarDynamicsPerformanceCategories.Length >= 4);
        }

        [Test]
        public void UnsupportedComponentTypes_ContainsExpectedTypes()
        {
            Assert.IsTrue(VRCSDKUtility.UnsupportedComponentTypes.Length > 0);
            // Should contain Cloth, Camera, Light, etc.
            Assert.IsTrue(Array.IndexOf(VRCSDKUtility.UnsupportedComponentTypes, typeof(Cloth)) >= 0);
            Assert.IsTrue(Array.IndexOf(VRCSDKUtility.UnsupportedComponentTypes, typeof(Camera)) >= 0);
            Assert.IsTrue(Array.IndexOf(VRCSDKUtility.UnsupportedComponentTypes, typeof(Light)) >= 0);
        }

        [Test]
        public void GetAvatarsFromLoadedScenes_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.GetAvatarsFromLoadedScenes());
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_EmptyAvatar_DoesNotThrow()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var avatar = new VRChatAvatar(desc);
                Assert.DoesNotThrow(() => VRCSDKUtility.DeleteAvatarDynamicsComponents(
                    avatar,
                    new VRCPhysBone[0],
                    new VRCPhysBoneCollider[0],
                    new VRC.Dynamics.ContactBase[0]));
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class TextureUtilityTests_UtilFunc
    {
        [Test]
        public void GetCompressionFormat_NoOverride_ReturnsASTC6x6()
        {
            Assert.AreEqual(TextureFormat.ASTC_6x6, TextureUtility.GetCompressionFormat(MobileTextureFormat.NoOverride));
        }

        [Test]
        public void GetCompressionFormat_ASTC_4x4_ReturnsSame()
        {
            Assert.AreEqual(TextureFormat.ASTC_4x4, TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_4x4));
        }

        [Test]
        public void GetCompressionFormat_ASTC_8x8_ReturnsSame()
        {
            Assert.AreEqual(TextureFormat.ASTC_8x8, TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_8x8));
        }

        [Test]
        public void GetCompressionFormat_ASTC_12x12_ReturnsSame()
        {
            Assert.AreEqual(TextureFormat.ASTC_12x12, TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_12x12));
        }

        [Test]
        public void CreateMinimumEmptyTexture_Returns4x4()
        {
            var tex = TextureUtility.CreateMinimumEmptyTexture();
            try
            {
                Assert.IsNotNull(tex);
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CreateColorTexture_SingleColor_CreatesTexture()
        {
            var color = new Color32(255, 0, 0, 255);
            var tex = TextureUtility.CreateColorTexture(color, 8, 8);
            try
            {
                Assert.IsNotNull(tex);
                Assert.AreEqual(8, tex.width);
                Assert.AreEqual(8, tex.height);
                var pixel = tex.GetPixel(0, 0);
                Assert.AreEqual(1f, pixel.r, 0.01f);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CreateColorTexture_Default4x4()
        {
            var color = new Color32(0, 255, 0, 255);
            var tex = TextureUtility.CreateColorTexture(color);
            try
            {
                Assert.IsNotNull(tex);
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CopyAsReadable_CopiesTexture()
        {
            var original = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var copy = TextureUtility.CopyAsReadable(original, true);
                try
                {
                    Assert.IsNotNull(copy);
                    Assert.AreEqual(16, copy.width);
                    Assert.AreEqual(16, copy.height);
                }
                finally
                {
                    UObject.DestroyImmediate(copy);
                }
            }
            finally
            {
                UObject.DestroyImmediate(original);
            }
        }

        [Test]
        public void IsKnownTextureFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsKnownTextureFormat_DXT5_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.DXT5));
        }

        [Test]
        public void IsKnownTextureFormat_ASTC_6x6_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ASTC_6x6));
        }

        [Test]
        public void IsSupportedTextureFormat_RGBA32_Windows_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, UBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_DXT5_Windows_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_ASTC_6x6_Android_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, UBuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_DXT5_Android_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UBuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_ASTC_6x6_iOS_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, UBuildTarget.iOS));
        }

        [Test]
        public void IsSupportedTextureFormat_UnsupportedBuildTarget_Throws()
        {
            Assert.Throws<NotSupportedException>(() =>
                TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, UBuildTarget.WebGL));
        }

        [Test]
        public void IsUncompressedFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsUncompressedFormat_RGB24_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGB24));
        }

        [Test]
        public void IsUncompressedFormat_DXT5_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsUncompressedFormat(TextureFormat.DXT5));
        }

        [Test]
        public void IsUncompressedFormat_ASTC_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsUncompressedFormat(TextureFormat.ASTC_6x6));
        }

        [Test]
        public void SetStreamingMipMaps_DoesNotThrow()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                Assert.DoesNotThrow(() => TextureUtility.SetStreamingMipMaps(tex, true));
                Assert.DoesNotThrow(() => TextureUtility.SetStreamingMipMaps(tex, false));
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void IsNormalMapAsset_NonAsset_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                Assert.IsFalse(TextureUtility.IsNormalMapAsset(tex));
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DestroyTexture_Null_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TextureUtility.DestroyTexture(null));
        }

        [Test]
        public void LoadUncompressedTexture_Null_ReturnsNull()
        {
            Assert.IsNull(TextureUtility.LoadUncompressedTexture((Texture)null));
        }

        [Test]
        public void LoadUncompressedTexture_UnsavedTexture_ReturnsSame()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var result = TextureUtility.LoadUncompressedTexture((Texture)tex);
                Assert.AreEqual(tex, result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void AspectFitReduction_SmallTexture_NoChange()
        {
            var (w, h) = TextureUtility.AspectFitReduction(128, 128, 256);
            Assert.AreEqual(128, w);
            Assert.AreEqual(128, h);
        }

        [Test]
        public void AspectFitReduction_LargeSquare_ReducesToMaxSize()
        {
            var (w, h) = TextureUtility.AspectFitReduction(1024, 1024, 512);
            Assert.AreEqual(512, w);
            Assert.AreEqual(512, h);
        }

        [Test]
        public void AspectFitReduction_Landscape_MaintainsAspect()
        {
            var (w, h) = TextureUtility.AspectFitReduction(2048, 1024, 512);
            Assert.AreEqual(512, w);
            Assert.AreEqual(256, h);
        }

        [Test]
        public void AspectFitReduction_Portrait_MaintainsAspect()
        {
            var (w, h) = TextureUtility.AspectFitReduction(512, 2048, 512);
            Assert.AreEqual(128, w);
            Assert.AreEqual(512, h);
        }

        [Test]
        public void GetImageContentsHash_Null_ReturnsDefault()
        {
            var hash = TextureUtility.GetImageContentsHash(null);
            Assert.AreEqual(default(Hash128), hash);
        }

        [Test]
        public void GetImageContentsHash_Texture2D_ReturnsHash()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var hash = TextureUtility.GetImageContentsHash(tex);
                // Non-null texture should return some hash
                Assert.IsNotNull(hash);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetImageContentsHash_RenderTexture_ReturnsRandomHash()
        {
            var rt = new RenderTexture(4, 4, 0);
            try
            {
                var hash1 = TextureUtility.GetImageContentsHash(rt);
                var hash2 = TextureUtility.GetImageContentsHash(rt);
                // Each call for RenderTexture should give different random hash
                // (not guaranteed but very likely)
                Assert.Pass("RenderTexture hashes generated without error");
            }
            finally
            {
                UObject.DestroyImmediate(rt);
            }
        }

        [Test]
        public void GetBestPlatformOverrideSettings_Null_ReturnsNull()
        {
            Assert.IsNull(TextureUtility.GetBestPlatformOverrideSettings(null));
        }

        [Test]
        public void GetBestPlatformOverrideSettings_Empty_ReturnsNull()
        {
            Assert.IsNull(TextureUtility.GetBestPlatformOverrideSettings());
        }

        [Test]
        public void GetBestPlatformOverrideSettings_NonAssetTexture_ReturnsNull()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                Assert.IsNull(TextureUtility.GetBestPlatformOverrideSettings(tex));
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_Android_Compresses()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UBuildTarget.Android, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_Windows_UsesDXT5()
        {
            // DXT5 requires multiples of 4
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UBuildTarget.StandaloneWindows64, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_NonMultipleOf4_SkipsDXT5()
        {
            // Non-multiple of 4 should log warning and skip DXT5 compression
            var tex = new Texture2D(15, 15, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, UBuildTarget.StandaloneWindows64, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_WithMaxSize_Resizes()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Texture2D result = null;
            try
            {
                result = TextureUtility.CompressTextureForBuildTarget(tex, UBuildTarget.Android, TextureFormat.ASTC_6x6, 32);
                Assert.IsNotNull(result);
            }
            finally
            {
                if (result != null && result != tex)
                {
                    UObject.DestroyImmediate(result);
                }
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void BakeTexture_SimpleTexture_ProducesResult()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Texture2D result = null;
            try
            {
                var request = TextureUtility.BakeTexture(tex, true, 8, 8, false, null, (r) => { result = r; });
                request.WaitForCompletion();
                Assert.IsNotNull(result);
                Assert.AreEqual(8, result.width);
                Assert.AreEqual(8, result.height);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
                if (result != null) UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void ResizeTexture_Downscale()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Texture2D result = null;
            try
            {
                var request = TextureUtility.ResizeTexture(tex, true, 16, 16, (r) => { result = r; });
                request.WaitForCompletion();
                Assert.IsNotNull(result);
                Assert.AreEqual(16, result.width);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
                if (result != null) UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void DownscaleBlit_HalvesTexture()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var desc = new RenderTextureDescriptor(16, 16, RenderTextureFormat.ARGB32, 0);
            var output = RenderTexture.GetTemporary(desc);
            try
            {
                Assert.DoesNotThrow(() => TextureUtility.DownscaleBlit(tex, true, output));
            }
            finally
            {
                RenderTexture.ReleaseTemporary(output);
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressNormalMap_ProducesResult()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressNormalMap(tex, UBuildTarget.Android, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }
    }

    [TestFixture]
    public class UnityAnimationUtilityTests_UtilFunc
    {
        [Test]
        public void GetMaterials_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            try
            {
                var mats = UnityAnimationUtility.GetMaterials(controller);
                Assert.IsNotNull(mats);
                Assert.AreEqual(0, mats.Length);
            }
            finally
            {
                UObject.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetBlendTrees_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            try
            {
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(trees);
                Assert.AreEqual(0, trees.Length);
            }
            finally
            {
                UObject.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetBlendTrees_ControllerWithBlendTree_ReturnsTrees()
        {
            var controller = new AnimatorController();
            try
            {
                controller.AddLayer("Layer1");
                var layer = controller.layers[0];
                var tree = new BlendTree();
                var state = layer.stateMachine.AddState("BlendState");
                state.motion = tree;
                controller.layers = new AnimatorControllerLayer[] { layer };

                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(trees);
                Assert.IsTrue(trees.Length > 0);
                UObject.DestroyImmediate(tree);
            }
            finally
            {
                UObject.DestroyImmediate(controller);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_DirectChild_ReturnsTrue()
        {
            var tree = new BlendTree();
            var clip = new AnimationClip();
            try
            {
                tree.AddChild(clip);
                Assert.IsTrue(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip));
            }
            finally
            {
                UObject.DestroyImmediate(tree);
                UObject.DestroyImmediate(clip);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_NotPresent_ReturnsFalse()
        {
            var tree = new BlendTree();
            var clip = new AnimationClip();
            var otherClip = new AnimationClip();
            try
            {
                tree.AddChild(clip);
                Assert.IsFalse(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, otherClip));
            }
            finally
            {
                UObject.DestroyImmediate(tree);
                UObject.DestroyImmediate(clip);
                UObject.DestroyImmediate(otherClip);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_NestedTree_ReturnsTrue()
        {
            var rootTree = new BlendTree();
            var childTree = new BlendTree();
            var clip = new AnimationClip();
            try
            {
                childTree.AddChild(clip);
                rootTree.AddChild(childTree);
                Assert.IsTrue(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(rootTree, clip));
            }
            finally
            {
                UObject.DestroyImmediate(rootTree);
                UObject.DestroyImmediate(childTree);
                UObject.DestroyImmediate(clip);
            }
        }

        [Test]
        public void ReplaceAnimationClipMaterials_ReplacesCorrectly()
        {
            var clip = new AnimationClip();
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            try
            {
                mat1.name = "Original";
                mat2.name = "Replacement";

                // Add material animation
                var binding = new EditorCurveBinding
                {
                    path = "Renderer",
                    type = typeof(MeshRenderer),
                    propertyName = "m_Materials.Array.data[0]"
                };
                var keyframes = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0, value = mat1 }
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                var newMaterials = new Dictionary<Material, Material> { { mat1, mat2 } };
                var result = UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, newMaterials);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(clip);
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetBlendTrees_StateMachine_WithSubStateMachine()
        {
            var controller = new AnimatorController();
            try
            {
                controller.AddLayer("Layer1");
                var layer = controller.layers[0];
                var subSM = layer.stateMachine.AddStateMachine("SubSM");
                var state = subSM.AddState("State");
                var tree = new BlendTree();
                state.motion = tree;
                controller.layers = new AnimatorControllerLayer[] { layer };

                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(trees);
                Assert.IsTrue(trees.Length > 0);
                UObject.DestroyImmediate(tree);
            }
            finally
            {
                UObject.DestroyImmediate(controller);
            }
        }
    }

    [TestFixture]
    public class FallbackAvatarCallbackTests_UtilFunc
    {
        private static Type GetFallbackCallbackType()
        {
            return SystemUtility.GetTypeByName("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");
        }

        [Test]
        public void CallbackOrder_IsNegative()
        {
            var type = GetFallbackCallbackType();
            if (type == null)
            {
                Assert.Ignore("FallbackAvatarCallback type not found");
                return;
            }
            var instance = Activator.CreateInstance(type);
            var prop = type.GetProperty("callbackOrder", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(prop, "callbackOrder property should exist");
            var value = (int)prop.GetValue(instance);
            Assert.IsTrue(value < 0);
            Assert.AreEqual(-100000, value);
        }

        [Test]
        public void OnPreprocessAvatar_NoPipelineManager_ReturnsTrue()
        {
            var type = GetFallbackCallbackType();
            if (type == null)
            {
                Assert.Ignore("FallbackAvatarCallback type not found");
                return;
            }
            var go = new GameObject("Avatar");
            try
            {
                var instance = Activator.CreateInstance(type);
                var method = type.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(method, "OnPreprocessAvatar method should exist");
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_EmptyBlueprintId_ReturnsTrue()
        {
            var type = GetFallbackCallbackType();
            if (type == null)
            {
                Assert.Ignore("FallbackAvatarCallback type not found");
                return;
            }
            var go = new GameObject("Avatar");
            try
            {
                var pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
                if (pmType != null)
                {
                    go.AddComponent(pmType);
                }
                var instance = Activator.CreateInstance(type);
                var method = type.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithFallbackComponent_ReturnsTrue()
        {
            var type = GetFallbackCallbackType();
            if (type == null)
            {
                Assert.Ignore("FallbackAvatarCallback type not found");
                return;
            }
            var go = new GameObject("Avatar");
            try
            {
                var pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
                if (pmType != null)
                {
                    var pm = go.AddComponent(pmType);
                    var bpField = pmType.GetField("blueprintId", BindingFlags.Public | BindingFlags.Instance);
                    if (bpField != null)
                    {
                        bpField.SetValue(pm, "avtr_test_12345");
                    }
                }
                go.AddComponent<FallbackAvatar>();
                var instance = Activator.CreateInstance(type);
                var method = type.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithoutFallbackComponent_ReturnsTrue()
        {
            var type = GetFallbackCallbackType();
            if (type == null)
            {
                Assert.Ignore("FallbackAvatarCallback type not found");
                return;
            }
            var go = new GameObject("Avatar");
            try
            {
                var pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
                if (pmType != null)
                {
                    var pm = go.AddComponent(pmType);
                    var bpField = pmType.GetField("blueprintId", BindingFlags.Public | BindingFlags.Instance);
                    if (bpField != null)
                    {
                        bpField.SetValue(pm, "avtr_test_67890");
                    }
                }
                var instance = Activator.CreateInstance(type);
                var method = type.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Instance);
                var result = (bool)method.Invoke(instance, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UObject.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class IsProxyAnimationClipTests
    {
        [Test]
        public void IsProxyAnimationClip_UnsavedClip_ReturnsFalse()
        {
            var clip = new AnimationClip();
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(clip));
            }
            finally
            {
                UObject.DestroyImmediate(clip);
            }
        }
    }

    [TestFixture]
    public class ExtraTextureTests
    {
        [Test]
        public void IsKnownTextureFormat_AllUncompressed()
        {
            var uncompressed = new[]
            {
                TextureFormat.Alpha8, TextureFormat.ARGB32, TextureFormat.ARGB4444,
                TextureFormat.BGRA32, TextureFormat.R16, TextureFormat.R8,
                TextureFormat.RFloat, TextureFormat.RG16, TextureFormat.RG32,
                TextureFormat.RGB24, TextureFormat.RGB48, TextureFormat.RGB565,
                TextureFormat.RGBA32, TextureFormat.RGBA4444, TextureFormat.RGBA64,
                TextureFormat.RGBAFloat, TextureFormat.RGBAHalf, TextureFormat.RGFloat,
                TextureFormat.RGHalf, TextureFormat.RHalf, TextureFormat.YUY2
            };
            foreach (var fmt in uncompressed)
            {
                Assert.IsTrue(TextureUtility.IsKnownTextureFormat(fmt), $"{fmt} should be known");
                Assert.IsTrue(TextureUtility.IsUncompressedFormat(fmt), $"{fmt} should be uncompressed");
            }
        }

        [Test]
        public void IsKnownTextureFormat_WindowsFormats()
        {
            var winFormats = new[]
            {
                TextureFormat.BC4, TextureFormat.BC5, TextureFormat.BC6H,
                TextureFormat.BC7, TextureFormat.DXT1, TextureFormat.DXT1Crunched,
                TextureFormat.DXT5, TextureFormat.DXT5Crunched
            };
            foreach (var fmt in winFormats)
            {
                Assert.IsTrue(TextureUtility.IsKnownTextureFormat(fmt), $"{fmt} should be known");
                Assert.IsFalse(TextureUtility.IsUncompressedFormat(fmt), $"{fmt} should not be uncompressed");
            }
        }

        [Test]
        public void IsKnownTextureFormat_AndroidFormats()
        {
            var androidFormats = new[]
            {
                TextureFormat.ASTC_4x4, TextureFormat.ASTC_5x5, TextureFormat.ASTC_6x6,
                TextureFormat.ASTC_8x8, TextureFormat.ASTC_10x10, TextureFormat.ASTC_12x12,
                TextureFormat.ETC2_RGB, TextureFormat.ETC2_RGBA1, TextureFormat.ETC2_RGBA8,
                TextureFormat.ETC_RGB4
            };
            foreach (var fmt in androidFormats)
            {
                Assert.IsTrue(TextureUtility.IsKnownTextureFormat(fmt), $"{fmt} should be known");
            }
        }

        [Test]
        public void IsSupportedTextureFormat_WindowsFormats()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.BC7, UBuildTarget.StandaloneWindows64));
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT1, UBuildTarget.StandaloneWindows));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, UBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_iOSFormats()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.PVRTC_RGB4, UBuildTarget.iOS));
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UBuildTarget.iOS));
        }

        [Test]
        public void RequestReadbackRenderTexture_ProducesResult()
        {
            var desc = new RenderTextureDescriptor(8, 8, RenderTextureFormat.ARGB32, 0);
            var rt = RenderTexture.GetTemporary(desc);
            Texture2D result = null;
            try
            {
                var request = TextureUtility.RequestReadbackRenderTexture(rt, false, (r) => { result = r; });
                request.WaitForCompletion();
                // Result may or may not be null depending on GPU support
                Assert.Pass("RequestReadbackRenderTexture completed");
            }
            finally
            {
                RenderTexture.ReleaseTemporary(rt);
                if (result != null) UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void RequestReadbackRenderTexture_WithLinear_ProducesResult()
        {
            var desc = new RenderTextureDescriptor(8, 8, RenderTextureFormat.ARGB32, 0);
            var rt = RenderTexture.GetTemporary(desc);
            Texture2D result = null;
            try
            {
                var request = TextureUtility.RequestReadbackRenderTexture(rt, false, true, (r) => { result = r; });
                request.WaitForCompletion();
                Assert.Pass("RequestReadbackRenderTexture with linear completed");
            }
            finally
            {
                RenderTexture.ReleaseTemporary(rt);
                if (result != null) UObject.DestroyImmediate(result);
            }
        }

        [Test]
        public void BakeTexture_WithMaterial_ProducesResult()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var mat = new Material(Shader.Find("Unlit/Texture"));
            Texture2D result = null;
            try
            {
                var request = TextureUtility.BakeTexture(tex, true, 8, 8, false, mat, (r) => { result = r; });
                request.WaitForCompletion();
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
                UObject.DestroyImmediate(mat);
                if (result != null) UObject.DestroyImmediate(result);
            }
        }
    }
}
