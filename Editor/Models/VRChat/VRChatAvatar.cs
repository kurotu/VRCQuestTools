// <copyright file="VRChatAvatar.cs" company="kurotu">
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
    /// Represents and wraps VRChat avatar.
    /// </summary>
    internal class VRChatAvatar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VRChatAvatar"/> class with an existing avatar.
        /// </summary>
        /// <param name="avatar">VRChat avatar.</param>
        internal VRChatAvatar(VRC_AvatarDescriptor avatar)
        {
            AvatarDescriptor = avatar;
        }

#pragma warning disable SA1600 // Elements should be documented
        internal delegate void TextureProgressCallback(int total, int index, Exception e, Material m);

        internal delegate void AnimationClipProgressCallback(int total, int index, Exception e, AnimationClip clip);

        internal delegate void RuntimeAnimatorProgressCallback(int total, int index, Exception e, RuntimeAnimatorController contoller);
#pragma warning restore SA1600 // Elements should be documented

        /// <summary>
        /// Gets VRC_AvatarDescriptor.
        /// </summary>
        internal VRC_AvatarDescriptor AvatarDescriptor { get; }

        /// <summary>
        /// Gets all related materials.
        /// </summary>
        internal Material[] Materials => GetRendererMaterials()
                    .Concat(GetAnimatedMaterials())
                    .Distinct()
                    .ToArray();

        /// <summary>
        /// Gets a value indicating whether the avatar has materials which are changed by animation clips.
        /// </summary>
        internal bool HasAnimatedMaterials => GetAnimatedMaterials().Length > 0;

        /// <summary>
        /// Covert the avatar for Quest.
        /// </summary>
        /// <param name="assetsDirectory">Root directory to save.</param>
        /// <param name="generateQuestTextures">Whether to generate textures.</param>
        /// <param name="maxTextureSize">Max textures size. 0 for no limits.</param>
        /// <param name="remover">ComponentRemover object.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted avatar.</returns>
        internal VRChatAvatar ConvertForQuest(string assetsDirectory, bool generateQuestTextures, int maxTextureSize, ComponentRemover remover, ProgressCallback progressCallback)
        {
            // Convert materials and generate textures.
            var convertedMaterials = ConvertMaterialsForToonLit(assetsDirectory);
            if (generateQuestTextures)
            {
                var generatedTextures = GenrateToonLitTextures(assetsDirectory, maxTextureSize, progressCallback.onTextureProgress);
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
            var questAvatarObject = UnityEngine.Object.Instantiate(AvatarDescriptor.gameObject);

            // Convert animator controllers and their animation clips.
            if (HasAnimatedMaterials)
            {
                var convertedAnimationClips = ConvertAnimationClipsForQuest(assetsDirectory, convertedMaterials, progressCallback.onAnimationClipProgress);
                var convertedAnimatorControllers = ConvertAnimatorControllersForQuest(assetsDirectory, convertedAnimationClips, progressCallback.onRuntimeAnimatorProgress);

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

            questAvatarObject.name = AvatarDescriptor.gameObject.name + " (Quest)";
            questAvatarObject.SetActive(true);
            PrefabUtility.SaveAsPrefabAssetAndConnect(questAvatarObject, $"{assetsDirectory}/{questAvatarObject.name}.prefab", InteractionMode.UserAction);
            return new VRChatAvatar(questAvatarObject.GetComponent<VRC_AvatarDescriptor>());
        }

        /// <summary>
        /// Generates textures for Toon Lit.
        /// </summary>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="maxTextureSize">Max texture size. 0 for no limits.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted textures (key: original material).</returns>
        internal Dictionary<Material, Texture2D> GenrateToonLitTextures(string assetsDirectory, int maxTextureSize, TextureProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/Textures";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var materials = Materials.Where(m => !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m)).ToArray();
            var convertedTextures = new Dictionary<Material, Texture2D>();
            for (int i = 0; i < materials.Length; i++)
            {
                var m = materials[i];
                progressCallback(materials.Length, i, null, m);
                try
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localId);
                    var material = MaterialBase.Create(m);
                    using (var image = material.GenerateToonLitImage())
                    {
                        if (maxTextureSize > 0 && Math.Max(image.Width, image.Height) > maxTextureSize)
                        {
                            image.Resize(maxTextureSize, maxTextureSize);
                        }
                        var texture = MagickImageUtility.SaveAsAsset($"{saveDirectory}/{m.name}_from_{guid}.png", image);
                        convertedTextures.Add(m, texture);
                    }
                }
                catch (Exception e)
                {
                    progressCallback(materials.Length, i, e, m);
                    throw e;
                }
            }
            return convertedTextures;
        }

        private Dictionary<Material, Material> ConvertMaterialsForToonLit(string assetsDirectory)
        {
            var saveDirectory = $"{assetsDirectory}/Materials";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var materials = Materials.Where(m => !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m)).ToArray();
            var convertedMaterials = new Dictionary<Material, Material>();
            for (int i = 0; i < materials.Length; i++)
            {
                var m = materials[i];
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localId);
                var material = MaterialBase.Create(m);
                var toonlit = material.ConvertToToonLit();
                AssetDatabase.CreateAsset(toonlit, $"{saveDirectory}/{m.name}_from_{guid}.mat");
                convertedMaterials.Add(m, toonlit);
            }
            return convertedMaterials;
        }

        private Dictionary<RuntimeAnimatorController, RuntimeAnimatorController> ConvertAnimatorControllersForQuest(string assetsDirectory, Dictionary<AnimationClip, AnimationClip> convertedAnimationClips, RuntimeAnimatorProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/AnimatorControllers";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var controllers = GetRuntimeAnimatorControllers();
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

        private Dictionary<AnimationClip, AnimationClip> ConvertAnimationClipsForQuest(string assetsDirectory, Dictionary<Material, Material> convertedMaterials, AnimationClipProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/Animations";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var controllers = GetRuntimeAnimatorControllers();
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

        private Material[] GetRendererMaterials()
        {
            var renderers = AvatarDescriptor.GetComponentsInChildren<Renderer>(true);
            return renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
        }

        private Material[] GetAnimatedMaterials()
        {
            var animMats = GetRuntimeAnimatorControllers()
                .SelectMany(controller => UnityAnimationUtility.GetMaterials(controller))
                .Distinct()
                .ToArray();

            foreach (var a in animMats)
            {
                Debug.Log(a.name);
            }
            return animMats;
        }

        private RuntimeAnimatorController[] GetRuntimeAnimatorControllers()
        {
#if VRC_SDK_VRCSDK3
            // AV3 Playable Layers
            RuntimeAnimatorController[] layers = AvatarDescriptor.gameObject
                .GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>()
                .baseAnimationLayers
                .Where(obj => !obj.isDefault)
                .Select(obj => obj.animatorController)
                .ToArray();
#else
            RuntimeAnimatorController[] layers = { };
#endif

            // Animator Controller
            RuntimeAnimatorController[] avatercontrollers = AvatarDescriptor.gameObject
                .GetComponentsInChildren<Animator>()
                .Select(obj => obj.runtimeAnimatorController)
                .ToArray();

            return layers.Concat(avatercontrollers).Where(c => c != null).Distinct().ToArray();
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
