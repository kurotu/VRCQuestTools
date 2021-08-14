// <copyright file="VRCQuestTools.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    [InitializeOnLoad]
    public static class VRCQuestTools
    {
        public const string Version = "0.7.0";
        internal const string GitHubRepository = "kurotu/VRCQuestTools";
        internal const string BoothURL = "https://booth.pm/ja/items/2436054";

        internal static class MenuPaths
        {
            private const string RootMenu = "VRCQuestTools/";
            internal const string ConvertAvatarForQuest = RootMenu + "Convert Avatar for Quest";
            internal const string RemoveUnsupportedComponents = RootMenu + "Tools/Remove Unsupported Components";
            internal const string RemoveMissingComponents = RootMenu + "Remove Missing Components";
            internal const string BlendShapesCopy = RootMenu + "Tools/BlendShapes Copy";
            internal const string MSMapGenerator = RootMenu + "Tools/Metallic Smoothness Map";
            internal const string AutoRemoveVertexColors = RootMenu + "Auto Remove Vertex Colors";
            internal const string UnitySettings = RootMenu + "Unity Settings for Quest";
            private const string LanguageMenu = RootMenu + "Languages/";
            internal const string LanguageAuto = LanguageMenu + "Auto (default)";
            internal const string LanguageEnglish = LanguageMenu + "English";
            internal const string LanguageJapanese = LanguageMenu + "日本語";
            internal const string CheckForUpdate = RootMenu + "Check for Update";
        }

        internal enum MenuPriorities : int
        {
            ConvertAvatarForQuest = 600, // VRChat SDK/Splash Screen: 500
            RemoveMissingComponents,
            BlendShapesCopy = 700,
            RemoveUnsupportedComponents,
            MSMapGenerator,
            AutoRemoveVertexColors = 800,
            UnitySettings = 900,
            LanguageAuto = 1000,
            LanguageEnglish,
            LanguageJapanese,
            CheckForUpdate = 1100
        }

        // Convert Avatar for Quest

        [MenuItem(MenuPaths.ConvertAvatarForQuest, false, (int)MenuPriorities.ConvertAvatarForQuest)]
        internal static void InitConvertAvatarForQuest()
        {
            VRCAvatarQuestConverterWindow.InitFromMenu();
        }

        // Remove Unsupported Components

        [MenuItem(MenuPaths.RemoveUnsupportedComponents, false, (int)MenuPriorities.RemoveUnsupportedComponents)]
        internal static void RemoveUnsupportedComponents()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var obj = Selection.activeGameObject;

            var components = VRCSDKUtils.GetUnsupportedComponentsInChildren(obj, true);
            if (components.Length == 0)
            {
                EditorUtility.DisplayDialog("VRCQuestTools", i18n.NoUnsupportedComponentsMessage(obj.name), "OK");
                return;
            }
            var message = i18n.UnsupportedRemoverConfirmationMessage(obj.name) + "\n\n" +
                string.Join("\n", components.Select(c => c.GetType()).Distinct().Select(c => $"  - {c}").OrderBy(c => c));
            if (!EditorUtility.DisplayDialog("VRCQuestTools", message, "OK", i18n.CancelLabel)) { return; }

            Undo.SetCurrentGroupName("Remove Unsupported Components");
            VRCSDKUtils.RemoveUnsupportedComponentsInChildren(obj, true, true);
        }

        // Remove Missing Components

        [MenuItem(MenuPaths.RemoveMissingComponents, false, (int)MenuPriorities.RemoveMissingComponents)]
        internal static void RemoveMissingComponents()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var obj = Selection.activeGameObject;
            var count = VRCSDKUtils.CountMissingComponentsInChildren(obj, true);
            Debug.Log($"[VRCQuestTools] {obj.name} has {count} missing scripts in children");
            if (count == 0)
            {
                EditorUtility.DisplayDialog("VRCQuestTools", i18n.NoMissingComponentsMessage(obj.name), "OK");
                return;
            }

            var needsToUnpackPrefab = PrefabUtility.IsPartOfPrefabInstance(obj);
            var message = i18n.MissingRemoverConfirmationMessage(obj.name);
            if (needsToUnpackPrefab) { message += $" ({i18n.UnpackPrefabMessage})"; }
            if (!EditorUtility.DisplayDialog("VRCQuestTools", message, "OK", i18n.CancelLabel))
            {
                return;
            }

            if (needsToUnpackPrefab)
            {
                Undo.SetCurrentGroupName("Remove Missing Components");
                // Somehow unpacking is needed to apply changes to the scene file.
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                Debug.Log($"[VRCQuestTools] {obj.name} has been unpacked");
            }
            VRCSDKUtils.RemoveMissingComponentsInChildren(obj, true);
        }

        [MenuItem(MenuPaths.RemoveMissingComponents, true)]
        [MenuItem(MenuPaths.RemoveUnsupportedComponents, true)]
        private static bool ValidateGameObjectMenu()
        {
            return Selection.activeGameObject != null;
        }
    }

    static class VRCQuestToolsSettings
    {
        private static class Keys
        {
            private const string PREFIX = "dev.kurotu.VRCQuestTools.";
            public const string LAST_VERSION = PREFIX + "LastQuestToolsVersion";
            public const string SHOW_SETTINGS_ON_LOAD = PREFIX + "ShowSettingsOnLoad";
            public const string AUTO_REMOVE_VERTEX_COLORS = PREFIX + "AutoRemoveVertexColors";
            public const string LAST_VERSION_CHECK_DATE = PREFIX + "LastVersionCheckDate";
            public const string DISPLAY_LANGUAGE = PREFIX + "DisplayLanguage";
        }

        private const string FALSE = "FALSE";
        private const string TRUE = "TRUE";
        private static DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private static void SetBooleanConfigValue(string name, bool value)
        {
            var v = value ? TRUE : FALSE;
            EditorUserSettings.SetConfigValue(name, v);
        }

        private static bool GetBooleanConfigValue(string name, bool defaultValue)
        {
            var d = defaultValue ? TRUE : FALSE;
            return (EditorUserSettings.GetConfigValue(name) ?? d) == TRUE;
        }

        public static string LastVersion
        {
            get { return EditorUserSettings.GetConfigValue(Keys.LAST_VERSION) ?? ""; }
            set { EditorUserSettings.SetConfigValue(Keys.LAST_VERSION, value); }
        }

        public static bool IsShowSettingsWindowOnLoadEnabled
        {
            get { return GetBooleanConfigValue(Keys.SHOW_SETTINGS_ON_LOAD, true); }
            set { SetBooleanConfigValue(Keys.SHOW_SETTINGS_ON_LOAD, value); }
        }

        public static bool IsAutoRemoveVertexColorsEnabled
        {
            get { return GetBooleanConfigValue(Keys.AUTO_REMOVE_VERTEX_COLORS, true); }
            set { SetBooleanConfigValue(Keys.AUTO_REMOVE_VERTEX_COLORS, value); }
        }

        public static DateTime LastVersionCheckDateTime
        {
            get
            {
                var unixTime = int.Parse(EditorUserSettings.GetConfigValue(Keys.LAST_VERSION_CHECK_DATE) ?? "0");
                return UnixEpoch.AddSeconds(unixTime);
            }
            set
            {
                var date = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
                var unixTime = (int)date.Subtract(UnixEpoch).TotalSeconds;
                EditorUserSettings.SetConfigValue(Keys.LAST_VERSION_CHECK_DATE, unixTime.ToString());
            }
        }

        internal static DisplayLanguage DisplayLanguage
        {
            get
            {
                var value = EditorUserSettings.GetConfigValue(Keys.DISPLAY_LANGUAGE);
                if (value == null) return DisplayLanguage.Auto;

                var result = Enum.TryParse(value, out DisplayLanguage language);
                if (result == false) return DisplayLanguage.Auto;

                return language;
            }
            set
            {
                var language = Enum.GetName(typeof(DisplayLanguage), value);
                EditorUserSettings.SetConfigValue(Keys.DISPLAY_LANGUAGE, language);
                _i18n = I18n.I18n.GetI18n();
            }
        }

        private static I18nBase _i18n = null;
        internal static I18nBase I18nResource
        {
            get
            {
                if (_i18n == null) { _i18n = I18n.I18n.GetI18n(); }
                return _i18n;
            }
        }
    }

    static class GameObjectMenu
    {
        const string MenuPrefix = "GameObject/VRCQuestTools/";
        internal const string GameObjectRemoveAllVertexColors = MenuPrefix + "Remove All Vertex Colors";
        const string GameObjectRemoveUnsupportedComponents = MenuPrefix + "Remove Unsupported Components";
        const string GameObjectRemoveMissingComponents = MenuPrefix + "Remove Missing Components";
        const string GameObjectConvertAvatarForQuest = MenuPrefix + "Convert Avatar For Quest";

        [MenuItem(GameObjectConvertAvatarForQuest, false, 30)]
        static void ConvertAvatarForQuest()
        {
            VRCQuestTools.InitConvertAvatarForQuest();
        }

        [MenuItem(GameObjectRemoveUnsupportedComponents, false)]
        static void RemoveUnsupportedComponents()
        {
            VRCQuestTools.RemoveUnsupportedComponents();
        }

        [MenuItem(GameObjectRemoveMissingComponents, false)]
        static void RemoveMissingComponents()
        {
            VRCQuestTools.RemoveMissingComponents();
        }

        [MenuItem(GameObjectConvertAvatarForQuest, true)]
        [MenuItem(GameObjectRemoveUnsupportedComponents, true)]
        static bool ValidateAvatarMenu()
        {
            var obj = Selection.activeGameObject;
            return VRCSDKUtils.IsAvatar(obj);
        }

        [MenuItem(GameObjectRemoveMissingComponents, true)]
        static bool ValidateActiveGameObject()
        {
            return Selection.activeGameObject != null;
        }
    }
}
