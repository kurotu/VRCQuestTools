// <copyright file="NoPCAvatarOnAmdroidRule.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
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
        private enum Result
        {
            Ok,
            ProhibitedMaterials,
            UnsupportedComponents,
            VeryPoorPhysBones,
            VeryPoorPhysBoneColliders,
            VeryPoorContacts,
        }

        /// <inheritdoc/>
        public NotificationItem Validate(VRChatAvatar avatar)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                return null;
            }

            var result = GetAvatarErrorForQuest(avatar);
            if (result.Item1 != Result.Ok)
            {
                var detail = string.Empty;
                switch (result.Item1)
                {
                    case Result.Ok:
                        break;
                    case Result.ProhibitedMaterials:
                        var mat = (Material)result.Item2;
                        detail = $"Material \"{mat.name}\" ({mat.shader.name}) is not allowed for Quest.";
                        break;
                    case Result.UnsupportedComponents:
                        var component = (Component)result.Item2;
                        detail = $"Component \"{component.GetType().Name}\" ({component.transform.name}) is not allowed for Quest.";
                        break;
                    case Result.VeryPoorPhysBones:
                        detail = $"Too many PhysBones: {(int)result.Item2} (Very Poor).";
                        break;
                    case Result.VeryPoorPhysBoneColliders:
                        detail = $"Too many PhysBoneColliders: {(int)result.Item2} (Very Poor).";
                        break;
                    case Result.VeryPoorContacts:
                        detail = $"Too many ContactSenders and ContactReceivers: {(int)result.Item2} (Very Poor).";
                        break;
                }

                return new NotificationItem(() =>
                {
                    if (avatar.AvatarDescriptor == null)
                    {
                        return true;
                    }
                    var i18n = VRCQuestToolsSettings.I18nResource;
                    GUILayout.Label(i18n.IncompatibleForQuest, EditorStyles.wordWrappedLabel);
                    GUILayout.Label($"- {avatar.GameObject.name}", EditorStyles.wordWrappedLabel);
                    GUILayout.Label($"  {detail}", EditorStyles.wordWrappedLabel);
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

        private Tuple<Result, dynamic> GetAvatarErrorForQuest(VRChatAvatar avatar)
        {
            if (!avatar.GameObject.activeInHierarchy)
            {
                return new Tuple<Result, dynamic>(Result.Ok, null);
            }
            foreach (var m in avatar.GetRendererMaterials())
            {
                if (!VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m))
                {
                    return new Tuple<Result, dynamic>(Result.ProhibitedMaterials, m);
                }
            }

            var unsupported = new ComponentRemover().GetUnsupportedComponentsInChildren(avatar.GameObject, true);
            if (unsupported.Length > 0)
            {
                return new Tuple<Result, dynamic>(Result.UnsupportedComponents, unsupported[0]);
            }

            var physbones = avatar.GetPhysBones();
            if (physbones.Length > VRCSDKUtility.PoorPhysBonesCountLimit)
            {
                return new Tuple<Result, dynamic>(Result.VeryPoorPhysBones, physbones.Length);
            }

            var colliders = avatar.GetPhysBoneColliders();
            if (colliders.Length > VRCSDKUtility.PoorPhysBoneCollidersCountLimit)
            {
                return new Tuple<Result, dynamic>(Result.VeryPoorPhysBoneColliders, colliders.Length);
            }

            var contacts = avatar.GetContacts();
            if (contacts.Length > VRCSDKUtility.PoorContactsCountLimit)
            {
                return new Tuple<Result, dynamic>(Result.VeryPoorContacts, contacts.Length);
            }

            return new Tuple<Result, dynamic>(Result.Ok, null);
        }
    }
}
