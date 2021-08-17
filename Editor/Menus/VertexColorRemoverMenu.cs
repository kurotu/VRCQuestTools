// <copyright file="VertexColorRemoverMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Automators;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.ViewModels;
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

        static VertexColorRemoverMenu()
        {
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked(VRCQuestToolsMenus.MenuPaths.AutoRemoveVertexColors, VRCQuestToolsSettings.IsVertexColorRemoverAutomatorEnabled);
            };
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.AutoRemoveVertexColors, false, (int)VRCQuestToolsMenus.MenuPriorities.AutoRemoveVertexColors)]
        private static void InitFromMenu()
        {
            ToggleVertexColorRemoverAutomatorMenu();
        }

        private static void ToggleVertexColorRemoverAutomatorMenu()
        {
            var enabled = !Menu.GetChecked(VRCQuestToolsMenus.MenuPaths.AutoRemoveVertexColors);
            VRCQuestToolsSettings.IsVertexColorRemoverAutomatorEnabled = enabled;
            Menu.SetChecked(VRCQuestToolsMenus.MenuPaths.AutoRemoveVertexColors, enabled);
            VertexColorRemoverAutomator.Enable(enabled);
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveAllVertexColors, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectRemoveAllVertexColors)]
        private static void InitFromGameObject()
        {
            var model = new VertexColorRemoverViewModel
            {
                target = Selection.activeGameObject,
            };
            model.RemoveVertexColor();
            Debug.LogFormat("[{0}] All vertex colors are removed from {1}", "VRCQuestTools", model.target);
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.RemoveAllVertexColors, true)]
        private static bool ValidateMenu()
        {
            return Selection.activeGameObject != null;
        }
    }
}
