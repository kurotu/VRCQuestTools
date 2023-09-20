// <copyright file="AvatarConverterViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for AvatarConverter.
    /// </summary>
    [Serializable]
    internal class AvatarConverterViewModel
    {
        /// <summary>
        /// Root path to save converted data.
        /// </summary>
        public string outputPath = string.Empty;

        /// <summary>
        /// Whether the converter generates textures for Quest.
        /// </summary>
        public bool generateQuestTextures = true;

        /// <summary>
        /// Maximum texture size.
        /// </summary>
        public TexturesSizeLimit texturesSizeLimit = TexturesSizeLimit.Max1024x1024;

        /// <summary>
        /// Main texture level.
        /// </summary>
        public float mainTextureBrightness = 0.83f; // 1 / 1.2 -> 0.8333...

        /// <summary>
        /// Whether the converter attaches VertexColorRemover to the converted avatar.
        /// </summary>
        public bool removeVertexColor = true;

        /// <summary>
        /// Animator Override Controller for base layers.
        /// </summary>
        public AnimatorOverrideController[] overrideControllers = new AnimatorOverrideController[] { };

        /// <summary>
        /// AvatarConverter to use.
        /// </summary>
        [NonSerialized]
        internal AvatarConverter AvatarConverter;

        /// <summary>
        /// VRC_AvatarDescriptor game object.
        /// Unity can't serialize VRC_AvatarDescriptor well, so keep as a game object.
        /// </summary>
        [SerializeField]
        private GameObject targetAvatarObject;

        /// <summary>
        /// Callback delegate to check overwriting.
        /// </summary>
        /// <returns>Return true to continue.</returns>
        internal delegate bool CanOverwriteCallback();

        /// <summary>
        /// Maximum textures size.
        /// </summary>
        internal enum TexturesSizeLimit
        {
            /// <summary>
            /// Do not resize textures.
            /// </summary>
            NoLimits = 0,

#pragma warning disable SA1602 // Enumeration items should be documented
            Max256x256 = 256,
            Max512x512 = 512,
            Max1024x1024 = 1024,
            Max2048x2048 = 2048,
#pragma warning restore SA1602 // Enumeration items should be documented
        }

        /// <summary>
        /// Gets or sets target PC avatar's VRC_AvatarDescriptor.
        /// </summary>
        internal VRC_AvatarDescriptor TargetAvatarDescriptor
        {
            get
            {
                if (targetAvatarObject == null)
                {
                    return null;
                }
                return targetAvatarObject.GetComponent<VRC_AvatarDescriptor>();
            }

            set
            {
                targetAvatarObject = value?.gameObject;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the output path contains invalid characters.
        /// </summary>
        internal bool HasInvalidCharsInOutputPath
        {
            get
            {
                var components = outputPath.Split('/');
                return components.FirstOrDefault(c => c.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) != null;
            }
        }

        /// <summary>
        /// Gets materials which are not verified in VRCQuestTools.
        /// </summary>
        internal Material[] UnverifiedShaderMaterials => TargetAvatar.Materials
            .Where(m => AvatarConverter.MaterialWrapperBuilder.DetectShaderCategory(m) == MaterialWrapperBuilder.ShaderCategory.Unverified)
            .ToArray();

        /// <summary>
        /// Gets a value indicating whether there are materials which are changed by animation clips.
        /// </summary>
        internal bool HasAnimatedMaterials => TargetAvatar.HasAnimatedMaterials;

        /// <summary>
        /// Gets a value indicating whether the avatar has Dynamic Bones.
        /// </summary>
        internal bool HasDynamicBones
        {
            get
            {
                if (AssetUtility.IsDynamicBoneImported())
                {
                    var dbs = TargetAvatar.GameObject.GetComponentsInChildren(AssetUtility.DynamicBoneType, true);
                    return dbs.Length > 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the avatar has missing network IDs.
        /// </summary>
        internal bool HasMissingNetIDs => VRCSDKUtility.HasMissingNetworkIds(TargetAvatarDescriptor);

        /// <summary>
        /// Gets GameObjects which have multiple PhysBones.
        /// </summary>
        internal GameObject[] GameObjectsWithMultiplePhysBones
        {
            get
            {
                var pbs = TargetAvatarDescriptor.GetComponentsInChildren(VRCSDKUtility.PhysBoneType, true);
                var multiPbObjs = pbs
                    .Select(pb => pb.gameObject)
                    .Where(go => go.GetComponents(VRCSDKUtility.PhysBoneType).Count() >= 2)
                    .Distinct()
                    .ToArray();
                return multiPbObjs;
            }
        }

        /// <summary>
        /// Gets unsupported components for Quest.
        /// </summary>
        internal Component[] UnsupportedComponents => Remover.GetUnsupportedComponentsInChildren(TargetAvatar.AvatarDescriptor.gameObject, true);

        /// <summary>
        /// Gets a value indicating whether overrideControllers contains animated materials which uses unsupported shaders for Quest.
        /// </summary>
        internal bool OverrideControllersHasUnsupportedMaterials => overrideControllers
            .Where(oc => oc != null)
            .FirstOrDefault(oc =>
            {
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                oc.GetOverrides(overrides);
                return overrides
                    .Where(pair => pair.Value != null)
                    .SelectMany(pair => UnityAnimationUtility.GetMaterials(pair.Value))
                    .Where(m => m != null)
                    .FirstOrDefault(m => !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m)) != null;
            }) != null;

        /// <summary>
        /// Gets a value indicating whether the converter can convert the target avatar.
        /// </summary>
        internal bool CanConvertAvatar
        {
            get
            {
                var hasTarget = TargetAvatarDescriptor != null;
                return hasTarget && !HasInvalidCharsInOutputPath && !OverrideControllersHasUnsupportedMaterials;
            }
        }

        private VRChatAvatar TargetAvatar => new VRChatAvatar(TargetAvatarDescriptor);

        private ComponentRemover Remover => VRCQuestTools.ComponentRemover;

        /// <summary>
        /// Update textures.
        /// </summary>
        /// <param name="progressCallback">Callback to show progress.</param>
        internal void UpdateTextures(AvatarConverter.TextureProgressCallback progressCallback)
        {
            var setting = new TextureGeneratorSetting
            {
                MainTextureBrightness = mainTextureBrightness,
            };
            AvatarConverter.GenrateToonLitTextures(TargetAvatar.Materials, outputPath, (int)texturesSizeLimit, setting, progressCallback);
        }

        /// <summary>
        /// Convert the target avatar for Quest.
        /// </summary>
        /// <param name="canOverwrite">Callback to check overwriting.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted avatar's game object.</returns>
        internal GameObject ConvertAvatar(CanOverwriteCallback canOverwrite, AvatarConverter.ProgressCallback progressCallback)
        {
            if (AssetDatabase.IsValidFolder(outputPath))
            {
                if (!canOverwrite())
                {
                    return null;
                }
            }

            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Convert Avatar for Quest");

            var converterSetting = new AvatarConverterSetting
            {
                generateQuestTextures = generateQuestTextures,
                maxTextureSize = (int)texturesSizeLimit,
                mainTextureBrightness = mainTextureBrightness,
                removeVertexColor = removeVertexColor,
                overrideControllers = overrideControllers,
            };
            var (questAvatar, prefabName) = AvatarConverter.ConvertForQuest(TargetAvatar, outputPath, Remover, converterSetting, progressCallback);

            /*
             * If missing references exist in a prefab instance, unexpected references to prefab happen at play mode.
             * So do not create prefabs at moment. To use prefabs in script, we need to carefully handle property override.
             *
             * PrefabUtility.SaveAsPrefabAssetAndConnect(questAvatar.AvatarDescriptor.gameObject, prefabName, InteractionMode.UserAction);
             */

            if (TargetAvatarDescriptor.gameObject.activeInHierarchy)
            {
                Undo.RecordObject(TargetAvatarDescriptor.gameObject, "Disable original avatar");
                TargetAvatarDescriptor.gameObject.SetActive(false);
            }

            Undo.CollapseUndoOperations(undoGroup);

            return questAvatar.AvatarDescriptor.gameObject;
        }

        /// <summary>
        /// Convert Bynamic Bones to PhysBones via menu item.
        /// </summary>
        internal void ConvertDynamicBonesToPhysBones()
        {
            Selection.activeGameObject = targetAvatarObject;
            EditorApplication.ExecuteMenuItem("VRChat SDK/Utilities/Convert DynamicBones To PhysBones");
        }

        /// <summary>
        /// Assign network IDs to PhysBones via menu item.
        /// </summary>
        internal void AssignNetIdsToPhysBones()
        {
            VRCSDKUtility.AssignNetworkIdsToPhysBones(TargetAvatarDescriptor);
        }
    }
}
