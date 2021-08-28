// <copyright file="AvatarConverterViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for AvatarConverter.
    /// </summary>
    internal class AvatarConverterViewModel : Object
    {
        /// <summary>
        /// Target PC avatar.
        /// </summary>
        internal VRC_AvatarDescriptor targetAvatar;

        /// <summary>
        /// Root path to save converted data.
        /// </summary>
        internal string outputPath = string.Empty;

        /// <summary>
        /// Whether the converter generates textures for Quest.
        /// </summary>
        internal bool generateQuestTextures = true;

        /// <summary>
        /// Maximum texture size.
        /// </summary>
        internal TexturesSizeLimit texturesSizeLimit = TexturesSizeLimit.Max1024x1024;

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
        internal Component[] UnsupportedComponents => TargetAvatar.UnsupportedComponents;

        private VRChatAvatar TargetAvatar => new VRChatAvatar(targetAvatar);

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

            var questAvatar = TargetAvatar.ConvertForQuest(outputPath, generateQuestTextures, (int)texturesSizeLimit, progressCallback);

            if (targetAvatar.gameObject.activeInHierarchy)
            {
                Undo.RecordObject(targetAvatar.gameObject, "Disable original avatar");
                targetAvatar.gameObject.SetActive(false);
            }

            Undo.CollapseUndoOperations(undoGroup);

            return questAvatar.AvatarDescriptor.gameObject;
        }
    }
}
