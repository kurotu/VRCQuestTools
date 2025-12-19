// <copyright file="ActualPerformanceCallback.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Validation.Performance;

namespace KRT.VRCQuestTools.NonDestructive
{
    /// <summary>
    /// Callback to calculate actual performance rating without NDMF dependency.
    /// </summary>
    internal class ActualPerformanceCallback : IVRCSDKPreprocessAvatarCallback
    {
        /// <summary>
        /// Gets the last actual performance rating dictionary.
        /// Key: Blueprint ID, Value: Performance rating.
        /// </summary>
        internal static readonly Dictionary<string, PerformanceRating> LastActualPerformanceRating = new Dictionary<string, PerformanceRating>();

#pragma warning disable SA1300
        /// <summary>
        /// Gets execution order for last processing.
        /// </summary>
        public int callbackOrder => int.MaxValue;
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
            LastActualPerformanceRating[pipelineManager.blueprintId] = rating;

            return true;
        }
    }
}
