// <copyright file="UnityQuestSettingsWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.I18n;
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
        private UnityQuestSettingsViewModel model = new UnityQuestSettingsViewModel();
        private I18nBase i18n;

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
            i18n = VRCQuestToolsSettings.I18nResource;
            titleContent.text = "Unity Settings for Quest";
        }

        private void OnGUI()
        {
            var allActions = new List<Action>();

#if !UNITY_2019_3_OR_NEWER
            EditorGUILayout.LabelField("Unity Preferences", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{i18n.CacheServerModeLabel}: {model.LegacyCacheServerMode}");

            if (!model.HasValidLegacyCacheServerMode)
            {
                EditorGUILayout.HelpBox(i18n.CacheServerHelp, MessageType.Warning);
                allActions.Add(OnClickCacheServerButton);
                if (GUILayout.Button(i18n.CacheServerButtonLabel))
                {
                    OnClickCacheServerButton();
                }
            }

            EditorGUILayout.Space();
#endif

            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"{i18n.TextureCompressionLabel}: {model.DefaultAndroidTextureCompression}");
            if (!model.HasValidAndroidTextureCompression)
            {
                EditorGUILayout.HelpBox(i18n.TextureCompressionHelp, MessageType.Warning);
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
            else if (allActions.Count == 0)
            {
                EditorGUILayout.HelpBox(i18n.AllAppliedHelp, MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            model.ShowWindowOnLoad = EditorGUILayout.Toggle(i18n.ShowOnStartupLabel, model.ShowWindowOnLoad);
        }

        private void OnClickCacheServerButton()
        {
            model.ApplyRecommendedLegacyCacheServerMode();
        }

        private void OnClickTextureCompressionButton()
        {
            model.ApplyRecommendedAndroidTextureCompression();
        }
    }
}
