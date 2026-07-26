// <copyright file="VRCQuestToolsSettingsProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Create settings providers for VRCQuestTools.
    /// </summary>
    internal static class VRCQuestToolsSettingsProvider
    {
        private const ulong MegaBytes = 1024 * 1024;

        /// <summary>
        /// Largest value accepted in the texture cache size field, in megabytes.
        /// </summary>
        private static readonly int MaxCacheSizeMegaBytes = (int)(VRCQuestToolsSettings.MaxTextureCacheSize / MegaBytes);

        [SettingsProvider]
        private static SettingsProvider CreateProjectSettingsProvider()
        {
            var provider = new SettingsProvider("Project/VRCQuestTools", SettingsScope.Project)
            {
                label = "VRCQuestTools",
                guiHandler = (searchContext) =>
                {
                    var originalLabelWidth = UnityEditor.EditorGUIUtility.labelWidth;
                    try
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = 200;

                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            var cacheSize = EditorGUILayout.IntField("Texture Cache Size (MB)", (int)(VRCQuestToolsSettings.TextureCacheSize / MegaBytes));
                            if (check.changed)
                            {
                                // Clamped before the unsigned cast, which is where an out-of-range value does its
                                // damage: a negative megabyte count would wrap around into a limit of nearly
                                // 2^64 bytes and silently disable eviction entirely, and a large positive one
                                // would overflow while being scaled to bytes. The field shows the clamped value
                                // back on the next repaint, since it is read from the stored setting.
                                cacheSize = Mathf.Clamp(cacheSize, 0, MaxCacheSizeMegaBytes);
                                VRCQuestToolsSettings.TextureCacheSize = (ulong)cacheSize * MegaBytes;
                                CacheManager.Texture.Clear(VRCQuestToolsSettings.TextureCacheSize);
                            }
                        }

                        EditorGUILayout.LabelField("Texture Cache Folder", VRCQuestToolsSettings.TextureCacheFolder);

                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(UnityEditor.EditorGUIUtility.labelWidth);
                            if (GUILayout.Button("Open Cache Folder", GUILayout.Width(150)))
                            {
                                SystemUtility.OpenFolder(VRCQuestToolsSettings.TextureCacheFolder);
                            }
                            if (GUILayout.Button("Clear Cache", GUILayout.Width(100)))
                            {
                                CacheManager.Texture.Clear();
                            }
                        }

                        EditorGUILayout.Space();

                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(UnityEditor.EditorGUIUtility.labelWidth);
                            if (GUILayout.Button("Reset to Default", GUILayout.Width(150)))
                            {
                                VRCQuestToolsSettings.ResetPreferences();
                            }
                        }
                    }
                    finally
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = originalLabelWidth;
                    }
                },
                keywords = new string[] { "Texture", "Cache" },
            };
            return provider;
        }
    }
}
