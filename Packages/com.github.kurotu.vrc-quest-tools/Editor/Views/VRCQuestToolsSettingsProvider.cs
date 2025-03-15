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

        [SettingsProvider]
        private static SettingsProvider CreatePreferencesProvider()
        {
            var provider = new SettingsProvider("Preferences/VRCQuestTools", SettingsScope.User)
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
                                VRCQuestToolsSettings.TextureCacheSize = (ulong)cacheSize * MegaBytes;
                                CacheManager.Texture.Clear(VRCQuestToolsSettings.TextureCacheSize);
                            }
                        }

                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Texture Cache Folder", VRCQuestToolsSettings.TextureCacheFolder);
                            if (GUILayout.Button("Select", GUILayout.Width(60)))
                            {
                                var path = UnityEditor.EditorUtility.OpenFolderPanel("Select Texture Cache Folder", VRCQuestToolsSettings.TextureCacheFolder, string.Empty);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    VRCQuestToolsSettings.TextureCacheFolder = path;
                                    CacheManager.Texture.Clear(VRCQuestToolsSettings.TextureCacheSize);
                                }
                            }
                        }

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
