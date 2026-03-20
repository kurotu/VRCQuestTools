// <copyright file="SemVerAdditionalTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Additional tests for <see cref="SemVer"/> to improve coverage.
    /// </summary>
    public class SemVerAdditionalTests
    {
        [Test]
        public void Prerelease_VersionIsPrerelease()
        {
            var ver = new SemVer("1.2.3-beta.1");
            Assert.IsTrue(ver.IsPrerelease);
        }

        [Test]
        public void Prerelease_StableVersionIsNotPrerelease()
        {
            var ver = new SemVer("1.2.3");
            Assert.IsFalse(ver.IsPrerelease);
        }

        [Test]
        public void IsMajorUpdate_HigherMajor_ReturnsTrue()
        {
            var newVer = new SemVer("2.0.0");
            var oldVer = new SemVer("1.9.9");
            Assert.IsTrue(newVer.IsMajorUpdate(oldVer));
        }

        [Test]
        public void IsMajorUpdate_SameMajor_ReturnsFalse()
        {
            var newVer = new SemVer("1.5.0");
            var oldVer = new SemVer("1.0.0");
            Assert.IsFalse(newVer.IsMajorUpdate(oldVer));
        }

        [Test]
        public void IsMajorUpdate_LowerMajor_ReturnsFalse()
        {
            var newVer = new SemVer("1.0.0");
            var oldVer = new SemVer("2.0.0");
            Assert.IsFalse(newVer.IsMajorUpdate(oldVer));
        }

        [Test]
        public void LessThanOrEqual_Equal_ReturnsTrue()
        {
            var a = new SemVer("1.2.3");
            var b = new SemVer("1.2.3");
            Assert.IsTrue(a <= b);
        }

        [Test]
        public void LessThanOrEqual_Less_ReturnsTrue()
        {
            var a = new SemVer("1.2.2");
            var b = new SemVer("1.2.3");
            Assert.IsTrue(a <= b);
        }

        [Test]
        public void GreaterThanOrEqual_Equal_ReturnsTrue()
        {
            var a = new SemVer("1.2.3");
            var b = new SemVer("1.2.3");
            Assert.IsTrue(a >= b);
        }

        [Test]
        public void GreaterThanOrEqual_Greater_ReturnsTrue()
        {
            var a = new SemVer("1.2.4");
            var b = new SemVer("1.2.3");
            Assert.IsTrue(a >= b);
        }

        [Test]
        public void LessThan_SameMajor_DifferentMinor()
        {
            var a = new SemVer("1.1.0");
            var b = new SemVer("1.2.0");
            Assert.IsTrue(a < b);
            Assert.IsFalse(b < a);
        }

        [Test]
        public void GreaterThan_DifferentPatch()
        {
            var a = new SemVer("1.2.4");
            var b = new SemVer("1.2.3");
            Assert.IsTrue(a > b);
        }

        [Test]
        public void ToString_WithPrerelease()
        {
            var ver = new SemVer("1.2.3-beta.1");
            Assert.AreEqual("1.2.3-beta.1", ver.ToString());
        }

        [Test]
        public void ToString_WithoutPrerelease()
        {
            var ver = new SemVer("1.2.3");
            Assert.AreEqual("1.2.3", ver.ToString());
        }

        [Test]
        public void Constructor_FourParts_ParsesMajorMinorPatch()
        {
            var ver = new SemVer(2, 5, 10, null);
            Assert.AreEqual("2.5.10", ver.ToString());
        }

        [Test]
        public void Prerelease_LessThanStable()
        {
            var pre = new SemVer("1.0.0-alpha");
            var stable = new SemVer("1.0.0");
            Assert.IsTrue(pre < stable);
        }

        [Test]
        public void GreaterThan_StableGreaterThanPrerelease()
        {
            var stable = new SemVer("1.0.0");
            var pre = new SemVer("1.0.0-alpha");
            Assert.IsTrue(stable > pre);
        }

        [Test]
        public void GreaterThan_PrereleaseComparison_HigherPrerelease()
        {
            var a = new SemVer("1.0.0-beta");
            var b = new SemVer("1.0.0-alpha");
            Assert.IsTrue(a > b);
        }

        [Test]
        public void LessThan_PrereleaseComparison_LowerPrerelease()
        {
            var a = new SemVer("1.0.0-alpha");
            var b = new SemVer("1.0.0-beta");
            Assert.IsTrue(a < b);
        }

        [Test]
        public void GreaterThan_SamePrerelease_ReturnsFalse()
        {
            var a = new SemVer("1.0.0-alpha");
            var b = new SemVer("1.0.0-alpha");
            Assert.IsFalse(a > b);
            Assert.IsFalse(a < b);
        }

        [Test]
        public void LessThan_MajorDifference_ReturnsFalse()
        {
            var a = new SemVer("2.0.0");
            var b = new SemVer("1.0.0");
            Assert.IsFalse(a < b);
        }

        [Test]
        public void GreaterThan_MajorDifference_ReturnsFalse()
        {
            var a = new SemVer("1.0.0");
            var b = new SemVer("2.0.0");
            Assert.IsFalse(a > b);
        }

        [Test]
        public void CompareTo_GreaterReturnsPositive()
        {
            var a = new SemVer("2.0.0");
            var b = new SemVer("1.0.0");
            Assert.Greater(a.CompareTo(b), 0);
        }

        [Test]
        public void CompareTo_LessReturnsNegative()
        {
            var a = new SemVer("1.0.0");
            var b = new SemVer("2.0.0");
            Assert.Less(a.CompareTo(b), 0);
        }

        [Test]
        public void CompareTo_EqualReturnsZero()
        {
            var a = new SemVer("1.2.3");
            var b = new SemVer("1.2.3");
            Assert.AreEqual(0, a.CompareTo(b));
        }

        [Test]
        public void GreaterThanOrEqual_PrereleaseEqual_ReturnsTrue()
        {
            var a = new SemVer("1.0.0-alpha");
            var b = new SemVer("1.0.0-alpha");
            Assert.IsTrue(a >= b);
            Assert.IsTrue(a <= b);
        }

        [Test]
        public void LessThanOrEqual_Greater_ReturnsFalse()
        {
            var a = new SemVer("2.0.0");
            var b = new SemVer("1.0.0");
            Assert.IsFalse(a <= b);
        }

        [Test]
        public void GreaterThanOrEqual_Less_ReturnsFalse()
        {
            var a = new SemVer("1.0.0");
            var b = new SemVer("2.0.0");
            Assert.IsFalse(a >= b);
        }

        [Test]
        public void GreaterThan_SameMajorMinor_GreaterPatch()
        {
            var a = new SemVer("1.2.4");
            var b = new SemVer("1.2.3");
            Assert.IsTrue(a > b);
            Assert.IsFalse(b > a);
        }

        [Test]
        public void LessThan_SameMajorMinor_LessPatch()
        {
            var a = new SemVer("1.2.2");
            var b = new SemVer("1.2.3");
            Assert.IsTrue(a < b);
            Assert.IsFalse(b < a);
        }
    }
}
