// <copyright file="LanguagesMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for language selection.
    /// </summary>
    [InitializeOnLoad]
    internal static class LanguagesMenu
    {
        static LanguagesMenu()
        {
            EditorApplication.delayCall += () =>
            {
                UpdateLanguageChecks(VRCQuestToolsSettings.DisplayLanguage);
            };
        }

        /// <summary>
        /// Use auto.
        /// </summary>
        [MenuItem(VRCQuestTools.MenuPaths.LanguageAuto, false, (int)VRCQuestTools.MenuPriorities.LanguageAuto)]
        private static void LanguageAuto()
        {
            SetLanguage(DisplayLanguage.Auto);
        }

        /// <summary>
        /// Use English.
        /// </summary>
        [MenuItem(VRCQuestTools.MenuPaths.LanguageEnglish, false, (int)VRCQuestTools.MenuPriorities.LanguageEnglish)]
        private static void LanguageEnglish()
        {
            SetLanguage(DisplayLanguage.English);
        }

        /// <summary>
        /// Use Japanese.
        /// </summary>
        [MenuItem(VRCQuestTools.MenuPaths.LanguageJapanese, false, (int)VRCQuestTools.MenuPriorities.LanguageJapanese)]
        private static void LanguageJapanese()
        {
            SetLanguage(DisplayLanguage.Japanese);
        }

        private static void SetLanguage(DisplayLanguage language)
        {
            VRCQuestToolsSettings.DisplayLanguage = language;
            UpdateLanguageChecks(language);
        }

        private static void UpdateLanguageChecks(DisplayLanguage language)
        {
            var menus = new Dictionary<DisplayLanguage, string>
            {
                { DisplayLanguage.Auto, VRCQuestTools.MenuPaths.LanguageAuto },
                { DisplayLanguage.English, VRCQuestTools.MenuPaths.LanguageEnglish },
                { DisplayLanguage.Japanese, VRCQuestTools.MenuPaths.LanguageJapanese },
            };
            Debug.Assert(menus.Count == Enum.GetValues(typeof(DisplayLanguage)).Length);

            foreach (var kvp in menus)
            {
                Menu.SetChecked(kvp.Value, kvp.Key == language);
            }
        }
    }
}
