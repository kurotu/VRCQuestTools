// Tests for UnityAnimationUtility - targeting remaining uncovered methods
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using KRT.VRCQuestTools.Utils;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class UnityAnimationUtilityTests_AnimUtil
    {
        [Test]
        public void ReplaceAnimationClipMaterials_NoMaterials_ReturnsCopy()
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

        [Test]
        public void ReplaceAnimationClipMaterials_WithMaterialKeyframe_ReplacesMaterial()
        {
            var clip = new AnimationClip();
            var origMat = new Material(Shader.Find("Standard"));
            origMat.name = "OrigMaterial";
            var newMat = new Material(Shader.Find("Standard"));
            newMat.name = "NewMaterial";
            try
            {
                // Set up object reference curve with a material keyframe
                var binding = new EditorCurveBinding
                {
                    path = "Body",
                    type = typeof(SkinnedMeshRenderer),
                    propertyName = "m_Materials.Array.data[0]",
                };
                var keyframes = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0, value = origMat },
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                var materialMap = new Dictionary<Material, Material> { { origMat, newMat } };
                var result = UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, materialMap);

                // Verify the material was replaced
                var resultBindings = AnimationUtility.GetObjectReferenceCurveBindings(result);
                Assert.IsTrue(resultBindings.Length > 0);
                var resultKeyframes = AnimationUtility.GetObjectReferenceCurve(result, resultBindings[0]);
                Assert.AreEqual(newMat, resultKeyframes[0].value);

                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(origMat);
                Object.DestroyImmediate(newMat);
            }
        }

        [Test]
        public void ReplaceAnimationClipMaterials_WithUnknownMaterial_DoesNotReplace()
        {
            var clip = new AnimationClip();
            var origMat = new Material(Shader.Find("Standard"));
            origMat.name = "OrigMaterial";
            var otherMat = new Material(Shader.Find("Standard"));
            otherMat.name = "OtherMaterial";
            var newMat = new Material(Shader.Find("Standard"));
            newMat.name = "NewMaterial";
            try
            {
                var binding = new EditorCurveBinding
                {
                    path = "Body",
                    type = typeof(SkinnedMeshRenderer),
                    propertyName = "m_Materials.Array.data[0]",
                };
                var keyframes = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0, value = origMat },
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                // Map only otherMat, not origMat
                var materialMap = new Dictionary<Material, Material> { { otherMat, newMat } };
                var result = UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, materialMap);

                var resultBindings = AnimationUtility.GetObjectReferenceCurveBindings(result);
                var resultKeyframes = AnimationUtility.GetObjectReferenceCurve(result, resultBindings[0]);
                // Should still be origMat
                Assert.AreEqual(origMat, resultKeyframes[0].value);

                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(origMat);
                Object.DestroyImmediate(otherMat);
                Object.DestroyImmediate(newMat);
            }
        }

        [Test]
        public void GetMaterials_ClipWithMaterialKeyframe_ReturnsMaterials()
        {
            var clip = new AnimationClip();
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMaterial";
            try
            {
                var binding = new EditorCurveBinding
                {
                    path = "Body",
                    type = typeof(SkinnedMeshRenderer),
                    propertyName = "m_Materials.Array.data[0]",
                };
                var keyframes = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0, value = mat },
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                var materials = UnityAnimationUtility.GetMaterials(clip);
                Assert.AreEqual(1, materials.Length);
                Assert.AreEqual(mat, materials[0]);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMaterials_ClipWithNoMaterial_ReturnsEmpty()
        {
            var clip = new AnimationClip();
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
        public void GetMaterials_ClipWithDuplicateMaterials_ReturnsDistinct()
        {
            var clip = new AnimationClip();
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMaterial";
            try
            {
                var binding = new EditorCurveBinding
                {
                    path = "Body",
                    type = typeof(SkinnedMeshRenderer),
                    propertyName = "m_Materials.Array.data[0]",
                };
                var keyframes = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0, value = mat },
                    new ObjectReferenceKeyframe { time = 1, value = mat },
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                var materials = UnityAnimationUtility.GetMaterials(clip);
                Assert.AreEqual(1, materials.Length);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void ReplaceAnimationClips_AnimatorController_WithoutSaveAsset()
        {
            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            var layer = controller.layers[0];
            var state = layer.stateMachine.AddState("State1");
            var origClip = new AnimationClip { name = "OrigClip" };
            var newClip = new AnimationClip { name = "NewClip" };
            state.motion = origClip;
            controller.layers = new AnimatorControllerLayer[] { layer };
            try
            {
                var newMotions = new Dictionary<Motion, Motion> { { origClip, newClip } };
                var result = UnityAnimationUtility.ReplaceAnimationClips(controller, false, "", newMotions);

                Assert.IsNotNull(result);
                Assert.AreNotSame(controller, result);

                // Check the motion was replaced
                var resultLayer = result.layers[0];
                var resultState = resultLayer.stateMachine.states[0].state;
                Assert.AreEqual(newClip, resultState.motion);

                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(origClip);
                Object.DestroyImmediate(newClip);
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void ReplaceAnimationClips_WithSubStateMachine()
        {
            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            var layer = controller.layers[0];
            var subSM = layer.stateMachine.AddStateMachine("SubSM");
            var state = subSM.AddState("SubState");
            var origClip = new AnimationClip { name = "OrigClip" };
            var newClip = new AnimationClip { name = "NewClip" };
            state.motion = origClip;
            controller.layers = new AnimatorControllerLayer[] { layer };
            try
            {
                var newMotions = new Dictionary<Motion, Motion> { { origClip, newClip } };
                var result = UnityAnimationUtility.ReplaceAnimationClips(controller, false, "", newMotions);

                Assert.IsNotNull(result);

                // Check sub state machine motion was replaced
                var resultSubSM = result.layers[0].stateMachine.stateMachines[0].stateMachine;
                var resultState = resultSubSM.states[0].state;
                Assert.AreEqual(newClip, resultState.motion);

                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(origClip);
                Object.DestroyImmediate(newClip);
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void ReplaceAnimationClips_WithBlendTreeMotion()
        {
            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            var layer = controller.layers[0];
            var state = layer.stateMachine.AddState("BlendState");
            var blendTree = new BlendTree { name = "BT" };
            var origClip = new AnimationClip { name = "OrigClip" };
            var newClip = new AnimationClip { name = "NewClip" };
            blendTree.AddChild(origClip);
            state.motion = blendTree;
            controller.layers = new AnimatorControllerLayer[] { layer };

            var newBlendTree = new BlendTree { name = "NewBT" };
            newBlendTree.AddChild(origClip);
            try
            {
                var newMotions = new Dictionary<Motion, Motion>
                {
                    { blendTree, newBlendTree },
                    { origClip, newClip },
                };
                var result = UnityAnimationUtility.ReplaceAnimationClips(controller, false, "", newMotions);
                Assert.IsNotNull(result);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(origClip);
                Object.DestroyImmediate(newClip);
                Object.DestroyImmediate(blendTree);
                Object.DestroyImmediate(newBlendTree);
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void ReplaceAnimationClips_OverrideController_WithoutSaveAsset()
        {
            var baseController = new AnimatorController();
            baseController.name = "BaseController";
            baseController.AddLayer("Base");
            var layer = baseController.layers[0];
            var state = layer.stateMachine.AddState("State1");
            var origClip = new AnimationClip { name = "OrigClip" };
            state.motion = origClip;
            baseController.layers = new AnimatorControllerLayer[] { layer };

            var overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = baseController;
            var overrideClip = new AnimationClip { name = "OverrideClip" };
            overrideController[origClip] = overrideClip;
            var newClip = new AnimationClip { name = "NewClip" };
            try
            {
                var newMotions = new Dictionary<Motion, Motion> { { overrideClip, newClip } };
                var result = UnityAnimationUtility.ReplaceAnimationClips(overrideController, false, "", newMotions);

                Assert.IsNotNull(result);
                Assert.AreNotSame(overrideController, result);

                // Check the override was replaced
                var clipPairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                result.GetOverrides(clipPairs);
                bool found = false;
                foreach (var pair in clipPairs)
                {
                    if (pair.Value == newClip)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found, "Override clip should be replaced with newClip");

                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(overrideClip);
                Object.DestroyImmediate(newClip);
                Object.DestroyImmediate(overrideController);
                Object.DestroyImmediate(origClip);
                Object.DestroyImmediate(baseController);
            }
        }

        [Test]
        public void GetBlendTrees_WithSubStateMachine()
        {
            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            var layer = controller.layers[0];
            var subSM = layer.stateMachine.AddStateMachine("SubSM");
            var state = subSM.AddState("BlendState");
            var blendTree = new BlendTree { name = "BT" };
            var clip = new AnimationClip { name = "Clip" };
            blendTree.AddChild(clip);
            state.motion = blendTree;
            controller.layers = new AnimatorControllerLayer[] { layer };
            try
            {
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsTrue(trees.Length > 0);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(blendTree);
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetBlendTrees_WithNestedBlendTree()
        {
            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            var layer = controller.layers[0];
            var state = layer.stateMachine.AddState("State1");
            var parentTree = new BlendTree { name = "ParentBT" };
            var childTree = new BlendTree { name = "ChildBT" };
            var clip = new AnimationClip { name = "Clip" };
            childTree.AddChild(clip);
            parentTree.AddChild(childTree);
            state.motion = parentTree;
            controller.layers = new AnimatorControllerLayer[] { layer };
            try
            {
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsTrue(trees.Length >= 2, $"Expected 2+ blend trees, got {trees.Length}");
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(childTree);
                Object.DestroyImmediate(parentTree);
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_DirectChild_ReturnsTrue()
        {
            var tree = new BlendTree { name = "Root" };
            var clip = new AnimationClip { name = "Clip" };
            tree.AddChild(clip);
            try
            {
                Assert.IsTrue(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip));
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(tree);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_NestedChild_ReturnsTrue()
        {
            var rootTree = new BlendTree { name = "Root" };
            var childTree = new BlendTree { name = "Child" };
            var clip = new AnimationClip { name = "Clip" };
            childTree.AddChild(clip);
            rootTree.AddChild(childTree);
            try
            {
                Assert.IsTrue(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(rootTree, clip));
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(childTree);
                Object.DestroyImmediate(rootTree);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_NotPresent_ReturnsFalse()
        {
            var tree = new BlendTree { name = "Root" };
            var clip1 = new AnimationClip { name = "Clip1" };
            var clip2 = new AnimationClip { name = "Clip2" };
            tree.AddChild(clip1);
            try
            {
                Assert.IsFalse(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip2));
            }
            finally
            {
                Object.DestroyImmediate(clip1);
                Object.DestroyImmediate(clip2);
                Object.DestroyImmediate(tree);
            }
        }

        [Test]
        public void DeepCopyBlendTree_CopiesProperties()
        {
            var tree = new BlendTree { name = "Original", blendParameter = "Speed" };
            var clip = new AnimationClip { name = "Clip" };
            tree.AddChild(clip, 0.5f);
            try
            {
                var copy = UnityAnimationUtility.DeepCopyBlendTree(tree);
                Assert.IsNotNull(copy);
                Assert.AreNotSame(tree, copy);
                Assert.AreEqual("Original", copy.name);
                Assert.AreEqual("Speed", copy.blendParameter);
                Assert.AreEqual(tree.children.Length, copy.children.Length);
                Object.DestroyImmediate(copy);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(tree);
            }
        }
    }
}
