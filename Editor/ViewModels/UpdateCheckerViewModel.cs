// <copyright file="UpdateCheckerViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
    internal class UpdateCheckerViewModel
    {
        private const bool IsDebug = false;

        private GitHubService github;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCheckerViewModel"/> class with github repository.
        /// </summary>
        /// <param name="repository">GitHub repository (username/reponame).</param>
        internal UpdateCheckerViewModel(string repository)
        {
            github = new GitHubService(repository);
        }

        /// <summary>
        /// Callback for CheckForUpdates().
        /// </summary>
        /// <param name="hasUpdate">Whether there is update.</param>
        /// <param name="currentVersion">Current version.</param>
        /// <param name="latestVersion">Latest version.</param>
        internal delegate void Callback(bool hasUpdate, SemVer currentVersion, SemVer latestVersion);

        /// <summary>
        /// Check latest version.
        /// </summary>
        /// <param name="callback">Callback on completion.</param>
        internal void CheckForUpdates(Callback callback)
        {
            var mainContext = SynchronizationContext.Current;
            Task.Run(async () =>
            {
                var latest = await github.GetLatestRelease();
                var latestVersion = new SemVer(latest.tag_name);
                var currentVersion = IsDebug ? new SemVer("0.0.0") : new SemVer(VRCQuestTools.Version);
                Debug.Log($"[VRCQuestTools] Current: {currentVersion}, Latest: {latestVersion}");
                mainContext.Post(
                    state =>
                    {
                        VRCQuestToolsSettings.LastVersionCheckDateTime = System.DateTime.UtcNow;
                        callback(latestVersion > currentVersion, currentVersion, latestVersion);
                    },
                    null);
            });
        }
    }
}
