// <copyright file="AvatarConverterMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for avatar converter.
    /// </summary>
    internal static class AvatarConverterMenu
    {
        [MenuItem(VRCQuestTools.MenuPaths.ConvertAvatarForQuest, false, (int)VRCQuestTools.MenuPriorities.ConvertAvatarForQuest)]
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

        [MenuItem(GameObjectMenu.GameObjectConvertAvatarForQuest, false, 30)]
        private static void InitFromContextForGameObject()
        {
            InitFromMenu();
        }

        [MenuItem(GameObjectMenu.GameObjectConvertAvatarForQuest, true)]
        private static bool ValidateContextForGameObject()
        {
            var obj = Selection.activeGameObject;
            return VRCSDKUtility.IsAvatarRoot(obj);
        }
    }
}
