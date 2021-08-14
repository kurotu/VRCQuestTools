// <copyright file="VertexColorRemoverMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Automators;
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
                Menu.SetChecked(VRCQuestTools.MenuPaths.AutoRemoveVertexColors, VRCQuestToolsSettings.IsAutoRemoveVertexColorsEnabled);
            };
        }

        [MenuItem(VRCQuestTools.MenuPaths.AutoRemoveVertexColors, false, (int)VRCQuestTools.MenuPriorities.AutoRemoveVertexColors)]
        private static void InitFromMenu()
        {
            ToggleVertexColorRemoverAutomatorMenu();
        }

        private static void ToggleVertexColorRemoverAutomatorMenu()
        {
            var enabled = !Menu.GetChecked(VRCQuestTools.MenuPaths.AutoRemoveVertexColors);
            VRCQuestToolsSettings.IsAutoRemoveVertexColorsEnabled = enabled;
            Menu.SetChecked(VRCQuestTools.MenuPaths.AutoRemoveVertexColors, enabled);
            VertexColorRemoverAutomator.Enable(enabled);
        }

        [MenuItem(GameObjectMenu.GameObjectRemoveAllVertexColors)]
        private static void InitFromGameObject()
        {
            var model = new VertexColorRemoverViewModel
            {
                target = Selection.activeGameObject,
            };
            model.RemoveVertexColor();
            Debug.LogFormat("[{0}] All vertex colors are removed from {1}", "VRCQuestTools", model.target);
        }

        [MenuItem(GameObjectMenu.GameObjectRemoveAllVertexColors, true)]
        private static bool ValidateMenu()
        {
            return Selection.activeGameObject != null;
        }
    }
}
