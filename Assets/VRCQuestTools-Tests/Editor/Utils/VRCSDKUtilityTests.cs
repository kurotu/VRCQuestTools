// <copyright file="VRCSDKUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

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
                Assert.IsTrue(AssetUtility.IsUncompressedFormat(newTex.format));
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
    }
}
