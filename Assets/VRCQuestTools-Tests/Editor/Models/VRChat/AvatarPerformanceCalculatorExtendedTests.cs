// <copyright file="AvatarPerformanceCalculatorExtendedTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;

namespace KRT.VRCQuestTools.Tests.Models.VRChat
{
    [TestFixture]
    internal class AvatarPerformanceCalculatorExtendedTests
    {
        private AvatarPerformanceStatsLevelSet mobileStatsLevelSet;
        private AvatarPerformanceStatsLevelSet pcStatsLevelSet;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            mobileStatsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            pcStatsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(false);
        }

        // ---- PhysBoneComponentCount tests ----

        [Test]
        public void GetPerformanceRating_PhysBoneComponentCount_ZeroIsExcellent()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = 0;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.Excellent, rating);
        }

        [Test]
        public void GetPerformanceRating_PhysBoneComponentCount_HighIsVeryPoor()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = 99999;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.VeryPoor, rating);
        }

        // ---- PhysBoneTransformCount tests ----

        [Test]
        public void GetPerformanceRating_PhysBoneTransformCount_ZeroIsExcellent()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesTransformCount = 0;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneTransformCount);
            Assert.AreEqual(PerformanceRating.Excellent, rating);
        }

        [Test]
        public void GetPerformanceRating_PhysBoneTransformCount_HighIsVeryPoor()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesTransformCount = 99999;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneTransformCount);
            Assert.AreEqual(PerformanceRating.VeryPoor, rating);
        }

        // ---- PhysBoneColliderCount tests ----

        [Test]
        public void GetPerformanceRating_PhysBoneColliderCount_ZeroIsExcellent()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesColliderCount = 0;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneColliderCount);
            Assert.AreEqual(PerformanceRating.Excellent, rating);
        }

        [Test]
        public void GetPerformanceRating_PhysBoneColliderCount_HighIsVeryPoor()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesColliderCount = 99999;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneColliderCount);
            Assert.AreEqual(PerformanceRating.VeryPoor, rating);
        }

        // ---- PhysBoneCollisionCheckCount tests ----

        [Test]
        public void GetPerformanceRating_PhysBoneCollisionCheckCount_ZeroIsExcellent()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCollisionCheckCount = 0;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneCollisionCheckCount);
            Assert.AreEqual(PerformanceRating.Excellent, rating);
        }

        [Test]
        public void GetPerformanceRating_PhysBoneCollisionCheckCount_HighIsVeryPoor()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCollisionCheckCount = 99999;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneCollisionCheckCount);
            Assert.AreEqual(PerformanceRating.VeryPoor, rating);
        }

        // ---- ContactCount tests ----

        [Test]
        public void GetPerformanceRating_ContactCount_ZeroIsExcellent()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.ContactsCount = 0;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.ContactCount);
            Assert.AreEqual(PerformanceRating.Excellent, rating);
        }

        [Test]
        public void GetPerformanceRating_ContactCount_HighIsVeryPoor()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.ContactsCount = 99999;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.ContactCount);
            Assert.AreEqual(PerformanceRating.VeryPoor, rating);
        }

        // ---- PC stats tests ----

        [Test]
        public void GetPerformanceRating_PC_PhysBoneComponentCount_ZeroIsExcellent()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = 0;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, pcStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.Excellent, rating);
        }

        [Test]
        public void GetPerformanceRating_PC_PhysBoneComponentCount_HighIsVeryPoor()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = 99999;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, pcStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.VeryPoor, rating);
        }

        // ---- Invalid category test ----

        [Test]
        public void GetPerformanceRating_InvalidCategory_Throws()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            Assert.Throws<System.InvalidProgramException>(() =>
            {
                AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, (AvatarPerformanceCategory)999);
            });
        }

        // ---- Boundary tests: check Good/Medium/Poor boundaries ----

        [Test]
        public void GetPerformanceRating_Mobile_PhysBone_MediumValue()
        {
            // Use a moderate value that should give Medium or Poor
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = mobileStatsLevelSet.medium.physBone.componentCount;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.Medium, rating);
        }

        [Test]
        public void GetPerformanceRating_Mobile_PhysBone_GoodValue()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = mobileStatsLevelSet.good.physBone.componentCount;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.Good, rating);
        }

        [Test]
        public void GetPerformanceRating_Mobile_PhysBone_PoorValue()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = mobileStatsLevelSet.poor.physBone.componentCount;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.Poor, rating);
        }

        [Test]
        public void GetPerformanceRating_Mobile_PhysBone_ExcellentBoundary()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = mobileStatsLevelSet.excellent.physBone.componentCount;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.Excellent, rating);
        }

        [Test]
        public void GetPerformanceRating_Mobile_PhysBone_OneAbovePoor()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCount = mobileStatsLevelSet.poor.physBone.componentCount + 1;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount);
            Assert.AreEqual(PerformanceRating.VeryPoor, rating);
        }

        // ---- Contact boundary tests ----

        [Test]
        public void GetPerformanceRating_Mobile_Contact_GoodBoundary()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.ContactsCount = mobileStatsLevelSet.good.contactCount;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.ContactCount);
            Assert.AreEqual(PerformanceRating.Good, rating);
        }

        [Test]
        public void GetPerformanceRating_Mobile_ColliderCount_PoorBoundary()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesColliderCount = mobileStatsLevelSet.poor.physBone.colliderCount;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneColliderCount);
            Assert.AreEqual(PerformanceRating.Poor, rating);
        }

        [Test]
        public void GetPerformanceRating_Mobile_CollisionCheck_MediumBoundary()
        {
            var stats = new AvatarDynamics.PerformanceStats();
            stats.PhysBonesCollisionCheckCount = mobileStatsLevelSet.medium.physBone.collisionCheckCount;
            var rating = AvatarPerformanceCalculator.GetPerformanceRating(stats, mobileStatsLevelSet, AvatarPerformanceCategory.PhysBoneCollisionCheckCount);
            Assert.AreEqual(PerformanceRating.Medium, rating);
        }
    }
}
