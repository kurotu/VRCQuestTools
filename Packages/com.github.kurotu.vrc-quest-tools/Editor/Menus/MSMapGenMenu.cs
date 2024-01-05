// <copyright file="MSMapGenMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_HAS_VRCSDK_BASE
using KRT.VRCQuestTools.Views;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Defines MSMapGen menu.
    /// </summary>
    internal static class MSMapGenMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.MSMapGenerator, false, (int)VRCQuestToolsMenus.MenuPriorities.MSMapGenerator)]
        private static void InitOnMenu()
        {
            MSMapGenWindow.ShowWindow();
        }
    }
}
#endif
