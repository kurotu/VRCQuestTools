// <copyright file="VertexColorRemoverAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
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
    internal static class VertexColorRemoverAutomator
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
#if VRC_SDK_VRCSDK2
                RemoveAllVertexColorsFromAvatars(SceneManager.GetActiveScene());
#else
                RemoveVetexColorsByComponent(SceneManager.GetActiveScene());
                ScanVertexColor(SceneManager.GetActiveScene());
#endif
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
#if VRC_SDK_VRCSDK3
            VRCSDKUtility.InjectAllowedComponents(new System.Type[] { typeof(VertexColorRemover) });
#endif
            Enable(true);
        }

        private static void HierarchyChanged()
        {
            Debug.Log($"[{Tag}] HierarchyChanged, {ClassName} tries to remove vertex colors");
#if VRC_SDK_VRCSDK2
            RemoveAllVertexColorsFromAvatars(SceneManager.GetActiveScene());
#else
            RemoveVetexColorsByComponent(SceneManager.GetActiveScene());
            ScanVertexColor(SceneManager.GetActiveScene());
#endif
        }

        private static void RemoveVetexColorsByComponent(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            var markers = roots.SelectMany(r => r.GetComponentsInChildren<VertexColorRemover>(true));
            foreach (var m in markers)
            {
                m.RemoveVertexColor();
            }
        }

        private static void ScanVertexColor(Scene scene)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var avatars = VRCSDKUtility.GetAvatarsFromScene(scene);
            foreach (var a in avatars)
            {
                if (!a.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (!a.name.Contains("(Quest)"))
                {
                    continue;
                }
                if (a.GetComponent<VertexColorRemover>() != null)
                {
                    continue;
                }
                var avatar = new VRChatAvatar(a);
                if (avatar.HasVertexColor)
                {
                    var result = EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.VertexColorRemoverDialogMessage(a.name), i18n.YesLabel, i18n.NoLabel);
                    a.gameObject.AddComponent<VertexColorRemover>();
                    var remover = a.GetComponent<VertexColorRemover>();
                    if (result)
                    {
                        remover.active = true;
                        remover.includeChildren = true;
                        remover.RemoveVertexColor();
                    }
                    else
                    {
                        remover.active = false;
                    }
                }
            }
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
