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
    }

    static class GameObjectMenu
    {
        const string MenuPrefix = "GameObject/VRCQuestTools/";
        internal const string GameObjectRemoveAllVertexColors = MenuPrefix + "Remove All Vertex Colors";
        internal const string GameObjectRemoveUnsupportedComponents = MenuPrefix + "Remove Unsupported Components";
        internal const string GameObjectRemoveMissingComponents = MenuPrefix + "Remove Missing Components";
        internal const string GameObjectConvertAvatarForQuest = MenuPrefix + "Convert Avatar For Quest";
    }
}
