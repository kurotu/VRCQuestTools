// <copyright file="GitHubService.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Net.Http;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Services
{
    /// <summary>
    /// GitHub API.
    /// </summary>
    internal class GitHubService
    {
        private static readonly HttpClient Client = new HttpClient();
        private string repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubService"/> class with repository.
        /// </summary>
        /// <param name="repository">Repository (username/reponame).</param>
        internal GitHubService(string repository)
        {
            this.repository = repository;
            Client.DefaultRequestHeaders.Add("User-Agent", "VRCQuestTools");
            Client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        /// <summary>
        /// Get latest release info.
        /// </summary>
        /// <returns>Latest github release.</returns>
        internal async Task<GitHubRelease> GetLatestRelease()
        {
            var url = $"https://api.github.com/repos/{repository}/releases/latest";
            var result = await Client.GetStringAsync(url);
            var json = JsonUtility.FromJson<GitHubRelease>(result);
            return json;
        }
    }
}
