// <copyright file="FallbackAvatarCallback.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;
using VRC.SDKBase.Editor.BuildPipeline;

namespace KRT.VRCQuestTools.NonDestructive
{
    /// <summary>
    /// Callback to detect FallbackAvatar component and set avatar as fallback after upload.
    /// </summary>
    [InitializeOnLoad]
    internal class FallbackAvatarCallback : IVRCSDKPreprocessAvatarCallback
    {
        private static readonly Dictionary<string, bool> PendingFallbackAvatars = new Dictionary<string, bool>();
        private static IVRCSdkBuilderApi sdkBuilder;

#pragma warning disable SA1300
        /// <summary>
        /// Gets execution order for preprocessing before NDMF optimizations.
        /// </summary>
        public int callbackOrder => -100000;
#pragma warning restore SA1300

        static FallbackAvatarCallback()
        {
            VRCSdkControlPanel.OnSdkPanelEnable += OnSdkPanelEnable;
        }

        /// <summary>
        /// Check if FallbackAvatar component is present before preprocessing.
        /// </summary>
        /// <param name="avatarGameObject">Target avatar object.</param>
        /// <returns>always true.</returns>
        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            // Callback is called also on play mode by "Apply on Play" of non-destructive context.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return true;
            }

            var pipelineManager = avatarGameObject.GetComponent<PipelineManager>();
            if (pipelineManager == null || string.IsNullOrEmpty(pipelineManager.blueprintId))
            {
                return true;
            }

            var hasFallbackComponent = avatarGameObject.GetComponent<FallbackAvatar>() != null;
            if (hasFallbackComponent)
            {
                PendingFallbackAvatars[pipelineManager.blueprintId] = true;
            }
            else
            {
                PendingFallbackAvatars.Remove(pipelineManager.blueprintId);
            }

            return true;
        }

        private static void OnSdkPanelEnable(object sender, EventArgs e)
        {
            if (sdkBuilder != null)
            {
                return;
            }

            if (VRCSdkControlPanel.TryGetBuilder(out sdkBuilder))
            {
                sdkBuilder.OnSdkUploadSuccess += OnSdkUploadSuccess;
            }
        }

        private static async void OnSdkUploadSuccess(object sender, string blueprintId)
        {
            try
            {
                // Only process for mobile builds
                var isMobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
                if (!isMobile)
                {
                    return;
                }

                if (!PendingFallbackAvatars.ContainsKey(blueprintId))
                {
                    return;
                }

                // Check performance rating before removing from pending list
                // so that if validation fails, we can retry on subsequent uploads
                if (!ActualPerformanceCallback.LastActualPerformanceRating.TryGetValue(blueprintId, out var overallRating))
                {
                    Logger.LogError($"Performance rating not found for {blueprintId}");
                    // Don't remove from pending list - might be calculated later
                    return;
                }

                // Originally, we should check performance rating for all platforms,
                // but VRCApi does not provide performance rating in VRCAvatar class.
                // So, we only check for mobile builds here as well as SDK Control Panel.
                if (!VRCSDKUtility.IsAllowedForFallbackAvatar(overallRating))
                {
                    Logger.LogWarning($"The avatar is not allowed to be set as a fallback avatar: {overallRating}");
                    // Remove from pending list - performance requirement not met
                    PendingFallbackAvatars.Remove(blueprintId);
                    return;
                }

                // Get avatar info
                var avatar = await VRCApi.GetAvatar(blueprintId, forceRefresh: true);
                if (!avatar.Tags.Contains(VRCSDKUtility.AvatarContentTag.Fallback))
                {
                    Logger.Log($"Setting {avatar.Name} as fallback");
                    avatar.Tags.Add(VRCSDKUtility.AvatarContentTag.Fallback);
                    await VRCApi.UpdateAvatarInfo(blueprintId, avatar);
                    Logger.Log($"Avatar {avatar.Name} is now ready to be used as fallback");
                }
                else
                {
                    Logger.Log($"Avatar {avatar.Name} is already configured for fallback");
                }

                // Remove from pending list only after successful processing
                PendingFallbackAvatars.Remove(blueprintId);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                // Note: Exception in async void can't be caught by caller
                // but we log it here to ensure visibility in Unity console
            }
        }
    }
}
