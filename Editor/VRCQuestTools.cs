// <copyright file="KRTQuestTools.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using System;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    [InitializeOnLoad]
    public static class VRCQuestTools
    {
        public const string Version = "0.3.0";

        static class MenuPaths
        {
            private const string RootMenu = "VRCQuestTools/";
            internal const string ConvertAvatarForQuest = RootMenu + "Convert Avatar for Quest";
            internal const string RemoveUnsupportedComponents = RootMenu + "Tools/Remove Unsupported Components";
            internal const string RemoveMissingComponents = RootMenu + "Remove Missing Components";
            internal const string BlendShapesCopy = RootMenu + "Tools/BlendShapes Copy";
            internal const string MSMapGenerator = RootMenu + "Tools/Metallic Smoothness Map";
            internal const string AutoRemoveVertexColors = RootMenu + "Auto Remove Vertex Colors";
            internal const string UnitySettings = RootMenu + "Unity Settings for Quest";
            internal const string CheckForUpdate = RootMenu + "Check for Update";
        }

        enum MenuPriorities : int
        {
            ConvertAvatarForQuest = 600, // VRChat SDK/Splash Screen: 500
            RemoveMissingComponents,
            BlendShapesCopy = 700,
            RemoveUnsupportedComponents,
            MSMapGenerator,
            AutoRemoveVertexColors = 800,
            UnitySettings = 900,
            CheckForUpdate
        }

        static VRCQuestTools()
        {
            EditorApplication.delayCall += DelayInit;
        }

        static void DelayInit()
        {
            EditorApplication.delayCall -= DelayInit;
            var enabled = VRCQuestToolsSettings.IsAutoRemoveVertexColorsEnabled;
            Menu.SetChecked(MenuPaths.AutoRemoveVertexColors, enabled);
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
            var obj = Selection.activeGameObject;
            Undo.SetCurrentGroupName("Remove Unsupported Components");
            VRCSDKUtils.RemoveUnsupportedComponentsInChildren(obj, true, true);
        }

        // Metallic Smoothness

        [MenuItem(MenuPaths.MSMapGenerator, false, (int)MenuPriorities.MSMapGenerator)]
        internal static void MSMapGenerator()
        {
            MSMapGen.ShowWindow();
        }

        // Remove Missing Components

        [MenuItem(MenuPaths.RemoveMissingComponents, false, (int)MenuPriorities.RemoveMissingComponents)]
        internal static void RemoveMissingComponents()
        {
            var obj = Selection.activeGameObject;
            var count = VRCSDKUtils.CountMissingComponentsInChildren(obj, true);
            Debug.Log($"[VRCQuestTools] {obj.name} has {count} missing scripts in children");
            if (count == 0)
            {
                return;
            }
            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                Undo.SetCurrentGroupName("Remove Missing Components");
                // Somehow unpacking is needed to apply changes to the scene file.
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                Debug.Log($"[VRCQuestTools] {obj.name} has been unpacked");
            }
            VRCSDKUtils.RemoveMissingComponentsInChildren(obj, true);
        }

        // BlendShapes Copy

        [MenuItem(MenuPaths.BlendShapesCopy, false, (int)MenuPriorities.BlendShapesCopy)]
        static void InitBlendShapesCopy()
        {
            BlendShapesCopy.InitFromMenu();
        }

        // Auto Remove Vertex Colors

        [MenuItem(MenuPaths.AutoRemoveVertexColors, false, (int)MenuPriorities.AutoRemoveVertexColors)]
        static void ToggleVertexColorRemoverAutomator()
        {
            var enabled = !Menu.GetChecked(MenuPaths.AutoRemoveVertexColors);
            VRCQuestToolsSettings.IsAutoRemoveVertexColorsEnabled = enabled;
            Menu.SetChecked(MenuPaths.AutoRemoveVertexColors, enabled);
            VertexColorRemoverAutomator.SetAutomation(enabled);
        }

        [MenuItem(MenuPaths.UnitySettings, false, (int)MenuPriorities.UnitySettings)]
        static void UnitySettings()
        {
            UnityQuestSettingsWindow.Init();
        }

        [MenuItem(MenuPaths.RemoveMissingComponents, true)]
        [MenuItem(MenuPaths.RemoveUnsupportedComponents, true)]
        private static bool ValidateGameObjectMenu()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem(MenuPaths.CheckForUpdate, false, (int)MenuPriorities.CheckForUpdate)]
        private static void CheckForUpdate()
        {
            UpdateChecker.CheckForUpdateFromMenu();
        }

    }

    static class ContextMenu
    {
        internal const string ContextBlendShapesCopy = "CONTEXT/SkinnedMeshRenderer/Copy BlendShape Weights";
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
    }

    static class GameObjectMenu
    {
        const string MenuPrefix = "GameObject/VRCQuestTools/";
        const string GameObjectRemoveAllVertexColors = MenuPrefix + "Remove All Vertex Colors";
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

        [MenuItem(GameObjectRemoveAllVertexColors)]
        static void RemoveAllVertexColors()
        {
            var obj = Selection.activeGameObject;
            VertexColorRemover.RemoveAllVertexColors(obj);
            Debug.LogFormat("[{0}] All vertex colors are removed from {1}", "VRCQuestTools", obj);
        }

        [MenuItem(GameObjectConvertAvatarForQuest, true)]
        [MenuItem(GameObjectRemoveUnsupportedComponents, true)]
        static bool ValidateAvatarMenu()
        {
            var obj = Selection.activeGameObject;
            return VRCSDKUtils.IsAvatar(obj);
        }

        [MenuItem(GameObjectRemoveMissingComponents, true)]
        [MenuItem(GameObjectRemoveAllVertexColors, true)]
        static bool ValidateActiveGameObject()
        {
            return Selection.activeGameObject != null;
        }
    }
}
