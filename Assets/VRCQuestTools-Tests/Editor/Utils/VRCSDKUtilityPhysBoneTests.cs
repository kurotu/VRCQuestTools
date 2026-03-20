// Tests for VRCSDKUtility - targeting remaining uncovered methods
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using VRC.SDKBase.Network;
using VRC.SDKBase.Validation.Performance;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class VRCSDKUtilityTests_SDKUtil
    {
        [Test]
        public void GetRootTransform_PhysBone_ReturnsRootTransform()
        {
            var go = new GameObject("PB");
            try
            {
                var pb = go.AddComponent<VRCPhysBone>();
                var rootGo = new GameObject("Root");
                pb.rootTransform = rootGo.transform;
                var result = VRCSDKUtility.GetRootTransform(pb);
                Assert.AreEqual(rootGo.transform, result);
                Object.DestroyImmediate(rootGo);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_PhysBoneCollider_ReturnsRootTransform()
        {
            var go = new GameObject("PBC");
            try
            {
                var pbc = go.AddComponent<VRCPhysBoneCollider>();
                var rootGo = new GameObject("Root");
                pbc.rootTransform = rootGo.transform;
                var result = VRCSDKUtility.GetRootTransform(pbc);
                Assert.AreEqual(rootGo.transform, result);
                Object.DestroyImmediate(rootGo);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_ContactReceiver_ReturnsRootTransform()
        {
            var go = new GameObject("CR");
            try
            {
                var cr = go.AddComponent<VRCContactReceiver>();
                var rootGo = new GameObject("Root");
                cr.rootTransform = rootGo.transform;
                var result = VRCSDKUtility.GetRootTransform(cr);
                Assert.AreEqual(rootGo.transform, result);
                Object.DestroyImmediate(rootGo);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_ContactSender_ReturnsRootTransform()
        {
            var go = new GameObject("CS");
            try
            {
                var cs = go.AddComponent<VRCContactSender>();
                var rootGo = new GameObject("Root");
                cs.rootTransform = rootGo.transform;
                var result = VRCSDKUtility.GetRootTransform(cs);
                Assert.AreEqual(rootGo.transform, result);
                Object.DestroyImmediate(rootGo);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_OtherComponent_ReturnsNull()
        {
            var go = new GameObject("Other");
            try
            {
                var comp = go.AddComponent<BoxCollider>();
                var result = VRCSDKUtility.GetRootTransform(comp);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_RemovesOrphanedIds()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                var pb = pbGo.AddComponent<VRCPhysBone>();

                desc.NetworkIDCollection = new List<NetworkIDPair>
                {
                    new NetworkIDPair { gameObject = pbGo, ID = 1 },
                    new NetworkIDPair { gameObject = null, ID = 2 },
                };

                VRCSDKUtility.StripeUnusedNetworkIds(desc);

                Assert.AreEqual(1, desc.NetworkIDCollection.Count);
                Assert.AreEqual(pbGo, desc.NetworkIDCollection[0].gameObject);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_RemovesGameObjectWithoutPhysBone()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var childGo = new GameObject("Child");
                childGo.transform.SetParent(go.transform);
                // No VRCPhysBone on childGo

                desc.NetworkIDCollection = new List<NetworkIDPair>
                {
                    new NetworkIDPair { gameObject = childGo, ID = 1 },
                };

                VRCSDKUtility.StripeUnusedNetworkIds(desc);

                Assert.AreEqual(0, desc.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_RootObject()
        {
            var go = new GameObject("Root");
            try
            {
                var path = VRCSDKUtility.GetFullPathInHierarchy(go);
                Assert.AreEqual("/Root", path);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_ChildObject()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                var path = VRCSDKUtility.GetFullPathInHierarchy(child);
                Assert.IsTrue(path.Contains("Root"));
                Assert.IsTrue(path.Contains("Child"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_DeepChild()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            var grandchild = new GameObject("GrandChild");
            child.transform.SetParent(root.transform);
            grandchild.transform.SetParent(child.transform);
            try
            {
                var path = VRCSDKUtility.GetFullPathInHierarchy(grandchild);
                Assert.IsTrue(path.Contains("Root"));
                Assert.IsTrue(path.Contains("Child"));
                Assert.IsTrue(path.Contains("GrandChild"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
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
        public void GetContentTagLabel_KnownTags()
        {
            Assert.AreEqual("Nudity/Sexuality", VRCSDKUtility.GetContentTagLabel(VRCSDKUtility.AvatarContentTag.Sex));
            Assert.AreEqual("Realistic Violence", VRCSDKUtility.GetContentTagLabel(VRCSDKUtility.AvatarContentTag.Violence));
            Assert.AreEqual("Blood/Gore", VRCSDKUtility.GetContentTagLabel(VRCSDKUtility.AvatarContentTag.Gore));
            Assert.AreEqual("Other NSFW", VRCSDKUtility.GetContentTagLabel(VRCSDKUtility.AvatarContentTag.Other));
            Assert.AreEqual("Fallback", VRCSDKUtility.GetContentTagLabel(VRCSDKUtility.AvatarContentTag.Fallback));
        }

        [Test]
        public void GetContentTagLabel_UnknownTag_ReturnsSameTag()
        {
            Assert.AreEqual("custom_tag", VRCSDKUtility.GetContentTagLabel("custom_tag"));
        }

        [Test]
        public void IsProxyAnimationClip_NonAssetClip_ReturnsFalse()
        {
            var clip = new AnimationClip();
            clip.name = "test_clip";
            try
            {
                // Non-asset clips have empty path, so not proxy
                Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(clip));
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void IsExampleAsset_NullPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(string.Empty));
        }

        [Test]
        public void IsExampleAsset_RegularPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset("Assets/MyAvatar/material.mat"));
        }

        [Test]
        public void IsExampleAsset_SDK3ExamplePath_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset("Assets/VRCSDK/Examples3/Some/Asset.mat"));
        }

        [Test]
        public void RemoveMissingComponentsInChildren_NoMissing()
        {
            var go = new GameObject("Test");
            try
            {
                VRCSDKUtility.RemoveMissingComponentsInChildren(go, true);
                // Should not throw
                Assert.IsNotNull(go);
            }
            finally
            {
                Object.DestroyImmediate(go);
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
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CountMissingComponentsInChildren_NoMissing_ReturnsZero()
        {
            var go = new GameObject("Test");
            try
            {
                var count = VRCSDKUtility.CountMissingComponentsInChildren(go, true);
                Assert.AreEqual(0, count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetTexturesFromMenu_NullMenu_ReturnsEmpty()
        {
            // Create an empty menu with no icons
            var menu = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            try
            {
                var result = VRCSDKUtility.GetTexturesFromMenu(menu);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(menu);
            }
        }

        [Test]
        public void GetTexturesFromMenu_MenuWithIcon_ReturnsIcon()
        {
            var menu = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            var tex = new Texture2D(4, 4);
            try
            {
                menu.controls.Add(new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control
                {
                    name = "Test",
                    icon = tex,
                    type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Button,
                });
                var result = VRCSDKUtility.GetTexturesFromMenu(menu);
                Assert.Contains(tex, result);
            }
            finally
            {
                Object.DestroyImmediate(menu);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DuplicateExpressionsMenu_Null_ReturnsNull()
        {
            var result = VRCSDKUtility.DuplicateExpressionsMenu(null);
            Assert.IsNull(result);
        }

        [Test]
        public void DuplicateExpressionsMenu_SimpleMenu_ReturnsCopy()
        {
            var menu = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            menu.name = "TestMenu";
            menu.controls.Add(new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control
            {
                name = "Button1",
                type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Button,
            });
            try
            {
                var result = VRCSDKUtility.DuplicateExpressionsMenu(menu);
                Assert.IsNotNull(result);
                Assert.AreNotSame(menu, result);
                Assert.AreEqual(1, result.controls.Count);
                Assert.AreEqual("Button1", result.controls[0].name);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(menu);
            }
        }

        [Test]
        public void DuplicateExpressionsMenu_WithSubMenu_DuplicatesSubMenu()
        {
            var rootMenu = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            rootMenu.name = "Root";
            var subMenu = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            subMenu.name = "Sub";
            rootMenu.controls.Add(new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control
            {
                name = "SubMenuControl",
                type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu,
                subMenu = subMenu,
            });
            try
            {
                var result = VRCSDKUtility.DuplicateExpressionsMenu(rootMenu);
                Assert.IsNotNull(result);
                Assert.AreNotSame(rootMenu, result);
                Assert.IsNotNull(result.controls[0].subMenu);
                Assert.AreNotSame(subMenu, result.controls[0].subMenu);
                Object.DestroyImmediate(result.controls[0].subMenu);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(subMenu);
                Object.DestroyImmediate(rootMenu);
            }
        }

        [Test]
        public void DuplicateExpressionsMenu_CircularReference_HandlesGracefully()
        {
            var menu = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            menu.name = "Circular";
            menu.controls.Add(new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control
            {
                name = "Self",
                type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu,
                subMenu = menu, // circular
            });
            try
            {
                var result = VRCSDKUtility.DuplicateExpressionsMenu(menu);
                Assert.IsNotNull(result);
                Assert.AreNotSame(menu, result);
                // The circular reference should point to the duplicated version
                Assert.AreNotSame(menu, result.controls[0].subMenu);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(menu);
            }
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile()
        {
            var result = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(result);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Desktop()
        {
            var result = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(false);
            Assert.IsNotNull(result);
        }

        [Test]
        public void LoadPerformanceIcon_Excellent()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Excellent);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Good()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Good);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Medium()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Medium);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Poor()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Poor);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_VeryPoor()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.VeryPoor);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_None_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.None);
            });
        }

        [Test]
        public void CalculatePerformanceStats_SimpleAvatar()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                go.AddComponent<Animator>();
                var stats = VRCSDKUtility.CalculatePerformanceStats(go, true);
                Assert.IsNotNull(stats);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_FromChild_ReturnsRoot()
        {
            var root = new GameObject("AvatarRoot");
            root.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            var grandchild = new GameObject("GrandChild");
            grandchild.transform.SetParent(child.transform);
            try
            {
                var result = VRCSDKUtility.GetAvatarRoot(grandchild);
                Assert.AreEqual(root, result);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetAvatarRoot_NoAvatar_ReturnsNull()
        {
            var go = new GameObject("NotAvatar");
            try
            {
                var result = VRCSDKUtility.GetAvatarRoot(go);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_Null_ReturnsNull()
        {
            var result = VRCSDKUtility.GetAvatarRoot(null);
            Assert.IsNull(result);
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_RemovesUnkeptPhysBones()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.NetworkIDCollection = new List<NetworkIDPair>();

                var pb1Go = new GameObject("PB1");
                pb1Go.transform.SetParent(go.transform);
                var pb1 = pb1Go.AddComponent<VRCPhysBone>();

                var pb2Go = new GameObject("PB2");
                pb2Go.transform.SetParent(go.transform);
                var pb2 = pb2Go.AddComponent<VRCPhysBone>();

                var avatar = new VRChatAvatar(desc);

                // Keep only pb1, pb2 should be deleted
                VRCSDKUtility.DeleteAvatarDynamicsComponents(
                    avatar,
                    new VRCPhysBone[] { pb1 },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);

                // pb2 should have been destroyed
                Assert.IsNotNull(pb1);
                Assert.IsTrue(pb2 == null); // Unity null check
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_RemovesColliderReferences()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.NetworkIDCollection = new List<NetworkIDPair>();

                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                var pb = pbGo.AddComponent<VRCPhysBone>();

                var colGo = new GameObject("Col");
                colGo.transform.SetParent(go.transform);
                var col = colGo.AddComponent<VRCPhysBoneCollider>();

                pb.colliders.Add(col);

                var avatar = new VRChatAvatar(desc);

                // Keep pb but remove collider
                VRCSDKUtility.DeleteAvatarDynamicsComponents(
                    avatar,
                    new VRCPhysBone[] { pb },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);

                Assert.IsTrue(col == null); // collider destroyed
                // Reference should be null'ed
                Assert.IsTrue(pb.colliders[0] == null);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_RemovesContacts()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.NetworkIDCollection = new List<NetworkIDPair>();

                var crGo = new GameObject("CR");
                crGo.transform.SetParent(go.transform);
                var cr = crGo.AddComponent<VRCContactReceiver>();

                var avatar = new VRChatAvatar(desc);

                VRCSDKUtility.DeleteAvatarDynamicsComponents(
                    avatar,
                    new VRCPhysBone[0],
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);

                Assert.IsTrue(cr == null); // contact destroyed
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_WithProviders_KeepsSpecified()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.NetworkIDCollection = new List<NetworkIDPair>();

                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                var pb = pbGo.AddComponent<VRCPhysBone>();

                var avatar = new VRChatAvatar(desc);
                var provider = new VRCPhysBoneProvider(pb);

                VRCSDKUtility.DeleteAvatarDynamicsComponents(
                    avatar,
                    new VRCPhysBoneProviderBase[] { provider },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0]);

                Assert.IsNotNull(pb);
                Assert.IsFalse(pb == null);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMissingNetworkIds_NoPhysBones_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.NetworkIDCollection = new List<NetworkIDPair>();
                Assert.IsFalse(VRCSDKUtility.HasMissingNetworkIds(desc));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMissingNetworkIds_AllAssigned_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                pbGo.AddComponent<VRCPhysBone>();

                desc.NetworkIDCollection = new List<NetworkIDPair>
                {
                    new NetworkIDPair { gameObject = pbGo, ID = 1 },
                };

                Assert.IsFalse(VRCSDKUtility.HasMissingNetworkIds(desc));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMissingNetworkIds_MissingId_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                pbGo.AddComponent<VRCPhysBone>();

                desc.NetworkIDCollection = new List<NetworkIDPair>();
                // No ID assigned for the PhysBone

                Assert.IsTrue(VRCSDKUtility.HasMissingNetworkIds(desc));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_AssignsIds()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                pbGo.AddComponent<VRCPhysBone>();

                desc.NetworkIDCollection = new List<NetworkIDPair>();

                VRCSDKUtility.AssignNetworkIdsToPhysBones(desc);

                Assert.IsTrue(desc.NetworkIDCollection.Count > 0);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResizeExpressionMenuIcons_NullMenu_NoException()
        {
            VRCSDKUtility.ResizeExpressionMenuIcons(null, 256, false, (a, b) => { });
            // Should not throw
        }

        [Test]
        public void ResizeExpressionMenuIcons_EmptyMenu_NoException()
        {
            var menu = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            try
            {
                VRCSDKUtility.ResizeExpressionMenuIcons(menu, 256, false, (a, b) => { });
                Assert.IsNotNull(menu);
            }
            finally
            {
                Object.DestroyImmediate(menu);
            }
        }
    }
}
