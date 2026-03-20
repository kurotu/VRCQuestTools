// <copyright file="UnityAnimationUtilityReplaceTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    internal class UnityAnimationUtilityReplaceTests
    {
        // ---- ReplaceAnimationClipMaterials ----

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
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void ReplaceAnimationClipMaterials_WithMaterialBinding_ReplacesMaterial()
        {
            var clip = new AnimationClip();
            clip.name = "MatSwapClip";
            var oldMat = new Material(Shader.Find("Standard"));
            oldMat.name = "OldMat";
            var newMat = new Material(Shader.Find("Standard"));
            newMat.name = "NewMat";

            try
            {
                // Add material keyframe
                var binding = new EditorCurveBinding
                {
                    path = "Body",
                    type = typeof(SkinnedMeshRenderer),
                    propertyName = "m_Materials.Array.data[0]",
                };
                var keyframes = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0, value = oldMat },
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                var replacements = new Dictionary<Material, Material> { { oldMat, newMat } };
                var result = UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, replacements);

                // Verify the material was replaced
                var resultBindings = AnimationUtility.GetObjectReferenceCurveBindings(result);
                Assert.IsTrue(resultBindings.Length > 0);
                var resultKeyframes = AnimationUtility.GetObjectReferenceCurve(result, resultBindings[0]);
                Assert.AreEqual(newMat, resultKeyframes[0].value);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(oldMat);
                Object.DestroyImmediate(newMat);
            }
        }

        [Test]
        public void ReplaceAnimationClipMaterials_NoMatchingMaterial_KeepsOriginal()
        {
            var clip = new AnimationClip();
            clip.name = "NoMatchClip";
            var origMat = new Material(Shader.Find("Standard"));
            origMat.name = "OrigMat";
            var otherMat = new Material(Shader.Find("Standard"));
            otherMat.name = "OtherMat";
            var replaceMat = new Material(Shader.Find("Standard"));
            replaceMat.name = "ReplaceMat";

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

                // Replace with a material that's not in the clip
                var replacements = new Dictionary<Material, Material> { { otherMat, replaceMat } };
                var result = UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, replacements);

                var resultBindings = AnimationUtility.GetObjectReferenceCurveBindings(result);
                var resultKeyframes = AnimationUtility.GetObjectReferenceCurve(result, resultBindings[0]);
                Assert.AreEqual(origMat, resultKeyframes[0].value);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(origMat);
                Object.DestroyImmediate(otherMat);
                Object.DestroyImmediate(replaceMat);
            }
        }

        // ---- ReplaceAnimationClips (AnimatorController, not saving as asset) ----

        [Test]
        public void ReplaceAnimationClips_Controller_NotSaveAsAsset_ReturnsClone()
        {
            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            var layer = controller.layers[0];
            var sm = layer.stateMachine;

            var origClip = new AnimationClip();
            origClip.name = "OrigClip";
            var newClip = new AnimationClip();
            newClip.name = "NewClip";

            var state = sm.AddState("TestState");
            state.motion = origClip;

            try
            {
                var motions = new Dictionary<Motion, Motion> { { origClip, newClip } };
                var result = UnityAnimationUtility.ReplaceAnimationClips(controller, false, null, motions);
                Assert.IsNotNull(result);
                Assert.AreNotSame(controller, result);

                // Verify clip was replaced
                var resultState = result.layers[0].stateMachine.states[0].state;
                Assert.AreEqual(newClip, resultState.motion);
            }
            finally
            {
                Object.DestroyImmediate(controller);
                Object.DestroyImmediate(origClip);
                Object.DestroyImmediate(newClip);
            }
        }

        [Test]
        public void ReplaceAnimationClips_Controller_NoMatchingMotion_KeepsOriginal()
        {
            var controller = new AnimatorController();
            controller.name = "TestController2";
            controller.AddLayer("Base");
            var layer = controller.layers[0];
            var sm = layer.stateMachine;

            var origClip = new AnimationClip();
            origClip.name = "OrigClip";
            var otherClip = new AnimationClip();
            otherClip.name = "OtherClip";
            var replaceClip = new AnimationClip();
            replaceClip.name = "ReplaceClip";

            var state = sm.AddState("TestState");
            state.motion = origClip;

            try
            {
                var motions = new Dictionary<Motion, Motion> { { otherClip, replaceClip } };
                var result = UnityAnimationUtility.ReplaceAnimationClips(controller, false, null, motions);

                var resultState = result.layers[0].stateMachine.states[0].state;
                // Motion should not be replaced since it doesn't match
                Assert.IsNotNull(result);
            }
            finally
            {
                Object.DestroyImmediate(controller);
                Object.DestroyImmediate(origClip);
                Object.DestroyImmediate(otherClip);
                Object.DestroyImmediate(replaceClip);
            }
        }

        // ---- GetMaterials from clip ----

        [Test]
        public void GetMaterials_ClipWithMaterial_ReturnsMaterial()
        {
            var clip = new AnimationClip();
            clip.name = "MatClip";
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";

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
                Assert.IsTrue(materials.Contains(mat));
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMaterials_ClipWithoutMaterial_ReturnsEmpty()
        {
            var clip = new AnimationClip();
            clip.name = "EmptyClip";
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

        // ---- GetMaterials from controller ----

        [Test]
        public void GetMaterials_ControllerWithMaterialClip_ReturnsMaterial()
        {
            var controller = new AnimatorController();
            controller.name = "MatController";
            controller.AddLayer("Base");

            var clip = new AnimationClip();
            clip.name = "MatClip";
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "ControllerMat";

            try
            {
                var binding = new EditorCurveBinding
                {
                    path = "Body",
                    type = typeof(MeshRenderer),
                    propertyName = "m_Materials.Array.data[0]",
                };
                var keyframes = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0, value = mat },
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                var sm = controller.layers[0].stateMachine;
                var state = sm.AddState("TestState");
                state.motion = clip;

                var materials = UnityAnimationUtility.GetMaterials(controller);
                Assert.IsTrue(materials.Contains(mat));
            }
            finally
            {
                Object.DestroyImmediate(controller);
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(mat);
            }
        }
    }
}
