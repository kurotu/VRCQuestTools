// <copyright file="AvatarConverter.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
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
        /// Covert the avatar for Quest.
        /// </summary>
        /// <param name="avatar">Avatar to convert.</param>
        /// <param name="assetsDirectory">Root directory to save.</param>
        /// <param name="generateQuestTextures">Whether to generate textures.</param>
        /// <param name="maxTextureSize">Max textures size. 0 for no limits.</param>
        /// <param name="remover">ComponentRemover object.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted avatar.</returns>
        internal virtual Tuple<VRChatAvatar, string> ConvertForQuest(VRChatAvatar avatar, string assetsDirectory, bool generateQuestTextures, int maxTextureSize, ComponentRemover remover, ProgressCallback progressCallback)
        {
            // Convert materials and generate textures.
            var convertedMaterials = ConvertMaterialsForToonLit(avatar.Materials, assetsDirectory);
            if (generateQuestTextures)
            {
                var generatedTextures = GenrateToonLitTextures(avatar.Materials, assetsDirectory, maxTextureSize, progressCallback.onTextureProgress);
                foreach (var tex in generatedTextures)
                {
                    if (convertedMaterials.ContainsKey(tex.Key))
                    {
                        convertedMaterials[tex.Key].mainTexture = tex.Value;
                    }
                }
                AssetDatabase.SaveAssets();
            }

            // Duplicate the original gameobject.
            var questAvatarObject = UnityEngine.Object.Instantiate(avatar.AvatarDescriptor.gameObject);

            // Convert animator controllers and their animation clips.
            if (avatar.HasAnimatedMaterials)
            {
                var convertedAnimationClips = ConvertAnimationClipsForQuest(avatar.GetRuntimeAnimatorControllers(), assetsDirectory, convertedMaterials, progressCallback.onAnimationClipProgress);
                var convertedAnimatorControllers = ConvertAnimatorControllersForQuest(avatar.GetRuntimeAnimatorControllers(), assetsDirectory, convertedAnimationClips, progressCallback.onRuntimeAnimatorProgress);

                // Apply converted animator controllers.
#if VRC_SDK_VRCSDK3
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

            VRCSDKUtility.RemoveMissingComponentsInChildren(questAvatarObject, true);
            remover.RemoveUnsupportedComponentsInChildren(questAvatarObject, true);

            questAvatarObject.name = avatar.AvatarDescriptor.gameObject.name + " (Quest)";
            questAvatarObject.SetActive(true);
            var prefabName = $"{assetsDirectory}/{questAvatarObject.name}.prefab";
            return new Tuple<VRChatAvatar, string>(new VRChatAvatar(questAvatarObject.GetComponent<VRC_AvatarDescriptor>()), prefabName);
        }

        /// <summary>
        /// Generates textures for Toon Lit.
        /// </summary>
        /// <param name="materials">Materials to generate textures.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="maxTextureSize">Max texture size. 0 for no limits.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted textures (key: original material).</returns>
        internal Dictionary<Material, Texture2D> GenrateToonLitTextures(Material[] materials, string assetsDirectory, int maxTextureSize, TextureProgressCallback progressCallback)
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
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localId);
                    var material = MaterialWrapperBuilder.Build(m);
                    using (var image = material.GenerateToonLitImage())
                    {
                        Texture2D texture = null;
                        if (image != null)
                        {
                            if (maxTextureSize > 0 && Math.Max(image.Width, image.Height) > maxTextureSize)
                            {
                                image.Resize(maxTextureSize, maxTextureSize);
                            }
                            texture = MagickImageUtility.SaveAsAsset($"{saveDirectory}/{m.name}_from_{guid}.png", image);
                        }
                        convertedTextures.Add(m, texture);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    progressCallback(materialsToConvert.Length, i, e, m);
                    throw new InvalidOperationException();
                }
            }
            return convertedTextures;
        }

        /// <summary>
        /// Converts materials for ToonLit.
        /// </summary>
        /// <param name="materials">Materials to convert.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <returns>Converted materials (key: original material).</returns>
        internal Dictionary<Material, Material> ConvertMaterialsForToonLit(Material[] materials, string assetsDirectory)
        {
            var saveDirectory = $"{assetsDirectory}/Materials";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var materialsToConvert = materials.Where(m => !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m)).ToArray();
            var convertedMaterials = new Dictionary<Material, Material>();
            for (int i = 0; i < materialsToConvert.Length; i++)
            {
                var m = materialsToConvert[i];
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localId);
                var material = new MaterialWrapperBuilder().Build(m);
                var toonlit = material.ConvertToToonLit();
                AssetDatabase.CreateAsset(toonlit, $"{saveDirectory}/{m.name}_from_{guid}.mat");
                convertedMaterials.Add(m, toonlit);
            }
            return convertedMaterials;
        }

        /// <summary>
        /// Converts animator controllers.
        /// </summary>
        /// <param name="controllers">Controllers to convert.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="convertedAnimationClips">Converted animation clips.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted controllers (key: original controller).</returns>
        internal Dictionary<RuntimeAnimatorController, RuntimeAnimatorController> ConvertAnimatorControllersForQuest(RuntimeAnimatorController[] controllers, string assetsDirectory, Dictionary<AnimationClip, AnimationClip> convertedAnimationClips, RuntimeAnimatorProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/AnimatorControllers";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var convertedControllers = new Dictionary<RuntimeAnimatorController, RuntimeAnimatorController>();
            var index = 0;
            foreach (var controller in controllers)
            {
                progressCallback(controllers.Length, index, null, controller);
                try
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(controller, out string guid, out long localId);
                    var outFile = $"{saveDirectory}/{controller.name}_from_{guid}.controller";
                    AnimatorController cloneController = UnityAnimationUtility.ReplaceAnimationClips(controller, outFile, convertedAnimationClips);
                    convertedControllers.Add(controller, cloneController);
                }
                catch (Exception e)
                {
                    progressCallback(controllers.Length, index, e, controller);
                    throw e;
                }
                index++;
            }
            return convertedControllers;
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
                    var outFile = $"{saveDirectory}/{animationClips[i].name}_from_{guid}.anim";
                    var anim = UnityAnimationUtility.ReplaceAnimationClipMaterials(animationClips[i], convertedMaterials);

                    AssetDatabase.CreateAsset(anim, outFile);
                    convertedAnimationClips.Add(clip, anim);
                    Debug.Log("create asset: " + outFile);
                }
                catch (System.Exception e)
                {
                    progressCallback(animationClips.Length, i, e, clip);
                    throw e;
                }
            }
            return convertedAnimationClips;
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
