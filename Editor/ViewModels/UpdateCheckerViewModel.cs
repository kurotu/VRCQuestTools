// <copyright file="UpdateCheckerViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Services;
using UnityEngine;

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for UpdateChecker.
    /// </summary>
    [Serializable]
    internal class UpdateCheckerViewModel
    {
        /// <summary>
        /// Latest GitHub release.
        /// </summary>
        public GitHubRelease LatestRelease = new GitHubRelease();

        /// <summary>
        /// GitHub API service.
        /// </summary>
        [NonSerialized]
        internal GitHubService github;

        /// <summary>
        /// Gets a value indicating whether latest release is an update for current version.
        /// </summary>
        internal bool HasUpdates
        {
            get
            {
                if (LatestRelease == null)
                {
                    return false;
                }
                return GitHubRelease.HasUpdates(CurrentVersion, DateTime.UtcNow, VRCQuestTools.DaysToDelayUpdateNotification, LatestRelease);
            }
        }

        private SemVer CurrentVersion => new SemVer(VRCQuestTools.Version);

        /// <summary>
        /// Check latest version.
        /// </summary>
        internal void CheckForUpdates()
        {
            Task.Run(async () =>
            {
                try
                {
                    LatestRelease = await github.GetLatestRelease();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }
    }
}
