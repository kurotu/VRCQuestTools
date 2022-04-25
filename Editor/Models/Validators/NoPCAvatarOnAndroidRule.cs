// <copyright file="NoPCAvatarOnAmdroidRule.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Validators
{
    /// <summary>
    /// Deactivate PC avatar on Android target.
    /// </summary>
    internal class NoPCAvatarOnAndroidRule : IAvatarValidationRule
    {
        /// <inheritdoc/>
        public NotificationItem Validate(VRChatAvatar avatar)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                return null;
            }

            if (AvatarHasErrorForQuest(avatar))
            {
                return new NotificationItem(() =>
                {
                    if (avatar.AvatarDescriptor == null)
                    {
                        return true;
                    }
                    var i18n = VRCQuestToolsSettings.I18nResource;
                    GUILayout.Label(i18n.IncompatibleForQuest, EditorStyles.wordWrappedLabel);
                    GUILayout.Label($"- {avatar.GameObject.name}", EditorStyles.wordWrappedLabel);
                    if (GUILayout.Button(i18n.DeactivateAvatar))
                    {
                        avatar.GameObject.SetActive(false);
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
            AvatarValidationRules.Add(new NoPCAvatarOnAndroidRule());
        }

        private bool AvatarHasErrorForQuest(VRChatAvatar avatar)
        {
            if (!avatar.GameObject.activeInHierarchy)
            {
                return false;
            }
            foreach (var m in avatar.GetRendererMaterials())
            {
                if (!VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m))
                {
                    return true;
                }
            }

            if (new ComponentRemover().GetUnsupportedComponentsInChildren(avatar.GameObject, true).Length > 0)
            {
                return true;
            }

            if (avatar.GetPhysBones().Length > VRCSDKUtility.PoorPhysBonesCountLimit
                || avatar.GetPhysBoneColliders().Length > VRCSDKUtility.PoorPhysBoneCollidersCountLimit
                || avatar.GetContacts().Length > VRCSDKUtility.PoorContactsCountLimit)
            {
                return true;
            }

            return false;
        }
    }
}
