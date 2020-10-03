// <copyright file="VertexColorRemoverAutomator.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KRTQuestTools
{
    [InitializeOnLoad]
    public class VertexColorRemoverAutomator
    {
        static readonly string Tag = typeof(VertexColorRemoverAutomator).Name;

        static VertexColorRemoverAutomator()
        {
            EditorApplication.hierarchyChanged += HierarchyChanged;
            RemoveAllVertexColorsFromAvatars(SceneManager.GetActiveScene());
            Debug.Log($"[{Tag}] Loaded");
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
