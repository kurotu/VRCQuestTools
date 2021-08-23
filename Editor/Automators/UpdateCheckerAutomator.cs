// <copyright file="UpdateCheckerAutomator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Views;
using UnityEditor;

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
            EditorApplication.delayCall += DelayCall;
        }

        /// <summary>
        /// Called by EditorApplication.delayCall.
        /// </summary>
        private static void DelayCall()
        {
            UpdateCheckerWindow.instance.CheckForUpdates();
            UpdateCheckerWindow.instance.Show();
        }
    }
}
