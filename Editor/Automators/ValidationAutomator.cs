// <copyright file="ValidationAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KRT.VRCQuestTools.Automators
{
    /// <summary>
    /// Automator for validation.
    /// </summary>
    [InitializeOnLoad]
    internal static class ValidationAutomator
    {
        static ValidationAutomator()
        {
            EditorApplication.delayCall += Update;
            EditorApplication.hierarchyChanged += Update;
        }

        private static void Update()
        {
            var scene = SceneManager.GetActiveScene();
            var avatars = VRCSDKUtility.GetAvatarsFromScene(scene);
            foreach (var avatar in avatars)
            {
                foreach (var rule in AvatarValidationRules.Rules)
                {
                    var result = rule.Validate(new VRChatAvatar(avatar));
                    var key = $"{rule.GetType().FullName}-{avatar.gameObject.GetInstanceID()}";
                    if (result != null)
                    {
                        NotificationWindow.instance.RegisterNotification(key, result);
                    }
                    else
                    {
                        NotificationWindow.instance.RemoveNotification(key);
                    }
                }
            }

            if (!Validate(scene))
            {
                NotificationWindow.instance.RegisterNotification("active", new NotificationItem(() =>
                {
                    GUILayout.Label("No active avatars in scene!");
                    return GUILayout.Button("Dismiss");
                }));
                NotificationWindow.instance.RegisterNotification("active1", new NotificationItem(() =>
                {
                    GUILayout.Label("No active avatars in scene!");
                    return GUILayout.Button("Dismiss");
                }));
                NotificationWindow.instance.RegisterNotification("active2", new NotificationItem(() =>
                {
                    GUILayout.Label("No active avatars in scene!");
                    return GUILayout.Button("Dismiss");
                }));
            }
            else
            {
                NotificationWindow.instance.RemoveNotification("active");
                NotificationWindow.instance.RemoveNotification("active1");
                NotificationWindow.instance.RemoveNotification("active2");
            }
        }

        private static bool Validate(Scene scene)
        {
            var avatars = VRCSDKUtility.GetAvatarsFromScene(scene);
            foreach (var a in avatars)
            {
                if (a.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
