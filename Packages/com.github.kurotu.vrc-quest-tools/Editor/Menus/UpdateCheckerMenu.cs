// <copyright file="UpdateCheckerMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_HAS_VRCSDK_BASE
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
        private static async void InitFromMenu()
        {
            await UpdateCheckerAutomator.CheckForUpdates(true);
        }
    }
}
#endif
