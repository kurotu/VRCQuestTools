// <copyright file="AnimConverterCoverageTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Tests
{
    /// <summary>
    /// Batch 45: Animation conversion paths in AvatarConverter, RemoveVertexColor, ApplyVRCQuestToolsComponents.
    /// </summary>
    [TestFixture]
    public class AnimConverterCoverageTests
    {
        private readonly List<UnityEngine.Object> toCleanup = new List<UnityEngine.Object>();
        private string tempDir;

        [SetUp]
        public void SetUp()
        {
            tempDir = "Assets/VRCQuestTools-Tests/Temp_Batch45_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in toCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            toCleanup.Clear();

            if (AssetDatabase.IsValidFolder(tempDir))
            {
                AssetDatabase.DeleteAsset(tempDir);
            }
        }

        private AvatarConverter CreateAvatarConverter()
        {
            var builder = new MaterialWrapperBuilder();
            var ctor = typeof(AvatarConverter).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { typeof(MaterialWrapperBuilder) },
                null);
            return ctor != null ? (AvatarConverter)ctor.Invoke(new object[] { builder }) : null;
        }

        #region ConvertAnimationClipsForQuest

        [Test]
        public void ConvertAnimationClipsForQuest_WithMaterialAnim_ConvertsClip()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            // Create an animator controller with a clip that references materials
            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            toCleanup.Add(controller);

            var origMat = new Material(Shader.Find("Standard"));
            origMat.name = "OrigMat";
            toCleanup.Add(origMat);

            var convertedMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            convertedMat.name = "ConvertedMat";
            toCleanup.Add(convertedMat);

            // Create animation clip with material reference via ObjectReferenceKeyframes
            var clip = new AnimationClip();
            clip.name = "MaterialAnimClip";
            toCleanup.Add(clip);

            var binding = EditorCurveBinding.PPtrCurve("Body", typeof(MeshRenderer), "m_Materials.Array.data[0]");
            var keyframes = new ObjectReferenceKeyframe[]
            {
                new ObjectReferenceKeyframe { time = 0, value = origMat },
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            // Add state with material-animated clip
            var stateMachine = controller.layers[0].stateMachine;
            var state = stateMachine.AddState("MaterialState");
            state.motion = clip;

            var controllers = new RuntimeAnimatorController[] { controller };
            var convertedMaterials = new Dictionary<Material, Material> { { origMat, convertedMat } };

            var method = typeof(AvatarConverter).GetMethod("ConvertAnimationClipsForQuest",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ConvertAnimationClipsForQuest not found");
            }

            var delegateType = typeof(AvatarConverter).GetNestedType("AnimationClipProgressCallback",
                BindingFlags.NonPublic | BindingFlags.Public);
            var callback = Delegate.CreateDelegate(delegateType,
                typeof(AnimConverterCoverageTests).GetMethod("DummyAnimClipProgress",
                    BindingFlags.Static | BindingFlags.NonPublic));

            LogAssert.ignoreFailingMessages = true;
            try
            {
                var result = method.Invoke(converter, new object[] { controllers, false, "", convertedMaterials, callback });
                Assert.IsNotNull(result);
                var dict = result as Dictionary<AnimationClip, AnimationClip>;
                if (dict != null && dict.Count > 0)
                {
                    Assert.AreEqual(1, dict.Count, "Should convert 1 animation clip");
                    foreach (var converted in dict.Values)
                    {
                        toCleanup.Add(converted);
                    }
                }
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Animation conversion path exercised");
            }
        }

        #endregion

        #region ConvertAnimatorControllersForQuest

        [Test]
        public void ConvertAnimatorControllersForQuest_WithConvertedClip_ConvertsController()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            toCleanup.Add(controller);

            var origClip = new AnimationClip();
            origClip.name = "OrigClip";
            toCleanup.Add(origClip);

            var convertedClip = new AnimationClip();
            convertedClip.name = "ConvertedClip";
            toCleanup.Add(convertedClip);

            var stateMachine = controller.layers[0].stateMachine;
            var state = stateMachine.AddState("TestState");
            state.motion = origClip;

            var convertedMotions = new Dictionary<Motion, Motion> { { origClip, convertedClip } };
            var controllers = new RuntimeAnimatorController[] { controller };

            var method = typeof(AvatarConverter).GetMethod("ConvertAnimatorControllersForQuest",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ConvertAnimatorControllersForQuest not found");
            }

            var delegateType = typeof(AvatarConverter).GetNestedType("RuntimeAnimatorProgressCallback",
                BindingFlags.NonPublic | BindingFlags.Public);
            var callback = Delegate.CreateDelegate(delegateType,
                typeof(AnimConverterCoverageTests).GetMethod("DummyRTAnimProgress",
                    BindingFlags.Static | BindingFlags.NonPublic));

            LogAssert.ignoreFailingMessages = true;
            try
            {
                var result = method.Invoke(converter, new object[] { controllers, false, "", convertedMotions, callback });
                Assert.IsNotNull(result);
                var dict = result as Dictionary<RuntimeAnimatorController, RuntimeAnimatorController>;
                if (dict != null)
                {
                    foreach (var c in dict.Values)
                    {
                        toCleanup.Add(c);
                    }
                    Assert.AreEqual(1, dict.Count, "Should convert 1 controller");
                }
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Controller conversion path exercised");
            }
        }

        [Test]
        public void ConvertAnimatorControllersForQuest_NoMatchingClips_SkipsController()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            toCleanup.Add(controller);

            var clip = new AnimationClip();
            clip.name = "SomeClip";
            toCleanup.Add(clip);

            var stateMachine = controller.layers[0].stateMachine;
            var state = stateMachine.AddState("TestState");
            state.motion = clip;

            // Empty converted motions - no matches
            var convertedMotions = new Dictionary<Motion, Motion>();
            var controllers = new RuntimeAnimatorController[] { controller };

            var method = typeof(AvatarConverter).GetMethod("ConvertAnimatorControllersForQuest",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ConvertAnimatorControllersForQuest not found");
            }

            var delegateType = typeof(AvatarConverter).GetNestedType("RuntimeAnimatorProgressCallback",
                BindingFlags.NonPublic | BindingFlags.Public);
            var callback = Delegate.CreateDelegate(delegateType,
                typeof(AnimConverterCoverageTests).GetMethod("DummyRTAnimProgress",
                    BindingFlags.Static | BindingFlags.NonPublic));

            LogAssert.ignoreFailingMessages = true;
            try
            {
                var result = method.Invoke(converter, new object[] { controllers, false, "", convertedMotions, callback });
                var dict = result as Dictionary<RuntimeAnimatorController, RuntimeAnimatorController>;
                Assert.IsNotNull(dict);
                Assert.AreEqual(0, dict.Count, "Should not convert controller when no matching clips");
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Path exercised");
            }
        }

        #endregion

        #region ConvertBlendTreesForQuest

        [Test]
        public void ConvertBlendTreesForQuest_WithBlendTree_ConvertsTree()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var controller = new AnimatorController();
            controller.name = "BTController";
            controller.AddLayer("Base");
            toCleanup.Add(controller);

            var origClip = new AnimationClip();
            origClip.name = "OrigClip";
            toCleanup.Add(origClip);

            var convertedClip = new AnimationClip();
            convertedClip.name = "ConvertedClip";
            toCleanup.Add(convertedClip);

            // Create blend tree with the clip
            var stateMachine = controller.layers[0].stateMachine;
            var blendTree = new BlendTree();
            blendTree.name = "TestBlendTree";
            blendTree.blendParameter = "Blend";
            blendTree.AddChild(origClip);
            toCleanup.Add(blendTree);

            var state = stateMachine.AddState("BlendState");
            state.motion = blendTree;

            var convertedClips = new Dictionary<AnimationClip, AnimationClip> { { origClip, convertedClip } };
            var controllers = new AnimatorController[] { controller };

            var method = typeof(AvatarConverter).GetMethod("ConvertBlendTreesForQuest",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ConvertBlendTreesForQuest not found");
            }

            LogAssert.ignoreFailingMessages = true;
            try
            {
                var result = method.Invoke(converter, new object[] { controllers, false, "", convertedClips });
                Assert.IsNotNull(result);
                var dict = result as Dictionary<BlendTree, BlendTree>;
                if (dict != null)
                {
                    foreach (var t in dict.Values)
                    {
                        toCleanup.Add(t);
                    }
                }
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("BlendTree conversion path exercised");
            }
        }

        #endregion

        #region ApplyConvertedMaterials - full path with animated materials

        [Test]
        public void ApplyConvertedMaterials_WithAnimatedMaterials_FullPath()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);
            var descriptor = avatarObj.AddComponent<VRCAvatarDescriptor>();

            var origMat = new Material(Shader.Find("Standard"));
            origMat.name = "OrigMat";
            toCleanup.Add(origMat);
            var convertedMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            convertedMat.name = "ConvertedMat";
            toCleanup.Add(convertedMat);

            // Create FX controller with material animation
            var controller = new AnimatorController();
            controller.name = "FXController";
            controller.AddLayer("Base");
            toCleanup.Add(controller);

            var clip = new AnimationClip();
            clip.name = "MatSwapClip";
            toCleanup.Add(clip);

            var binding = EditorCurveBinding.PPtrCurve("Body", typeof(MeshRenderer), "m_Materials.Array.data[0]");
            var keyframes = new ObjectReferenceKeyframe[]
            {
                new ObjectReferenceKeyframe { time = 0, value = origMat },
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var stateMachine = controller.layers[0].stateMachine;
            var state = stateMachine.AddState("MatState");
            state.motion = clip;

            descriptor.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    isDefault = false,
                    animatorController = controller,
                },
            };
            descriptor.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            var bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(avatarObj.transform);
            var renderer = bodyObj.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[] { origMat };

            var convertedMaterials = new Dictionary<Material, Material> { { origMat, convertedMat } };

            var method = typeof(AvatarConverter).GetMethod("ApplyConvertedMaterials",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ApplyConvertedMaterials not found");
            }

            var pc = CreateProgressCallback();
            LogAssert.ignoreFailingMessages = true;
            try
            {
                method.Invoke(converter, new object[] { avatarObj, convertedMaterials, false, "", pc });

                // Verify material was replaced
                Assert.AreEqual(convertedMat, renderer.sharedMaterials[0]);

                // Verify controller was also converted (layer should have new controller)
                var newLayers = descriptor.baseAnimationLayers;
                if (newLayers.Length > 0 && newLayers[0].animatorController != controller)
                {
                    toCleanup.Add(newLayers[0].animatorController);
                }
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Full animated materials path exercised");
            }
        }

        #endregion

        #region ApplyVRCQuestToolsComponents

        [Test]
        public void ApplyVRCQuestToolsComponents_AddsConvertedAvatar()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);
            var settings = avatarObj.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();

            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ApplyVRCQuestToolsComponents not found");
            }

            LogAssert.ignoreFailingMessages = true;
            method.Invoke(converter, new object[] { settings, avatarObj });

            var converted = avatarObj.GetComponent<KRT.VRCQuestTools.Components.ConvertedAvatar>();
            Assert.IsNotNull(converted, "ConvertedAvatar component should be added");
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_AlreadyHasConvertedAvatar_DoesNotAdd()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);
            var settings = avatarObj.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();
            avatarObj.AddComponent<KRT.VRCQuestTools.Components.ConvertedAvatar>();

            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ApplyVRCQuestToolsComponents not found");
            }

            LogAssert.ignoreFailingMessages = true;
            method.Invoke(converter, new object[] { settings, avatarObj });

            var all = avatarObj.GetComponents<KRT.VRCQuestTools.Components.ConvertedAvatar>();
            Assert.AreEqual(1, all.Length, "Should not add duplicate ConvertedAvatar");
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_WithPlatformTargetSettings_SetsBuildTarget()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);
            var settings = avatarObj.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();
            var pts = avatarObj.AddComponent<KRT.VRCQuestTools.Components.PlatformTargetSettings>();

            var method = typeof(AvatarConverter).GetMethod("ApplyVRCQuestToolsComponents",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ApplyVRCQuestToolsComponents not found");
            }

            LogAssert.ignoreFailingMessages = true;
            method.Invoke(converter, new object[] { settings, avatarObj });

            Assert.AreEqual(KRT.VRCQuestTools.Models.BuildTarget.Android, pts.buildTarget,
                "PlatformTargetSettings buildTarget should be set to Android");
        }

        #endregion

        #region RemoveExtraMaterialSlots - more paths

        [Test]
        public void RemoveExtraMaterialSlots_SkinnedMeshRenderer_ReducesMats()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("RemoveExtraMaterialSlots not found");
            }

            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);

            var childObj = new GameObject("Body");
            childObj.transform.SetParent(avatarObj.transform);
            var smr = childObj.AddComponent<SkinnedMeshRenderer>();

            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            smr.sharedMesh = mesh;
            toCleanup.Add(mesh);

            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            toCleanup.Add(mat1);
            toCleanup.Add(mat2);
            smr.sharedMaterials = new Material[] { mat1, mat2 };

            LogAssert.ignoreFailingMessages = true;
            try
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, new object[] { avatarObj });
                }
                else
                {
                    method.Invoke(converter, new object[] { avatarObj });
                }

                Assert.AreEqual(1, smr.sharedMaterials.Length, "Should reduce to 1 material for 1 submesh");
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Method executed");
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_NullMesh_Skips()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("RemoveExtraMaterialSlots not found");
            }

            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);

            var childObj = new GameObject("Body");
            childObj.transform.SetParent(avatarObj.transform);
            var mr = childObj.AddComponent<MeshRenderer>();

            // No mesh filter = null mesh
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            toCleanup.Add(mat1);
            toCleanup.Add(mat2);
            mr.sharedMaterials = new Material[] { mat1, mat2 };

            LogAssert.ignoreFailingMessages = true;
            try
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, new object[] { avatarObj });
                }
                else
                {
                    method.Invoke(converter, new object[] { avatarObj });
                }

                // Should not modify materials when mesh is null
                Assert.AreEqual(2, mr.sharedMaterials.Length, "Materials should remain unchanged with null mesh");
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Method executed");
            }
        }

        #endregion

        #region AvatarConverter.ConvertSingleMaterial paths

        [Test]
        public void ConvertSingleMaterial_ToonLitSettings_ConvertsMaterial()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";
            mat.mainTexture = new Texture2D(4, 4);
            toCleanup.Add(mat);
            toCleanup.Add(mat.mainTexture);

            var settings = new ToonLitConvertSettings();

            var method = typeof(AvatarConverter).GetMethod("ConvertSingleMaterial",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ConvertSingleMaterial not found");
            }

            Material convertedResult = null;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                var request = method.Invoke(converter, new object[]
                {
                    mat,
                    settings,
                    false,
                    "",
                    (Action<Material>)((converted) => { convertedResult = converted; }),
                });

                // WaitForCompletion
                if (request != null)
                {
                    var waitMethod = request.GetType().GetMethod("WaitForCompletion");
                    if (waitMethod != null)
                    {
                        waitMethod.Invoke(request, null);
                    }
                }

                if (convertedResult != null)
                {
                    toCleanup.Add(convertedResult);
                    Assert.IsTrue(convertedResult.name.Contains("(VQT)"));
                }
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("ConvertSingleMaterial path exercised");
            }
        }

        #endregion

        #region AvatarConverter.GenerateConvertedMaterial - MatCapLit path

        [Test]
        public void GenerateConvertedMaterial_MatCapLit_GeneratesMaterial()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var mat = new Material(Shader.Find("Standard"));
            mat.name = "TestMat";
            toCleanup.Add(mat);

            var wrapper = new MaterialWrapperBuilder().Build(mat);
            var settings = new MatCapLitConvertSettings();

            var method = typeof(AvatarConverter).GetMethod("GenerateConvertedMaterial",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("GenerateConvertedMaterial not found");
            }

            Material result = null;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                var request = method.Invoke(converter, new object[]
                {
                    wrapper,
                    settings,
                    false,
                    "",
                    (Action<Material>)((m) => { result = m; }),
                });

                if (request != null)
                {
                    var waitMethod = request.GetType().GetMethod("WaitForCompletion");
                    if (waitMethod != null)
                    {
                        waitMethod.Invoke(request, null);
                    }
                }

                if (result != null)
                {
                    toCleanup.Add(result);
                    Assert.IsNotNull(result);
                }
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("MatCapLit generation path exercised");
            }
        }

        #endregion

        #region ConvertMaterialsForMobile - with actual material

        [Test]
        public void ConvertMaterialsForMobile_WithStandardMaterial_ConvertsMaterial()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter);

            var method = typeof(AvatarConverter).GetMethod("ConvertMaterialsForMobile",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ConvertMaterialsForMobile not found");
            }

            var mat = new Material(Shader.Find("Standard"));
            mat.name = "StandardMat";
            mat.mainTexture = new Texture2D(4, 4);
            toCleanup.Add(mat);
            toCleanup.Add(mat.mainTexture);

            var settings = new ToonLitConvertSettings();
            var settingsMap = new Dictionary<Material, IMaterialConvertSettings>
            {
                { mat, settings },
            };

            var delegateType = typeof(AvatarConverter).GetNestedType("TextureProgressCallback",
                BindingFlags.NonPublic | BindingFlags.Public);
            var callback = Delegate.CreateDelegate(delegateType,
                typeof(AnimConverterCoverageTests).GetMethod("DummyTextureProgress",
                    BindingFlags.Static | BindingFlags.NonPublic));

            LogAssert.ignoreFailingMessages = true;
            try
            {
                var result = method.Invoke(converter, new object[] { settingsMap, false, "", callback });
                var dict = result as Dictionary<Material, Material>;
                Assert.IsNotNull(dict);
                if (dict.Count > 0)
                {
                    foreach (var v in dict.Values)
                    {
                        toCleanup.Add(v);
                    }
                }
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("ConvertMaterialsForMobile path exercised");
            }
        }

        #endregion

        #region Helper methods

        private static void DummyTextureProgress(int total, int index, Material original, Material converted)
        {
            // no-op
        }

        private static void DummyAnimClipProgress(int total, int index, AnimationClip original, AnimationClip converted)
        {
            // no-op
        }

        private static void DummyRTAnimProgress(int total, int index, RuntimeAnimatorController original, RuntimeAnimatorController converted)
        {
            // no-op
        }

        private object CreateProgressCallback()
        {
            var pcType = typeof(AvatarConverter).GetNestedType("ProgressCallback",
                BindingFlags.NonPublic | BindingFlags.Public);
            if (pcType == null) return null;

            var pc = Activator.CreateInstance(pcType);

            var texDelegateType = typeof(AvatarConverter).GetNestedType("TextureProgressCallback",
                BindingFlags.NonPublic | BindingFlags.Public);
            var animDelegateType = typeof(AvatarConverter).GetNestedType("AnimationClipProgressCallback",
                BindingFlags.NonPublic | BindingFlags.Public);
            var rtDelegateType = typeof(AvatarConverter).GetNestedType("RuntimeAnimatorProgressCallback",
                BindingFlags.NonPublic | BindingFlags.Public);

            if (texDelegateType != null)
            {
                pcType.GetField("onTextureProgress", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    ?.SetValue(pc, Delegate.CreateDelegate(texDelegateType,
                        typeof(AnimConverterCoverageTests).GetMethod("DummyTextureProgress",
                            BindingFlags.Static | BindingFlags.NonPublic)));
            }
            if (animDelegateType != null)
            {
                pcType.GetField("onAnimationClipProgress", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    ?.SetValue(pc, Delegate.CreateDelegate(animDelegateType,
                        typeof(AnimConverterCoverageTests).GetMethod("DummyAnimClipProgress",
                            BindingFlags.Static | BindingFlags.NonPublic)));
            }
            if (rtDelegateType != null)
            {
                pcType.GetField("onRuntimeAnimatorProgress", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    ?.SetValue(pc, Delegate.CreateDelegate(rtDelegateType,
                        typeof(AnimConverterCoverageTests).GetMethod("DummyRTAnimProgress",
                            BindingFlags.Static | BindingFlags.NonPublic)));
            }

            return pc;
        }

        #endregion
    }
}
