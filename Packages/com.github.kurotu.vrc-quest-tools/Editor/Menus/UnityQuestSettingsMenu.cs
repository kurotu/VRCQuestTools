// <copyright file="UnityQuestSettingsMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_HAS_VRCSDK_BASE
using KRT.VRCQuestTools.Views;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for UnityQuestSetting.
    /// </summary>
    internal static class UnityQuestSettingsMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.UnitySettings, false, (int)VRCQuestToolsMenus.MenuPriorities.UnitySettings)]
        private static void InitFromMenu()
        {
            UnityQuestSettingsWindow.ShowWindow();
        }
    }
}
#endif
