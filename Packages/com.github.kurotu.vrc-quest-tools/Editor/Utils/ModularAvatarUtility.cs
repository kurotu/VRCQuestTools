// <copyright file="ModularAvatarUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Modular Avatar utilities.
    /// </summary>
    internal static class ModularAvatarUtility
    {
        /// <summary>
        /// Gets unsupported MA components for Android.
        /// </summary>
        /// <param name="gameObject">GameObject to inspect.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        /// <returns>Unsupported components.</returns>
        internal static Component[] GetUnsupportedComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            return new Component[] { };
        }

        /// <summary>
        /// Remove unsupported MA components for Android.
        /// </summary>
        /// <param name="gameObject">GameObject to inspect.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        internal static void RemoveUnsupportedComponents(GameObject gameObject, bool includeInactive)
        {
            var components = GetUnsupportedComponentsInChildren(gameObject, includeInactive);
            foreach (var component in components)
            {
                var obj = component.gameObject;
                var message = $"[{VRCQuestTools.Name}] Removed {component.GetType().Name} from {obj.name}";
                UnityEngine.Object.DestroyImmediate(component);
                Debug.Log(message, obj);
            }
        }
    }
}
