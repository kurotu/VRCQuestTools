// <copyright file="UpdateCheckerMenu.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for UpdateChecker.
    /// </summary>
    internal static class UpdateCheckerMenu
    {
        [MenuItem(VRCQuestTools.MenuPaths.CheckForUpdate, false, (int)VRCQuestTools.MenuPriorities.CheckForUpdate)]
        private static void InitFromMenu()
        {
            var viewModel = new UpdateCheckerViewModel(VRCQuestTools.GitHubRepository);
            viewModel.CheckForUpdates((hasUpdate, currentVersion, latestVersion) =>
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                if (hasUpdate)
                {
                    Debug.LogWarning($"[VRCQuestTools] New version {latestVersion} is available, see {VRCQuestTools.BoothURL}");
                    if (EditorUtility.DisplayDialog("VRCQuestTools", i18n.NewVersionIsAvailable(latestVersion.ToString()), i18n.GetUpdate, i18n.CheckLater))
                    {
                        Application.OpenURL(VRCQuestTools.BoothURL);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("VRCQuestTools", i18n.ThereIsNoUpdate, "OK");
                }
            });
        }
    }
}
