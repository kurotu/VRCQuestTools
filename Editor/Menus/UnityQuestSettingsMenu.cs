// <copyright file="UnityQuestSettingsMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Views;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for UnityQuestSetting.
    /// </summary>
    internal static class UnityQuestSettingsMenu
    {
        [MenuItem(VRCQuestTools.MenuPaths.UnitySettings, false, (int)VRCQuestTools.MenuPriorities.UnitySettings)]
        private static void InitFromMenu()
        {
            UnityQuestSettingsWindow.ShowWindow();
        }
    }
}
