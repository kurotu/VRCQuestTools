// <copyright file="VRCQuestToolsMenus.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1602 // Enumeration items should be documented

using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Menus
{
    internal static class VRCQuestToolsMenus
    {
        internal static class MenuPaths
        {
            internal const string RootMenu = "Tools/" + VRCQuestTools.Name + "/";
            internal const string ConvertAvatarForAndroid = RootMenu + "Convert Avatar for Android";
            internal const string ShowAvatarBuilder = RootMenu + "Show Avatar Builder";
            internal const string RemoveUnsupportedComponents = RootMenu + "Remove Unsupported Components";
            internal const string RemoveMissingComponents = RootMenu + "Remove Missing Components";
            internal const string RemoveAllVertexColors = RootMenu + "Remove All Vertex Colors";
            internal const string RemovePhysBones = RootMenu + "Remove PhysBones";
            internal const string BlendShapesCopy = RootMenu + "BlendShapes Copy";
            internal const string MSMapGenerator = RootMenu + "Metallic Smoothness Map";
            internal const string UnitySettings = RootMenu + "Unity Settings for Android";
            internal const string ClearTextureCache = RootMenu + "Clear Texture Cache";
            private const string SettingsMenu = RootMenu + "Settings/";
            internal const string EnableValidationAutomator = SettingsMenu + "Enable Validation Automator";
            internal const string EnableTextureFormatCheckOnStandalone = SettingsMenu + "[NDMF] Enable Texture Format Check on Windows Build";
            private const string LanguageMenu = RootMenu + "Languages/";
            internal const string LanguageAuto = LanguageMenu + "Auto (default)";
            internal const string LanguageEnglish = LanguageMenu + "English";
            internal const string LanguageJapanese = LanguageMenu + "日本語";
            internal const string LanguageRussian = LanguageMenu + "Русский";
            internal const string CheckForUpdate = RootMenu + "Check for Update";
            internal const string MissingSDK = RootMenu + "VRCSDK is missing or incompatible";
            internal const string Help = RootMenu + "Help";
            internal const string Version = RootMenu + "Version " + VRCQuestTools.Version;
        }

        internal enum MenuPriorities : int
        {
            ConvertAvatarForQuest = 600, // VRChat SDK/Splash Screen: 500
            ShowAvatarBuilder,
            RemovePhysBones = 700,
            RemoveMissingComponents,
            RemoveUnsupportedComponents,
            RemoveAllVertexColors,
            BlendShapesCopy = 800,
            MSMapGenerator,
            UnitySettings = 900,
            ClearTextureCache,
            EnableValidationAutomator = 950,
            EnableTextureFormatCheckOnStandalone,
            LanguageAuto = 1000,
            LanguageEnglish,
            LanguageJapanese,
            LanguageRussian,
            CheckForUpdate = 1100,
            MissingSDK,
            Help,
            Version,
        }

        internal static class GameObjectMenuPaths
        {
            private const string MenuPrefix = "GameObject/VRCQuestTools/";
            internal const string ConvertAvatarForQuest = MenuPrefix + "Convert Avatar for Android";
            internal const string NdmfManualBakeWithAndroidSettings = MenuPrefix + "[NDMF] Manual Bake with Android Settings";
            internal const string RemovePhysBones = MenuPrefix + "Remove PhysBones";
            internal const string RemoveMissingComponents = MenuPrefix + "Remove Missing Components";
            internal const string RemoveUnsupportedComponents = MenuPrefix + "Remove Unsupported Components";
            internal const string RemoveAllVertexColors = MenuPrefix + "Remove All Vertex Colors";
        }

        internal enum GameObjectMenuPriorities : int
        {
            GameObjectConvertAvatarForQuest = 30,
            GameObjectNdmfManualBakeWithAndroidSettings,
            GameObjectRemovePhysBones = 130,
            GameObjectRemoveMissingComponents,
            GameObjectRemoveUnsupportedComponents,
            GameObjectRemoveAllVertexColors,
        }

        internal static class ContextMenuPaths
        {
            internal const string CopyBlendShapeWeights = "CONTEXT/SkinnedMeshRenderer/Copy BlendShape Weights";
        }

#if !VQT_HAS_VRCSDK_BASE
        [MenuItem(MenuPaths.MissingSDK, false, (int)MenuPriorities.MissingSDK)]
        private static void MissingSDK()
        {
        }

        [MenuItem(MenuPaths.MissingSDK, true)]
        private static bool MissingSDKValidation()
        {
            return false;
        }
#endif

        [MenuItem(MenuPaths.Help, false, (int)MenuPriorities.Help)]
        private static void HelpMenu()
        {
            Application.OpenURL(VRCQuestTools.DocsURL + "?lang=auto");
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
