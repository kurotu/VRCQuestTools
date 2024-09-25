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
    }
}
