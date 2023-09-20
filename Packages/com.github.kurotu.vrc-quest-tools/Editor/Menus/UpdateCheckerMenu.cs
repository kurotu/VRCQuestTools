// <copyright file="UpdateCheckerMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Automators;
using KRT.VRCQuestTools.Models;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for UpdateChecker.
    /// </summary>
    internal static class UpdateCheckerMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.CheckForUpdate, false, (int)VRCQuestToolsMenus.MenuPriorities.CheckForUpdate)]
        private static void InitFromMenu()
        {
            VRCQuestToolsSettings.SkippedVersion = new SemVer("0.0.0");
            VRCQuestToolsSettings.LastVersionCheckDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            UpdateCheckerAutomator.CheckForUpdates();
        }
    }
}
