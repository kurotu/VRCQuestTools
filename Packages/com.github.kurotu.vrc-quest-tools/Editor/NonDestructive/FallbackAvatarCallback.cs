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

            PendingFallbackAvatars.Remove(blueprintId);

            try
            {
                // Check performance rating
                if (!ActualPerformanceCallback.LastActualPerformanceRating.ContainsKey(blueprintId))
                {
                    Logger.LogWarning($"Performance rating not found for {blueprintId}");
                    return;
                }

                var overallRating = ActualPerformanceCallback.LastActualPerformanceRating[blueprintId];
                if (!VRCSDKUtility.IsAllowedForFallbackAvatar(overallRating))
                {
                    Logger.LogWarning($"The avatar is not allowed to be set as a fallback avatar: {overallRating}");
                    return;
                }

                // Get avatar info
                var avatar = await VRCApi.GetAvatar(blueprintId, forceRefresh: true);
                if (!avatar.Tags.Contains(VRCSDKUtility.AvatarContentTag.Fallback))
                {
                    Logger.Log($"Setting avatar as fallback");
                    await VRCApi.SetAvatarAsFallback(blueprintId, avatar);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
