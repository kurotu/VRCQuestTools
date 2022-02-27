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
            var hasDynamicBone = AssetUtility.IsDynamicBoneImported();

            if (VRCSDKUtility.CountMissingComponentsInChildren(avatar.GameObject, true) > 0)
            {
                return new NotificationItem(() =>
                {
                    if (avatar.AvatarDescriptor == null)
                    {
                        return true;
                    }
                    var i18n = VRCQuestToolsSettings.I18nResource;

                    if (hasDynamicBone)
                    {
                        GUILayout.Label(i18n.MissingScripts, EditorStyles.wordWrappedLabel);
                    }
                    else
                    {
                        GUILayout.Label(i18n.MissingDynamicBone, EditorStyles.wordWrappedLabel);
                    }
                    GUILayout.Label($"- {avatar.GameObject.name}", EditorStyles.wordWrappedLabel);
                    if (GUILayout.Button(i18n.RemoveMissing))
                    {
                        Selection.activeGameObject = avatar.GameObject;
                        EditorApplication.ExecuteMenuItem(VRCQuestToolsMenus.MenuPaths.RemoveMissingComponents);
                        return false;
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
