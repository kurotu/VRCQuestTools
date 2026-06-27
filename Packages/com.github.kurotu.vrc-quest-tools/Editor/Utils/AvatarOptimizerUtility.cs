// <copyright file="AvatarOptimizerUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Avatar Optimizer utilities.
    /// </summary>
    internal static class AvatarOptimizerUtility
    {
        /// <summary>
        /// Checks if the Avatar Optimizer package is imported.
        /// </summary>
        /// <returns>True if imported, false otherwise.</returns>
        internal static bool IsAvatarOptimizerImported()
        {
#if VQT_AVATAR_OPTIMIZER
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Checks if a GameObject has a TraceAndOptimize component.
        /// </summary>
        /// <param name="gameObject">GameObject to check.</param>
        /// <returns>True if the component exists, false otherwise.</returns>
        internal static bool HasTraceAndOptimizeComponent(GameObject gameObject)
        {
#if VQT_AVATAR_OPTIMIZER
            return gameObject.GetComponent<Anatawa12.AvatarOptimizer.TraceAndOptimize>() != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Adds a TraceAndOptimize component to a GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject to add the component to.</param>
        internal static void AddTraceAndOptimizeComponent(GameObject gameObject)
        {
#if VQT_AVATAR_OPTIMIZER
            Undo.AddComponent<Anatawa12.AvatarOptimizer.TraceAndOptimize>(gameObject);
#else
            throw new System.InvalidProgramException("Cannot add TraceAndOptimize component: Avatar Optimizer is not installed.");
#endif
        }
    }
}
