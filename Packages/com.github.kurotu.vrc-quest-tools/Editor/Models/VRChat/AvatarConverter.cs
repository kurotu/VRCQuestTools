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
#if VQT_HAS_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if VQT_HAS_VRCSDK_BASE
using VRC.Core;
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
        internal delegate void TextureProgressCallback(int total, int index, Material original, Material converted);

        internal delegate void AnimationClipProgressCallback(int total, int index, AnimationClip original, AnimationClip converted);

        internal delegate void RuntimeAnimatorProgressCallback(int total, int index, RuntimeAnimatorController original, RuntimeAnimatorController converted);
#pragma warning restore SA1600 // Elements should be documented

        /// <summary>
        /// V2 of ConvertForQuest.
        /// </summary>
        /// <param name="avatarConverterSettings">Avatar converter settings component.</param>
        /// <param name="remover">ComponentRemover object.</param>
        /// <param name="saveAssetsAsFile">Whether to save assets as file.</param>
        /// <param name="assetsDirectory">Root directory to save assets.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted avatar.</returns>
        internal VRChatAvatar ConvertForQuest(AvatarConverterSettings avatarConverterSettings, ComponentRemover remover, bool saveAssetsAsFile, string assetsDirectory, ProgressCallback progressCallback)
        {
            // Duplicate the original gameobject by keeping instantiated prefabs.
            // https://forum.unity.com/threads/solved-duplicate-prefab-issue.778553/#post-7562128
            Selection.activeGameObject = avatarConverterSettings.AvatarDescriptor.gameObject;
            Unsupported.DuplicateGameObjectsUsingPasteboard();
            var questAvatarObject = Selection.activeGameObject;
            questAvatarObject.name = avatarConverterSettings.gameObject.name + " (Android)";

            var questAvatar = new VRChatAvatar(questAvatarObject.GetComponent<VRC_AvatarDescriptor>());
            PrepareConvertForQuestInPlace(questAvatar);
            ConvertForQuestInPlace(questAvatar, remover, saveAssetsAsFile, assetsDirectory, progressCallback);

            return questAvatar;
        }

        /// <summary>
        /// Prepare to convert the avatar for Quest in place.
        /// </summary>
        /// <param name="avatar">Avatar object to convert.</param>
        internal void PrepareConvertForQuestInPlace(VRChatAvatar avatar)
        {
            ApplyVirtualLens2Support(avatar.GameObject);
        }

        /// <summary>
        /// Prepare modular avatar components in place.
        /// </summary>
        /// <param name="avatar">Avatar object to convert.</param>
        internal void PrepareModularAvatarComponentsInPlace(VRChatAvatar avatar)
        {
#if VQT_HAS_MA_CONVERT_CONSTRAINTS
            avatar.GameObject.GetOrAddComponent<ModularAvatarConvertConstraints>();
#endif
        }

        /// <summary>
        /// Convert the avatar for Quest in place.
        /// </summary>
        /// <param name="avatar">Avatar object to convert.</param>
        /// <param name="remover">ComponentRemover object.</param>
        /// <param name="saveAssetsAsFile">Whether to save assets as file.</param>
        /// <param name="assetsDirectory">Root directory to save.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        internal void ConvertForQuestInPlace(VRChatAvatar avatar, ComponentRemover remover, bool saveAssetsAsFile, string assetsDirectory, ProgressCallback progressCallback)
        {
            var questAvatarObject = avatar.GameObject;
            questAvatarObject.SetActive(true);
            var converterSettings = questAvatarObject.GetComponent<AvatarConverterSettings>();

            // Remove extra material slots such as lilToon FakeShadow.
            var primaryRootConversion = questAvatarObject
                .GetComponents<IMaterialConversionComponent>()
                .FirstOrDefault(c => c.IsPrimaryRoot);
            if (primaryRootConversion != null && primaryRootConversion.RemoveExtraMaterialSlots)
            {
                RemoveExtraMaterialSlots(questAvatarObject);
            }

            // Convert materials and generate textures.
            var convertSettingsMap = CreateMaterialConvertSettingsMap(avatar);
            var convertedMaterials = ConvertMaterialsForAndroid(convertSettingsMap, saveAssetsAsFile, assetsDirectory, progressCallback.onTextureProgress);
            CacheManager.Texture.Clear(VRCQuestToolsSettings.TextureCacheSize);

            ApplyConvertedMaterials(questAvatarObject, convertedMaterials, saveAssetsAsFile, assetsDirectory, progressCallback);

            if (converterSettings != null)
            {
#if VQT_HAS_MA_CONVERT_CONSTRAINTS && VQT_HAS_VRCSDK_NO_PRECHECK
                if (saveAssetsAsFile && questAvatarObject.GetComponent<ModularAvatarConvertConstraints>() != null) {
                    remover.RemoveUnsupportedComponentsInChildren(questAvatarObject, true, false, new Type[] { typeof(UnityEngine.Animations.IConstraint) });
                }
                else
                {
                    remover.RemoveUnsupportedComponentsInChildren(questAvatarObject, true);
                }
#else
                remover.RemoveUnsupportedComponentsInChildren(questAvatarObject, true);
#endif
                ModularAvatarUtility.RemoveUnsupportedComponents(questAvatarObject, true);

                ApplyVRCQuestToolsComponents(converterSettings, questAvatarObject);

                var contactsToKeep = converterSettings.contactsToKeep
                    .Concat(avatar.GetLocalContactReceivers())
                    .Concat(avatar.GetLocalContactSenders())
                    .Distinct()
                    .ToArray();

                if (converterSettings.removeAvatarDynamics)
                {
                    VRCSDKUtility.DeleteAvatarDynamicsComponents(avatar, converterSettings.physBonesToKeep, converterSettings.physBoneCollidersToKeep, contactsToKeep);
                }
            }

            foreach (var component in questAvatarObject.GetComponentsInChildren<IMaterialOperatorComponent>(true))
            {
                if (component is Component c)
                {
                    UnityEngine.Object.DestroyImmediate(c);
                }
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(questAvatarObject);
        }

        /// <summary>
        /// Applies converted materials to the avatar.
        /// </summary>
        /// <param name="questAvatarObject">Target avatar.</param>
        /// <param name="convertedMaterials">Converted materials map.</param>
        /// <param name="saveAssetsAsFile">Whether to save assets as file.</param>
        /// <param name="assetsDirectory">Root directory to save.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        internal void ApplyConvertedMaterials(GameObject questAvatarObject, Dictionary<Material, Material> convertedMaterials, bool saveAssetsAsFile, string assetsDirectory, ProgressCallback progressCallback)
        {
            var avatar = new VRChatAvatar(questAvatarObject.GetComponent<VRC_AvatarDescriptor>());

            var setting = questAvatarObject.GetComponent<AvatarConverterSettings>();
            var overrideControllers = setting != null ? setting.animatorOverrideControllers.Where(oc => oc != null).ToArray() : new AnimatorOverrideController[0];

            // Convert animator controllers and their animation clips.
            if (avatar.HasAnimatedMaterials || overrideControllers.Length > 0)
            {
                var convertedAnimationClips = ConvertAnimationClipsForQuest(avatar.GetRuntimeAnimatorControllers(), saveAssetsAsFile, assetsDirectory, convertedMaterials, progressCallback.onAnimationClipProgress);

                // Inject animation override.
                foreach (var oc in overrideControllers)
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

                var convertedBlendTrees = ConvertBlendTreesForQuest(
                    avatar.GetRuntimeAnimatorControllers().Where(c => c is AnimatorController).Cast<AnimatorController>().ToArray(),
                    saveAssetsAsFile,
                    assetsDirectory,
                    convertedAnimationClips);
                var convertedAnimMotions = convertedAnimationClips.ToDictionary(c => (Motion)c.Key, c => (Motion)c.Value);
                var convertedTreeMotions = convertedBlendTrees.ToDictionary(c => (Motion)c.Key, c => (Motion)c.Value);
                var convertedMotions = convertedAnimMotions.Concat(convertedTreeMotions).ToDictionary(c => c.Key, c => c.Value);
                var convertedAnimatorControllers = ConvertAnimatorControllersForQuest(avatar.GetRuntimeAnimatorControllers(), saveAssetsAsFile, assetsDirectory, convertedMotions, progressCallback.onRuntimeAnimatorProgress);

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

#if VQT_HAS_MODULAR_AVATAR
                foreach (var ma in questAvatarObject.GetComponentsInChildren<ModularAvatarMergeAnimator>(true))
                {
                    if (ma.animator != null)
                    {
                        if (convertedAnimatorControllers.ContainsKey(ma.animator))
                        {
                            ma.animator = convertedAnimatorControllers[ma.animator];
                        }
                    }
                }
#endif
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
        }

        /// <summary>
        /// Generates textures for Toon Lit.
        /// </summary>
        /// <param name="materials">Materials to generate textures.</param>
        /// <param name="saveAsPng">Whether to save textures as png.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="settings">Avatar converter settings component.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        internal void GenerateAndroidTextures(Material[] materials, bool saveAsPng, string assetsDirectory, AvatarConverterSettings settings, TextureProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/Textures";
            if (saveAsPng)
            {
                Directory.CreateDirectory(saveDirectory);
                AssetDatabase.Refresh();
            }

            var materialsToConvert = materials.Where(m => !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m)).ToArray();
            var convertedTextures = new Dictionary<Material, Texture2D>();

            for (int i = 0; i < materialsToConvert.Length; i++)
            {
                var m = materialsToConvert[i];
                var currentIndex = i;
                void Completion()
                {
                    progressCallback(materialsToConvert.Length, currentIndex, m, null);
                }
                try
                {
                    AsyncCallbackRequest request;
                    var materialSetting = settings.GetMaterialConvertSettings(m);
                    var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                    switch (materialSetting)
                    {
                        case ToonLitConvertSettings toonLitConvertSettings:
                            if (toonLitConvertSettings.generateQuestTextures)
                            {
                                var m2 = MaterialWrapperBuilder.Build(m);
                                request = new ToonLitGenerator(toonLitConvertSettings).GenerateTextures(m2, buildTarget, saveAsPng, saveDirectory, Completion);
                            }
                            else
                            {
                                request = new ResultRequest(Completion);
                            }
                            break;
                        case MatCapLitConvertSettings matCapLitConvertSettings:
                            if (matCapLitConvertSettings.generateQuestTextures)
                            {
                                var m2 = MaterialWrapperBuilder.Build(m);
                                request = new MatCapLitGenerator(matCapLitConvertSettings).GenerateTextures(m2, buildTarget, saveAsPng, saveDirectory, Completion);
                            }
                            else
                            {
                                request = new ResultRequest(Completion);
                            }
                            break;
                        case ToonStandardConvertSettings toonStandardConvertSettings:
                            if (toonStandardConvertSettings.generateQuestTextures)
                            {
                                var m2 = MaterialWrapperBuilder.Build(m);
                                if (m2 is LilToonMaterial lil)
                                {
                                    request = new LilToonToonStandardGenerator(lil, toonStandardConvertSettings).GenerateTextures(m2, buildTarget, saveAsPng, saveDirectory, Completion);
                                }
                                else
                                {
                                    request = new GenericToonStandardGenerator(toonStandardConvertSettings).GenerateTextures(m2, buildTarget, saveAsPng, saveDirectory, Completion);
                                }
                            }
                            else
                            {
                                request = new ResultRequest(Completion);
                            }
                            break;
                        case MaterialReplaceSettings materialReplaceSettings:
                            request = new ResultRequest(Completion);
                            break;
                        default:
                            throw new InvalidProgramException($"Unhandled material convert setting: {materialSetting.GetType().Name}");
                    }
                    request.WaitForCompletion();
                }
                catch (Exception e)
                {
                    throw new MaterialConversionException("Failed to generate texture", m, e);
                }
            }
            CacheManager.Texture.Clear(VRCQuestToolsSettings.TextureCacheSize);
        }

        /// <summary>
        /// Converts materials and generate textures for Android.
        /// </summary>
        /// <param name="convertSettingsMap">Materials convert settings map.</param>
        /// <param name="saveAsFile">Whether to save materials as file.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted materials (key: original material).</returns>
        private Dictionary<Material, Material> ConvertMaterialsForAndroid(
            Dictionary<Material, IMaterialConvertSettings> convertSettingsMap,
            bool saveAsFile,
            string assetsDirectory,
            TextureProgressCallback progressCallback)
        {
            var convertedMaterials = new Dictionary<Material, Material>();

            // Get unique materials that need conversion
            var materialsToConvert = convertSettingsMap.Keys
                .Where(m => !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m))
                .Distinct()
                .ToArray();

            for (int i = 0; i < materialsToConvert.Length; i++)
            {
                var currentIndex = i;
                var material = materialsToConvert[i];
                progressCallback(materialsToConvert.Length, i, material, null);

                try
                {
                    var request = ConvertSingleMaterial(material, convertSettingsMap[material], saveAsFile, assetsDirectory, (output) =>
                    {
                        convertedMaterials[material] = output;
                        progressCallback(materialsToConvert.Length, currentIndex, material, output);
                    });
                    request.WaitForCompletion();
                }
                catch (Exception e)
                {
                    throw new MaterialConversionException("Failed to convert material", material, e);
                }
            }

            return convertedMaterials;
        }

        private void RemoveExtraMaterialSlots(GameObject questAvatarObject)
        {
            var renderers = questAvatarObject.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Cast<Renderer>()
                .Concat(questAvatarObject.GetComponentsInChildren<MeshRenderer>(true));
            foreach (var renderer in renderers)
            {
                var mesh = RendererUtility.GetSharedMesh(renderer);
                if (mesh == null)
                {
                    continue;
                }
                if (renderer.sharedMaterials.Length > mesh.subMeshCount)
                {
                    renderer.sharedMaterials = renderer.sharedMaterials.Take(mesh.subMeshCount).ToArray();
                }
            }
        }

        private Dictionary<Material, IMaterialConvertSettings> CreateMaterialConvertSettingsMap(VRChatAvatar avatar)
        {
            var convertSettingsMap = new Dictionary<Material, IMaterialConvertSettings>();

            // Apply MaterialConversionSettings
            var materialConversionSettings = avatar.GameObject.GetComponentsInChildren<MaterialConversionSettings>(true);
            foreach (var setting in materialConversionSettings)
            {
                foreach (var s in setting.additionalMaterialConvertSettings)
                {
                    if (s.targetMaterial == null)
                    {
                        throw new TargetMaterialNullException($"Target material is not set in the additionalMaterialConvertSettings of {setting.GetType().Name}", setting);
                    }
                    if (s.materialConvertSettings is MaterialReplaceSettings replaceSettings)
                    {
                        if (!VRCSDKUtility.IsMaterialAllowedForQuestAvatar(replaceSettings.material))
                        {
                            throw new InvalidReplacementMaterialException("Replacement material is not allowed for mobile avatars", setting, replaceSettings.material);
                        }
                    }
                    if (!convertSettingsMap.ContainsKey(s.targetMaterial))
                    {
                        convertSettingsMap.Add(s.targetMaterial, s.materialConvertSettings);
                    }
                }
            }

            // Apply MaterialSwap
            var materialSwaps = avatar.GameObject.GetComponentsInChildren<MaterialSwap>(true);
            foreach (var swap in materialSwaps)
            {
                foreach (var (mapping, index) in swap.materialMappings.Select((value, index) => (value, index)))
                {
                    if (mapping.originalMaterial == null)
                    {
                        throw new InvalidMaterialSwapNullException("Original material is not set in the VQT Material Swap", swap, index);
                    }

                    if (mapping.replacementMaterial == null)
                    {
                        throw new InvalidMaterialSwapNullException("Replacement material is not set in the VQT Material Swap", swap, index);
                    }

                    if (!VRCSDKUtility.IsMaterialAllowedForQuestAvatar(mapping.replacementMaterial))
                    {
                        throw new InvalidReplacementMaterialException("Replacement material is not allowed for mobile avatars", swap, mapping.replacementMaterial);
                    }

                    if (!convertSettingsMap.ContainsKey(mapping.originalMaterial))
                    {
                        convertSettingsMap.Add(mapping.originalMaterial, new MaterialReplaceSettings
                        {
                            material = mapping.replacementMaterial,
                        });
                    }
                }
            }

            // Apply AvatarConverterSettings
            var converterSettings = avatar.GameObject.GetComponent<AvatarConverterSettings>();
            if (converterSettings != null)
            {
                foreach (var setting in converterSettings.additionalMaterialConvertSettings)
                {
                    if (setting.targetMaterial == null)
                    {
                        throw new TargetMaterialNullException($"Target material is not set in the additionalMaterialConvertSettings of {converterSettings.GetType().Name}", converterSettings);
                    }
                    if (setting.materialConvertSettings is MaterialReplaceSettings replaceSettings)
                    {
                        if (!VRCSDKUtility.IsMaterialAllowedForQuestAvatar(replaceSettings.material))
                        {
                            throw new InvalidReplacementMaterialException("Replacement material is not allowed for mobile avatars", converterSettings, replaceSettings.material);
                        }
                    }
                    if (!convertSettingsMap.ContainsKey(setting.targetMaterial))
                    {
                        convertSettingsMap.Add(setting.targetMaterial, setting.materialConvertSettings);
                    }
                }
            }

            // Apply default material convert settings
            var avatarMaterials = avatar.Materials;
            var primaryConversion = avatar.GameObject
                .GetComponents<IMaterialConversionComponent>()
                .FirstOrDefault(c => c.IsPrimaryRoot);
            if (primaryConversion != null)
            {
                foreach (var material in avatarMaterials)
                {
                    var setting = primaryConversion.DefaultMaterialConvertSettings;
                    if (!convertSettingsMap.ContainsKey(material))
                    {
                        convertSettingsMap.Add(material, setting);
                    }
                }
            }

            // Omit materials that are not used in the avatar.
            convertSettingsMap = convertSettingsMap
                .Where(pair => avatarMaterials.Contains(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return convertSettingsMap;
        }

        private AsyncCallbackRequest ConvertSingleMaterial(
            Material material,
            IMaterialConvertSettings convertSettings,
            bool saveAsFile,
            string assetsDirectory,
            Action<Material> completion)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long localId);
            var materialWrapper = MaterialWrapperBuilder.Build(material);

            // Generate converted material based on settings
            return GenerateConvertedMaterial(materialWrapper, convertSettings, saveAsFile, assetsDirectory, convertedMaterial =>
            {
                // Save as asset if required
                if (saveAsFile && !(convertSettings is MaterialReplaceSettings))
                {
                    convertedMaterial = SaveMaterialAsset(
                        convertedMaterial,
                        material.name,
                        guid,
                        assetsDirectory);
                }
                completion(convertedMaterial);
            });
        }

        private AsyncCallbackRequest GenerateConvertedMaterial(
            MaterialBase material,
            IMaterialConvertSettings settings,
            bool saveAsFile,
            string assetsDirectory,
            Action<Material> completion)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var texturesPath = $"{assetsDirectory}/Textures";

            switch (settings)
            {
                case ToonLitConvertSettings toonLitSettings:
                    return new ToonLitGenerator(toonLitSettings).GenerateMaterial(material, buildTarget, saveAsFile, texturesPath, completion);
                case MatCapLitConvertSettings matCapSettings:
                    return new MatCapLitGenerator(matCapSettings).GenerateMaterial(material, buildTarget, saveAsFile, texturesPath, completion);
                case ToonStandardConvertSettings toonStandardSettings:
                    if (material is LilToonMaterial)
                    {
                        return new LilToonToonStandardGenerator((LilToonMaterial)material, toonStandardSettings).GenerateMaterial(material, buildTarget, saveAsFile, texturesPath, completion);
                    }
                    return new GenericToonStandardGenerator(toonStandardSettings).GenerateMaterial(material, buildTarget, saveAsFile, texturesPath, completion);
                case MaterialReplaceSettings replaceSettings:
                    if (replaceSettings.material == null)
                    {
                        throw new InvalidOperationException($"Replaced material is not set for {material.Material.name} in the VQT component.");
                    }
                    return new ResultRequest<Material>(replaceSettings.material, completion);
                default:
                    throw new InvalidProgramException($"Unhandled material convert setting: {settings.GetType().Name}");
            }
        }

        private Material SaveMaterialAsset(Material material, string originalName, string guid, string assetsDirectory)
        {
            var saveDirectory = $"{assetsDirectory}/Materials";
            Directory.CreateDirectory(saveDirectory);
            AssetDatabase.Refresh();

            var outFile = $"{saveDirectory}/{originalName}_from_{guid}.mat";

            // Handle nested paths in material names
            if (originalName.Contains("/"))
            {
                var dir = Path.GetDirectoryName(outFile);
                Directory.CreateDirectory(dir);
            }

            return AssetUtility.CreateAsset(material, outFile);
        }

        /// <summary>
        /// Converts animator controllers.
        /// </summary>
        /// <param name="controllers">Controllers to convert.</param>
        /// <param name="saveAsAsset">Whether to save controllers as asset.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="convertedMotions">Converted motions.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted controllers (key: original controller).</returns>
        private Dictionary<RuntimeAnimatorController, RuntimeAnimatorController> ConvertAnimatorControllersForQuest(RuntimeAnimatorController[] controllers, bool saveAsAsset, string assetsDirectory, Dictionary<Motion, Motion> convertedMotions, RuntimeAnimatorProgressCallback progressCallback)
        {
            var convertedControllers = new Dictionary<RuntimeAnimatorController, RuntimeAnimatorController>();
            for (var index = 0; index < controllers.Length; index++)
            {
                var controller = controllers[index];
                if (controller.animationClips.Where(c => c != null).Count(clip => convertedMotions.ContainsKey(clip) && convertedMotions[clip] != null) == 0)
                {
                    continue;
                }

                progressCallback(controllers.Length, index, controller, null);
                try
                {
                    RuntimeAnimatorController cloneController = null;
                    switch (controller)
                    {
                        case AnimatorController animatorController:
                            cloneController = UnityAnimationUtility.ReplaceAnimationClips(animatorController, saveAsAsset, assetsDirectory, convertedMotions);
                            break;
                        case AnimatorOverrideController overrideController:
                            cloneController = UnityAnimationUtility.ReplaceAnimationClips(overrideController, saveAsAsset, assetsDirectory, convertedMotions);
                            if (overrideController.runtimeAnimatorController)
                            {
                                ((AnimatorOverrideController)cloneController).runtimeAnimatorController = convertedControllers[overrideController.runtimeAnimatorController];
                            }
                            break;
                        default:
                            Debug.LogWarning($"Unsupported controller type: {controller.name}: {controller.GetType().Name}");
                            break;
                    }
                    if (cloneController)
                    {
                        convertedControllers.Add(controller, cloneController);
                        progressCallback(controllers.Length, index, controller, cloneController);
                    }
                }
                catch (Exception e)
                {
                    throw new AnimatorControllerConversionException("Failed to convert animator controller", controller, e);
                }
            }
            return convertedControllers;
        }

        /// <summary>
        /// Converts blend trees.
        /// </summary>
        /// <param name="controllers">Original controllers.</param>
        /// <param name="saveAsAsset">Whether to save blend trees as asset.</param>
        /// <param name="assetsDirectory">Root directory for converted blend trees.</param>
        /// <param name="convertedAnimationClips">Converted animation clips.</param>
        /// <returns>Converted blend trees (key: original blend tree).</returns>
        private Dictionary<BlendTree, BlendTree> ConvertBlendTreesForQuest(AnimatorController[] controllers, bool saveAsAsset, string assetsDirectory, Dictionary<AnimationClip, AnimationClip> convertedAnimationClips)
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
            if (trees.Count > 0 && saveAsAsset)
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

                if (saveAsAsset)
                {
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
                }
                dict.Add(tree, newTree);
            }
            return dict;
        }

        /// <summary>
        /// Converts animation clips.
        /// </summary>
        /// <param name="controllers">Controllers to convert clips.</param>
        /// <param name="saveAsAsset">Whether to save clips as asset.</param>
        /// <param name="assetsDirectory">Root directory for converted avatar.</param>
        /// <param name="convertedMaterials">Converted materials.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted controllers (key: original controller).</returns>
        private Dictionary<AnimationClip, AnimationClip> ConvertAnimationClipsForQuest(RuntimeAnimatorController[] controllers, bool saveAsAsset, string assetsDirectory, Dictionary<Material, Material> convertedMaterials, AnimationClipProgressCallback progressCallback)
        {
            var saveDirectory = $"{assetsDirectory}/Animations";
            if (saveAsAsset)
            {
                Directory.CreateDirectory(saveDirectory);
                AssetDatabase.Refresh();
            }

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
                progressCallback(animationClips.Length, i, clip, null);
                try
                {
                    var anim = UnityAnimationUtility.ReplaceAnimationClipMaterials(animationClips[i], convertedMaterials);

                    if (saveAsAsset)
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
                        anim = AssetUtility.CreateAsset(anim, outFile);
                        Debug.Log("create asset: " + outFile);
                    }
                    convertedAnimationClips.Add(clip, anim);
                    progressCallback(animationClips.Length, i, clip, anim);
                }
                catch (System.Exception e)
                {
                    throw new AnimationClipConversionException("Failed to convert animation clip", clip, e);
                }
            }
            return convertedAnimationClips;
        }

        private void ApplyVRCQuestToolsComponents(AvatarConverterSettings setting, GameObject questAvatarObject)
        {
            if (questAvatarObject.GetComponent<ConvertedAvatar>() == null)
            {
                questAvatarObject.AddComponent<ConvertedAvatar>();
            }
#if VQT_HAS_VRCSDK_BASE
            if (setting.removeVertexColor)
            {
                var vcr = questAvatarObject.GetComponent<VertexColorRemover>();
                if (vcr == null)
                {
                    vcr = questAvatarObject.AddComponent<VertexColorRemover>();
                }
                vcr.includeChildren = true;
                vcr.enabled = true;
            }
#endif

#if VQT_HAS_NDMF
            if (setting.compressExpressionsMenuIcons)
            {
                var resizer = questAvatarObject.GetComponentInChildren<MenuIconResizer>(true);
                if (resizer == null)
                {
                    resizer = questAvatarObject.AddComponent<MenuIconResizer>();
                }
                resizer.compressTextures = true;
            }
#endif

            var platformSettings = questAvatarObject.GetComponent<PlatformTargetSettings>();
            if (platformSettings != null)
            {
                platformSettings.buildTarget = BuildTarget.Android;
            }
        }

        private void ApplyVirtualLens2Support(GameObject avatar)
        {
            // Use remote only mode to force enable in order to get same result also on PC platform.
            if (VirtualLensUtility.VirtualLensSettingsType != null)
            {
                var component = avatar.GetComponentInChildren(VirtualLensUtility.VirtualLensSettingsType, true);
                if (component != null)
                {
                    var settings = new VirtualLensUtility.VirtualLensSettingsProxy(component);
                    settings.remoteOnlyMode = VirtualLensUtility.RemoteOnlyMode.ForceEnable;
                }
            }

            // Since VirtualLens2 2.10.x, it is not necessary to disable VirtualLens2 objects because there is remote only mode for mobile platform.
            // When _VirtualLens_Root exists, it is assumed that legacy VirtualLens2 is installed.
            var root = FindDescendant(avatar, "_VirtualLens_Root");
            if (root != null)
            {
                root.tag = "EditorOnly";
                root.SetActive(false);

                var origin = FindDescendant(avatar, "VirtualLensOrigin");
                if (origin != null)
                {
                    origin.tag = "EditorOnly";
                    origin.SetActive(false);
                }
            }
        }

        private GameObject FindDescendant(GameObject gameObject, string name)
        {
            var transform = gameObject.transform;
            var child = transform.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }
            for (var i = 0; i < transform.childCount; i++)
            {
                var c = transform.GetChild(i);
                var result = FindDescendant(c.gameObject, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
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
