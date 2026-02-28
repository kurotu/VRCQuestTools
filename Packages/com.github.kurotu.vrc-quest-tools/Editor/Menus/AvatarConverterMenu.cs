// <copyright file="AvatarConverterMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for avatar converter.
    /// </summary>
    internal static class AvatarConverterMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.ConvertAvatarForMobile, false, (int)VRCQuestToolsMenus.MenuPriorities.ConvertAvatarForMobile)]
        private static void InitFromMenu()
        {
            var target = Selection.activeGameObject;
            if (target != null && VRCSDKUtility.IsAvatarRoot(target))
            {
                var avatar = target.GetComponent<VRC_AvatarDescriptor>();
                AvatarConverterWindow.ShowWindow(avatar);
            }
            else
            {
                AvatarConverterWindow.ShowWindow();
            }
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.ConvertAvatarForMobile, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectConvertAvatarForMobile)]
        private static void InitFromContextForGameObject()
        {
            InitFromMenu();
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.ConvertAvatarForMobile, true)]
        private static bool ValidateContextForGameObject()
        {
            var obj = Selection.activeGameObject;
            return VRCSDKUtility.IsAvatarRoot(obj);
        }
    }
}
