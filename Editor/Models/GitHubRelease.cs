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
        public string tag_name = "0.0.0";

        /// <summary>
        /// Published date.
        /// </summary>
        public string published_at = null;

        /// <summary>
        /// Gets published date.
        /// </summary>
        internal DateTime PublishedDateTime => published_at == null ? DateTime.MinValue : DateTime.Parse(published_at).ToUniversalTime();

        /// <summary>
        /// Gets SemVer by <see cref="tag_name"/>.
        /// </summary>
        internal SemVer Version => new SemVer(tag_name);

        /// <summary>
        /// Check whether there are updates.
        /// </summary>
        /// <param name="current">Current version.</param>
        /// <param name="now">Current datetime.</param>
        /// <param name="daysToDelay">Delay from latest release date.</param>
        /// <param name="latest">Latest release.</param>
        /// <returns>true when updates are available.</returns>
        internal static bool HasUpdates(SemVer current, DateTime now, int daysToDelay, GitHubRelease latest)
        {
            var span = now - latest.PublishedDateTime;
            if (latest.Version > current && span.Days > daysToDelay)
            {
                return true;
            }
            return false;
        }
    }
}
