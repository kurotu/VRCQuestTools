// <copyright file="ComponentRemoverMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_HAS_VRCSDK_BASE
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu to remove components.
    /// </summary>
    internal static class ComponentRemoverMenu
    {
        /// <summary>
        /// Remove Unsupported Components.
        /// </summary>
        [MenuItem(VRCQuestToolsMenus.MenuPaths.RemoveUnsupportedComponents, false, (int)VRCQuestToolsMenus.MenuPriorities.RemoveUnsupportedComponents)]
        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveUnsupportedComponents, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectRemoveUnsupportedComponents)]
        private static void RemoveUnsupportedComponents()
        {
            UnsupportedComponentRemoverWindow.Show(Selection.activeGameObject);
        }

        /// <summary>
        /// Remove Missing Components.
        /// </summary>
        [MenuItem(VRCQuestToolsMenus.MenuPaths.RemoveMissingComponents, false, (int)VRCQuestToolsMenus.MenuPriorities.RemoveMissingComponents)]
        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveMissingComponents, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectRemoveMissingComponents)]
        private static void RemoveMissingComponents()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var obj = Selection.activeGameObject;
            var count = VRCSDKUtility.CountMissingComponentsInChildren(obj, true);
            Debug.Log($"[{VRCQuestTools.Name}] {obj.name} has {count} missing scripts in children");
            if (count == 0)
            {
                EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.NoMissingComponentsMessage(obj.name), "OK");
                return;
            }

            var needsToUnpackPrefab = PrefabUtility.IsPartOfPrefabInstance(obj);
            var message = i18n.MissingRemoverConfirmationMessage(obj.name);
            if (needsToUnpackPrefab)
            {
                message += $" ({i18n.UnpackPrefabMessage})";
            }
            if (!EditorUtility.DisplayDialog(VRCQuestTools.Name, message, "OK", i18n.CancelLabel))
            {
                return;
            }

            if (needsToUnpackPrefab)
            {
                Undo.SetCurrentGroupName("Remove Missing Components");

                // Somehow unpacking is needed to apply changes to the scene file.
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                Debug.Log($"[{VRCQuestTools.Name}] {obj.name} has been unpacked");
            }
            VRCSDKUtility.RemoveMissingComponentsInChildren(obj, true);
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.RemoveMissingComponents, true)]
        [MenuItem(VRCQuestToolsMenus.MenuPaths.RemoveUnsupportedComponents, true)]
        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveMissingComponents, true)]
        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveUnsupportedComponents, true)]
        private static bool ValidateGameObjectMenu()
        {
            return Selection.activeGameObject != null;
        }
    }
}
#endif
