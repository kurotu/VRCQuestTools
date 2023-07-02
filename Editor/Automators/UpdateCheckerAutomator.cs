// <copyright file="UpdateCheckerAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Views;
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
        internal static void CheckForUpdates()
        {
            var instance = NotificationWindow.instance;
            var lastVersionCheckDate = VRCQuestToolsSettings.LastVersionCheckDateTime;
            var noNotificationDate = lastVersionCheckDate.AddDays(1);
            if (DateTime.Now < noNotificationDate)
            {
                Debug.Log($"[{VRCQuestTools.Name}] Version check is skipped until {noNotificationDate.ToLocalTime()}");
                return;
            }

            var skippedVersion = VRCQuestToolsSettings.SkippedVersion;
            Task.Run(async () =>
            {
                try
                {
                    var latestRelease = await GetLatestRelease();
                    Debug.Log($"[{VRCQuestTools.Name}] Latest version is {latestRelease.Version} (Current: {CurrentVersion})");
                    var hasUpdate = latestRelease.Version > CurrentVersion;
                    if (hasUpdate)
                    {
                        Debug.LogWarning($"[{VRCQuestTools.Name}] New version {latestRelease.Version} is available, see {VRCQuestTools.BoothURL}");
                        if (latestRelease.Version.ToString() == skippedVersion.ToString())
                        {
                            Debug.Log($"[{VRCQuestTools.Name}] Notification was skipped because {skippedVersion} was marked as \"skipped\"");
                            return;
                        }

                        NotificationWindow.instance.RegisterNotification("vrcquesttools-update", new NotificationItem(() =>
                        {
                            var i18n = VRCQuestToolsSettings.I18nResource;

                            GUILayout.Label(i18n.NewVersionIsAvailable(latestRelease.Version.ToString()));
                            if (latestRelease.Version.IsMajorUpdate(CurrentVersion))
                            {
                                EditorGUILayout.HelpBox(i18n.NewVersionHasBreakingChanges, MessageType.Warning);
                            }
                            GUILayout.Space(8);
                            GUILayout.BeginHorizontal();
                            if (!VRCQuestTools.IsImportedAsPackage && GUILayout.Button(i18n.GetUpdate))
                            {
                                Application.OpenURL(VRCQuestTools.BoothURL);
                                return true;
                            }
                            if (GUILayout.Button(i18n.SeeChangelog))
                            {
                                Application.OpenURL(latestRelease.html_url);
                                return false;
                            }
                            if (GUILayout.Button(i18n.CheckLater))
                            {
                                VRCQuestToolsSettings.LastVersionCheckDateTime = DateTime.Now;
                                return true;
                            }
                            if (GUILayout.Button(i18n.SkipThisVersion))
                            {
                                VRCQuestToolsSettings.SkippedVersion = latestRelease.Version;
                                return true;
                            }
                            GUILayout.EndHorizontal();
                            return false;
                        }));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[{VRCQuestTools.Name}] Failed to get latest version");
                    Debug.LogException(e);
                }
            });
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

        private static async Task<GitHubRelease> GetLatestRelease()
        {
            var repo = await VRCQuestTools.VPM.GetVPMRepository(VRCQuestTools.VPMRepositoryURL);
            var vqt = repo.packages[VRCQuestTools.PackageName];
            var latest = vqt.versions.Keys
                .Select(v => new SemVer(v))
                .OrderByDescending(v => v)
                .First();
            var release = new GitHubRelease();
            release.tag_name = $"v{latest}";
            release.published_at = DateTime.UtcNow.ToString(); // fake timestamp.
            release.html_url = $"https://github.com/{VRCQuestTools.GitHubRepository}/releases/tag/{release.tag_name}";
            return release;
        }
    }
}
