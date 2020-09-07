using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KRTQuestTools
{
    enum CacheServerMode
    {
        Local = 0,
        Remote,
        Disable
    }

    static class EditorPrefsKey
    {
        public const string CacheServerMode = "CacheServerMode";
        public const string CompressTexturesOnImport = "kCompressTexturesOnImport";
    }

    static class UnitySettings
    {
        public static CacheServerMode GetCacheServerMode()
        {
            var mode = EditorPrefs.GetInt(EditorPrefsKey.CacheServerMode, (int)CacheServerMode.Disable);
            return (CacheServerMode)System.Enum.ToObject(typeof(CacheServerMode), mode);
        }

        public static void EnableLocalCacheServer()
        {
            EditorPrefs.SetInt(EditorPrefsKey.CacheServerMode, (int)CacheServerMode.Local);
        }

        public static bool GetCompressTexturesOnImport()
        {
            return EditorPrefs.GetBool(EditorPrefsKey.CompressTexturesOnImport);
        }

        public static void DisableCompressTexturesOnImport()
        {
            EditorPrefs.SetBool(EditorPrefsKey.CompressTexturesOnImport, false);
        }

        public static MobileTextureSubtarget GetAndroidTextureCompression()
        {
            return EditorUserBuildSettings.androidBuildSubtarget;
        }

        public static void EnableAndroidASTC()
        {
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        }
    }

    public class UnitySettingsWindow : EditorWindow
    {
        private delegate void Action();
        private const string CURRENT_VERSION = "0.0.0";
        private const string LAST_VERSION_KEY = "dev.kurotu.LastQuestToolsVersion";

        [InitializeOnLoadMethod]
        static void InitOnInstall()
        {
            var lastVersion = EditorUserSettings.GetConfigValue(LAST_VERSION_KEY);
            if (!lastVersion.Equals(CURRENT_VERSION))
            {
                Init();
            }
            EditorUserSettings.SetConfigValue(LAST_VERSION_KEY, CURRENT_VERSION);
        }

        [MenuItem("KRTQuestTools/Unity Settings")]
        static void Init()
        {
            var window = GetWindow(typeof(UnitySettingsWindow));
            window.Show();
        }

        private void OnGUI()
        {
            titleContent.text = "Unity Settings";
            EditorGUILayout.LabelField("Unity Preferences", EditorStyles.boldLabel);
            var allActions = new List<Action>();

            EditorGUILayout.LabelField($"Cache Server Mode: {UnitySettings.GetCacheServerMode()}");
            EditorGUILayout.LabelField($"Compress Assets on Import: {(UnitySettings.GetCompressTexturesOnImport() ? "Enable" : "Disable") }");

            if (UnitySettings.GetCacheServerMode() == CacheServerMode.Disable)
            {
                EditorGUILayout.HelpBox("Cache server is not enabled. You can save time for switching platform by enabling cache server.", MessageType.Warning);
                allActions.Add(UnitySettings.EnableLocalCacheServer);
                if (GUILayout.Button("Enable Local Cache Server"))
                {
                    UnitySettings.EnableLocalCacheServer();
                }
            }

            if (UnitySettings.GetCompressTexturesOnImport())
            {
                EditorGUILayout.HelpBox("\"Compress Assets on Import\" is enabled. You can postpone compression on build.", MessageType.Warning);
                allActions.Add(UnitySettings.DisableCompressTexturesOnImport);
                if (GUILayout.Button("Disable \"Compress Assets on Import\""))
                {
                    UnitySettings.DisableCompressTexturesOnImport();
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"Android Texture Compression: {UnitySettings.GetAndroidTextureCompression()}");
            if (UnitySettings.GetAndroidTextureCompression() != MobileTextureSubtarget.ASTC)
            {
                EditorGUILayout.HelpBox("\"Texture compress is not ASTC. ASTC improves texture quality.", MessageType.Warning);
                allActions.Add(UnitySettings.EnableAndroidASTC);
                if (GUILayout.Button("Set texture compression to ASTC"))
                {
                    UnitySettings.EnableAndroidASTC();
                }
            }

            EditorGUILayout.Space();

            if (allActions.Count >= 2)
            {
                if (GUILayout.Button("Apply All Settings"))
                {
                    foreach (var action in allActions)
                    {
                        action();
                    }
                }
            }
            else if (allActions.Count == 0)
            {
                EditorGUILayout.HelpBox("OK, all recommended settings are applied.", MessageType.Info);
            }
        }
    }
}
