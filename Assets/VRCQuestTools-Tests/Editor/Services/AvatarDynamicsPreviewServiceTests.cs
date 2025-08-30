// <copyright file="AvatarDynamicsPreviewServiceTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Services;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Tests
{
    /// <summary>
    /// Tests for AvatarDynamicsPreviewService.
    /// </summary>
    internal class AvatarDynamicsPreviewServiceTests
    {
        /// <summary>
        /// Test that the service can be initialized and cleaned up without errors.
        /// </summary>
        [Test]
        public void InitializeAndCleanupTest()
        {
            // Test basic initialization and cleanup
            Assert.DoesNotThrow(() => AvatarDynamicsPreviewService.Initialize());
            Assert.DoesNotThrow(() => AvatarDynamicsPreviewService.Cleanup());
        }

        /// <summary>
        /// Test that preview component can be set to null without errors.
        /// </summary>
        [Test]
        public void SetPreviewComponentNullTest()
        {
            Assert.DoesNotThrow(() => AvatarDynamicsPreviewService.SetPreviewComponent(null));
        }
    }
}