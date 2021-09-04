// <copyright file="MSMapGenWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Metallic Smoothness Map (MSMap) Generator window.
    /// </summary>
    internal class MSMapGenWindow : EditorWindow
    {
        [SerializeField]
        private MSMapGenViewModel model = new MSMapGenViewModel();

        /// <summary>
        /// Show a MSMapGenWindow.
        /// </summary>
        internal static void ShowWindow()
        {
            var window = GetWindow<MSMapGenWindow>(typeof(MSMapGenWindow));
            window.Show();
        }

        private void OnEnable()
        {
            titleContent.text = "Metallic Smoothness";
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            EditorGUILayout.BeginVertical("HelpBox");
            {
                EditorGUILayout.LabelField("Metallic", EditorStyles.boldLabel);
                model.metallicMap = (Texture2D)EditorGUILayout.ObjectField(i18n.TextureLabel, model.metallicMap, typeof(Texture2D), false);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("HelpBox");
            {
                EditorGUILayout.LabelField("Smoothness", EditorStyles.boldLabel);
                model.smoothnessMap = (Texture2D)EditorGUILayout.ObjectField(i18n.TextureLabel, model.smoothnessMap, typeof(Texture2D), false);

                EditorGUI.BeginDisabledGroup(model.smoothnessMap == null);
                {
                    model.invertSmoothness = EditorGUILayout.Toggle(i18n.InvertLabel, model.invertSmoothness);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(model.DisableGenerateButton);
            if (GUILayout.Button(i18n.GenerateButtonLabel))
            {
                OnClickGenerateButton();
            }
        }

        private void OnClickGenerateButton()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var dest = EditorUtility.SaveFilePanelInProject(i18n.SaveFileDialogTitle("Metallic Smoothness"), "MetallicSmoothness", "png", i18n.SaveFileDialogMessage);
            if (dest != string.Empty)
            {
                model.GenerateMetallicSmoothness(dest);
            }
        }
    }
}
