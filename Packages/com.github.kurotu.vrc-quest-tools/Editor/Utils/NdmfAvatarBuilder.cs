// <copyright file="NdmfAvatarBuilder.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Ndmf;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3A.Editor;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility to build avatars with NDMF from menus and inspectors.
    /// </summary>
    internal static class NdmfAvatarBuilder
    {
        /// <summary>
        /// Builds the avatar with mobile settings and tests it on PC.
        /// </summary>
        /// <param name="avatar">Avatar root object.</param>
        /// <returns>A task which completes when the build finished.</returns>
        internal static async Task BuildAndTestWithMobileSettings(GameObject avatar)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            NdmfSessionState.BuildTarget = Models.BuildTarget.Android;
            try
            {
                if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var sdkBuilder))
                {
                    await sdkBuilder.BuildAndTest(avatar);
                    EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.BuildAndTestSucceeded(avatar.name), "OK");
                }
                else
                {
                    Logger.LogError(i18n.BuildAndTestRequiresSdkControlPanel);
                    EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.BuildAndTestRequiresSdkControlPanel, "OK");
                }
            }
            catch (System.Exception e)
            {
                Logger.LogException(e);
                EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.BuildAndTestFailed(e.Message), "OK");
            }
            finally
            {
                NdmfSessionState.BuildTarget = Models.BuildTarget.Auto;
            }
        }

        /// <summary>
        /// Gets whether the avatar can be built and tested on PC.
        /// </summary>
        /// <param name="avatar">Avatar root object.</param>
        /// <returns>true when the avatar can be built and tested on PC.</returns>
        internal static bool CanBuildAndTestWithMobileSettings(GameObject avatar)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return false;
            }
            if (EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            {
                return false;
            }
            if (avatar == null)
            {
                return false;
            }
            if (!avatar.activeInHierarchy)
            {
                return false;
            }
            return avatar.GetComponent<VRCAvatarDescriptor>() != null;
        }
    }
}
