using UnityEditor;

namespace KRTQuestTools
{
    public static class KRTQuestTools
    {
        public const string Version = "0.0.0";
    }

    internal static class MenuPaths
    {
        private const string RootMenu = "KRTQuestTools/";
        internal const string ConvertAvatarForQuest = RootMenu + "Convert Avatar For Quest";
        internal const string BlendShapesCopy = RootMenu + "BlendShapes Copy";
        internal const string AutoRemoveVertexColors = RootMenu + "Auto Remove Vertex Colors";
        internal const string UnitySettings = RootMenu + "Unity Settings";

        internal const string GameObjectRemoveAllVertexColors = "GameObject/Remove All Vertex Colors";
        internal const string GameObjectConvertAvatarForQuest = "GameObject/Convert Avatar For Quest";
        internal const string ContextBlendShapesCopy = "CONTEXT/SkinnedMeshRenderer/Copy BlendShape Weights";
    }

    internal enum MenuPriorities : int
    {
        ConvertAvatarForQuest = 0,
        BlendShapesCopy,
        AutoRemoveVertexColors = 100,
        UnitySettings = 200
    }

    static class KRTQuestToolsSettings
    {
        private static class Keys
        {
            private const string PREFIX = "dev.kurotu.KRTQuestTools.";
            public const string LAST_VERSION = PREFIX + "LastQuestToolsVersion";
            public const string DONT_SHOW_ON_LOAD = PREFIX + "DontShowOnLoad";
            public const string AUTO_REMOVE_VERTEX_COLORS = PREFIX + "AutoRemoveVertexColors";
        }

        private const string FALSE = "FALSE";
        private const string TRUE = "TRUE";
        private static string GetStringValue(bool boolean)
        {
            return boolean ? TRUE : FALSE;
        }

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
            var value = GetStringValue(enabled);
            EditorUserSettings.SetConfigValue(Keys.DONT_SHOW_ON_LOAD, value);
        }

        public static bool IsAutoRemoveVertexColorsEnabled()
        {
            var defaultValue = TRUE;
            return (EditorUserSettings.GetConfigValue(Keys.AUTO_REMOVE_VERTEX_COLORS) ?? defaultValue) == TRUE;
        }

        public static void SetAutoRemoveVertexColors(bool enabled)
        {
            var value = GetStringValue(enabled);
            EditorUserSettings.SetConfigValue(Keys.AUTO_REMOVE_VERTEX_COLORS, value);
        }
    }
}
