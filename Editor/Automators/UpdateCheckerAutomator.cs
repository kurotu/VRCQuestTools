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
            Task.Run(async () =>
            {
                try
                {
                    var latestRelease = await VRCQuestTools.GitHub.GetLatestRelease();
                    var hasUpdate = GitHubRelease.HasUpdates(CurrentVersion, DateTime.UtcNow, VRCQuestTools.DaysToDelayUpdateNotification, latestRelease);
                    if (hasUpdate)
                    {
                        Debug.LogWarning($"[{VRCQuestTools.Name}] New version {latestRelease.Version} is available, see {VRCQuestTools.BoothURL}");
                        NotificationWindow.instance.RegisterNotification("vrcquesttools-update", new NotificationItem(() =>
                        {
                            var i18n = VRCQuestToolsSettings.I18nResource;

                            GUILayout.Label(i18n.NewVersionIsAvailable(latestRelease.Version.ToString()));
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(i18n.GetUpdate))
                            {
                                Application.OpenURL(VRCQuestTools.BoothURL);
                                return true;
                            }
                            if (GUILayout.Button(i18n.DismissLabel))
                            {
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
