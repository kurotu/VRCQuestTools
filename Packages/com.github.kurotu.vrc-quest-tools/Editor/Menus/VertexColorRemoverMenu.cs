// <copyright file="VertexColorRemoverMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_HAS_VRCSDK_BASE
using KRT.VRCQuestTools.Components;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// 頂点カラーを消去.
    /// </summary>
    [InitializeOnLoad]
    internal static class VertexColorRemoverMenu
    {
        private const string Tag = "VertexColorRemover";

        [MenuItem(VRCQuestToolsMenus.MenuPaths.RemoveAllVertexColors, false, (int)VRCQuestToolsMenus.MenuPriorities.RemoveAllVertexColors)]
        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveAllVertexColors, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectRemoveAllVertexColors)]
        private static void InitFromGameObject()
        {
            var target = Selection.activeGameObject;
            if (target == null)
            {
                return;
            }
            var remover = target.GetComponent<VertexColorRemover>();
            if (remover == null)
            {
                remover = Undo.AddComponent<VertexColorRemover>(target);
            }
            remover.includeChildren = true;
            remover.enabled = true;
            Debug.Log($"[{VRCQuestTools.Name}] All vertex colors will be removed from {target}", target);
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.RemoveAllVertexColors, true)]
        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveAllVertexColors, true)]
        private static bool ValidateMenu()
        {
            return Selection.activeGameObject != null;
        }
    }
}
#endif
