// <copyright file="VRCFuryMaterialRemovalPatch.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Suppresses VRCFury's RemoveNonQuestMaterialsService.Apply() while VRCQuestTools is still going to
    /// convert the same avatar's materials in the NDMF Optimizing phase. VRCFury removes non-mobile materials
    /// earlier (effectively in the Transforming phase), so without this patch it would strip the original PC
    /// materials before VRCQuestTools' Optimizing-phase pass can read them.
    /// See https://github.com/kurotu/VRCQuestTools/issues/31 and https://github.com/kurotu/VRCQuestTools/issues/198.
    /// </summary>
    internal static class VRCFuryMaterialRemovalPatch
    {
        private const string HarmonyId = "com.github.kurotu.vrc-quest-tools.vrcfury-compat";
        private const string TargetTypeName = "VF.Service.RemoveNonQuestMaterialsService";
        private const string TargetMethodName = "Apply";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            try
            {
                Patch();
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        private static void Patch()
        {
            var targetType = AccessTools.TypeByName(TargetTypeName);
            if (targetType == null)
            {
                // VRCFury is not installed.
                return;
            }

            var applyMethod = AccessTools.Method(targetType, TargetMethodName);
            if (applyMethod == null)
            {
                Logger.LogWarning($"{TargetTypeName}.{TargetMethodName} was not found. VRCFury may have made breaking changes, so VRCQuestTools can't avoid VRCFury removing non-mobile materials before its own conversion.");
                return;
            }

            var harmony = new Harmony(HarmonyId);
            harmony.Patch(applyMethod, prefix: new HarmonyMethod(typeof(VRCFuryMaterialRemovalPatch), nameof(ApplyPrefix)));
            AssemblyReloadEvents.beforeAssemblyReload += () => harmony.UnpatchAll(HarmonyId);
        }

        /// <summary>
        /// Harmony prefix for RemoveNonQuestMaterialsService.Apply(). Returning false skips the original method.
        /// </summary>
        /// <param name="__instance">The RemoveNonQuestMaterialsService instance being invoked.</param>
        /// <returns>False to skip VRCFury's material removal, true to let it run as usual.</returns>
        internal static bool ApplyPrefix(object __instance)
        {
            var avatarRoot = GetAvatarRoot(__instance);
            if (avatarRoot == null)
            {
                return true;
            }

            if (!AvatarConverterPassUtility.HasMaterialOperatorComponents(avatarRoot))
            {
                return true;
            }

            return AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(avatarRoot) != AvatarConverterNdmfPhase.Optimizing;
        }

        /// <summary>
        /// Extracts the avatar root GameObject from a RemoveNonQuestMaterialsService instance's
        /// private "avatarObject" field (typed as VRCFury's internal VFGameObject wrapper).
        /// </summary>
        /// <param name="serviceInstance">The RemoveNonQuestMaterialsService instance.</param>
        /// <returns>The avatar root GameObject, or null if it could not be resolved.</returns>
        internal static GameObject GetAvatarRoot(object serviceInstance)
        {
            if (serviceInstance == null)
            {
                return null;
            }

            var avatarObjectField = AccessTools.Field(serviceInstance.GetType(), "avatarObject");
            var avatarObjectValue = avatarObjectField?.GetValue(serviceInstance);
            if (avatarObjectValue == null)
            {
                return null;
            }

            if (avatarObjectValue is GameObject gameObject)
            {
                return gameObject;
            }

            var toGameObject = avatarObjectValue.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Implicit" && m.ReturnType == typeof(GameObject));
            return toGameObject?.Invoke(null, new[] { avatarObjectValue }) as GameObject;
        }
    }
}
