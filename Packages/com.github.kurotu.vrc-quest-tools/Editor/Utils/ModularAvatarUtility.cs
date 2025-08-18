// <copyright file="ModularAvatarUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Modular Avatar utilities.
    /// </summary>
    internal static class ModularAvatarUtility
    {
        /// <summary>
        /// Checks if the Modular Avatar package is imported.
        /// </summary>
        /// <returns>True if imported, false otherwise.</returns>
        internal static bool IsModularAvatarImported()
        {
#if VQT_MODULAR_AVATAR
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Checks if the Modular Avatar package is using a legacy version.
        /// </summary>
        /// <returns>True if legacy version, false otherwise.</returns>
        internal static bool IsLegacyVersion()
        {
#if VQT_MODULAR_AVATAR_LEGACY
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Checks if the Modular Avatar package is using a breaking version.
        /// </summary>
        /// <returns>True if breaking version, false otherwise.</returns>
        internal static bool IsBreakingVersion()
        {
#if VQT_MODULAR_AVATAR_BREAKING
            return true;
#else
            return false;
#endif
        }

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

        /// <summary>
        /// Checks if a GameObject has a ModularAvatarConvertConstraints component.
        /// </summary>
        /// <param name="gameObject">GameObject to check.</param>
        /// <returns>True if the component exists, false otherwise.</returns>
        internal static bool HasConvertConstraintsComponent(GameObject gameObject)
        {
#if VQT_MODULAR_AVATAR && !VQT_MODULAR_AVATAR_LEGACY && !VQT_MODULAR_AVATAR_BREAKING
            return gameObject.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>() != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Adds a ModularAvatarConvertConstraints component to a GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject to add the component to.</param>
        internal static void AddConvertConstraintsComponent(GameObject gameObject)
        {
#if VQT_MODULAR_AVATAR && !VQT_MODULAR_AVATAR_LEGACY && !VQT_MODULAR_AVATAR_BREAKING
            Undo.AddComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>(gameObject);
#else
            throw new System.InvalidProgramException("Cannot add ModularAvatarConvertConstraints component: Modular Avatar package is not installed or version is incompatible.");
#endif
        }

        /// <summary>
        /// Gets or adds a ModularAvatarConvertConstraints component to a GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject to get or add the component to.</param>
        /// <returns>The component, or null if Modular Avatar is not available.</returns>
        internal static Component GetOrAddConvertConstraintsComponent(GameObject gameObject)
        {
#if VQT_MODULAR_AVATAR && !VQT_MODULAR_AVATAR_LEGACY && !VQT_MODULAR_AVATAR_BREAKING
            var component = gameObject.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>();
            if (component == null)
            {
                component = gameObject.AddComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>();
            }
            return component;
#else
            throw new System.InvalidProgramException("Cannot get or add ModularAvatarConvertConstraints component: Modular Avatar package is not installed or version is incompatible.");
#endif
        }

        /// <summary>
        /// Gets all ModularAvatarMergeAnimator components in children.
        /// </summary>
        /// <param name="gameObject">Root GameObject to search.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        /// <returns>Array of ModularAvatarMergeAnimator components.</returns>
        internal static Component[] GetMergeAnimatorComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
#if VQT_MODULAR_AVATAR && !VQT_MODULAR_AVATAR_LEGACY && !VQT_MODULAR_AVATAR_BREAKING
            return gameObject.GetComponentsInChildren<nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator>(includeInactive)
                .Cast<Component>().ToArray();
#else
            return new Component[] { };
#endif
        }

        /// <summary>
        /// Gets the animator property from a ModularAvatarMergeAnimator component.
        /// </summary>
        /// <param name="component">The ModularAvatarMergeAnimator component.</param>
        /// <returns>The animator controller, or null if not applicable.</returns>
        internal static RuntimeAnimatorController GetMergeAnimatorController(Component component)
        {
#if VQT_MODULAR_AVATAR && !VQT_MODULAR_AVATAR_LEGACY && !VQT_MODULAR_AVATAR_BREAKING
            if (component is nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator ma)
            {
                return ma.animator;
            }
#endif
            string typeName = (component == null) ? "null" : component.GetType().FullName;
            throw new System.ArgumentException($"Component is not ModularAvatarMergeAnimator: {typeName}");
        }

        /// <summary>
        /// Sets the animator property of a ModularAvatarMergeAnimator component.
        /// </summary>
        /// <param name="component">The ModularAvatarMergeAnimator component.</param>
        /// <param name="controller">The animator controller to set.</param>
        internal static void SetMergeAnimatorController(Component component, RuntimeAnimatorController controller)
        {
#if VQT_MODULAR_AVATAR && !VQT_MODULAR_AVATAR_LEGACY && !VQT_MODULAR_AVATAR_BREAKING
            if (component is nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator ma)
            {
                ma.animator = controller;
                return;
            }
#endif
            string typeName = (component == null) ? "null" : component.GetType().FullName;
            throw new System.ArgumentException($"Component is not ModularAvatarMergeAnimator: {typeName}");
        }
    }
}
