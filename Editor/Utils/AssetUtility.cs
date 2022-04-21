// <copyright file="AssetUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for assets.
    /// </summary>
    internal static class AssetUtility
    {
        /// <summary>
        /// Type object of DynamicBone.
        /// </summary>
        internal static Type DynamicBoneType = SystemUtility.GetTypeByName("DynamicBone");

        /// <summary>
        /// Gets whether Dynamic Bone is imported.
        /// </summary>
        /// <returns>true when Dynamic Bone is imported.</returns>
        internal static bool IsDynamicBoneImported()
        {
            return DynamicBoneType != null;
        }

        /// <summary>
        /// Gets whether lilToon is imported.
        /// </summary>
        /// <returns>true when lilToon shader and lilToonInspector are imported.</returns>
        internal static bool IsLilToonImported()
        {
            var shader = Shader.Find("lilToon");
            var inspector = SystemUtility.GetTypeByName("lilToon.lilToonInspector");
            return (shader != null) && (inspector != null);
        }
    }
}
