// <copyright file="UnitySettings.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    enum CacheServerMode
    {
        Local = 0,
        Remote,
        Disable
    }

    static class UnityQuestSettings
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

    [InitializeOnLoad]
    public class UnityQuestSettingsWindow : EditorWindow
    {
        private delegate void Action();

        static UnityQuestSettingsWindow()
        {
            EditorApplication.delayCall += DelayInit;
        }

        static void DelayInit()
        {
            EditorApplication.delayCall -= DelayInit;
            var lastVersion = VRCQuestToolsSettings.LastVersion;
            var hasUpdated = !lastVersion.Equals(VRCQuestTools.Version);
            if (hasUpdated)
            {
                VRCQuestToolsSettings.LastVersion = VRCQuestTools.Version;
            }
            var hasInvalidSettings = !UnityQuestSettings.ValidateAll();

            if (hasInvalidSettings && (VRCQuestToolsSettings.IsShowSettingsWindowOnLoadEnabled || hasUpdated))
            {
                Init();
            }
        }

        internal static void Init()
        {
            var window = GetWindow(typeof(UnityQuestSettingsWindow));
            window.Show();
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            titleContent.text = "Unity Settings for Quest";
            EditorGUILayout.LabelField("Unity Preferences", EditorStyles.boldLabel);
            var allActions = new List<Action>();

            EditorGUILayout.LabelField($"{i18n.CacheServerModeLabel}: {UnityQuestSettings.GetCacheServerMode()}");

            if (!UnityQuestSettings.ValidateCacheServerMode())
            {
                EditorGUILayout.HelpBox(i18n.CacheServerHelp, MessageType.Warning);
                allActions.Add(UnityQuestSettings.EnableLocalCacheServer);
                if (GUILayout.Button(i18n.CacheServerButtonLabel))
                {
                    UnityQuestSettings.EnableLocalCacheServer();
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"{i18n.TextureCompressionLabel}: {UnityQuestSettings.GetAndroidTextureCompression()}");
            if (!UnityQuestSettings.ValidateAndroidTextureCompression())
            {
                EditorGUILayout.HelpBox(i18n.TextureCompressionHelp, MessageType.Warning);
                allActions.Add(UnityQuestSettings.EnableAndroidASTC);
                if (GUILayout.Button(i18n.TextureCompressionButtonLabel))
                {
                    UnityQuestSettings.EnableAndroidASTC();
                }
            }

            EditorGUILayout.Space();

            if (allActions.Count >= 2)
            {
                if (GUILayout.Button(i18n.ApplyAllButtonLabel))
                {
                    foreach (var action in allActions)
                    {
                        action();
                    }
                }
            }
            else if (allActions.Count == 0)
            {
                EditorGUILayout.HelpBox(i18n.AllAppliedHelp, MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            var showOnLoad = VRCQuestToolsSettings.IsShowSettingsWindowOnLoadEnabled;
            showOnLoad = EditorGUILayout.Toggle(i18n.ShowOnStartupLabel, showOnLoad);
            VRCQuestToolsSettings.IsShowSettingsWindowOnLoadEnabled = showOnLoad;
        }
    }
}
