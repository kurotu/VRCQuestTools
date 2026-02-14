// <copyright file="UnityQuestSettingsWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Editor window for UnityQuestSettings.
    /// </summary>
    internal class UnityQuestSettingsWindow : EditorWindow
    {
        [SerializeField]
        private UnityQuestSettingsViewModel model = new UnityQuestSettingsViewModel();

        /// <summary>
        /// Show a UnityQuestSettingsWindow.
        /// </summary>
        internal static void ShowWindow()
        {
            var window = GetWindow(typeof(UnityQuestSettingsWindow));
            window.Show();
        }

        private void OnEnable()
        {
            titleContent.text = "Unity Settings for Android";
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var allActions = new List<Action>();

            EditorGUILayout.LabelField(i18n.RecommendedUnitySettingsForAndroid, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"Android Build Support: {GetYesNoString(model.HasAndroidBuildSupport)}");
            if (!model.HasAndroidBuildSupport)
            {
                EditorGUILayout.HelpBox(i18n.AndroidBuildSupportHelp, MessageType.Error);
                if (GUILayout.Button(i18n.AndroidBuildSupportButtonLabel))
                {
                    OnClickAndroidBuildSupportButton();
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"{i18n.TextureCompressionLabel}: {model.DefaultAndroidTextureCompression}");
            if (!model.HasValidAndroidTextureCompression)
            {
                EditorGUILayout.HelpBox(i18n.TextureCompressionHelp, MessageType.Info);
                allActions.Add(OnClickTextureCompressionButton);
                if (GUILayout.Button(i18n.TextureCompressionButtonLabel))
                {
                    OnClickTextureCompressionButton();
                }
            }

            EditorGUILayout.Space();

            if (allActions.Count >= 2)
            {
                if (GUILayout.Button(i18n.ApplyAllButtonLabel))
                {
                    foreach (var action in allActions)
                    {
                        action();
                    }
                }
            }
            else if (model.AllSettingsValid)
            {
                EditorGUILayout.HelpBox(i18n.AllAppliedHelp, MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            model.ShowWindowOnLoad = EditorGUILayout.Toggle(i18n.ShowOnStartupLabel, model.ShowWindowOnLoad);
        }

        private void OnClickAndroidBuildSupportButton()
        {
            var url = "https://kurotu.github.io/VRCQuestTools/docs/tutorial/set-up-environment/?lang=auto";
            Application.OpenURL(url);
        }

        private void OnClickTextureCompressionButton()
        {
            model.ApplyRecommendedAndroidTextureCompression();
        }

        private string GetYesNoString(bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}
