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
        private const MobileTextureSubtarget RecommendedAndroidTextureCompression = MobileTextureSubtarget.ASTC;

        /// <summary>
        /// Gets legacy cache server mode.
        /// </summary>
        internal LegacyCacheServerMode LegacyCacheServerMode => UnitySettings.LegacyCacheServerMode;

        /// <summary>
        /// Gets default texuture compression for Android.
        /// </summary>
        internal MobileTextureSubtarget DefaultAndroidTextureCompression => UnitySettings.DefaultAndroidTextureCompression;

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
#if UNITY_2019_3_OR_NEWER
                // Do not check cache server on Unity 2019 (Asset Pipeline v2)
                return HasValidAndroidTextureCompression;
#else
                return HasValidAndroidTextureCompression && HasValidLegacyCacheServerMode;
#endif
            }
        }

        /// <summary>
        /// Gets a value indicating whether texture compression is valid.
        /// </summary>
        internal bool HasValidAndroidTextureCompression => DefaultAndroidTextureCompression == RecommendedAndroidTextureCompression;

        /// <summary>
        /// Gets a value indicating whether legacy cache server mode is valid.
        /// </summary>
        internal bool HasValidLegacyCacheServerMode => LegacyCacheServerMode != LegacyCacheServerMode.Disable;

        /// <summary>
        /// Apply recommended texture compression for Android.
        /// </summary>
        internal void ApplyRecommendedAndroidTextureCompression()
        {
            UnitySettings.DefaultAndroidTextureCompression = RecommendedAndroidTextureCompression;
        }

        /// <summary>
        /// Apply recommended cache server mode.
        /// </summary>
        internal void ApplyRecommendedLegacyCacheServerMode()
        {
            if (UnitySettings.LegacyCacheServerMode == LegacyCacheServerMode.Remote)
            {
                return;
            }

            UnitySettings.LegacyCacheServerMode = LegacyCacheServerMode.Local;
        }
    }
}
