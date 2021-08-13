// <copyright file="MSMapGenMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Views;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Defines MSMapGen menu.
    /// </summary>
    internal static class MSMapGenMenu
    {
        [MenuItem(VRCQuestTools.MenuPaths.MSMapGenerator, false, (int)VRCQuestTools.MenuPriorities.MSMapGenerator)]
        private static void InitOnMenu()
        {
            MSMapGenWindow.ShowWindow();
        }
    }
}
