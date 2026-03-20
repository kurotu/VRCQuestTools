// Extended tests for UnityAnimationUtility
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class UnityAnimationUtilityExtendedTests
    {
        [Test]
        public void GetBlendTrees_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            controller.AddLayer("BaseLayer");
            try
            {
                var result = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetBlendTrees_WithBlendTree_FindsIt()
        {
            var controller = new AnimatorController();
            controller.AddParameter("Blend", AnimatorControllerParameterType.Float);
            controller.AddLayer("BaseLayer");
            var layer = controller.layers[0];
            var state = layer.stateMachine.AddState("BlendState");
            var blendTree = new BlendTree();
            blendTree.blendParameter = "Blend";
            state.motion = blendTree;
            try
            {
                var result = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.AreEqual(1, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(blendTree);
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetBlendTrees_WithSubStateMachine_FindsNestedBlendTrees()
        {
            var controller = new AnimatorController();
            controller.AddParameter("Blend", AnimatorControllerParameterType.Float);
            controller.AddLayer("BaseLayer");
            var layer = controller.layers[0];
            var subSM = layer.stateMachine.AddStateMachine("SubSM");
            var state = subSM.AddState("SubState");
            var blendTree = new BlendTree();
            blendTree.blendParameter = "Blend";
            state.motion = blendTree;
            try
            {
                var result = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.AreEqual(1, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(blendTree);
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_EmptyBlendTree_ReturnsFalse()
        {
            var blendTree = new BlendTree();
            var clip = new AnimationClip();
            try
            {
                var result = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(blendTree, clip);
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(blendTree);
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_ContainsMotion_ReturnsTrue()
        {
            var blendTree = new BlendTree();
            var clip = new AnimationClip();
            blendTree.AddChild(clip);
            try
            {
                var result = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(blendTree, clip);
                Assert.IsTrue(result);
            }
            finally
            {
                Object.DestroyImmediate(blendTree);
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_NestedBlendTree_FindsMotion()
        {
            var rootTree = new BlendTree();
            var childTree = new BlendTree();
            var clip = new AnimationClip();
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
        public void DoesMotionExistInBlendTreeDescendants_DifferentMotion_ReturnsFalse()
        {
            var blendTree = new BlendTree();
            var clip1 = new AnimationClip();
            var clip2 = new AnimationClip();
            blendTree.AddChild(clip1);
            try
            {
                var result = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(blendTree, clip2);
                Assert.IsFalse(result);
            }
            finally
            {
                Object.DestroyImmediate(blendTree);
                Object.DestroyImmediate(clip1);
                Object.DestroyImmediate(clip2);
            }
        }

        [Test]
        public void GetMaterials_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            controller.AddLayer("BaseLayer");
            try
            {
                var result = UnityAnimationUtility.GetMaterials(controller);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetMaterials_NullController_ThrowsNullReferenceException()
        {
            Assert.Throws<System.NullReferenceException>(() =>
            {
                UnityAnimationUtility.GetMaterials((RuntimeAnimatorController)null);
            });
        }

        [Test]
        public void DeepCopyBlendTree_CopiesBlendTree()
        {
            var original = new BlendTree();
            original.name = "OriginalTree";
            original.blendType = BlendTreeType.Simple1D;
            try
            {
                var copy = UnityAnimationUtility.DeepCopyBlendTree(original);
                try
                {
                    Assert.IsNotNull(copy);
                    Assert.AreNotSame(original, copy);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void DeepCopyBlendTree_WithChildren_CopiesChildren()
        {
            var original = new BlendTree();
            var clip = new AnimationClip();
            original.AddChild(clip);
            try
            {
                var copy = UnityAnimationUtility.DeepCopyBlendTree(original);
                try
                {
                    Assert.IsNotNull(copy);
                    Assert.AreEqual(original.children.Length, copy.children.Length);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
                Object.DestroyImmediate(clip);
            }
        }
    }
}
