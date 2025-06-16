// <copyright file="VRCQuestToolsSettingsMenus.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Settings menus for VRCQuestTools.
    /// </summary>
    internal static class VRCQuestToolsSettingsMenus
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.EnableValidationAutomator, false, (int)VRCQuestToolsMenus.MenuPriorities.EnableValidationAutomator)]
        private static void ToggleValidationAutomator()
        {
            VRCQuestToolsSettings.IsValidationAutomatorEnabled = !VRCQuestToolsSettings.IsValidationAutomatorEnabled;
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.EnableValidationAutomator, true)]
        private static bool ToggleValidationAutomatorValidation()
        {
            Menu.SetChecked(VRCQuestToolsMenus.MenuPaths.EnableValidationAutomator, VRCQuestToolsSettings.IsValidationAutomatorEnabled);
            return true;
        }
    }
} 