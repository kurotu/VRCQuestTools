// Tests for UnityAnimationUtility - covering remaining uncovered methods

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    internal class UnityAnimationUtilityBlendTreeTests
    {
        [Test]
        public void GetBlendTrees_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            controller.AddLayer("Base");
            try
            {
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.AreEqual(0, trees.Length);
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetBlendTrees_WithBlendTree_ReturnsTree()
        {
            var controller = new AnimatorController();
            controller.AddLayer("Base");
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var layer = controller.layers[0];
            var tree = new BlendTree { name = "TestTree", blendParameter = "Speed" };
            var state = layer.stateMachine.AddState("BlendState");
            state.motion = tree;
            controller.layers = new[] { layer };
            try
            {
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.AreEqual(1, trees.Length);
                Assert.AreEqual("TestTree", trees[0].name);
            }
            finally
            {
                Object.DestroyImmediate(controller);
                Object.DestroyImmediate(tree);
            }
        }

        [Test]
        public void GetBlendTrees_WithChildStateMachine_ReturnsTreesFromBoth()
        {
            var controller = new AnimatorController();
            controller.AddLayer("Base");
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var layer = controller.layers[0];

            var tree1 = new BlendTree { name = "Tree1", blendParameter = "Speed" };
            var state1 = layer.stateMachine.AddState("State1");
            state1.motion = tree1;

            var childSM = layer.stateMachine.AddStateMachine("ChildSM");
            var tree2 = new BlendTree { name = "Tree2", blendParameter = "Speed" };
            var state2 = childSM.AddState("State2");
            state2.motion = tree2;

            controller.layers = new[] { layer };
            try
            {
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.AreEqual(2, trees.Length);
            }
            finally
            {
                Object.DestroyImmediate(controller);
                Object.DestroyImmediate(tree1);
                Object.DestroyImmediate(tree2);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_DirectChild_ReturnsTrue()
        {
            var rootTree = new BlendTree { name = "Root" };
            var clip = new AnimationClip { name = "Clip1" };
            rootTree.AddChild(clip);
            try
            {
                var result = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(rootTree, clip);
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(rootTree);
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_NotPresent_ReturnsFalse()
        {
            var rootTree = new BlendTree { name = "Root" };
            var clip1 = new AnimationClip { name = "Clip1" };
            var clip2 = new AnimationClip { name = "Clip2" };
            rootTree.AddChild(clip1);
            try
            {
                var result = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(rootTree, clip2);
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(rootTree);
                Object.DestroyImmediate(clip1);
                Object.DestroyImmediate(clip2);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_NestedTree_ReturnsTrue()
        {
            var rootTree = new BlendTree { name = "Root" };
            var childTree = new BlendTree { name = "Child" };
            var clip = new AnimationClip { name = "Clip1" };
            childTree.AddChild(clip);
            rootTree.AddChild(childTree);
            try
            {
                var result = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(rootTree, clip);
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(rootTree);
                Object.DestroyImmediate(childTree);
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void DeepCopyBlendTree_CopiesProperties()
        {
            var original = new BlendTree { name = "Original", blendType = BlendTreeType.Simple1D };
            var clip = new AnimationClip { name = "Clip" };
            original.AddChild(clip);
            try
            {
                var copy = UnityAnimationUtility.DeepCopyBlendTree(original);
                Assert.IsNotNull(copy);
                Assert.AreNotSame(original, copy);
                Assert.AreEqual(BlendTreeType.Simple1D, copy.blendType);
                Assert.AreEqual(original.children.Length, copy.children.Length);
                Object.DestroyImmediate(copy);
            }
            finally
            {
                Object.DestroyImmediate(original);
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void GetMaterials_AnimationClip_WithNoMaterialBindings_ReturnsEmpty()
        {
            var clip = new AnimationClip();
            clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, 0, 1, 1));
            try
            {
                var materials = UnityAnimationUtility.GetMaterials(clip);
                Assert.AreEqual(0, materials.Length);
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void GetMaterials_RuntimeAnimatorController_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            controller.AddLayer("Base");
            try
            {
                var materials = UnityAnimationUtility.GetMaterials(controller);
                Assert.AreEqual(0, materials.Length);
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void ReplaceAnimationClipMaterials_WithNoMaterialKeyframes_ReturnsCopy()
        {
            var clip = new AnimationClip { name = "TestClip" };
            clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, 0, 1, 1));
            var newMaterials = new Dictionary<Material, Material>();
            try
            {
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

        [Test]
        public void ReplaceAnimationClips_AnimatorController_NoSaveAsAsset_ReturnsCopy()
        {
            var controller = new AnimatorController();
            controller.name = "TestCtrl";
            controller.AddLayer("Base");
            var clip = new AnimationClip { name = "Clip1" };
            var state = controller.layers[0].stateMachine.AddState("State1");
            state.motion = clip;
            var newMotions = new Dictionary<Motion, Motion>();
            try
            {
                var result = UnityAnimationUtility.ReplaceAnimationClips(controller, false, null, newMotions);
                Assert.IsNotNull(result);
                Assert.AreNotSame(controller, result);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(controller);
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void ReplaceAnimationClips_AnimatorController_ReplacesClip()
        {
            var controller = new AnimatorController();
            controller.name = "TestCtrl";
            controller.AddLayer("Base");
            var oldClip = new AnimationClip { name = "OldClip" };
            var newClip = new AnimationClip { name = "NewClip" };
            var state = controller.layers[0].stateMachine.AddState("State1");
            state.motion = oldClip;

            var newMotions = new Dictionary<Motion, Motion> { { oldClip, newClip } };
            try
            {
                var result = UnityAnimationUtility.ReplaceAnimationClips(controller, false, null, newMotions);
                Assert.IsNotNull(result);
                // Verify the state's motion was replaced
                var resultState = result.layers[0].stateMachine.states[0].state;
                Assert.AreEqual("NewClip", resultState.motion.name);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(controller);
                Object.DestroyImmediate(oldClip);
                Object.DestroyImmediate(newClip);
            }
        }

        [Test]
        public void ReplaceAnimationClips_OverrideController_NoSaveAsAsset_ReturnsCopy()
        {
            var baseController = new AnimatorController();
            baseController.name = "BaseCtrl";
            baseController.AddLayer("Base");
            var clip = new AnimationClip { name = "BaseClip" };
            var state = baseController.layers[0].stateMachine.AddState("State1");
            state.motion = clip;

            var overrideController = new AnimatorOverrideController(baseController);
            overrideController.name = "OverrideCtrl";

            var newMotions = new Dictionary<Motion, Motion>();
            try
            {
                var result = UnityAnimationUtility.ReplaceAnimationClips(overrideController, false, null, newMotions);
                Assert.IsNotNull(result);
                Assert.AreNotSame(overrideController, result);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(overrideController);
                Object.DestroyImmediate(baseController);
                Object.DestroyImmediate(clip);
            }
        }
    }
}
