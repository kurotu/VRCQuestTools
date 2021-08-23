// <copyright file="VertexColorRemoverAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KRT.VRCQuestTools.Automators
{
    /// <summary>
    /// Automates VertexColorRemover.
    /// </summary>
    [InitializeOnLoad]
    public class VertexColorRemoverAutomator
    {
        private static readonly string Tag = VRCQuestTools.Name;
        private static readonly string ClassName = typeof(VertexColorRemoverAutomator).Name;

        static VertexColorRemoverAutomator()
        {
            EditorApplication.delayCall += DelayCall;
        }

        /// <summary>
        /// Enable automation.
        /// </summary>
        /// <param name="enabled">Whether the automator is enabled.</param>
        internal static void Enable(bool enabled)
        {
            if (enabled)
            {
                EditorApplication.hierarchyChanged += HierarchyChanged;
                RemoveAllVertexColorsFromAvatars(SceneManager.GetActiveScene());
                Debug.Log($"[{Tag}] {ClassName} Enabled");
            }
            else
            {
                EditorApplication.hierarchyChanged -= HierarchyChanged;
                Debug.Log($"[{Tag}] {ClassName} Disabled");
            }
        }

        private static void DelayCall()
        {
            Enable(VRCQuestToolsSettings.IsVertexColorRemoverAutomatorEnabled);
        }

        private static void HierarchyChanged()
        {
            Debug.Log($"[{Tag}] HierarchyChanged, {ClassName} tries to remove vertex colors");
            RemoveAllVertexColorsFromAvatars(SceneManager.GetActiveScene());
        }

        private static void RemoveAllVertexColorsFromAvatars(Scene scene)
        {
            var model = new VertexColorRemoverViewModel();
            var avatars = VRCSDKUtility.GetAvatarsFromScene(scene);
            foreach (var a in avatars)
            {
                model.target = a.gameObject;
                model.RemoveVertexColor();
            }
        }
    }
}
