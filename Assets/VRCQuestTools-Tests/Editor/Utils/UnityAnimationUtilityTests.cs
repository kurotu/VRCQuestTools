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

        /// <summary>
        /// Test GetMaterials with empty AnimationClip returns empty.
        /// </summary>
        [Test]
        public void GetMaterials_EmptyClip_ReturnsEmpty()
        {
            var clip = new AnimationClip();
            try
            {
                var materials = UnityAnimationUtility.GetMaterials(clip);
                Assert.IsNotNull(materials);
                Assert.AreEqual(0, materials.Length);
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        /// <summary>
        /// Test GetMaterials with empty RuntimeAnimatorController returns empty.
        /// </summary>
        [Test]
        public void GetMaterials_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            controller.AddLayer("TestLayer");
            try
            {
                var materials = UnityAnimationUtility.GetMaterials((RuntimeAnimatorController)controller);
                Assert.IsNotNull(materials);
                Assert.AreEqual(0, materials.Length);
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        /// <summary>
        /// Test GetBlendTrees with empty controller returns empty.
        /// </summary>
        [Test]
        public void GetBlendTrees_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            controller.AddLayer("TestLayer");
            try
            {
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(trees);
                Assert.AreEqual(0, trees.Length);
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        /// <summary>
        /// Test GetBlendTrees finds blend tree in controller.
        /// </summary>
        [Test]
        public void GetBlendTrees_ControllerWithBlendTree_ReturnsTree()
        {
            var controller = new AnimatorController();
            controller.AddLayer("TestLayer");
            controller.AddParameter("blend", AnimatorControllerParameterType.Float);
            var tree = new BlendTree();
            tree.blendParameter = "blend";
            var state = controller.layers[0].stateMachine.AddState("BlendState");
            state.motion = tree;
            try
            {
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(trees);
                Assert.AreEqual(1, trees.Length);
            }
            finally
            {
                Object.DestroyImmediate(tree);
                Object.DestroyImmediate(controller);
            }
        }

        /// <summary>
        /// Test DoesMotionExistInBlendTreeDescendants returns false for empty tree.
        /// </summary>
        [Test]
        public void DoesMotionExistInBlendTreeDescendants_EmptyTree_ReturnsFalse()
        {
            var tree = new BlendTree();
            var clip = new AnimationClip();
            try
            {
                var result = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip);
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(tree);
            }
        }

        /// <summary>
        /// Test DoesMotionExistInBlendTreeDescendants returns true when motion present.
        /// </summary>
        [Test]
        public void DoesMotionExistInBlendTreeDescendants_MotionPresent_ReturnsTrue()
        {
            var clip = new AnimationClip();
            var tree = new BlendTree();
            tree.AddChild(clip);
            try
            {
                var result = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip);
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(tree);
            }
        }

        /// <summary>
        /// Test DeepCopyBlendTree creates a new copy.
        /// </summary>
        [Test]
        public void DeepCopyBlendTree_CopiesTree()
        {
            var tree = new BlendTree();
            tree.name = "OriginalTree";
            try
            {
                var copy = UnityAnimationUtility.DeepCopyBlendTree(tree);
                Assert.IsNotNull(copy);
                Assert.AreNotSame(tree, copy);
                Object.DestroyImmediate(copy);
            }
            finally
            {
                Object.DestroyImmediate(tree);
            }
        }

        /// <summary>
        /// Test ReplaceAnimationClipMaterials returns a clone.
        /// </summary>
        [Test]
        public void ReplaceAnimationClipMaterials_EmptyClip_ReturnsClone()
        {
            var clip = new AnimationClip();
            clip.name = "TestClip";
            try
            {
                var newMaterials = new Dictionary<Material, Material>();
                var result = UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, newMaterials);
                Assert.IsNotNull(result);
                Assert.AreNotSame(clip, result);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }
    }
}
