// <copyright file="VertexColorRemoverMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveAllVertexColors, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectRemoveAllVertexColors)]
        private static void InitFromGameObject()
        {
            var model = new VertexColorRemoverViewModel
            {
                target = Selection.activeGameObject,
            };
            model.RemoveVertexColor();
            Debug.LogFormat("[{0}] All vertex colors are removed from {1}", VRCQuestTools.Name, model.target);
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveAllVertexColors, true)]
        private static bool ValidateMenu()
        {
            return Selection.activeGameObject != null;
        }
    }
}
