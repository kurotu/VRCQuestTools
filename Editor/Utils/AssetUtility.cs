// <copyright file="AssetUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for assets.
    /// </summary>
    internal static class AssetUtility
    {
        /// <summary>
        /// Gets whether Dynamic Bone is imported.
        /// </summary>
        /// <returns>true when Dynamic Bone is imported.</returns>
        internal static bool IsDynamicBoneImported()
        {
            return SystemUtility.GetTypeByName("DynamicBone") != null;
        }
    }
}
