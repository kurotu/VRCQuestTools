// <copyright file="ComponentRemoverTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for ComponentRemover.
    /// </summary>
    public class ComponentRemoverTests
    {
        private readonly ComponentRemover remover = new ComponentRemover();

        /// <summary>
        /// Test built-in components and subclasses.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <param name="expected">Expected result.</param>
        [Test]
        [TestCase(typeof(SkinnedMeshRenderer), false)]
        [TestCase(typeof(Camera), true)]
        [TestCase(typeof(BoxCollider), true)]
        [TestCase(typeof(ParentConstraint), true)]
        public void TestTypes(System.Type type, bool expected)
        {
            Assert.AreEqual(expected, remover.IsUnsupportedComponent(type));
        }

        /// <summary>
        /// Test DynamicBone classes.
        /// </summary>
        /// <param name="typeName">type name to test.</param>
        [Test]
        [TestCase("DynamicBone")]
        [TestCase("DynamicBoneCollider")]
        public void TestDynamicBone(string typeName)
        {
            if (!TestUtils.HasDynamicBone)
            {
                Assert.Ignore("DynamicBone is not imported.");
            }
            var type = SystemUtility.GetTypeByName(typeName);
            Assert.NotNull(type);
            Assert.True(remover.IsUnsupportedComponent(type));
        }

        /// <summary>
        /// Test FinalIK classes. VRCQuestTools should allow FinalIK.
        /// </summary>
        /// <param name="typeName">type name to test.</param>
        [Test]
        [TestCase("RootMotion.FinalIK.AimIK")]
        [TestCase("RootMotion.FinalIK.FullBodyBipedIK")]
        public void TestFinalIK(string typeName)
        {
            if (!TestUtils.HasFinalIK)
            {
                Assert.Ignore("FinalIK is not imported.");
            }
            var type = SystemUtility.GetTypeByName(typeName);
            Assert.NotNull(type);
            Assert.False(remover.IsUnsupportedComponent(type));
        }
    }
}
