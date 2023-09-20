// <copyright file="NoPCAvatarOnAmdroidRule.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
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
            ProhibitedShaders,
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
                var i18n = VRCQuestToolsSettings.I18nResource;
                switch (result.Item1)
                {
                    case Result.Ok:
                        break;
                    case Result.ProhibitedShaders:
                        var mats = (Material[])result.Item2;
                        detail = i18n.ValidatorAlertsProhibitedShaders(mats[0].shader.name,  mats.Select(m => m.name).ToArray());
                        break;
                    case Result.UnsupportedComponents:
                        var component = (Component)result.Item2;
                        detail = i18n.ValidatorAlertsUnsupportedComponents(component.GetType().Name, component.transform.name);
                        break;
                    case Result.VeryPoorPhysBones:
                        detail = i18n.ValidatorAlertsVeryPoorPhysBones((int)result.Item2);
                        break;
                    case Result.VeryPoorPhysBoneColliders:
                        detail = i18n.ValidatorAlertsVeryPoorPhysBoneColliders((int)result.Item2);
                        break;
                    case Result.VeryPoorContacts:
                        detail = i18n.ValidatorAlertsVeryPoorContacts((int)result.Item2);
                        break;
                }

                return new NotificationItem(() =>
                {
                    if (avatar.AvatarDescriptor == null)
                    {
                        return true;
                    }
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
#if !VQT_VRCSDK_HAS_PUBLIC_API
            AvatarValidationRules.Add(new NoPCAvatarOnAndroidRule());
#endif
        }

        private Tuple<Result, dynamic> GetAvatarErrorForQuest(VRChatAvatar avatar)
        {
            if (!avatar.GameObject.activeInHierarchy)
            {
                return new Tuple<Result, dynamic>(Result.Ok, null);
            }
            var rendererMaterials = avatar.GetRendererMaterials();
            foreach (var m in rendererMaterials)
            {
                if (!VRCSDKUtility.IsMaterialAllowedForQuestAvatar(m))
                {
                    return new Tuple<Result, dynamic>(Result.ProhibitedShaders, rendererMaterials.Where(rm => rm.shader == m.shader).ToArray());
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
