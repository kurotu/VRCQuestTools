// <copyright file="GitHubRelease.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable SA1310 // Field names should not contain underscore

using System;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Metadata for a GitHub release.
    /// </summary>
    [Serializable]
    internal class GitHubRelease
    {
        /// <summary>
        /// Git tag name.
        /// </summary>
        public string tag_name = null;
    }
}
