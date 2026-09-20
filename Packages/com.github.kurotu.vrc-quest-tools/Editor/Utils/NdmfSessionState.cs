// <copyright file="NdmfSessionState.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Validation.Performance;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Session state for VRCQuestTools NDMF.
    /// This lives in the main editor assembly because inspectors also request a build target.
    /// </summary>
    internal static class NdmfSessionState
    {
        /// <summary>
        /// Gets or sets the last actual performance rating.
        /// </summary>
        internal static readonly Dictionary<string, PerformanceRating> LastActualPerformanceRating = new Dictionary<string, PerformanceRating>();

        private const string BaseKey = "KRT.VRCQuestTools.Ndmf.";
        private const string BuildTargetKey = BaseKey + "BuildTarget";

        /// <summary>
        /// Gets or sets the build target platform.
        /// </summary>
        internal static Models.BuildTarget BuildTarget
        {
            get => (Models.BuildTarget)SessionState.GetInt(BuildTargetKey, (int)Models.BuildTarget.Auto);
            set => SessionState.SetInt(BuildTargetKey, (int)value);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            BuildTarget = Models.BuildTarget.Auto;
        }
    }
}
