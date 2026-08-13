// <copyright file="AvatarDynamicsMigrationMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Views;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Defines the menu to migrate legacy Avatar Dynamics settings on demand.
    /// </summary>
    internal static class AvatarDynamicsMigrationMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.MigrateAvatarDynamicsSettings, false, (int)VRCQuestToolsMenus.MenuPriorities.MigrateAvatarDynamicsSettings)]
        private static void InitFromMenu()
        {
            AvatarDynamicsMigrationWindow.ShowWindow();
        }
    }
}
