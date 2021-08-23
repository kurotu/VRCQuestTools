// <copyright file="UpdateCheckerWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Singleton GUI window for update checker.
    /// </summary>
    internal class UpdateCheckerWindow : ScriptableSingleton<UpdateCheckerWindow>
    {
        private const int WindowMargin = 8;
        private const int SceneToolBarHeight = 20;
        private static readonly Rect DefaultRect = new Rect(WindowMargin, SceneToolBarHeight + WindowMargin, 200, 80);

        [SerializeField]
        private UpdateCheckerViewModel model = new UpdateCheckerViewModel(new GitHubService(VRCQuestTools.GitHubRepository));

        [SerializeField]
        private Rect windowRect = DefaultRect;

        /// <summary>
        /// Show window.
        /// </summary>
        internal void Show()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        /// <summary>
        /// Close window.
        /// </summary>
        internal void Close()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        /// <summary>
        /// Check for updates.
        /// </summary>
        internal void CheckForUpdates()
        {
            model.CheckForUpdates();
        }

        /// <summary>
        /// Set latest release info.
        /// </summary>
        /// <param name="release">Latest release.</param>
        internal void SetLatestRelease(GitHubRelease release)
        {
            model.LatestRelease = release;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            ConstraintWindowRect(sceneView);
            if (!model.HasUpdates)
            {
                return;
            }

            Handles.BeginGUI();
            windowRect = GUILayout.Window(0, windowRect, OnWindow, VRCQuestTools.Name);
            Handles.EndGUI();
        }

        private void OnWindow(int windowId)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            GUILayout.Label(i18n.NewVersionIsAvailable(model.LatestRelease.Version.ToString()));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(i18n.GetUpdate))
            {
                OnClickDownload();
            }
            if (GUILayout.Button(i18n.CloseLabel))
            {
                Close();
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void OnClickDownload()
        {
            Application.OpenURL(VRCQuestTools.BoothURL);
        }

        private void ConstraintWindowRect(SceneView sceneView)
        {
            windowRect.size = DefaultRect.size;
            var validPositionRect = new Rect
            {
                position = new Vector2(WindowMargin, SceneToolBarHeight + WindowMargin),
            };
            validPositionRect.size = new Vector2(
                sceneView.position.width - WindowMargin - windowRect.width - validPositionRect.x,
                sceneView.position.height - WindowMargin - windowRect.height - validPositionRect.y);

            var position = windowRect.position;
            position.x = Saturate(position.x, validPositionRect.x, validPositionRect.x + validPositionRect.width);
            position.y = Saturate(position.y, validPositionRect.y, validPositionRect.y + validPositionRect.height);
            windowRect.position = position;
        }

        private float Saturate(float value, float min, float max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }
            return value;
        }
    }
}
