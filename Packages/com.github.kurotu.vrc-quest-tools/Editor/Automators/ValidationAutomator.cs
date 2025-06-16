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
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayModeStateChanged(PlayModeStateChange.EnteredEditMode);
            }
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    EditorApplication.hierarchyChanged -= Update;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += Update;
                    EditorApplication.hierarchyChanged += Update;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new System.NotImplementedException($"Case {state} is not implemented.");
            }
        }

        private static void Update()
        {
            if (!VRCQuestToolsSettings.IsValidationAutomatorEnabled)
            {
                return;
            }

            var avatars = VRCSDKUtility.GetAvatarsFromLoadedScenes();
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
        }
    }
}
