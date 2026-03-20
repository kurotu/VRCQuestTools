// <copyright file="AvatarPerformanceCalculatorTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using VRC.SDKBase.Validation.Performance;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="AvatarPerformanceCalculator"/>.
    /// </summary>
    public class AvatarPerformanceCalculatorTests
    {
        [Test]
        public void GetPerformanceRating_NullStatsLevelSet_Throws()
        {
            var stats = new AvatarDynamics.PerformanceStats
            {
                PhysBonesCount = 0,
                PhysBonesTransformCount = 0,
                PhysBonesColliderCount = 0,
                PhysBonesCollisionCheckCount = 0,
                ContactsCount = 0,
            };
            // With null level set, accessing properties should throw NullReferenceException
            Assert.Throws<System.NullReferenceException>(() =>
            {
                AvatarPerformanceCalculator.GetPerformanceRating(
                    stats,
                    null,
                    AvatarPerformanceCategory.PhysBoneComponentCount);
            });
        }
    }
}
