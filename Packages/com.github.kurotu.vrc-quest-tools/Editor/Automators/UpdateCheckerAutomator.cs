// <copyright file="UpdateCheckerAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Automators
{
    /// <summary>
    /// Automatically check updates.
    /// </summary>
    [InitializeOnLoad]
    internal static class UpdateCheckerAutomator
    {
        static UpdateCheckerAutomator()
        {
#if !VQT_HAS_NEWTONSOFT_JSON
            Debug.LogWarning($"[{VRCQuestTools.Name}] Newtonsoft Json package is not installed. Update checker is not working.");
            return;
#else
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayModeStateChanged(PlayModeStateChange.EnteredEditMode);
            }
#endif
        }

        private static SemVer CurrentVersion => new SemVer(VRCQuestTools.Version);

        /// <summary>
        /// Check for updates.
        /// </summary>
        internal static async void CheckForUpdates()
        {
            await CheckForUpdates(false);
        }

        /// <summary>
        /// Check for updates.
        /// </summary>
        /// <param name="ignoreCache">Ignore cache.</param>
        /// <returns>true when update exists.</returns>
        internal static async Task<bool> CheckForUpdates(bool ignoreCache)
        {
            try
            {
                var latestVersion = await GetLatestVersion(CurrentVersion.IsPrerelease, ignoreCache);
                Debug.Log($"[{VRCQuestTools.Name}] Latest version is {latestVersion} (Current: {CurrentVersion})");
                var hasUpdate = latestVersion > CurrentVersion;
                if (hasUpdate)
                {
                    Debug.LogWarning($"[{VRCQuestTools.Name}] New version {latestVersion} is available, see {VRCQuestTools.BoothURL}");
                }
                return hasUpdate;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{VRCQuestTools.Name}] Failed to get latest version");
                Debug.LogException(e);
                return false;
            }
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += CheckForUpdates;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new NotImplementedException($"Case {state} is missng.");
            }
        }

        private static async Task<SemVer> GetLatestVersion(bool allowPrerelease, bool ignoreCache)
        {
            var lastVersionCheckDate = VRCQuestToolsSettings.LastVersionCheckDateTime;
            if (DateTime.Now < lastVersionCheckDate.AddDays(1) && !ignoreCache)
            {
                Debug.Log($"[{VRCQuestTools.Name}] Version check is skipped until {lastVersionCheckDate.AddDays(1).ToLocalTime()}");
                return VRCQuestToolsSettings.LatestVersionCache;
            }

            var repo = await VRCQuestTools.VPM.GetVPMRepository(VRCQuestTools.VPMRepositoryURL);
            var vqt = repo.packages[VRCQuestTools.PackageName];
            var latest = vqt.versions.Keys
                .Select(v => new SemVer(v))
                .Where(v => allowPrerelease || !v.IsPrerelease)
                .OrderByDescending(v => v)
                .First();
            VRCQuestToolsSettings.LatestVersionCache = latest;
            VRCQuestToolsSettings.LastVersionCheckDateTime = DateTime.Now;
            return latest;
        }
    }
}
