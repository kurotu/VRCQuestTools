// <copyright file="VRCSDKUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using NUnit.Framework;
using KRT.VRCQuestTools.Models;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase.Validation.Performance;

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
                return newTex;
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
                return newTex;
            });
            Assert.IsFalse(callbackCalled);
        }

        /// <summary>
        /// Verifies that when the progress callback returns a different texture instance than it was given (as
        /// AstcencTextureCompressor does: it destroys its input and returns a new instance), that returned instance
        /// -- not the pre-compression texture -- ends up assigned to the control's icon. This is the scenario behind
        /// the astcenc "compress texture" callback used by MenuIconResizerPass.
        /// </summary>
        [Test]
        public void ResizeMenuIcons_CallbackReplacementInstance_BecomesControlIcon()
        {
            var menu = TestUtils.LoadFixtureAssetAtPath<VRCExpressionsMenu>("Expressions/RecursiveExMenu.asset");
            var newMenu = VRCSDKUtility.DuplicateExpressionsMenu(menu);
            var newSize = 128;

            Texture2D replacement = null;
            VRCSDKUtility.ResizeExpressionMenuIcons(newMenu, newSize, true, (oldTex, newTex) =>
            {
                // Simulate a compressor with AstcencTextureCompressor's semantics: it consumes (destroys) its
                // input and hands back a brand-new instance.
                var compressed = TextureUtility.CompressTextureForBuildTarget(newTex, UnityEditor.BuildTarget.Android, TextureFormat.ASTC_6x6);
                replacement = compressed;
                return compressed;
            });

            Assert.IsNotNull(replacement);
            Assert.IsTrue(replacement, "The returned replacement texture must be a valid (non-destroyed) object.");

            var iconControl = FindControlWithIcon(newMenu);
            Assert.IsNotNull(iconControl, "Fixture is expected to contain at least one control with an icon.");
            Assert.AreSame(replacement, iconControl.icon, "control.icon must be the callback's returned instance, not the pre-compression texture.");
            Assert.AreEqual((int)TextureFormat.ASTC_6x6, (int)iconControl.icon.format);
        }

        private static VRCExpressionsMenu.Control FindControlWithIcon(VRCExpressionsMenu menu)
        {
            var visited = new System.Collections.Generic.HashSet<VRCExpressionsMenu>();
            VRCExpressionsMenu.Control Search(VRCExpressionsMenu m)
            {
                if (m == null || !visited.Add(m))
                {
                    return null;
                }
                foreach (var control in m.controls)
                {
                    if (control.icon != null)
                    {
                        return control;
                    }
                    var found = Search(control.subMenu);
                    if (found != null)
                    {
                        return found;
                    }
                }
                return null;
            }
            return Search(menu);
        }

        /// <summary>
        /// GetAvatarDynamicsVeryPoorViolationMessage test.
        /// </summary>
        [Test]
        public void GetAvatarDynamicsVeryPoorViolationMessage()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            Assert.AreEqual(i18n.PhysBonesWillBeRemovedAtRunTime, VRCSDKUtility.GetAvatarDynamicsVeryPoorViolationMessage(AvatarPerformanceCategory.PhysBoneComponentCount, i18n));
            Assert.AreEqual(i18n.PhysBonesTransformsShouldBeReduced, VRCSDKUtility.GetAvatarDynamicsVeryPoorViolationMessage(AvatarPerformanceCategory.PhysBoneTransformCount, i18n));
            Assert.AreEqual(i18n.PhysBoneCollidersWillBeRemovedAtRunTime, VRCSDKUtility.GetAvatarDynamicsVeryPoorViolationMessage(AvatarPerformanceCategory.PhysBoneColliderCount, i18n));
            Assert.AreEqual(i18n.PhysBonesCollisionCheckCountShouldBeReduced, VRCSDKUtility.GetAvatarDynamicsVeryPoorViolationMessage(AvatarPerformanceCategory.PhysBoneCollisionCheckCount, i18n));
            Assert.AreEqual(i18n.ContactsWillBeRemovedAtRunTime, VRCSDKUtility.GetAvatarDynamicsVeryPoorViolationMessage(AvatarPerformanceCategory.ContactCount, i18n));
            Assert.Throws<System.InvalidProgramException>(() => VRCSDKUtility.GetAvatarDynamicsVeryPoorViolationMessage(AvatarPerformanceCategory.Overall, i18n));
        }

        /// <summary>
        /// IsEditorOnlyInHierarchy test.
        /// </summary>
        [Test]
        public void IsEditorOnlyInHierarchy()
        {
            var root = new GameObject("Root");
            var editorOnlyChild = new GameObject("EditorOnlyChild");
            editorOnlyChild.tag = "EditorOnly";
            editorOnlyChild.transform.SetParent(root.transform);
            var grandchild = new GameObject("Grandchild");
            grandchild.transform.SetParent(editorOnlyChild.transform);
            var normalChild = new GameObject("NormalChild");
            normalChild.transform.SetParent(root.transform);

            try
            {
                Assert.IsTrue(VRCSDKUtility.IsEditorOnlyInHierarchy(root, editorOnlyChild), "the object itself is tagged as EditorOnly");
                Assert.IsTrue(VRCSDKUtility.IsEditorOnlyInHierarchy(root, grandchild), "a descendant of an EditorOnly object is EditorOnly");
                Assert.IsFalse(VRCSDKUtility.IsEditorOnlyInHierarchy(root, normalChild), "an untagged object under untagged parents is not EditorOnly");

                root.tag = "EditorOnly";
                Assert.IsFalse(VRCSDKUtility.IsEditorOnlyInHierarchy(root, normalChild), "the walk stops before checking the avatar root's own tag");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
