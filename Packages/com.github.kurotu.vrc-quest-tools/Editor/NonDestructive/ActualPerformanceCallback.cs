// <copyright file="ActualPerformanceCallback.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor.BuildPipeline;

namespace KRT.VRCQuestTools.NonDestructive
{
    /// <summary>
    /// Callback to calculate actual performance rating without NDMF dependency.
    /// </summary>
    internal class ActualPerformanceCallback : IVRCSDKPreprocessAvatarCallback
    {
#pragma warning disable SA1300
        /// <summary>
        /// Gets execution order for last processing before NDMF.
        /// </summary>
        public int callbackOrder => int.MaxValue - 10000;
#pragma warning restore SA1300

        /// <summary>
        /// Calculate actual performance rating.
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

            var isMobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
            var stats = VRCSDKUtility.CalculatePerformanceStats(avatarGameObject, isMobile);
            var rating = stats.GetPerformanceRatingForCategory(VRC.SDKBase.Validation.Performance.AvatarPerformanceCategory.Overall);
            VRCQuestToolsSessionState.LastActualPerformanceRating[pipelineManager.blueprintId] = rating;

            return true;
        }
    }
}
