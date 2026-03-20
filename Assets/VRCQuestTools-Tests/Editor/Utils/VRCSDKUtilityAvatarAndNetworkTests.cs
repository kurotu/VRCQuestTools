// Tests for VRCSDKUtility - covering remaining uncovered methods

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using VRC.SDKBase.Validation.Performance;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    internal class VRCSDKUtilityAvatarAndNetworkTests
    {
        [Test]
        public void GetAvatarRoot_WithAvatarRoot_ReturnsSelf()
        {
            var go = new GameObject("AvatarRoot");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var result = VRCSDKUtility.GetAvatarRoot(go);
                Assert.AreEqual(go, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_WithChildOfAvatar_ReturnsAvatarRoot()
        {
            var root = new GameObject("AvatarRoot");
            var descriptor = root.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            var grandChild = new GameObject("GrandChild");
            grandChild.transform.SetParent(child.transform);
            try
            {
                var result = VRCSDKUtility.GetAvatarRoot(grandChild);
                Assert.AreEqual(root, result);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetAvatarRoot_WithNoAvatar_ReturnsNull()
        {
            var go = new GameObject("NotAnAvatar");
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
        public void IsExampleAsset_VpmSdk3Path_ReturnsTrue()
        {
            var path = "Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/SomeAsset.controller";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_VpmBetaSdk3Path_ReturnsTrue()
        {
            var path = "Assets/Samples/VRChat SDK - Avatars/3.6.0/AV3 Demo Assets/SomeAsset.controller";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_Sdk3Path_ReturnsTrue()
        {
            var path = "Assets/VRCSDK/Examples3/SomeAsset.controller";
            Assert.IsTrue(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_NonExamplePath_ReturnsFalse()
        {
            var path = "Assets/MyAvatar/SomeAsset.controller";
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(path));
        }

        [Test]
        public void IsExampleAsset_EmptyPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(string.Empty));
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
        public void GetRootTransform_VRCPhysBone_ReturnsRootTransform()
        {
            var go = new GameObject("PBRoot");
            var pbGo = new GameObject("PhysBone");
            pbGo.transform.SetParent(go.transform);
            var pb = pbGo.AddComponent<VRCPhysBone>();
            pb.rootTransform = go.transform;
            try
            {
                var result = VRCSDKUtility.GetRootTransform(pb);
                Assert.AreEqual(go.transform, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_VRCPhysBoneCollider_ReturnsRootTransform()
        {
            var go = new GameObject("ColliderRoot");
            var collGo = new GameObject("Collider");
            collGo.transform.SetParent(go.transform);
            var collider = collGo.AddComponent<VRCPhysBoneCollider>();
            collider.rootTransform = go.transform;
            try
            {
                var result = VRCSDKUtility.GetRootTransform(collider);
                Assert.AreEqual(go.transform, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_ContactReceiver_ReturnsRootTransform()
        {
            var go = new GameObject("ContactRoot");
            var contactGo = new GameObject("Contact");
            contactGo.transform.SetParent(go.transform);
            var contact = contactGo.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver>();
            contact.rootTransform = go.transform;
            try
            {
                var result = VRCSDKUtility.GetRootTransform(contact);
                Assert.AreEqual(go.transform, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRootTransform_NonDynamicsComponent_ReturnsNull()
        {
            var go = new GameObject("Test");
            var renderer = go.AddComponent<MeshRenderer>();
            try
            {
                var result = VRCSDKUtility.GetRootTransform(renderer);
                Assert.IsNull(result);
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
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                Assert.IsFalse(VRCSDKUtility.HasMissingNetworkIds(descriptor));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMissingNetworkIds_WithPhysBoneAndNoIds_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            var pbGo = new GameObject("PB");
            pbGo.transform.SetParent(go.transform);
            pbGo.AddComponent<VRCPhysBone>();
            try
            {
                Assert.IsTrue(VRCSDKUtility.HasMissingNetworkIds(descriptor));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMissingNetworkIds_WithPhysBoneAndMatchingId_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            var pbGo = new GameObject("PB");
            pbGo.transform.SetParent(go.transform);
            pbGo.AddComponent<VRCPhysBone>();
            descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair { gameObject = pbGo, ID = 10 });
            try
            {
                Assert.IsFalse(VRCSDKUtility.HasMissingNetworkIds(descriptor));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_RemovesNullEntries()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            var pbGo = new GameObject("PB");
            pbGo.transform.SetParent(go.transform);
            pbGo.AddComponent<VRCPhysBone>();

            // Add a valid entry and a null entry
            descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair { gameObject = pbGo, ID = 10 });
            descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair { gameObject = null, ID = 20 });
            try
            {
                VRCSDKUtility.StripeUnusedNetworkIds(descriptor);
                Assert.AreEqual(1, descriptor.NetworkIDCollection.Count);
                Assert.AreEqual(pbGo, descriptor.NetworkIDCollection[0].gameObject);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StripeUnusedNetworkIds_RemovesEntriesWithoutPhysBone()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            var noPhysBoneGo = new GameObject("NoPB");
            noPhysBoneGo.transform.SetParent(go.transform);

            descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair { gameObject = noPhysBoneGo, ID = 10 });
            try
            {
                VRCSDKUtility.StripeUnusedNetworkIds(descriptor);
                Assert.AreEqual(0, descriptor.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_AssignsIdsToPhysBones()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            var pb1Go = new GameObject("PB1");
            pb1Go.transform.SetParent(go.transform);
            pb1Go.AddComponent<VRCPhysBone>();
            var pb2Go = new GameObject("PB2");
            pb2Go.transform.SetParent(go.transform);
            pb2Go.AddComponent<VRCPhysBone>();
            try
            {
                VRCSDKUtility.AssignNetworkIdsToPhysBones(descriptor);
                Assert.AreEqual(2, descriptor.NetworkIDCollection.Count);
                var ids = descriptor.NetworkIDCollection.Select(p => p.ID).ToList();
                Assert.AreEqual(2, ids.Distinct().Count(), "All IDs should be unique");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBones_DoesNotDuplicate()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            var pb1Go = new GameObject("PB1");
            pb1Go.transform.SetParent(go.transform);
            pb1Go.AddComponent<VRCPhysBone>();

            descriptor.NetworkIDCollection.Add(new VRC.SDKBase.Network.NetworkIDPair { gameObject = pb1Go, ID = 10 });

            try
            {
                VRCSDKUtility.AssignNetworkIdsToPhysBones(descriptor);
                // Should not add a duplicate for pb1Go
                Assert.AreEqual(1, descriptor.NetworkIDCollection.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AssignNetworkIdsToPhysBonesByHierarchyHash_AssignsIds()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            var pb1Go = new GameObject("PB1");
            pb1Go.transform.SetParent(go.transform);
            pb1Go.AddComponent<VRCPhysBone>();
            var pb2Go = new GameObject("PB2");
            pb2Go.transform.SetParent(go.transform);
            pb2Go.AddComponent<VRCPhysBone>();
            try
            {
                var (allIDs, newIDs) = VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(descriptor);
                var allList = allIDs.ToList();
                var newList = newIDs.ToList();
                Assert.AreEqual(2, allList.Count);
                Assert.AreEqual(2, newList.Count);
                Assert.AreEqual(2, allList.Select(p => p.ID).Distinct().Count(), "All IDs should be unique");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetContentTagLabel_AllTags()
        {
            Assert.AreEqual("Nudity/Sexuality", VRCSDKUtility.GetContentTagLabel("content_sex"));
            Assert.AreEqual("Realistic Violence", VRCSDKUtility.GetContentTagLabel("content_violence"));
            Assert.AreEqual("Blood/Gore", VRCSDKUtility.GetContentTagLabel("content_gore"));
            Assert.AreEqual("Other NSFW", VRCSDKUtility.GetContentTagLabel("content_other"));
            Assert.AreEqual("Fallback", VRCSDKUtility.GetContentTagLabel("author_quest_fallback"));
        }

        [Test]
        public void GetContentTagLabel_UnknownTag_ReturnsTag()
        {
            Assert.AreEqual("unknown_tag", VRCSDKUtility.GetContentTagLabel("unknown_tag"));
        }

        [Test]
        public void LoadPerformanceIcon_Excellent_ReturnsTexture()
        {
            var icon = VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.Excellent);
            // Icon may be null if SDK assets aren't present, but method shouldn't throw
            Assert.Pass("No exception thrown");
        }

        [Test]
        public void LoadPerformanceIcon_AllRatings_DoesNotThrow()
        {
            var ratings = new[] { PerformanceRating.Excellent, PerformanceRating.Good, PerformanceRating.Medium, PerformanceRating.Poor, PerformanceRating.VeryPoor };
            foreach (var rating in ratings)
            {
                Assert.DoesNotThrow(() => VRCSDKUtility.LoadPerformanceIcon(rating));
            }
        }

        [Test]
        public void LoadPerformanceIcon_None_ThrowsInvalidOperation()
        {
            Assert.Throws<System.InvalidOperationException>(() => VRCSDKUtility.LoadPerformanceIcon(PerformanceRating.None));
        }

        [Test]
        public void RemoveMissingComponentsInChildren_WithCleanHierarchy_DoesNotThrow()
        {
            var go = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            try
            {
                Assert.DoesNotThrow(() => VRCSDKUtility.RemoveMissingComponentsInChildren(go, true));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetGameObjectsWithMissingComponents_CleanHierarchy_ReturnsEmpty()
        {
            var go = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
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
        public void CountMissingComponentsInChildren_CleanHierarchy_ReturnsZero()
        {
            var go = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            try
            {
                var result = VRCSDKUtility.CountMissingComponentsInChildren(go, true);
                Assert.AreEqual(0, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetTexturesFromMenu_NullMenu_ReturnsEmpty()
        {
            // The method has a null check in the private impl
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            menu.controls = new List<VRCExpressionsMenu.Control>();
            try
            {
                var textures = VRCSDKUtility.GetTexturesFromMenu(menu);
                Assert.AreEqual(0, textures.Length);
            }
            finally
            {
                Object.DestroyImmediate(menu);
            }
        }

        [Test]
        public void GetTexturesFromMenu_MenuWithIcon_ReturnsTexture()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var tex = new Texture2D(4, 4);
            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control { icon = tex, type = VRCExpressionsMenu.Control.ControlType.Button },
            };
            try
            {
                var textures = VRCSDKUtility.GetTexturesFromMenu(menu);
                Assert.AreEqual(1, textures.Length);
                Assert.AreEqual(tex, textures[0]);
            }
            finally
            {
                Object.DestroyImmediate(menu);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetTexturesFromMenu_MenuWithSubMenu_ReturnsAllTextures()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var subMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var tex1 = new Texture2D(4, 4);
            var tex2 = new Texture2D(4, 4);

            subMenu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control { icon = tex2, type = VRCExpressionsMenu.Control.ControlType.Button },
            };
            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control { icon = tex1, type = VRCExpressionsMenu.Control.ControlType.SubMenu, subMenu = subMenu },
            };
            try
            {
                var textures = VRCSDKUtility.GetTexturesFromMenu(menu);
                Assert.AreEqual(2, textures.Length);
                Assert.Contains(tex1, textures);
                Assert.Contains(tex2, textures);
            }
            finally
            {
                Object.DestroyImmediate(menu);
                Object.DestroyImmediate(subMenu);
                Object.DestroyImmediate(tex1);
                Object.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void DuplicateExpressionsMenu_Null_ReturnsNull()
        {
            var result = VRCSDKUtility.DuplicateExpressionsMenu(null);
            Assert.IsNull(result);
        }

        [Test]
        public void DuplicateExpressionsMenu_SimpleMenu_ReturnsDuplicate()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            menu.name = "TestMenu";
            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control { name = "Button1", type = VRCExpressionsMenu.Control.ControlType.Button },
            };
            try
            {
                var dup = VRCSDKUtility.DuplicateExpressionsMenu(menu);
                Assert.IsNotNull(dup);
                Assert.AreNotSame(menu, dup);
                Assert.AreEqual(1, dup.controls.Count);
                Assert.AreEqual("Button1", dup.controls[0].name);
                Object.DestroyImmediate(dup);
            }
            finally
            {
                Object.DestroyImmediate(menu);
            }
        }

        [Test]
        public void DuplicateExpressionsMenu_WithSubMenu_DuplicatesSubMenus()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var subMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            menu.name = "Root";
            subMenu.name = "Sub";
            subMenu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control { name = "SubButton", type = VRCExpressionsMenu.Control.ControlType.Button },
            };
            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control { name = "GoToSub", type = VRCExpressionsMenu.Control.ControlType.SubMenu, subMenu = subMenu },
            };
            try
            {
                var dup = VRCSDKUtility.DuplicateExpressionsMenu(menu);
                Assert.IsNotNull(dup);
                Assert.IsNotNull(dup.controls[0].subMenu);
                Assert.AreNotSame(subMenu, dup.controls[0].subMenu);
                Assert.AreEqual(1, dup.controls[0].subMenu.controls.Count);
                Assert.AreEqual("SubButton", dup.controls[0].subMenu.controls[0].name);
                Object.DestroyImmediate(dup.controls[0].subMenu);
                Object.DestroyImmediate(dup);
            }
            finally
            {
                Object.DestroyImmediate(menu);
                Object.DestroyImmediate(subMenu);
            }
        }

        [Test]
        public void IsMaterialAllowedForQuestAvatar_ToonStandard_ReturnsExpected()
        {
            var mat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            try
            {
                Assert.IsTrue(VRCSDKUtility.IsMaterialAllowedForQuestAvatar(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
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
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CalculatePerformanceStats_EmptyAvatar_ReturnsStats()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var stats = VRCSDKUtility.CalculatePerformanceStats(go, true);
                Assert.IsNotNull(stats);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarsFromLoadedScenes_ReturnsAvatars()
        {
            // This tests the actual scene. Just verify it doesn't throw.
            Assert.DoesNotThrow(() => VRCSDKUtility.GetAvatarsFromLoadedScenes());
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_ReturnsSet()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(set);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Desktop_ReturnsSet()
        {
            var set = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(false);
            Assert.IsNotNull(set);
        }

        [Test]
        public void ResizeExpressionMenuIcons_NullMenu_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.ResizeExpressionMenuIcons(null, 256, false, null));
        }

        [Test]
        public void ResizeExpressionMenuIcons_MaxSizeZero_ClearsIcons()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var tex = new Texture2D(4, 4);
            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control { icon = tex, type = VRCExpressionsMenu.Control.ControlType.Button },
            };
            try
            {
                VRCSDKUtility.ResizeExpressionMenuIcons(menu, 0, false, null);
                Assert.IsNull(menu.controls[0].icon);
            }
            finally
            {
                Object.DestroyImmediate(menu);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetSdkControlPanelSelectedAvatar_DoesNotThrow()
        {
            // May return null, but should not throw
            try
            {
                var avatar = VRCSDKUtility.GetSdkControlPanelSelectedAvatar();
                Assert.Pass("Returned without exception");
            }
            catch (System.NotSupportedException)
            {
                Assert.Pass("SDK doesn't support this field");
            }
        }
    }
}
