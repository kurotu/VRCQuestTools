// <copyright file="AvatarConverterViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
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
        /// Gets materials which are not verified in VRCQuestTools.
        /// </summary>
        internal Material[] UnverifiedShaderMaterials => TargetAvatar.Materials
            .Where(m => MaterialBase.DetectShaderCategory(m) == MaterialBase.ShaderCategory.Unverified)
            .ToArray();

        /// <summary>
        /// Gets a value indicating whether there are materials which are changed by animation clips.
        /// </summary>
        internal bool HasAnimatedMaterials => TargetAvatar.HasAnimatedMaterials;

        /// <summary>
        /// Gets unsupported components for Quest.
        /// </summary>
        internal Component[] UnsupportedComponents => Remover.GetUnsupportedComponentsInChildren(TargetAvatar.AvatarDescriptor.gameObject, true);

        private VRChatAvatar TargetAvatar => new VRChatAvatar(TargetAvatarDescriptor);

        private ComponentRemover Remover => VRCQuestTools.ComponentRemover;

        /// <summary>
        /// Update textures.
        /// </summary>
        /// <param name="progressCallback">Callback to show progress.</param>
        internal void UpdateTextures(VRChatAvatar.TextureProgressCallback progressCallback)
        {
            TargetAvatar.GenrateToonLitTextures(outputPath, (int)texturesSizeLimit, progressCallback);
        }

        /// <summary>
        /// Convert the target avatar for Quest.
        /// </summary>
        /// <param name="canOverwrite">Callback to check overwriting.</param>
        /// <param name="progressCallback">Callback to show progress.</param>
        /// <returns>Converted avatar's game object.</returns>
        internal GameObject ConvertAvatar(CanOverwriteCallback canOverwrite, VRChatAvatar.ProgressCallback progressCallback)
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

            var questAvatar = TargetAvatar.ConvertForQuest(outputPath, generateQuestTextures, (int)texturesSizeLimit, Remover, progressCallback);

            if (TargetAvatarDescriptor.gameObject.activeInHierarchy)
            {
                Undo.RecordObject(TargetAvatarDescriptor.gameObject, "Disable original avatar");
                TargetAvatarDescriptor.gameObject.SetActive(false);
            }

            Undo.CollapseUndoOperations(undoGroup);

            return questAvatar.AvatarDescriptor.gameObject;
        }
    }
}
