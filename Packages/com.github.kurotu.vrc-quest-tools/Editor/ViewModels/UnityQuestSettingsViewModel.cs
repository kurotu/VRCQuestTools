// <copyright file="UnityQuestSettingsViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using UnityEditor;

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for UnityQuestSettings.
    /// </summary>
    [Serializable]
    internal class UnityQuestSettingsViewModel
    {
        private const MobileTextureSubtarget RecommendedMobileTextureCompression = MobileTextureSubtarget.ASTC;

        /// <summary>
        /// Gets default texture compression for Mobile.
        /// </summary>
        internal MobileTextureSubtarget DefaultMobileTextureCompression => UnitySettings.DefaultMobileTextureCompression;

        /// <summary>
        /// Gets or sets a value indicating whether a window appears on editor loaded.
        /// </summary>
        internal bool ShowWindowOnLoad
        {
            get => VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled;
            set
            {
                VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether all settings are valid.
        /// </summary>
        internal bool AllSettingsValid
        {
            get
            {
                // Do not check cache server on Unity 2019 (Asset Pipeline v2)
                return HasValidMobileTextureCompression && HasMobileBuildSupport;
            }
        }

        /// <summary>
        /// Gets a value indicating whether texture compression is valid.
        /// </summary>
        internal bool HasValidMobileTextureCompression => DefaultMobileTextureCompression == RecommendedMobileTextureCompression;

        /// <summary>
        /// Gets a value indicating whether Mobile build support is installed.
        /// </summary>
        internal bool HasMobileBuildSupport => UnitySettings.HasMobileBuildSupport;

        /// <summary>
        /// Apply recommended texture compression for Mobile.
        /// </summary>
        internal void ApplyRecommendedMobileTextureCompression()
        {
            UnitySettings.DefaultMobileTextureCompression = RecommendedMobileTextureCompression;
        }
    }
}
