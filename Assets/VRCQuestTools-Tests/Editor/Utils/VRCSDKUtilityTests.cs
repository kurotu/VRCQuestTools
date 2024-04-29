// <copyright file="VRCSDKUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using NUnit.Framework;

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
    }
}
