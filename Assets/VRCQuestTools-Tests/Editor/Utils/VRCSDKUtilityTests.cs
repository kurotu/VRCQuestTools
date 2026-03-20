// <copyright file="VRCSDKUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using NUnit.Framework;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for VRCSDK.
    /// </summary>
    public class VRCSDKUtilityTests
    {
        /// <summary>
        /// GetSdkControlPanelSelectedAvatar test.
        /// </summary>
        [Test]
        public void GetSdkControlPanelSelectedAvatar()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.GetSdkControlPanelSelectedAvatar());
        }

        /// <summary>
        /// GetTexturesFromMenu test.
        /// </summary>
        [Test]
        public void GetTexturesFromMenu()
        {
            var menu = TestUtils.LoadFixtureAssetAtPath<VRCExpressionsMenu>("Expressions/RecursiveExMenu.asset");
            Texture2D[] textures = { };
            Assert.DoesNotThrow(() =>
            {
                textures = VRCSDKUtility.GetTexturesFromMenu(menu);
            });
            Assert.AreEqual(1, textures.Length);
        }

        /// <summary>
        /// DuplicateMenu test.
        /// </summary>
        [Test]
        public void DuplicateMenu()
        {
            var menu = TestUtils.LoadFixtureAssetAtPath<VRCExpressionsMenu>("Expressions/RecursiveExMenu.asset");
            var newMenu = VRCSDKUtility.DuplicateExpressionsMenu(menu);
            Assert.AreNotEqual(menu, newMenu);
            Assert.AreEqual(menu.controls.Count, newMenu.controls.Count);
            for (int i = 0; i < menu.controls.Count; i++)
            {
                Assert.AreNotEqual(menu.controls[i], newMenu.controls[i]);
                Assert.AreEqual(menu.controls[i].name, newMenu.controls[i].name);

                Assert.NotNull(newMenu.controls[0].subMenu);
                Assert.AreEqual(newMenu, newMenu.controls[0].subMenu);
            }
        }

        /// <summary>
        /// ResizeMenuIcons test.
        /// </summary>
        [Test]
        public void ResizeMenuIcons()
        {
            var menu = TestUtils.LoadFixtureAssetAtPath<VRCExpressionsMenu>("Expressions/RecursiveExMenu.asset");
            var newMenu = VRCSDKUtility.DuplicateExpressionsMenu(menu);
            var newSize = 128;
            var callbackCalled = false;
            VRCSDKUtility.ResizeExpressionMenuIcons(newMenu, newSize, true, (oldTex, newTex) =>
            {
                callbackCalled = true;
                Assert.LessOrEqual(newTex.width, newSize);
                Assert.LessOrEqual(newTex.height, newSize);
                Assert.IsTrue(TextureUtility.IsUncompressedFormat(newTex.format));
            });
            Assert.IsTrue(callbackCalled);
        }

        /// <summary>
        /// RemoveMenuIcons test.
        /// </summary>
        [Test]
        public void RemoveMenuIcons()
        {
            var menu = TestUtils.LoadFixtureAssetAtPath<VRCExpressionsMenu>("Expressions/RecursiveExMenu.asset");
            var newMenu = VRCSDKUtility.DuplicateExpressionsMenu(menu);
            var newSize = 0;
            var callbackCalled = false;
            VRCSDKUtility.ResizeExpressionMenuIcons(newMenu, newSize, true, (oldTex, newTex) =>
            {
                callbackCalled = true;
            });
            Assert.IsFalse(callbackCalled);
        }

        [Test]
        public void IsAvatarRoot_WithDescriptor_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                Assert.IsTrue(VRCSDKUtility.IsAvatarRoot(go));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsAvatarRoot_WithoutDescriptor_ReturnsFalse()
        {
            var go = new GameObject("NotAvatar");
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsAvatarRoot(go));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAvatarRoot_FromChildObject_ReturnsRoot()
        {
            var root = new GameObject("Avatar");
            root.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                Assert.AreEqual(root, VRCSDKUtility.GetAvatarRoot(child));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetAvatarRoot_NoAvatar_ReturnsNull()
        {
            var go = new GameObject("Orphan");
            try
            {
                Assert.IsNull(VRCSDKUtility.GetAvatarRoot(go));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_ReturnsCorrectPath()
        {
            var root = new GameObject("Root");
            var child = new GameObject("Child");
            var grandChild = new GameObject("GrandChild");
            child.transform.SetParent(root.transform);
            grandChild.transform.SetParent(child.transform);
            try
            {
                Assert.AreEqual("/Root//Child//GrandChild", VRCSDKUtility.GetFullPathInHierarchy(grandChild));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_RootOnly()
        {
            var root = new GameObject("Root");
            try
            {
                Assert.AreEqual("/Root", VRCSDKUtility.GetFullPathInHierarchy(root));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CountMissingComponentsInChildren_NoneReturnsZero()
        {
            var go = new GameObject("Clean");
            try
            {
                Assert.AreEqual(0, VRCSDKUtility.CountMissingComponentsInChildren(go, true));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetGameObjectsWithMissingComponents_Clean_ReturnsEmpty()
        {
            var go = new GameObject("Clean");
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
        public void IsUnsupportedComponentType_Transform_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(Transform)));
        }

        [Test]
        public void IsUnsupportedComponentType_VRCAvatarDescriptor_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(VRCAvatarDescriptor)));
        }

        [Test]
        public void RemoveMissingComponentsInChildren_CleanObject_NoError()
        {
            var go = new GameObject("Clean");
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
        public void DeleteAvatarDynamicsComponents_RemovesPhysBones()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Bone");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();
            try
            {
                var avatar = new KRT.VRCQuestTools.Models.VRChat.VRChatAvatar(desc);
                VRCSDKUtility.DeleteAvatarDynamicsComponents(avatar, new VRCPhysBone[0], new VRCPhysBoneCollider[0], new ContactBase[0]);
                var physBones = go.GetComponentsInChildren<VRCPhysBone>(true);
                Assert.AreEqual(0, physBones.Length);
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
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Contact");
            child.transform.SetParent(go.transform);
            child.AddComponent<ContactReceiver>();
            try
            {
                var avatar = new KRT.VRCQuestTools.Models.VRChat.VRChatAvatar(desc);
                VRCSDKUtility.DeleteAvatarDynamicsComponents(avatar, new VRCPhysBone[0], new VRCPhysBoneCollider[0], new ContactBase[0]);
                var contacts = go.GetComponentsInChildren<ContactReceiver>(true);
                Assert.AreEqual(0, contacts.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeleteAvatarDynamicsComponents_RemovesColliders()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Collider");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBoneCollider>();
            try
            {
                var avatar = new KRT.VRCQuestTools.Models.VRChat.VRChatAvatar(desc);
                VRCSDKUtility.DeleteAvatarDynamicsComponents(avatar, new VRCPhysBone[0], new VRCPhysBoneCollider[0], new ContactBase[0]);
                var colliders = go.GetComponentsInChildren<VRCPhysBoneCollider>(true);
                Assert.AreEqual(0, colliders.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Excellent_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsAllowedForFallbackAvatar(VRC.SDKBase.Validation.Performance.PerformanceRating.Excellent));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Good_ReturnsTrue()
        {
            Assert.IsTrue(VRCSDKUtility.IsAllowedForFallbackAvatar(VRC.SDKBase.Validation.Performance.PerformanceRating.Good));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_VeryPoor_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(VRC.SDKBase.Validation.Performance.PerformanceRating.VeryPoor));
        }

        [Test]
        public void IsProxyAnimationClip_Null_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(null));
        }

        [Test]
        public void IsProxyAnimationClip_EmptyClip_ReturnsFalse()
        {
            var clip = new AnimationClip();
            clip.name = "TestClip";
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(clip));
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void IsMaterialAllowedForQuestAvatar_ToonLit_ReturnsTrue()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Lit shader not available");
                return;
            }
            var mat = new Material(shader);
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
        public void IsMaterialAllowedForQuestAvatar_StandardShader_ReturnsFalse()
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
        public void HasMissingNetworkIds_NoPhysBones_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                Assert.IsFalse(VRCSDKUtility.HasMissingNetworkIds(desc));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasMissingNetworkIds_WithPhysBone_NoIds_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Bone");
            child.transform.SetParent(go.transform);
            child.AddComponent<VRCPhysBone>();
            try
            {
                Assert.IsTrue(VRCSDKUtility.HasMissingNetworkIds(desc));
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
        public void GetAvatarsFromLoadedScenes_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => VRCSDKUtility.GetAvatarsFromLoadedScenes());
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Medium_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(VRC.SDKBase.Validation.Performance.PerformanceRating.Medium));
        }

        [Test]
        public void IsAllowedForFallbackAvatar_Poor_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAllowedForFallbackAvatar(VRC.SDKBase.Validation.Performance.PerformanceRating.Poor));
        }
    }
}
