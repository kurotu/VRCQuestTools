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

    static class UnitySettings
    {
        private static class EditorPrefsKeys
        {
            public const string CacheServerMode = "CacheServerMode";
            public const string CompressTexturesOnImport = "kCompressTexturesOnImport";
        }

        public static bool ValidateAll()
        {
            return ValidateCacheServerMode() && ValidateAndroidTextureCompression();
        }

        public static CacheServerMode GetCacheServerMode()
        {
            var mode = EditorPrefs.GetInt(EditorPrefsKeys.CacheServerMode, (int)CacheServerMode.Disable);
            return (CacheServerMode)System.Enum.ToObject(typeof(CacheServerMode), mode);
        }

        public static bool ValidateCacheServerMode()
        {
            return GetCacheServerMode() != CacheServerMode.Disable;
        }

        public static void EnableLocalCacheServer()
        {
            EditorPrefs.SetInt(EditorPrefsKeys.CacheServerMode, (int)CacheServerMode.Local);
        }

        public static MobileTextureSubtarget GetAndroidTextureCompression()
        {
            return EditorUserBuildSettings.androidBuildSubtarget;
        }

        public static bool ValidateAndroidTextureCompression()
        {
            return GetAndroidTextureCompression() == MobileTextureSubtarget.ASTC;
        }

        public static void EnableAndroidASTC()
        {
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        }
    }

    static class KRTQuestToolsSettings
    {
        private static class Keys
        {
            private const string PREFIX = "dev.kurotu.KRTQuestTools.";
            public const string LAST_VERSION = PREFIX + "LastQuestToolsVersion";
            public const string DONT_SHOW_ON_LOAD = PREFIX + "DontShowOnLoad";
        }

        private const string FALSE = "FALSE";
        private const string TRUE = "TRUE";

        public static string GetLastVersion()
        {
            return EditorUserSettings.GetConfigValue(Keys.LAST_VERSION) ?? "";
        }

        public static void SetLastVersion(string version)
        {
            EditorUserSettings.SetConfigValue(Keys.LAST_VERSION, version);
        }

        public static bool IsDontShowOnLoadEnabled()
        {
            return (EditorUserSettings.GetConfigValue(Keys.DONT_SHOW_ON_LOAD) ?? FALSE) == TRUE;
        }

        public static void SetDontShowOnLoad(bool enabled)
        {
            var value = enabled ? TRUE : FALSE;
            EditorUserSettings.SetConfigValue(Keys.DONT_SHOW_ON_LOAD, value);
        }
    }

    public class UnitySettingsWindow : EditorWindow
    {
        private delegate void Action();

        [InitializeOnLoadMethod]
        static void InitOnLoad()
        {
            var lastVersion = KRTQuestToolsSettings.GetLastVersion();
            var hasUpdated = !lastVersion.Equals(KRTQuestTools.Version);
            if (hasUpdated)
            {
                KRTQuestToolsSettings.SetLastVersion(KRTQuestTools.Version);
            }
            var shouldShowWindow = !UnitySettings.ValidateAll();

            if (shouldShowWindow && (!KRTQuestToolsSettings.IsDontShowOnLoadEnabled() || hasUpdated))
            {
                Init();
            }
        }

        [MenuItem(KRTQuestTools.RootMenu + "Unity Settings", false, (int)MenuPriority.UnitySettings)]
        static void Init()
        {
            var window = GetWindow(typeof(UnitySettingsWindow));
            window.Show();
        }

        private void OnGUI()
        {
            titleContent.text = "KRT Unity Settings";
            EditorGUILayout.LabelField("Unity Preferences", EditorStyles.boldLabel);
            var allActions = new List<Action>();

            EditorGUILayout.LabelField($"Cache Server Mode: {UnitySettings.GetCacheServerMode()}");

            if (!UnitySettings.ValidateCacheServerMode())
            {
                EditorGUILayout.HelpBox("Cache server is not enabled. You can save time for texture compression by enabling cache server.", MessageType.Warning);
                allActions.Add(UnitySettings.EnableLocalCacheServer);
                if (GUILayout.Button("Enable Local Cache Server"))
                {
                    UnitySettings.EnableLocalCacheServer();
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"Android Texture Compression: {UnitySettings.GetAndroidTextureCompression()}");
            if (!UnitySettings.ValidateAndroidTextureCompression())
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

            var donotshow = KRTQuestToolsSettings.IsDontShowOnLoadEnabled();
            donotshow = EditorGUILayout.Toggle("Don't show on startup", donotshow);
            KRTQuestToolsSettings.SetDontShowOnLoad(donotshow);
        }
    }
}
