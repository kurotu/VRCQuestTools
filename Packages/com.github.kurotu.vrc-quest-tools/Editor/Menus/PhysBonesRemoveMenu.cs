// <copyright file="PhysBonesRemoveMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_HAS_VRCSDK_BASE
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Defines PhysBones remover menu.
    /// </summary>
    internal static class PhysBonesRemoveMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.RemovePhysBones, false, (int)VRCQuestToolsMenus.MenuPriorities.RemovePhysBones)]
        private static void InitFromMenu()
        {
            var target = Selection.activeGameObject;
            if (target != null && VRCSDKUtility.IsAvatarRoot(target))
            {
                var avatar = target.GetComponent<VRC_AvatarDescriptor>();
                PhysBonesRemoveWindow.ShowWindow(avatar);
            }
            else
            {
                PhysBonesRemoveWindow.ShowWindow();
            }
        }
    }
}
#endif
