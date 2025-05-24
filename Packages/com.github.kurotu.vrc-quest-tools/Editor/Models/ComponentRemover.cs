// <copyright file="ComponentRemover.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Remove unsupported components for Quest.
    /// </summary>
    internal class ComponentRemover
    {
        /// <summary>
        /// Whether a component is not supported for Quest.
        /// </summary>
        /// <param name="component">Component to check.</param>
        /// <returns>true when unsuported.</returns>
        internal bool IsUnsupportedComponent(Component component)
        {
            return IsUnsupportedComponent(component.GetType());
        }

        /// <summary>
        /// Whether a component type is not supported for Quest.
        /// </summary>
        /// <param name="type">Component type to check.</param>
        /// <returns>true when unsuported.</returns>
        internal virtual bool IsUnsupportedComponent(System.Type type)
        {
            return VRCSDKUtility.IsUnsupportedComponentType(type);
        }

        /// <summary>
        /// Gets unsupported components for Quest.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        /// <returns>Unsupported components.</returns>
        internal virtual Component[] GetUnsupportedComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            return VRCSDKUtility.UnsupportedComponentTypes
                .SelectMany(type => gameObject.GetComponentsInChildren(type, includeInactive))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Remove unsupported components for Quest.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        /// <param name="canUndo">Whether can undo.</param>
        /// <param name="allowedComponents">Excludable list of component types.</param>
        internal void RemoveUnsupportedComponentsInChildren(GameObject gameObject, bool includeInactive, bool canUndo, System.Type[] allowedComponents)
        {
            foreach (var c in GetUnsupportedComponentsInChildren(gameObject, includeInactive))
            {
                if (allowedComponents.FirstOrDefault(allowed => allowed.IsAssignableFrom(c.GetType())) != null)
                {
                    continue;
                }
                var obj = c.gameObject;
                var message = $"[{VRCQuestTools.Name}] Removed {c.GetType().Name} from {c.gameObject.name}";
                if (canUndo)
                {
                    Undo.DestroyObjectImmediate(c);
                }
                else
                {
                    Object.DestroyImmediate(c);
                }
                Debug.Log(message, obj);
            }
        }

        /// <summary>
        /// Remove unsupported components for Quest.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        /// <param name="canUndo">Whether can undo.</param>
        internal void RemoveUnsupportedComponentsInChildren(GameObject gameObject, bool includeInactive, bool canUndo = false)
        {
            RemoveUnsupportedComponentsInChildren(gameObject, includeInactive, canUndo, new System.Type[] { });
        }
    }
}
