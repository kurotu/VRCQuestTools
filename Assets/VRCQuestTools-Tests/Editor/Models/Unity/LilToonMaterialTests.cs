// <copyright file="LilToonMaterialTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Tests for LilToonMaterial.
    /// </summary>
    public class LilToonMaterialTests
    {
        /// <summary>
        /// Required version for lilToon compatibility.
        /// </summary>
        private static readonly SemVer RequiredVersion = new SemVer(1, 10, 0);

        /// <summary>
        /// Breaking version for lilToon compatibility.
        /// </summary>
        private static readonly SemVer BreakingVersion = new SemVer(3, 0, 0);

        /// <summary>
        /// Material for testing.
        /// </summary>
        private Material testMaterial;

        /// <summary>
        /// Set up tests.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // Create a dummy material for testing
            testMaterial = new Material(Shader.Find("Standard"));
        }

        /// <summary>
        /// Clean up tests.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (testMaterial != null)
            {
                Object.DestroyImmediate(testMaterial);
            }
        }

        /// <summary>
        /// Test that LilToonLegacyException is thrown when lilToon version is legacy.
        /// This test runs when lilToon version is between 0.0.1 and 1.10.0 (exclusive).
        /// </summary>
        [Test]
        public void Constructor_WhenLilToonVersionIsLegacy_ThrowsLilToonLegacyException()
        {
            var lilToonVersion = AssetUtility.LilToonVersion;
            var isInstalled = AssetUtility.IsLilToonImported();

            if (isInstalled && lilToonVersion < RequiredVersion)
            {
                var ex = Assert.Throws<LilToonLegacyException>(() => new LilToonMaterial(testMaterial));
                Assert.IsNotNull(ex);
                Assert.IsInstanceOf<LilToonCompatibilityException>(ex);
                Assert.IsNotNull(ex.LocalizedMessage);
                Assert.IsNotEmpty(ex.LocalizedMessage);
                StringAssert.Contains(RequiredVersion.ToString(), ex.LocalizedMessage);
            }
            else
            {
                Assert.Ignore($"lilToon legacy version not detected. Installed: {isInstalled}, Version: {lilToonVersion}");
            }
        }

        /// <summary>
        /// Test that LilToonBreakingException is thrown when lilToon version is breaking.
        /// This test runs when lilToon version is exactly 3.0.0.
        /// </summary>
        [Test]
        public void Constructor_WhenLilToonVersionIsBreaking_ThrowsLilToonBreakingException()
        {
            var lilToonVersion = AssetUtility.LilToonVersion;
            var isInstalled = AssetUtility.IsLilToonImported();

            if (isInstalled && lilToonVersion >= BreakingVersion)
            {
                var ex = Assert.Throws<LilToonBreakingException>(() => new LilToonMaterial(testMaterial));
                Assert.IsNotNull(ex);
                Assert.IsInstanceOf<LilToonCompatibilityException>(ex);
                Assert.IsNotNull(ex.LocalizedMessage);
                Assert.IsNotEmpty(ex.LocalizedMessage);
                StringAssert.Contains(BreakingVersion.ToString(), ex.LocalizedMessage);
            }
            else
            {
                Assert.Ignore($"lilToon breaking version not detected. Installed: {isInstalled}, Version: {lilToonVersion}");
            }
        }

        /// <summary>
        /// Test that LilToonWrongInstallationException is thrown when lilToon is not installed.
        /// This test runs when lilToon package is not installed.
        /// </summary>
        [Test]
        public void Constructor_WhenLilToonNotInstalled_ThrowsLilToonWrongInstallationException()
        {
            var isInstalled = AssetUtility.IsLilToonImported();

            if (!isInstalled)
            {
                var ex = Assert.Throws<LilToonWrongInstallationException>(() => new LilToonMaterial(testMaterial));
                Assert.IsNotNull(ex);
                Assert.IsInstanceOf<LilToonCompatibilityException>(ex);
                Assert.IsNotNull(ex.LocalizedMessage);
                Assert.IsNotEmpty(ex.LocalizedMessage);
            }
            else
            {
                Assert.Ignore($"lilToon is installed, test is not applicable. Version: {AssetUtility.LilToonVersion}");
            }
        }

        /// <summary>
        /// Test that no exception is thrown when lilToon is properly installed with supported version.
        /// This test runs when lilToon is installed and version is supported.
        /// </summary>
        [Test]
        public void Constructor_WhenLilToonVersionIsSupported_DoesNotThrowException()
        {
            var lilToonVersion = AssetUtility.LilToonVersion;
            var isInstalled = AssetUtility.IsLilToonImported();

            if (isInstalled && lilToonVersion >= RequiredVersion && lilToonVersion < BreakingVersion)
            {
                Assert.DoesNotThrow(() => new LilToonMaterial(testMaterial));
            }
            else
            {
                Assert.Ignore($"lilToon supported version not detected. Installed: {isInstalled}, Version: {lilToonVersion}");
            }
        }

        /// <summary>
        /// Test that all exception types inherit from LilToonCompatibilityException.
        /// </summary>
        [Test]
        public void ExceptionTypes_InheritFromLilToonCompatibilityException()
        {
            Assert.IsTrue(typeof(LilToonLegacyException).IsSubclassOf(typeof(LilToonCompatibilityException)));
            Assert.IsTrue(typeof(LilToonBreakingException).IsSubclassOf(typeof(LilToonCompatibilityException)));
            Assert.IsTrue(typeof(LilToonWrongInstallationException).IsSubclassOf(typeof(LilToonCompatibilityException)));
        }

        /// <summary>
        /// Test exception message content based on current lilToon installation.
        /// </summary>
        [Test]
        public void ExceptionMessages_ContainExpectedVersionInformation()
        {
            var lilToonVersion = AssetUtility.LilToonVersion;
            var isInstalled = AssetUtility.IsLilToonImported();

            if (!isInstalled)
            {
                var ex = Assert.Throws<LilToonWrongInstallationException>(() => new LilToonMaterial(testMaterial));
                Assert.IsNotNull(ex.Message);
                Assert.IsNotNull(ex.LocalizedMessage);
                Assert.IsNotEmpty(ex.Message);
                Assert.IsNotEmpty(ex.LocalizedMessage);
            }
            else if (lilToonVersion < RequiredVersion)
            {
                var ex = Assert.Throws<LilToonLegacyException>(() => new LilToonMaterial(testMaterial));
                StringAssert.Contains(RequiredVersion.ToString(), ex.Message);
                StringAssert.Contains(RequiredVersion.ToString(), ex.LocalizedMessage);
            }
            else if (lilToonVersion >= BreakingVersion)
            {
                var ex = Assert.Throws<LilToonBreakingException>(() => new LilToonMaterial(testMaterial));
                StringAssert.Contains(BreakingVersion.ToString(), ex.Message);
                StringAssert.Contains(BreakingVersion.ToString(), ex.LocalizedMessage);
            }
            else
            {
                Assert.Pass($"lilToon is properly installed and supported (Version: {lilToonVersion}), no exception expected");
            }
        }

        /// <summary>
        /// Test to verify current lilToon installation status.
        /// This helps with debugging test environments.
        /// </summary>
        [Test]
        public void LilToonInstallation_VerifyCurrentEnvironment()
        {
            var lilToonVersion = AssetUtility.LilToonVersion;
            var isInstalled = AssetUtility.IsLilToonImported();
            var canBakeShadowRamp = AssetUtility.CanLilToonBakeShadowRamp();

            string environmentInfo = $"Current lilToon environment: " +
                $"Installed={isInstalled}, " +
                $"Version={lilToonVersion}, " +
                $"CanBakeShadowRamp={canBakeShadowRamp}";

            TestContext.WriteLine(environmentInfo);

            // Basic assertions about environment consistency
            if (isInstalled)
            {
                Assert.IsNotNull(lilToonVersion);
                Assert.IsTrue(lilToonVersion >= new SemVer(0, 0, 0), "Version should be non-negative");
            }
            else
            {
                Assert.AreEqual(new SemVer(0, 0, 0).ToString(), lilToonVersion.ToString(), "Version should be 0.0.0 when not installed");
                Assert.IsFalse(canBakeShadowRamp, "Should not be able to bake shadow ramp when not installed");
            }
        }

        /// <summary>
        /// Test version comparison scenarios to ensure proper exception throwing.
        /// </summary>
        [Test]
        public void VersionComparison_VerifyExceptionConditions()
        {
            // Test version comparison logic
            Assert.IsTrue(new SemVer(1, 9, 0) < RequiredVersion, "1.9.0 should be less than required version");
            Assert.IsTrue(new SemVer(1, 10, 0) >= RequiredVersion, "1.10.0 should be greater than or equal to required version");
            Assert.IsTrue(new SemVer(2, 0, 0) >= RequiredVersion, "2.0.0 should be greater than or equal to required version");
            Assert.IsTrue(new SemVer(3, 0, 0).CompareTo(BreakingVersion) == 0, "3.0.0 should equal breaking version");
            Assert.IsFalse(new SemVer(3, 0, 1).CompareTo(BreakingVersion) == 0, "3.0.1 should not equal breaking version");
        }
    }
}
