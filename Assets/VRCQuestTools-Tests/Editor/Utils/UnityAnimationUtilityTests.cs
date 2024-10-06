// <copyright file="UnityAnimationUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for Unity Animation.
    /// </summary>
    public class UnityAnimationUtilityTests
    {
        /// <summary>
        /// Case which two animator controllers share an animation clip.
        /// </summary>
        [Test]
        public void SharedAnimationClip()
        {
            var fixturesFolder = TestUtils.FixturesFolder + "/SharedAnimationClip";

            var controller1 = AssetDatabase.LoadAssetAtPath<AnimatorController>(fixturesFolder + "/Controller1.controller");
            Assert.NotNull(controller1);
            Assert.AreEqual(1, controller1.animationClips.Length);

            var controller2 = AssetDatabase.LoadAssetAtPath<AnimatorController>(fixturesFolder + "/Controller2.controller");
            Assert.NotNull(controller2);
            Assert.AreEqual(1, controller2.animationClips.Length);

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fixturesFolder + "/Animation.anim");
            Assert.NotNull(clip);

            Assert.True(controller1.animationClips.Contains(clip));
            Assert.True(controller2.animationClips.Contains(clip));

            Assert.AreEqual(1, controller1.animationClips.Concat(controller2.animationClips).Distinct().Count());

            var guid = TestUtils.GetAssetGUID(clip);
            var guid1 = TestUtils.GetAssetGUID(controller1.animationClips[0]);
            var guid2 = TestUtils.GetAssetGUID(controller2.animationClips[0]);
            Assert.AreEqual(guid, guid1);
            Assert.AreEqual(guid, guid2);
        }

        /// <summary>
        /// Case which an animator controller has nested state machines.
        /// </summary>
        [Test]
        public void DuplicateNestedStateMachines()
        {
            var fixturesFolder = TestUtils.FixturesFolder + "/Animations";

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(fixturesFolder + "/NestedStateMachines.controller");
            Assert.NotNull(controller);

            Assert.DoesNotThrow(() => UnityAnimationUtility.ReplaceAnimationClips(controller, false, null, new Dictionary<Motion, Motion> { }));
        }
    }
}
