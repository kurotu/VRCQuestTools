// <copyright file="PhysBonesRemoveMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
using UnityEditor;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

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