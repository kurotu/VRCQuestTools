// <copyright file="ComponentRemoverMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using System.Linq;
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
        [MenuItem(VRCQuestTools.MenuPaths.RemoveUnsupportedComponents, false, (int)VRCQuestTools.MenuPriorities.RemoveUnsupportedComponents)]
        [MenuItem(GameObjectMenu.GameObjectRemoveUnsupportedComponents, false)]
        private static void RemoveUnsupportedComponents()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var obj = Selection.activeGameObject;

            var components = VRCSDKUtility.GetUnsupportedComponentsInChildren(obj, true);
            if (components.Length == 0)
            {
                EditorUtility.DisplayDialog("VRCQuestTools", i18n.NoUnsupportedComponentsMessage(obj.name), "OK");
                return;
            }
            var message = i18n.UnsupportedRemoverConfirmationMessage(obj.name) + "\n\n" +
                string.Join("\n", components.Select(c => c.GetType()).Distinct().Select(c => $"  - {c.Name}").OrderBy(c => c));
            if (!EditorUtility.DisplayDialog("VRCQuestTools", message, "OK", i18n.CancelLabel))
            {
                return;
            }

            Undo.SetCurrentGroupName("Remove Unsupported Components");
            VRCSDKUtility.RemoveUnsupportedComponentsInChildren(obj, true, true);
        }

        /// <summary>
        /// Remove Missing Components.
        /// </summary>
        [MenuItem(VRCQuestTools.MenuPaths.RemoveMissingComponents, false, (int)VRCQuestTools.MenuPriorities.RemoveMissingComponents)]
        [MenuItem(GameObjectMenu.GameObjectRemoveMissingComponents, false)]
        private static void RemoveMissingComponents()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var obj = Selection.activeGameObject;
            var count = VRCSDKUtility.CountMissingComponentsInChildren(obj, true);
            Debug.Log($"[VRCQuestTools] {obj.name} has {count} missing scripts in children");
            if (count == 0)
            {
                EditorUtility.DisplayDialog("VRCQuestTools", i18n.NoMissingComponentsMessage(obj.name), "OK");
                return;
            }

            var needsToUnpackPrefab = PrefabUtility.IsPartOfPrefabInstance(obj);
            var message = i18n.MissingRemoverConfirmationMessage(obj.name);
            if (needsToUnpackPrefab)
            {
                message += $" ({i18n.UnpackPrefabMessage})";
            }
            if (!EditorUtility.DisplayDialog("VRCQuestTools", message, "OK", i18n.CancelLabel))
            {
                return;
            }

            if (needsToUnpackPrefab)
            {
                Undo.SetCurrentGroupName("Remove Missing Components");

                // Somehow unpacking is needed to apply changes to the scene file.
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                Debug.Log($"[VRCQuestTools] {obj.name} has been unpacked");
            }
            VRCSDKUtility.RemoveMissingComponentsInChildren(obj, true);
        }

        [MenuItem(VRCQuestTools.MenuPaths.RemoveMissingComponents, true)]
        [MenuItem(VRCQuestTools.MenuPaths.RemoveUnsupportedComponents, true)]
        [MenuItem(GameObjectMenu.GameObjectRemoveMissingComponents, true)]
        [MenuItem(GameObjectMenu.GameObjectRemoveUnsupportedComponents, true)]
        private static bool ValidateGameObjectMenu()
        {
            return Selection.activeGameObject != null;
        }
    }
}
