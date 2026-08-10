// <copyright file="MissingNdmfRuleTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Components;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Validators
{
    /// <summary>
    /// Tests for MissingNdmfRule.
    /// </summary>
    public class MissingNdmfRuleTests
    {
        private GameObject avatar;

        /// <summary>
        /// Creates an avatar object for each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            avatar = new GameObject("MissingNdmfRuleTestsAvatar");
        }

        /// <summary>
        /// Destroys the avatar object.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(avatar);
        }

        /// <summary>
        /// Components which don't work without NDMF should be reported.
        /// </summary>
        [Test]
        public void ComponentsRequiringNdmfAreReported()
        {
            var meshFlipper = avatar.AddComponent<MeshFlipper>();

            var components = MissingNdmfRule.GetComponentsRequiringNdmf(avatar);

            Assert.AreEqual(new Component[] { meshFlipper }, components);
        }

        /// <summary>
        /// Components which the manual conversion applies should not be reported, because they work
        /// without NDMF.
        /// </summary>
        [Test]
        public void ManualConversionComponentsAreNotReported()
        {
            var child = new GameObject("Child");
            child.transform.SetParent(avatar.transform);
            child.AddComponent<PlatformComponentRemover>();
            child.AddComponent<PlatformGameObjectRemover>();

            var components = MissingNdmfRule.GetComponentsRequiringNdmf(avatar);

            Assert.IsEmpty(components);
        }

        /// <summary>
        /// Only components which require NDMF should be reported when both kinds exist.
        /// </summary>
        [Test]
        public void OnlyComponentsRequiringNdmfAreReported()
        {
            var meshFlipper = avatar.AddComponent<MeshFlipper>();
            avatar.AddComponent<PlatformComponentRemover>();
            avatar.AddComponent<PlatformGameObjectRemover>();

            var components = MissingNdmfRule.GetComponentsRequiringNdmf(avatar);

            Assert.AreEqual(new Component[] { meshFlipper }, components.ToArray());
        }
    }
}
