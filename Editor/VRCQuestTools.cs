// <copyright file="KRTQuestTools.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    public static class VRCQuestTools
    {
        public const string Version = "0.2.0";

        static class MenuPaths
        {
            private const string RootMenu = "VRCQuestTools/";
            internal const string ConvertAvatarForQuest = RootMenu + "Convert Avatar for Quest";
            internal const string RemoveUnsupportedComponents = RootMenu + "Tools/Remove Unsupported Components";
            internal const string RemoveMissingComponents = RootMenu + "Remove Missing Components";
            internal const string BlendShapesCopy = RootMenu + "Tools/BlendShapes Copy";
            internal const string AutoRemoveVertexColors = RootMenu + "Auto Remove Vertex Colors";
            internal const string UnitySettings = RootMenu + "Unity Settings for Quest";
            internal const string CheckForUpdate = RootMenu + "Check for Update";
        }

        enum MenuPriorities : int
        {
            ConvertAvatarForQuest = 0,
            RemoveMissingComponents,
            BlendShapesCopy = 100,
            RemoveUnsupportedComponents,
            AutoRemoveVertexColors = 200,
            UnitySettings = 300,
            CheckForUpdate = 400
        }

        // Convert Avatar for Quest

        [MenuItem(MenuPaths.ConvertAvatarForQuest, false, (int)MenuPriorities.ConvertAvatarForQuest)]
        internal static void InitConvertAvatarForQuest()
        {
            if (!ValidateConvertAvatarForQuest())
            {
                Debug.LogError("[VRCQuestTools] Slected object is not an avatar");
                return;
            }
            VRCAvatarQuestConverterWindow.InitFromMenu();
        }

        [MenuItem(MenuPaths.ConvertAvatarForQuest, true)]
        internal static bool ValidateConvertAvatarForQuest()
        {
            var obj = Selection.activeGameObject;
            return VRCSDKUtils.IsAvatar(obj);
        }

        // Remove Unsupported Components

        [MenuItem(MenuPaths.RemoveUnsupportedComponents, false, (int)MenuPriorities.RemoveUnsupportedComponents)]
        internal static void RemoveUnsupportedComponents()
        {
            var obj = Selection.activeGameObject;
            Undo.SetCurrentGroupName("Remove Unsupported Components");
            VRCSDKUtils.RemoveUnsupportedComponentsInChildren(obj, true, true);
        }

        // Remove Missing Components

        [MenuItem(MenuPaths.RemoveMissingComponents, false, (int)MenuPriorities.RemoveMissingComponents)]
        internal static void RemoveMissingComponents()
        {
            var obj = Selection.activeGameObject;
            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                Undo.SetCurrentGroupName("Remove Missing Components");
                // Somehow unpacking is needed to apply changes to the scene file.
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
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

        [MenuItem(MenuPaths.CheckForUpdate, false, (int)MenuPriorities.CheckForUpdate]
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
        }

        private const string FALSE = "FALSE";
        private const string TRUE = "TRUE";

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
