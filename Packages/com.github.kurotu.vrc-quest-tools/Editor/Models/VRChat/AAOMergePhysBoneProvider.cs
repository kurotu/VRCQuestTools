// <copyright file="AAOMergePhysBoneProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Provider for AAO (Avatar Optimizer) MergePhysBone component.
    /// Uses reflection to access internal AAO components.
    /// </summary>
    internal class AAOMergePhysBoneProvider : VRCPhysBoneProviderBase
    {
        private readonly Component mergePhysBoneComponent;
        private readonly Type mergePhysBoneType;
        private VRCPhysBone[] cachedPhysBones;

        /// <summary>
        /// Initializes a new instance of the <see cref="AAOMergePhysBoneProvider"/> class.
        /// </summary>
        /// <param name="component">The AAO MergePhysBone component.</param>
        internal AAOMergePhysBoneProvider(Component component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            mergePhysBoneComponent = component;
            mergePhysBoneType = component.GetType();

            // Verify this is an AAO MergePhysBone component by checking for the physBones field
            var physBonesField = mergePhysBoneType.GetField("physBones", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var physBonesProperty = mergePhysBoneType.GetProperty("physBones", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (physBonesField == null && physBonesProperty == null)
            {
                throw new ArgumentException("Component does not have a 'physBones' field or property", nameof(component));
            }
        }

        /// <inheritdoc/>
        public override Component Component => mergePhysBoneComponent;

        /// <inheritdoc/>
        public override GameObject GameObject => mergePhysBoneComponent.gameObject;

        /// <inheritdoc/>
        public override Transform RootTransform
        {
            get
            {
                var physBones = GetPhysBones();
                if (physBones.Length > 0 && physBones[0] != null)
                {
                    return physBones[0].rootTransform == null ? physBones[0].transform : physBones[0].rootTransform;
                }
                return mergePhysBoneComponent.transform;
            }
        }

        /// <inheritdoc/>
        public override List<Transform> IgnoreTransforms
        {
            get
            {
                var physBones = GetPhysBones();
                var ignoreTransforms = new List<Transform>();
                foreach (var pb in physBones.Where(pb => pb != null))
                {
                    ignoreTransforms.AddRange(pb.ignoreTransforms);
                }
                return ignoreTransforms.Distinct().ToList();
            }
        }

        /// <inheritdoc/>
        public override Vector3 EndpointPosition
        {
            get
            {
                var physBones = GetPhysBones();
                if (physBones.Length > 0 && physBones[0] != null)
                {
                    return physBones[0].endpointPosition;
                }
                return Vector3.zero;
            }
        }

        /// <inheritdoc/>
        public override MultiChildType MultiChildType
        {
            get
            {
                var physBones = GetPhysBones();
                if (physBones.Length > 0 && physBones[0] != null)
                {
                    return (MultiChildType)(int)physBones[0].multiChildType;
                }
                return MultiChildType.Ignore;
            }
        }

        /// <inheritdoc/>
        public override List<Component> Colliders
        {
            get
            {
                var physBones = GetPhysBones();
                var allColliders = new List<Component>();
                foreach (var pb in physBones.Where(pb => pb != null))
                {
                    allColliders.AddRange(pb.colliders.Cast<Component>());
                }
                return allColliders.Distinct().Where(c => c != null).ToList();
            }
        }

        /// <inheritdoc/>
        public override float Radius
        {
            get
            {
                var physBones = GetPhysBones();
                if (physBones.Length > 0 && physBones[0] != null)
                {
                    return physBones[0].radius;
                }
                return 0f;
            }
        }

        /// <inheritdoc/>
        public override AnimationCurve RadiusCurve
        {
            get
            {
                var physBones = GetPhysBones();
                if (physBones.Length > 0 && physBones[0] != null)
                {
                    return physBones[0].radiusCurve;
                }
                return null;
            }
        }

        /// <inheritdoc/>
        public override void ClearCollider(int index)
        {
            // MergePhysBone doesn't support direct collider modification
            // This operation is not applicable for merged PhysBones
        }

        /// <inheritdoc/>
        public override VRCPhysBone[] GetPhysBones()
        {
            if (cachedPhysBones != null)
            {
                return cachedPhysBones;
            }

            try
            {
                // Try to access the physBones field/property using reflection
                var physBonesField = mergePhysBoneType.GetField("physBones", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (physBonesField != null)
                {
                    var value = physBonesField.GetValue(mergePhysBoneComponent);
                    if (value is VRCPhysBone[] physBonesArray)
                    {
                        cachedPhysBones = physBonesArray.Where(pb => pb != null).ToArray();
                        return cachedPhysBones;
                    }
                    else if (value is List<VRCPhysBone> physBonesList)
                    {
                        cachedPhysBones = physBonesList.Where(pb => pb != null).ToArray();
                        return cachedPhysBones;
                    }
                }

                // Try property access
                var physBonesProperty = mergePhysBoneType.GetProperty("physBones", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (physBonesProperty != null)
                {
                    var value = physBonesProperty.GetValue(mergePhysBoneComponent);
                    if (value is VRCPhysBone[] physBonesArray)
                    {
                        cachedPhysBones = physBonesArray.Where(pb => pb != null).ToArray();
                        return cachedPhysBones;
                    }
                    else if (value is List<VRCPhysBone> physBonesList)
                    {
                        cachedPhysBones = physBonesList.Where(pb => pb != null).ToArray();
                        return cachedPhysBones;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Debug.LogWarning($"Failed to access physBones from AAO MergePhysBone component: {ex.Message}");
            }
            catch (TargetException ex)
            {
                Debug.LogWarning($"Failed to access physBones from AAO MergePhysBone component: {ex.Message}");
            }

            cachedPhysBones = Array.Empty<VRCPhysBone>();
            return cachedPhysBones;
        }
    }
}
