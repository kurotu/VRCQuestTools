// <copyright file="ActualPerformanceCallback.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;

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
        /// <remarks>
        /// This dictionary is accessed from the Unity main thread only.
        /// The OnPreprocessAvatar callback runs synchronously on the main thread,
        /// and the async OnSdkUploadSuccess handler in FallbackAvatarCallback
        /// continues execution on the Unity synchronization context (main thread)
        /// due to Unity's async/await implementation.
        /// Therefore, no additional thread synchronization is required.
        /// </remarks>
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
            if (pipelineManager == null)
            {
                return true;
            }

            var isMobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
            var stats = VRCSDKUtility.CalculatePerformanceStats(avatarGameObject, isMobile);
            if (!string.IsNullOrEmpty(pipelineManager.blueprintId))
            {
                var rating = stats.GetPerformanceRatingForCategory(VRC.SDKBase.Validation.Performance.AvatarPerformanceCategory.Overall);
                LastActualPerformanceRating[pipelineManager.blueprintId] = rating;
            }

            if (isMobile)
            {
                var i18n = new I18nEnglish();
                var statsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(isMobile);
                foreach (var category in VRCSDKUtility.AvatarDynamicsPerformanceCategories)
                {
                    if (stats.GetPerformanceRatingForCategory(category) != PerformanceRating.VeryPoor)
                    {
                        continue;
                    }

                    var categoryDisplayName = category.ToString();
                    try
                    {
                        categoryDisplayName = AvatarPerformanceStats.GetPerformanceCategoryDisplayName(category);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogWarning($"Failed to get performance category display name for '{category}'. Using fallback name. {ex}", avatarGameObject);
                    }

                    var warning = VRCSDKUtility.GetAvatarDynamicsVeryPoorViolationMessage(category, i18n);
                    var currentValue = VRCSDKUtility.GetAvatarDynamicsCurrentPerformanceValue(stats, category);
                    var poorRankLimit = VRCSDKUtility.GetAvatarDynamicsPoorRankLimit(statsLevelSet, category);
                    Logger.LogWarning(
                        $"Avatar '{avatarGameObject.name}' - {categoryDisplayName}: {warning} (Current: {currentValue}, Poor upper limit: {poorRankLimit})",
                        avatarGameObject);
                }
            }

            return true;
        }
    }
}
