// <copyright file="VertexColorRemoverAutomator.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KRT.VRCQuestTools
{
    [InitializeOnLoad]
    public class VertexColorRemoverAutomator
    {
        static readonly string Tag = typeof(VertexColorRemoverAutomator).Name;

        [MenuItem(MenuPaths.AutoRemoveVertexColors, false, (int)MenuPriorities.AutoRemoveVertexColors)]
        private static void ToggleAutomation()
        {
            var enabled = !Menu.GetChecked(MenuPaths.AutoRemoveVertexColors);
            SetAutomation(enabled);
        }

        static VertexColorRemoverAutomator()
        {
            EditorApplication.delayCall += DelayInit;
        }

        private static void DelayInit()
        {
            SetAutomation(VRCQuestToolsSettings.IsAutoRemoveVertexColorsEnabled);
            EditorApplication.delayCall -= DelayInit;
        }

        private static void SetAutomation(bool enabled)
        {
            Menu.SetChecked(MenuPaths.AutoRemoveVertexColors, enabled);
            VRCQuestToolsSettings.IsAutoRemoveVertexColorsEnabled = enabled;
            if (enabled)
            {
                EditorApplication.hierarchyChanged += HierarchyChanged;
                RemoveAllVertexColorsFromAvatars(SceneManager.GetActiveScene());
                Debug.Log($"[{Tag}] Enabled");
            }
            else
            {
                EditorApplication.hierarchyChanged -= HierarchyChanged;
                Debug.Log($"[{Tag}] Disabled");
            }
        }

        private static void HierarchyChanged()
        {
            Debug.Log($"[{Tag}] HierarchyChanged");
            RemoveAllVertexColorsFromAvatars(SceneManager.GetActiveScene());
        }

        private static void RemoveAllVertexColorsFromAvatars(Scene scene)
        {
            var avatars = GetAvatars(scene);
            foreach (var a in avatars)
            {
                VertexColorRemover.RemoveAllVertexColors(a.gameObject);
            }
        }

        private static VRC.SDKBase.VRC_AvatarDescriptor[] GetAvatars(Scene scene)
        {
            var avatars = new List<VRC.SDKBase.VRC_AvatarDescriptor>();
            var rootGameObjects = scene.GetRootGameObjects();
            foreach (var obj in rootGameObjects)
            {
                avatars.AddRange(obj.GetComponentsInChildren<VRC.SDKBase.VRC_AvatarDescriptor>());
            }
            return avatars.ToArray();
        }
    }
}
