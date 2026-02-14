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
        /// Gets a value indicating whether the Android build support is installed.
        /// </summary>
        internal static bool HasAndroidBuildSupport
        {
            get
            {
                return BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, UnityEditor.BuildTarget.Android);
            }
        }

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

        private static class EditorPrefsKeys
        {
            internal const string LegacyCacheServerMode = "CacheServerMode";
        }
    }
}
