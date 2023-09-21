// <copyright file="NotificationWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Singleton GUI window for update checker.
    /// </summary>
    internal class NotificationWindow : ScriptableSingleton<NotificationWindow>
    {
        private const int WindowMargin = 8;
        private const int SceneToolBarHeight = 20;
        private static readonly Rect DefaultRect = new Rect(WindowMargin, SceneToolBarHeight + WindowMargin, 300, 80);

        private NotificationViewModel model;

        [SerializeField]
        private Rect windowRect = DefaultRect;

        private Vector2 scrollPosition;

        /// <summary>
        /// Register a notification item.
        /// </summary>
        /// <param name="key">key for the item to regster.</param>
        /// <param name="notification">notification item.</param>
        internal void RegisterNotification(string key, NotificationItem notification)
        {
            model.RegisterNotification(key, notification);
            if (model.HasNotifications)
            {
                Show();
            }
        }

        /// <summary>
        /// Remove a notification item.
        /// </summary>
        /// <param name="key">key for the item to remove.</param>
        internal void RemoveNotification(string key)
        {
            model.RemoveNotification(key);
        }

        private void OnEnable()
        {
            model = new NotificationViewModel();
        }

        /// <summary>
        /// Show window.
        /// </summary>
#pragma warning disable SA1202 // Elements should be ordered by access
        internal void Show()
#pragma warning restore SA1202 // Elements should be ordered by access
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

        private void OnSceneGUI(SceneView sceneView)
        {
            ConstraintWindowRect(sceneView);
            if (!model.HasNotifications)
            {
                return;
            }

            Handles.BeginGUI();
            windowRect = GUILayout.Window(0, windowRect, OnWindow, VRCQuestTools.Name, GUILayout.Width(300), GUILayout.Height(200));
            Handles.EndGUI();
        }

        private void OnWindow(int windowId)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                var results = model.Notifications.Select((n, i) =>
                {
                    if (i != 0)
                    {
                        GUILayout.Space(8);
                    }
                    GUILayout.BeginVertical(GUI.skin.box);
                    var result = new KeyValuePair<string, bool>(n.Key, n.Value.GuiDelegate());
                    GUILayout.EndVertical();
                    return result;
                }).ToArray();
                foreach (var r in results)
                {
                    if (r.Value)
                    {
                        model.RemoveNotification(r.Key);
                    }
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        private void ConstraintWindowRect(SceneView sceneView)
        {
            // windowRect.Set(windowRect.x, windowRect.y, DefaultRect.width, windowRect.height);
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
