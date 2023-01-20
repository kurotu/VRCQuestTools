// <copyright file="ModularAvatarUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Modular Avatar utilities.
    /// </summary>
    internal class ModularAvatarUtility
    {
        /// <summary>
        /// Type object of ModularAvatarMergeAnimator.
        /// </summary>
        internal static Type MergeAnimatorType = SystemUtility.GetTypeByName("nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator");

        /// <summary>
        /// Gets a value indicating whether Modular Avatar is imported.
        /// </summary>
        internal static bool IsModularAvatarImported => MergeAnimatorType != null;
    }
}
