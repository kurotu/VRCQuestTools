// <copyright file="UnityQuestSettingsAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.ViewModels;
using KRT.VRCQuestTools.Views;
using UnityEditor;

namespace KRT.VRCQuestTools.Automators
{
    /// <summary>
    /// Automates UnityQuestSettings.
    /// </summary>
    [InitializeOnLoad]
    internal static class UnityQuestSettingsAutomator
    {
        /// <summary>
        /// Initializes static members of the <see cref="UnityQuestSettingsAutomator"/> class.
        /// </summary>
        static UnityQuestSettingsAutomator()
        {
            EditorApplication.delayCall += DelayCall;
        }

        private static void DelayCall()
        {
            var viewModel = new UnityQuestSettingsViewModel();
            if (viewModel.ShowWindowOnLoad && !viewModel.AllSettingsValid)
            {
                UnityQuestSettingsWindow.ShowWindow();
            }
        }
    }
}
