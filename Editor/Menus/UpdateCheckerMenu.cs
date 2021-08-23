// <copyright file="UpdateCheckerMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for UpdateChecker.
    /// </summary>
    internal static class UpdateCheckerMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.CheckForUpdate, false, (int)VRCQuestToolsMenus.MenuPriorities.CheckForUpdate)]
        private static void InitFromMenu()
        {
            var mainContext = SynchronizationContext.Current;
            var i18n = VRCQuestToolsSettings.I18nResource;
            Task.Run(async () =>
            {
                try
                {
                    var github = new GitHubService(VRCQuestTools.GitHubRepository);
                    var release = await github.GetLatestRelease();
                    mainContext.Post(
                        (state) =>
                        {
                            if (GitHubRelease.HasUpdates(new SemVer(VRCQuestTools.Version), DateTime.UtcNow, VRCQuestTools.DaysToDelayUpdateNotification, release))
                            {
                                UpdateCheckerWindow.instance.SetLatestRelease(release);
                                UpdateCheckerWindow.instance.Show();
                                Debug.LogWarning($"[VRCQuestTools] New version {release.Version} is available, see {VRCQuestTools.BoothURL}");
                                if (EditorUtility.DisplayDialog("VRCQuestTools", i18n.NewVersionIsAvailable(release.Version.ToString()), i18n.GetUpdate, i18n.CheckLater))
                                {
                                    Application.OpenURL(VRCQuestTools.BoothURL);
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("VRCQuestTools", i18n.ThereIsNoUpdate, "OK");
                            }
                        },
                        null);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }
    }
}
