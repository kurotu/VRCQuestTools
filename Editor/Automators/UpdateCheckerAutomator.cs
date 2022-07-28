// <copyright file="UpdateCheckerAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
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
        private const int DelayDays = 1;

        static UpdateCheckerAutomator()
        {
            EditorApplication.delayCall += CheckForUpdates;
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
                    var latestRelease = await VRCQuestTools.GitHub.GetLatestRelease();
                    var hasUpdate = GitHubRelease.HasUpdates(CurrentVersion, DateTime.UtcNow, VRCQuestTools.DaysToDelayUpdateNotification, latestRelease);
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
                            if (GUILayout.Button(i18n.GetUpdate))
                            {
                                Application.OpenURL(VRCQuestTools.BoothURL);
                                return true;
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
                    Debug.LogException(e);
                }
            });
        }
    }
}
