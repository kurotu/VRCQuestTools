// <copyright file="PlatformTargetSettingsTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="PlatformTargetSettings"/>.
    /// </summary>
    public class PlatformTargetSettingsTests
    {
        [Test]
        public void DefaultBuildTarget_IsAuto()
        {
            var go = new GameObject("Test");
            try
            {
                var settings = go.AddComponent<PlatformTargetSettings>();
                Assert.AreEqual(BuildTarget.Auto, settings.buildTarget);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BuildTarget_CanBeSetToPC()
        {
            var go = new GameObject("Test");
            try
            {
                var settings = go.AddComponent<PlatformTargetSettings>();
                settings.buildTarget = BuildTarget.PC;
                Assert.AreEqual(BuildTarget.PC, settings.buildTarget);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BuildTarget_CanBeSetToAndroid()
        {
            var go = new GameObject("Test");
            try
            {
                var settings = go.AddComponent<PlatformTargetSettings>();
                settings.buildTarget = BuildTarget.Android;
                Assert.AreEqual(BuildTarget.Android, settings.buildTarget);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
