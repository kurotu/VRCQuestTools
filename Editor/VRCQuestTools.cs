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
        public const string Version = "0.1.0";
    }

    internal static class MenuPaths
    {
        private const string RootMenu = "VRCQuestTools/";
        internal const string ConvertAvatarForQuest = RootMenu + "Convert Avatar for Quest";
        internal const string BlendShapesCopy = RootMenu + "BlendShapes Copy";
        internal const string AutoRemoveVertexColors = RootMenu + "Auto Remove Vertex Colors";
        internal const string UnitySettings = RootMenu + "Unity Settings";

        internal const string ContextBlendShapesCopy = "CONTEXT/SkinnedMeshRenderer/Copy BlendShape Weights";
    }

    internal enum MenuPriorities : int
    {
        ConvertAvatarForQuest = 0,
        BlendShapesCopy,
        AutoRemoveVertexColors = 100,
        UnitySettings = 200
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

    static class VRCSDKUtils
    {
        internal static bool IsAvatar(GameObject obj)
        {
            if (obj == null) { return false; }
            if (obj.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>() == null) { return false; }
            return true;
        }
    }

    static class GameObjectMenu
    {
        const string MenuPrefix = "GameObject/VRCQuestTools/";
        const string GameObjectRemoveAllVertexColors = MenuPrefix + "Remove All Vertex Colors";
        const string GameObjectConvertAvatarForQuest = MenuPrefix + "Convert Avatar For Quest";

        [MenuItem(GameObjectConvertAvatarForQuest, false)]
        static void ConvertAvatarForQuest()
        {
            VRCAvatarQuestConverterWindow.Init();
        }

        [MenuItem(GameObjectConvertAvatarForQuest, true)]
        static bool ValidateAvatarMenu()
        {
            var obj = Selection.activeGameObject;
            return VRCSDKUtils.IsAvatar(obj);
        }

        [MenuItem(GameObjectRemoveAllVertexColors)]
        static void RemoveAllVertexColors()
        {
            var obj = Selection.activeGameObject;
            VertexColorRemover.RemoveAllVertexColors(obj);
            Debug.LogFormat("[{0}] All vertex colors are removed from {1}", "VRCQuestTools", obj);
        }

        [MenuItem(GameObjectRemoveAllVertexColors, true)]
        static bool ValidateVertexColorRemover()
        {
            return Selection.activeGameObject != null;
        }
    }
}
