// <copyright file="BlendShapesCopyMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_HAS_VRCSDK_BASE
using KRT.VRCQuestTools.Views;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Defines BlendShapes Copy menu.
    /// </summary>
    internal static class BlendShapesCopyMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.BlendShapesCopy, false, (int)VRCQuestToolsMenus.MenuPriorities.BlendShapesCopy)]
        private static void InitOnMenu()
        {
            BlendShapesCopyWindow.ShowWindow();
        }

        [MenuItem(VRCQuestToolsMenus.ContextMenuPaths.CopyBlendShapeWeights)]
        private static void InitOnContext(MenuCommand command)
        {
            BlendShapesCopyWindow.ShowWindow(command.context as SkinnedMeshRenderer);
        }
    }
}
#endif
