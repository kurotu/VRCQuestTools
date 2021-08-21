// <copyright file="UnitySettings.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEditor;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents Unity preferences and build settings.
    /// </summary>
    internal static partial class UnitySettings
    {
        /// <summary>
        /// Gets or sets default texture compression method for Android.
        /// </summary>
        internal static MobileTextureSubtarget DefaultAndroidTextureCompression
        {
            get => EditorUserBuildSettings.androidBuildSubtarget;
            set
            {
                EditorUserBuildSettings.androidBuildSubtarget = value;
            }
        }

        /// <summary>
        /// Gets or sets legacy cache server mode.
        /// </summary>
        internal static LegacyCacheServerMode LegacyCacheServerMode
        {
            get
            {
                var mode = EditorPrefs.GetInt(EditorPrefsKeys.LegacyCacheServerMode, (int)LegacyCacheServerMode.Disable);
                return (LegacyCacheServerMode)System.Enum.ToObject(typeof(LegacyCacheServerMode), mode);
            }

            set
            {
                EditorPrefs.SetInt(EditorPrefsKeys.LegacyCacheServerMode, (int)value);
            }
        }

        private static class EditorPrefsKeys
        {
            internal const string LegacyCacheServerMode = "CacheServerMode";
        }
    }
}
