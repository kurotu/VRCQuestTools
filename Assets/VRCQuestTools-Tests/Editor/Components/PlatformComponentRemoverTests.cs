// <copyright file="PlatformComponentRemoverTests.cs" company="kurotu">
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
    /// Tests for <see cref="PlatformComponentRemover"/>.
    /// </summary>
    public class PlatformComponentRemoverTests
    {
        /// <summary>
        /// Test default componentSettings is empty.
        /// </summary>
        [Test]
        public void DefaultComponentSettings_IsEmpty()
        {
            var go = new GameObject("TestPCR");
            try
            {
                var pcr = go.AddComponent<PlatformComponentRemover>();
                Assert.IsNotNull(pcr.componentSettings);
                Assert.AreEqual(0, pcr.componentSettings.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test UpdateComponentSettings adds components.
        /// </summary>
        [Test]
        public void UpdateComponentSettings_AddsComponents()
        {
            var go = new GameObject("TestPCR");
            try
            {
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                var pcr = go.AddComponent<PlatformComponentRemover>();

                pcr.UpdateComponentSettings();

                // Should include MeshFilter and MeshRenderer (not Transform or PlatformComponentRemover)
                Assert.IsTrue(pcr.componentSettings.Length >= 2);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test UpdateComponentSettings removes non-existing components.
        /// </summary>
        [Test]
        public void UpdateComponentSettings_RemovesNonExistingComponents()
        {
            var go = new GameObject("TestPCR");
            try
            {
                go.AddComponent<MeshFilter>();
                var mr = go.AddComponent<MeshRenderer>();
                var pcr = go.AddComponent<PlatformComponentRemover>();

                pcr.UpdateComponentSettings();
                var countBefore = pcr.componentSettings.Length;

                Object.DestroyImmediate(mr);
                pcr.UpdateComponentSettings();

                Assert.Less(pcr.componentSettings.Length, countBefore);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test PlatformComponentRemoverItem default values.
        /// </summary>
        [Test]
        public void PlatformComponentRemoverItem_DefaultValues()
        {
            var item = new PlatformComponentRemoverItem();
            Assert.IsNull(item.component);
            Assert.IsFalse(item.removeOnPC);
            Assert.IsFalse(item.removeOnAndroid);
        }

        /// <summary>
        /// Test UpdateComponentSettings preserves existing settings.
        /// </summary>
        [Test]
        public void UpdateComponentSettings_PreservesExistingSettings()
        {
            var go = new GameObject("TestPCR");
            try
            {
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                var pcr = go.AddComponent<PlatformComponentRemover>();

                pcr.UpdateComponentSettings();

                // Modify settings
                if (pcr.componentSettings.Length > 0)
                {
                    pcr.componentSettings[0].removeOnAndroid = true;
                }

                // Update again
                pcr.UpdateComponentSettings();

                // Verify that the existing setting is preserved
                if (pcr.componentSettings.Length > 0)
                {
                    Assert.IsTrue(pcr.componentSettings[0].removeOnAndroid);
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
