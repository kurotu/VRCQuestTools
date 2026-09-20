// <copyright file="AvatarBuilderMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Menu for NDMF operations.
    /// </summary>
    internal static class AvatarBuilderMenu
    {
        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfManualBakeWithMobileSettings, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectNdmfManualBakeWithMobileSettings)]
        private static void ManualBake()
        {
            NdmfPluginUtility.ManualBakeWithMobileSettings(Selection.activeGameObject);
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfManualBakeWithMobileSettings, true)]
        private static bool ManualBakeValidate()
        {
            var target = Selection.activeGameObject;
            if (target == null)
            {
                return false;
            }
            return target.GetComponent<VRCAvatarDescriptor>() != null;
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfBuildAndTestWithMobileSettings, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectNdmfBuildAndTestWithMobileSettings)]
        private static async void BuildAndTest()
        {
            await NdmfAvatarBuilder.BuildAndTestWithMobileSettings(Selection.activeGameObject);
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfBuildAndTestWithMobileSettings, true)]
        private static bool BuildAndTestValidate()
        {
            return NdmfAvatarBuilder.CanBuildAndTestWithMobileSettings(Selection.activeGameObject);
        }
    }
}
