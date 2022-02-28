// <copyright file="ValidationAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
        }
    }
}
