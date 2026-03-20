// <copyright file="UnitySettingsTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="UnitySettings"/>.
    /// </summary>
    public class UnitySettingsTests
    {
        [Test]
        public void HasAndroidBuildSupport_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var result = UnitySettings.HasAndroidBuildSupport;
            });
        }

        [Test]
        public void DefaultAndroidTextureCompression_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var result = UnitySettings.DefaultAndroidTextureCompression;
            });
        }
    }
}
