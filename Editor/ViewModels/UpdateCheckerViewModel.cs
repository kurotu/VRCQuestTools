// <copyright file="UpdateCheckerViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Threading;
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
        internal GitHubRelease LatestRelease = new GitHubRelease();

        private readonly GitHubService github;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCheckerViewModel"/> class with github repository.
        /// </summary>
        /// <param name="github">GitHub API service.</param>
        internal UpdateCheckerViewModel(GitHubService github)
        {
            this.github = github;
        }

        /// <summary>
        /// Gets a value indicating whether latest release is an update for current version.
        /// </summary>
        internal bool HasUpdates => GitHubRelease.HasUpdates(CurrentVersion, DateTime.UtcNow, VRCQuestTools.DaysToDelayUpdateNotification, LatestRelease);

        private SemVer CurrentVersion => new SemVer(VRCQuestTools.Version);

        /// <summary>
        /// Check latest version.
        /// </summary>
        internal void CheckForUpdates()
        {
            var mainContext = SynchronizationContext.Current;
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
