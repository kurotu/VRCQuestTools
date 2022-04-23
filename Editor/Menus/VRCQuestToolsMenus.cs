// <copyright file="VRCQuestToolsMenus.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1602 // Enumeration items should be documented

using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    internal static class VRCQuestToolsMenus
    {
        internal static class MenuPaths
        {
            internal const string RootMenu = VRCQuestTools.Name + "/";
            internal const string ConvertAvatarForQuest = RootMenu + "Convert Avatar for Quest";
            internal const string RemoveUnsupportedComponents = RootMenu + "Tools/Remove Unsupported Components";
            internal const string RemoveMissingComponents = RootMenu + "Remove Missing Components";
            internal const string RemovePhysBones = RootMenu + "Remove PhysBones";
            internal const string BlendShapesCopy = RootMenu + "Tools/BlendShapes Copy";
            internal const string MSMapGenerator = RootMenu + "Tools/Metallic Smoothness Map";
            internal const string AutoRemoveVertexColors = RootMenu + "Auto Remove Vertex Colors";
            internal const string UnitySettings = RootMenu + "Unity Settings for Quest";
            private const string LanguageMenu = RootMenu + "Languages/";
            internal const string LanguageAuto = LanguageMenu + "Auto (default)";
            internal const string LanguageEnglish = LanguageMenu + "English";
            internal const string LanguageJapanese = LanguageMenu + "日本語";
            internal const string CheckForUpdate = RootMenu + "Check for Update";
            internal const string Version = RootMenu + "Version " + VRCQuestTools.Version;
        }

        internal enum MenuPriorities : int
        {
            ConvertAvatarForQuest = 600, // VRChat SDK/Splash Screen: 500
            RemoveMissingComponents,
            RemovePhysBones,
            BlendShapesCopy = 700,
            RemoveUnsupportedComponents,
            MSMapGenerator,
            AutoRemoveVertexColors = 800,
            UnitySettings = 900,
            LanguageAuto = 1000,
            LanguageEnglish,
            LanguageJapanese,
            CheckForUpdate = 1100,
            Version,
        }

        internal static class GameObjectMenuPaths
        {
            private const string MenuPrefix = "GameObject/VRCQuestTools/";
            internal const string ConvertAvatarForQuest = MenuPrefix + "Convert Avatar For Quest";
            internal const string RemoveMissingComponents = MenuPrefix + "Remove Missing Components";
            internal const string RemoveUnsupportedComponents = MenuPrefix + "Remove Unsupported Components";
            internal const string RemoveAllVertexColors = MenuPrefix + "Remove All Vertex Colors";
        }

        internal enum GameObjectMenuPriorities : int
        {
            GameObjectConvertAvatarForQuest = 30,
            GameObjectRemoveMissingComponents,
            GameObjectRemoveUnsupportedComponents,
            GameObjectRemoveAllVertexColors,
        }

        internal static class ContextMenuPaths
        {
            internal const string CopyBlendShapeWeights = "CONTEXT/SkinnedMeshRenderer/Copy BlendShape Weights";
        }

        [MenuItem(MenuPaths.Version, false, (int)MenuPriorities.Version)]
        private static void Dummy()
        {
        }

        [MenuItem(MenuPaths.Version, true)]
        private static bool DummyValidation()
        {
            return false;
        }
    }
}
