// <copyright file="MissingScriptsRule.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Validators
{
    /// <summary>
    /// Validation rule for "missing" scripts.
    /// </summary>
    internal class MissingScriptsRule : IAvatarValidationRule
    {
        /// <inheritdoc/>
        public NotificationItem Validate(VRChatAvatar avatar)
        {
            if (!avatar.GameObject.activeInHierarchy)
            {
                return null;
            }

            if (VRCSDKUtility.CountMissingComponentsInChildren(avatar.GameObject, true) > 0)
            {
                return new NotificationItem(() =>
                {
                    if (avatar.AvatarDescriptor == null)
                    {
                        return true;
                    }
                    var i18n = VRCQuestToolsSettings.I18nResource;

                    GUILayout.Label(i18n.MissingScripts, EditorStyles.wordWrappedLabel);
                    GUILayout.Label($"{avatar.GameObject.name}", EditorStyles.wordWrappedLabel);
                    using (new EditorGUI.DisabledScope(true))
                    {
                        var objects = VRCSDKUtility.GetGameObjectsWithMissingComponents(avatar.GameObject, true);
                        foreach (var obj in objects)
                        {
                            EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                        }
                    }
                    if (GUILayout.Button(i18n.DismissLabel))
                    {
                        return true;
                    }
                    return false;
                });
            }
            return null;
        }

        [InitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            AvatarValidationRules.Add(new MissingScriptsRule());
        }
    }
}
