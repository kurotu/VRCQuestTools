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
        /// ReplaceAnimationClipMaterials should rewrite a material-swap object reference curve on a
        /// duplicated clip while leaving the source clip untouched. This is a regression test for the
        /// non-NDMF-standard animation rewriting used by the NDMF passes (see NDMF/AvatarConverterPassUtility).
        /// </summary>
        [Test]
        public void ReplaceAnimationClipMaterials_RewritesRendererMaterialCurve()
        {
            var originalMaterial = new Material(Shader.Find("Standard"));
            var newMaterial = new Material(Shader.Find("Standard"));
            AnimationClip clip = null;
            AnimationClip replaced = null;
            try
            {
                clip = new AnimationClip();
                var binding = EditorCurveBinding.PPtrCurve("Body", typeof(SkinnedMeshRenderer), "m_Materials.Array.data[0]");
                var keyframes = new[]
                {
                    new ObjectReferenceKeyframe { time = 0, value = originalMaterial },
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                replaced = UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, new Dictionary<Material, Material> { [originalMaterial] = newMaterial });

                Assert.AreNotSame(clip, replaced, "The pass must duplicate the clip rather than mutate it in place.");

                var originalKeyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                Assert.AreSame(originalMaterial, originalKeyframes[0].value, "Source clip must remain untouched.");

                var replacedKeyframes = AnimationUtility.GetObjectReferenceCurve(replaced, binding);
                Assert.AreSame(newMaterial, replacedKeyframes[0].value, "Duplicated clip's material reference curve must point to the new material.");
            }
            finally
            {
                if (clip != null)
                {
                    Object.DestroyImmediate(clip);
                }

                if (replaced != null)
                {
                    Object.DestroyImmediate(replaced);
                }

                Object.DestroyImmediate(originalMaterial);
                Object.DestroyImmediate(newMaterial);
            }
        }
    }
}
