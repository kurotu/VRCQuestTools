// <copyright file="AvatarConverter.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if VQT_HAS_VRCSDK_BASE
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Avatar converter for Quest.
    /// </summary>
    internal class AvatarConverter
    {
        /// <summary>
        /// MaterialWrapperBuilder to use.
        /// </summary>
        internal readonly MaterialWrapperBuilder MaterialWrapperBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarConverter"/> class.
        /// </summary>
        /// <param name="materialWrapperBuilder">MaterialWrapperBuilder to use.</param>
        internal AvatarConverter(MaterialWrapperBuilder materialWrapperBuilder)
        {
            MaterialWrapperBuilder = materialWrapperBuilder;
        }

#pragma warning disable SA1600 // Elements should be documented
        internal delegate void TextureProgressCallback(int total, int index, Exception e, Material m);

        internal delegate void AnimationClipProgressCallback(int total, int index, Exception e, AnimationClip clip);

        internal delegate void RuntimeAnimatorProgressCallback(int total, int index, Exception e, RuntimeAnimatorController contoller);
#pragma warning restore SA1600 // Elements should be documented

        /// <summary>
        /// V2 of ConvertForQuest.
        /// </summary>
        /// <param name="avatarConverterSettings">Avatar converter settings component.</param>
        /// <param name="assetsDirectory">Root directory to save assets.</param>
        /// <param name="remover">ComponentRemover object.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted avatar.</returns>
        internal VRChatAvatar ConvertForQuest(AvatarConverterSettings avatarConverterSettings, string assetsDirectory, ComponentRemover remover, ProgressCallback progressCallback)
        {
            var converted = ConvertForQuestImpl(new VRChatAvatar(avatarConverterSettings.AvatarDescriptor), assetsDirectory, remover, avatarConverterSettings, progressCallback);
            var convertedConverter = converted.GameObject.GetComponent<Components.AvatarConverterSettings>();
            VRCSDKUtility.DeleteAvatarDynamicsComponents(converted, convertedConverter.physBonesToKeep, convertedConverter.physBoneCollidersToKeep, convertedConverter.contactsToKeep);

            var convertedConverterObj = convertedConverter.gameObject;
            UnityEngine.Object.DestroyImmediate(convertedConverter);
            PrefabUtility.RecordPrefabInstancePropertyModifications(convertedConverterObj);
            return converted;
        }

        /// <summary>
        /// Covert the avatar for Quest.
        /// </summary>
        /// <param name="avatar">Avatar to convert.</param>
        /// <param name="assetsDirectory">Root directory to save.</param>
        /// <param name="remover">ComponentRemover object.</param>
        /// <param name="setting">Converter setting object.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted avatar.</returns>
        internal virtual VRChatAvatar ConvertForQuestImpl(VRChatAvatar avatar, string assetsDirectory, ComponentRemover remover, AvatarConverterSettings setting, ProgressCallback progressCallback)
        {
            // Convert materials and generate textures.
            var convertedMaterials = ConvertMaterialsForAndroid(avatar.Materials, setting, assetsDirectory, progressCallback.onTextureProgress);

            // Duplicate the original gameobject by keeping instantiated prefabs.
            // https://forum.unity.com/threads/solved-duplicate-prefab-issue.778553/#post-7562128
            Selection.activeGameObject = avatar.AvatarDescriptor.gameObject;
            Unsupported.DuplicateGameObjectsUsingPasteboard();
            var questAvatarObject = Selection.activeGameObject;

            // Convert animator controllers and their animation clips.
            if (avatar.HasAnimatedMaterials || setting.animatorOverrideControllers.Count(oc => oc != null) > 0)
            {
                var convertedAnimationClips = ConvertAnimationClipsForQuest(avatar.GetRuntimeAnimatorControllers(), assetsDirectory, convertedMaterials, progressCallback.onAnimationClipProgress);

                // Inject animation override.
                foreach (var oc in setting.animatorOverrideControllers)
                {
                    if (oc == null)
                    {
                        continue;
                    }

                    var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                    oc.GetOverrides(overrides);
                    foreach (var pair in overrides)
                    {
                        if (pair.Value)
                        {
                            convertedAnimationClips[pair.Key] = pair.Value;
                        }
                    }
                }

                var convertedBlendTrees = ConvertBlendTreesForQuest(avatar.GetRuntimeAnimatorControllers().Select(c => (AnimatorController)c).ToArray(), assetsDirectory, convertedAnimationClips);
                var convertedAnimMotions = convertedAnimationClips.ToDictionary(c => (Motion)c.Key, c => (Motion)c.Value);
                var convertedTreeMotions = convertedBlendTrees.ToDictionary(c => (Motion)c.Key, c => (Motion)c.Value);
                var convertedMotions = convertedAnimMotions.Concat(convertedTreeMotions).ToDictionary(c => c.Key, c => c.Value);
                var convertedAnimatorControllers = ConvertAnimatorControllersForQuest(avatar.GetRuntimeAnimatorControllers(), assetsDirectory, convertedMotions, progressCallback.onRuntimeAnimatorProgress);

                // Apply converted animator controllers.
#if VQT_HAS_VRCSDK_BASE
                var layers = questAvatarObject.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>().baseAnimationLayers;
                for (int i = 0; i < layers.Length; i++)
                {
                    if (!layers[i].isDefault && layers[i].animatorController != null)
                    {
                        if (convertedAnimatorControllers.ContainsKey(layers[i].animatorController))
                        {
                            layers[i].animatorController = convertedAnimatorControllers[layers[i].animatorController];
                        }
                    }
                }
#endif

                foreach (var animator in questAvatarObject.GetComponentsInChildren<Animator>(true))
                {
                    if (animator.runtimeAnimatorController != null)
                    {
                        if (convertedAnimatorControllers.ContainsKey(animator.runtimeAnimatorController))
                        {
                            animator.runtimeAnimatorController = convertedAnimatorControllers[animator.runtimeAnimatorController];
                        }
                    }
                }

                if (ModularAvatarUtility.IsModularAvatarImported)
                {
                    foreach (var ma in questAvatarObject.GetComponentsInChildren(ModularAvatarUtility.MergeAnimatorType, true))
                    {
                        var proxy = new MergeAnimatorProxy(ma);
                        if (proxy.Animator != null)
                        {
                            if (convertedAnimatorControllers.ContainsKey(proxy.Animator))
                            {
                                proxy.Animator = convertedAnimatorControllers[proxy.Animator];
                            }
                        }
                    }
                }
            }

            // Apply converted materials to renderers.
            // Apply AFTER animator controllers because original animation clips overwrite sharedMaterials.
            foreach (var renderer in questAvatarObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterials = renderer.sharedMaterials.Select(m =>
                {
                    if (m == null)
                    {
                        return null;
                    }
                    if (convertedMaterials.ContainsKey(m))
                    {
                        return convertedMaterials[m];
                    }
                    return m;
                }).ToArray();
            }

            remover.RemoveUnsupportedComponentsInChildren(questAvatarObject, true);

#if VQT_HAS_VRCSDK_BASE
            var vcr = questAvatarObject.GetComponent<VertexColorRemover>();
            if (vcr == null)
            {
                questAvatarObject.AddComponent<VertexColorRemover>();
                vcr = questAvatarObject.GetComponent<VertexColorRemover>();
            }
            if (setting.removeVertexColor)
            {
                vcr.active = true;
                vcr.includeChildren = true;
                vcr.RemoveVertexColor();
            }
            else
            {
                vcr.active = false;
            }
#endif

            questAvatarObject.name = avatar.AvatarDescriptor.gameObject.name + " (Quest)";
            questAvatarObject.SetActive(true);
            var prefabName = $"{assetsDirectory}/{questAvatarObject.name}.prefab";
            return new VRChatAvatar(questAvatarObject.GetComponent<VRC_AvatarDescriptor>());
        }

        /// <summary>
        /// Generates textures for Toon Lit.
        /// </summary>
        /// <param name="materials">Materials to generate textures.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="settings">Avatar converter settings component.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted textures (key: original material).</returns>
        internal Dictionary<Material, Texture2D> GenerateAndroidTextures(Material[] materials, string assetsDirectory, AvatarConverterSettings settings, TextureProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/Textures";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var materialsToConvert = materials.Where(m => !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m)).ToArray();
            var convertedTextures = new Dictionary<Material, Texture2D>();
            for (int i = 0; i < materialsToConvert.Length; i++)
            {
                var m = materialsToConvert[i];
                progressCallback(materialsToConvert.Length, i, null, m);
                try
                {
                    var materialSetting = settings.GetMaterialConvertSettings(m);
                    switch (materialSetting)
                    {
                        case ToonLitConvertSettings toonLitConvertSettings:
                            if (toonLitConvertSettings.generateQuestTextures)
                            {
                                var tex = GenerateToonLitTexture((int)toonLitConvertSettings.maxTextureSize, toonLitConvertSettings, saveDirectory, m);
                                convertedTextures.Add(m, tex);
                            }
                            break;
                        case MaterialReplaceSettings materialReplaceSettings:
                            // don't have to generate textures
                            break;
                        default:
                            throw new InvalidProgramException($"Unhandled material convert setting: {materialSetting.GetType().Name}");
                    }
                }
                catch (Exception e)
                {
                    progressCallback(materialsToConvert.Length, i, e, m);
                    ExceptionDispatchInfo.Capture(e).Throw();
                }
            }
            return convertedTextures;
        }

        /// <summary>
        /// Converts materials and generate textures for Android.
        /// </summary>
        /// <param name="materials">Materials to convert.</param>
        /// <param name="avatarConverterSettings">Avatar converter settings component.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted materials (key: original material).</returns>
        internal Dictionary<Material, Material> ConvertMaterialsForAndroid(Material[] materials, AvatarConverterSettings avatarConverterSettings, string assetsDirectory, TextureProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/Materials";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var materialsToConvert = materials.Where(m => !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m)).ToArray();
            var convertedMaterials = new Dictionary<Material, Material>();
            for (int i = 0; i < materialsToConvert.Length; i++)
            {
                progressCallback(materialsToConvert.Length, i, null, materialsToConvert[i]);
                try
                {
                    var m = materialsToConvert[i];
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localId);
                    var material = new MaterialWrapperBuilder().Build(m);
                    var setting = avatarConverterSettings.GetMaterialConvertSettings(m);
                    Material output;
                    switch (setting)
                    {
                        case ToonLitConvertSettings toonLitConvertSettings:
                            output = material.ConvertToToonLit();
                            if (toonLitConvertSettings.generateQuestTextures)
                            {
                                var texDir = $"{assetsDirectory}/Textures";
                                var tex = GenerateToonLitTexture((int)toonLitConvertSettings.maxTextureSize, toonLitConvertSettings, texDir, m);
                                output.mainTexture = tex;
                                AssetDatabase.SaveAssets();
                            }
                            break;
                        case MaterialReplaceSettings materialReplaceSettings:
                            output = materialReplaceSettings.material;
                            break;
                        default:
                            throw new InvalidProgramException($"Unhandled material convert setting: {setting.GetType().Name}");
                    }
                    var outFile = $"{saveDirectory}/{m.name}_from_{guid}.mat";

                    if (!(setting is MaterialReplaceSettings))
                    {
                        // When the material is added into another asset, "/" is acceptable as name.
                        if (m.name.Contains("/"))
                        {
                            var dir = Path.GetDirectoryName(outFile);
                            Directory.CreateDirectory(dir);
                        }
                        output = AssetUtility.CreateAsset(output, outFile);
                    }

                    convertedMaterials.Add(m, output);
                }
                catch (Exception e)
                {
                    progressCallback(materialsToConvert.Length, i, e, materialsToConvert[i]);
                    ExceptionDispatchInfo.Capture(e).Throw();
                }
            }
            return convertedMaterials;
        }

        /// <summary>
        /// Converts animator controllers.
        /// </summary>
        /// <param name="controllers">Controllers to convert.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="convertedMotions">Converted motions.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted controllers (key: original controller).</returns>
        internal Dictionary<RuntimeAnimatorController, RuntimeAnimatorController> ConvertAnimatorControllersForQuest(RuntimeAnimatorController[] controllers, string assetsDirectory, Dictionary<Motion, Motion> convertedMotions, RuntimeAnimatorProgressCallback progressCallback)
        {
            var convertedControllers = new Dictionary<RuntimeAnimatorController, RuntimeAnimatorController>();
            for (var index = 0; index < controllers.Length; index++)
            {
                var controller = controllers[index];
                if (controller.animationClips.Where(c => c != null).Count(clip => convertedMotions.ContainsKey(clip) && convertedMotions[clip] != null) == 0)
                {
                    continue;
                }

                progressCallback(controllers.Length, index, null, controller);
                try
                {
                    AnimatorController cloneController = UnityAnimationUtility.ReplaceAnimationClips(controller, assetsDirectory, convertedMotions);
                    convertedControllers.Add(controller, cloneController);
                }
                catch (Exception e)
                {
                    progressCallback(controllers.Length, index, e, controller);
                    ExceptionDispatchInfo.Capture(e).Throw();
                }
            }
            return convertedControllers;
        }

        /// <summary>
        /// Converts blend trees.
        /// </summary>
        /// <param name="controllers">Original controllers.</param>
        /// <param name="assetsDirectory">Root directory for converted blend trees.</param>
        /// <param name="convertedAnimationClips">Converted animation clips.</param>
        /// <returns>Converted blend trees (key: original blend tree).</returns>
        internal Dictionary<BlendTree, BlendTree> ConvertBlendTreesForQuest(AnimatorController[] controllers, string assetsDirectory, Dictionary<AnimationClip, AnimationClip> convertedAnimationClips)
        {
            var trees = controllers
                .SelectMany(c => UnityAnimationUtility.GetBlendTrees(c))
                .Distinct()
                .Where(tree =>
                {
                    return convertedAnimationClips.Keys.FirstOrDefault(clip => UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip)) != null;
                })
                .ToList();
            trees.Sort((a, b) => UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(a, b) ? 1 : 0); // Sort because trees have dependency.

            var saveDirectory = $"{assetsDirectory}/BlendTrees";
            if (trees.Count > 0)
            {
                Directory.CreateDirectory(saveDirectory);
                AssetDatabase.Refresh();
            }

            var dict = new Dictionary<BlendTree, BlendTree>();
            for (var i = 0; i < trees.Count; i++)
            {
                var tree = trees[i];
                if (VRCSDKUtility.IsExampleAsset(tree))
                {
                    continue;
                }
                var newTree = UnityAnimationUtility.DeepCopyBlendTree(tree);
                var newChildren = newTree.children.Select(child =>
                {
                    switch (child.motion)
                    {
                        case BlendTree childTree:
                            if (dict.ContainsKey(childTree))
                            {
                                child.motion = dict[childTree];
                            }
                            break;
                        case AnimationClip clip:
                            if (convertedAnimationClips.ContainsKey(clip))
                            {
                                child.motion = convertedAnimationClips[clip];
                            }
                            break;
                        default:
                            // do nothing
                            break;
                    }
                    return child;
                });
                newTree.children = newChildren.ToArray();

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tree, out string guid, out long localId);
                newTree.name += $"_from_{guid}";

                var originalAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!originalAssetPath.EndsWith(".controller"))
                {
                    var asset = $"{saveDirectory}/{newTree.name}.asset";

                    // When the tree is added into another asset, "/" is acceptable as name.
                    if (newTree.name.Contains("/"))
                    {
                        var dir = Path.GetDirectoryName(asset);
                        Directory.CreateDirectory(dir);
                    }
                    newTree = AssetUtility.CreateAsset(newTree, asset);
                }
                dict.Add(tree, newTree);
            }
            return dict;
        }

        /// <summary>
        /// Converts animation clips.
        /// </summary>
        /// <param name="controllers">Controllers to convert clips.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="convertedMaterials">Converted materials.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted controllers (key: original controller).</returns>
        internal Dictionary<AnimationClip, AnimationClip> ConvertAnimationClipsForQuest(RuntimeAnimatorController[] controllers, string assetsDirectory, Dictionary<Material, Material> convertedMaterials, AnimationClipProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/Animations";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var animationClips = controllers
                .SelectMany(c => c.animationClips)
                .Where(a => a != null)
                .Distinct()
                .Where(a => UnityAnimationUtility.GetMaterials(a).Length > 0)
                .ToArray();

            var convertedAnimationClips = new Dictionary<AnimationClip, AnimationClip>();
            for (var i = 0; i < animationClips.Length; i++)
            {
                var clip = animationClips[i];
                if (VRCSDKUtility.IsProxyAnimationClip(clip))
                {
                    continue;
                }
                progressCallback(animationClips.Length, i, null, clip);
                try
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(animationClips[i], out string guid, out long localid);
                    var originalName = animationClips[i].name;
                    var outFile = $"{saveDirectory}/{originalName}_from_{guid}.anim";

                    // When the animation clip is added into another asset, "/" is acceptable as name.
                    if (originalName.Contains("/"))
                    {
                        var dir = Path.GetDirectoryName(outFile);
                        Directory.CreateDirectory(dir);
                    }
                    var anim = UnityAnimationUtility.ReplaceAnimationClipMaterials(animationClips[i], convertedMaterials);

                    anim = AssetUtility.CreateAsset(anim, outFile);
                    convertedAnimationClips.Add(clip, anim);
                    Debug.Log("create asset: " + outFile);
                }
                catch (System.Exception e)
                {
                    progressCallback(animationClips.Length, i, e, clip);
                    ExceptionDispatchInfo.Capture(e).Throw();
                }
            }
            return convertedAnimationClips;
        }

        private Texture2D GenerateToonLitTexture(int maxTextureSize, ToonLitConvertSettings settings, string saveDirectory, Material m)
        {
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localId);
            var material = MaterialWrapperBuilder.Build(m);
            using (var tex = DisposableObject.New(material.GenerateToonLitImage(settings)))
            {
                Texture2D texture = null;
                if (tex.Object != null)
                {
                    using (var disposables = new CompositeDisposable())
                    {
                        var texToWrite = tex.Object;
                        if (maxTextureSize > 0 && Math.Max(tex.Object.width, tex.Object.height) > maxTextureSize)
                        {
                            var resized = AssetUtility.ResizeTexture(tex.Object, maxTextureSize, maxTextureSize);
                            disposables.Add(DisposableObject.New(resized));
                            texToWrite = resized;
                        }

                        var outFile = $"{saveDirectory}/{m.name}_from_{guid}.png";

                        // When the texture is added into another asset, "/" is acceptable as name.
                        if (m.name.Contains("/"))
                        {
                            var dir = Path.GetDirectoryName(outFile);
                            Directory.CreateDirectory(dir);
                        }
                        texture = AssetUtility.SaveUncompressedTexture(outFile, texToWrite);
                    }
                }
                return texture;
            }
        }

#pragma warning disable SA1600 // Elements should be documented
        internal class ProgressCallback
        {
            internal TextureProgressCallback onTextureProgress;
            internal AnimationClipProgressCallback onAnimationClipProgress;
            internal RuntimeAnimatorProgressCallback onRuntimeAnimatorProgress;
        }
#pragma warning restore SA1600 // Elements should be documented
    }
}
