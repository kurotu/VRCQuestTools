// <copyright file="VRCQuestToolsSessionState.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using VRC.SDKBase.Validation.Performance;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Session state for VRCQuestTools.
    /// </summary>
    internal static class VRCQuestToolsSessionState
    {
        /// <summary>
        /// Gets the last actual performance rating dictionary.
        /// Key: Blueprint ID, Value: Performance rating.
        /// </summary>
        internal static readonly Dictionary<string, PerformanceRating> LastActualPerformanceRating = new Dictionary<string, PerformanceRating>();
    }
}
