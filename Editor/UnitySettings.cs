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

    static class EditorUserSettingsKey
    {
        private const string PREFIX = "dev.kurotu.";
        public const string LAST_VERSION = PREFIX + "LastQuestToolsVersion";
        public const string DONT_SHOW_ON_LOAD = PREFIX + "DontShowOnLoad";
    }

    public class UnitySettingsWindow : EditorWindow
    {
        private delegate void Action();
        private const string FALSE = "FALSE";
        private const string TRUE = "TRUE";

        [InitializeOnLoadMethod]
        static void InitOnInstall()
        {
            var lastVersion = EditorUserSettings.GetConfigValue(EditorUserSettingsKey.LAST_VERSION) ?? "";
            if (!lastVersion.Equals(KRTQuestTools.Version))
            {
                EditorUserSettings.SetConfigValue(EditorUserSettingsKey.LAST_VERSION, KRTQuestTools.Version);
                if ((EditorUserSettings.GetConfigValue(EditorUserSettingsKey.DONT_SHOW_ON_LOAD) ?? FALSE) != TRUE)
                {
                    Init();
                }
            }
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

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            var donotshow = (EditorUserSettings.GetConfigValue(EditorUserSettingsKey.DONT_SHOW_ON_LOAD) ?? FALSE) == TRUE;
            var donotshowValue = EditorGUILayout.Toggle("Don't show on startup", donotshow) ? TRUE : FALSE;
            EditorUserSettings.SetConfigValue(EditorUserSettingsKey.DONT_SHOW_ON_LOAD, donotshowValue);
        }
    }
}
