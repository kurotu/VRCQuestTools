// <copyright file="MockTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Mocks;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for Mock classes.
    /// </summary>
    public class MockTests
    {
        [Test]
        public void Mock_AvatarPerformanceStats_GetPerformanceRating_ReturnsNone()
        {
            var stats = new Mock_AvatarPerformanceStats(true);
            var rating = stats.GetPerformanceRatingForCategory(Mock_AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(Mock_PerformanceRating.None, rating);
        }

        [Test]
        public void Mock_AvatarPerformance_CalculatePerformanceStats_DoesNotThrow()
        {
            var go = new GameObject("MockAvatar");
            try
            {
                var stats = new Mock_AvatarPerformanceStats(false);
                Assert.DoesNotThrow(() =>
                {
                    Mock_AvatarPerformance.CalculatePerformanceStats("test", go, stats, true);
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Mock_AvatarPerformanceStats_AllCategories_ReturnNone()
        {
            var stats = new Mock_AvatarPerformanceStats(true);
            Assert.AreEqual(Mock_PerformanceRating.None, stats.GetPerformanceRatingForCategory(Mock_AvatarPerformanceCategory.PhysBoneColliderCount));
            Assert.AreEqual(Mock_PerformanceRating.None, stats.GetPerformanceRatingForCategory(Mock_AvatarPerformanceCategory.PhysBoneCollisionCheckCount));
            Assert.AreEqual(Mock_PerformanceRating.None, stats.GetPerformanceRatingForCategory(Mock_AvatarPerformanceCategory.ContactCount));
        }
    }
}
