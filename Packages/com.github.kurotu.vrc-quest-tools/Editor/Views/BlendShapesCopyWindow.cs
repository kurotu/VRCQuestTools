// <copyright file="BlendShapesCopyWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Editor window for BlendShapes Copy.
    /// </summary>
    internal class BlendShapesCopyWindow : EditorWindow
    {
        [SerializeField]
        private BlendShapesCopyViewModel model = new BlendShapesCopyViewModel();

        /// <summary>
        /// Show a BlendShapes Copy window.
        /// </summary>
        internal static void ShowWindow()
        {
            var window = (BlendShapesCopyWindow)GetWindow(typeof(BlendShapesCopyWindow));
            window.Show();
        }

        /// <summary>
        /// Show a BlendShapes Copy window with a source skinned mesh renderer.
        /// </summary>
        /// <param name="source">Source mesh.</param>
        internal static void ShowWindow(SkinnedMeshRenderer source)
        {
            var window = (BlendShapesCopyWindow)GetWindow(typeof(BlendShapesCopyWindow));
            window.model.sourceMesh = source;
            window.Show();
        }

        private void OnEnable()
        {
            titleContent.text = "BlendShapes Copy";
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            model.sourceMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(i18n.SourceMeshLabel, model.sourceMesh, typeof(SkinnedMeshRenderer), true);
            model.targetMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(i18n.TargetMeshLabel, model.targetMesh, typeof(SkinnedMeshRenderer), true);

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(model.ShouldDisableCopyButton);
            {
                if (GUILayout.Button(i18n.CopyButtonLabel))
                {
                    OnClickCopyButton();
                }
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(i18n.SwitchButtonLabel))
            {
                OnClickSwitchButton();
            }
        }

        private void OnClickCopyButton()
        {
            model.CopyBlendShapesCopy();
        }

        private void OnClickSwitchButton()
        {
            model.SwitchMeshes();
        }
    }
}
