using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Project settings provider for VRCQuestTools.
    /// </summary>
    internal static class ProjectSettingsProvider
    {
        private const ulong MegaBytes = 1024 * 1024;

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
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

                        EditorGUIUtility.LanguageSelector();

                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            var show = EditorGUILayout.Toggle("Show UnitySettings on Load", VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled);
                            if (check.changed)
                            {
                                VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled = show;
                            }
                        }

                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            var cacheSize = EditorGUILayout.TextField("Texture Cache Size (MB)", (VRCQuestToolsSettings.TextureCacheSize / MegaBytes).ToString());
                            if (check.changed && ulong.TryParse(cacheSize, out var size))
                            {
                                VRCQuestToolsSettings.TextureCacheSize = size * MegaBytes;
                                CacheManager.Texture.Clear(VRCQuestToolsSettings.TextureCacheSize);
                            }
                        }
                    }
                    finally
                    {
                        UnityEditor.EditorGUIUtility.labelWidth = originalLabelWidth;
                    }
                },
                keywords = new string[] { "Language", "Texture", "Cache" },
            };
            return provider;
        }
    }
}
