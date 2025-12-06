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
        private const string ComponentsSetFieldName = "componentsSet";
        private const string GetAsListMethodName = "GetAsList";
        private const string MergePhysBoneTypeName = "Anatawa12.AvatarOptimizer.MergePhysBone";
        private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        // Static caches for reflection results to improve performance
        private static Type cachedMergePhysBoneType;
        private static FieldInfo cachedComponentsSetField;
        private static MethodInfo cachedGetAsListMethod;

        private readonly Component mergePhysBoneComponent;

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

            // Initialize static caches if not already done
            if (cachedMergePhysBoneType == null)
            {
                cachedMergePhysBoneType = Utils.SystemUtility.GetTypeByName(MergePhysBoneTypeName);
            }

            if (cachedMergePhysBoneType == null || !cachedMergePhysBoneType.IsInstanceOfType(component))
            {
                throw new ArgumentException($"Component is not an instance of {MergePhysBoneTypeName}", nameof(component));
            }

            // Cache componentsSet field if not already cached
            if (cachedComponentsSetField == null)
            {
                cachedComponentsSetField = cachedMergePhysBoneType.GetField(ComponentsSetFieldName, MemberBindingFlags);
                if (cachedComponentsSetField == null)
                {
                    throw new ArgumentException($"Component does not have a '{ComponentsSetFieldName}' field", nameof(component));
                }
            }
        }

        /// <inheritdoc/>
        public override Component Component => mergePhysBoneComponent;

        /// <inheritdoc/>
        public override GameObject GameObject => mergePhysBoneComponent.gameObject;

        /// <inheritdoc/>
        public override Transform RootTransform => mergePhysBoneComponent.transform;

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
        public override MultiChildType MultiChildType => MultiChildType.Ignore;

        /// <inheritdoc/>
        public override List<Component> Colliders
        {
            get
            {
                var physBones = GetPhysBones();
                var allColliders = new List<Component>();
                foreach (var pb in physBones.Where(pb => pb != null))
                {
                    foreach (var collider in pb.colliders)
                    {
                        if (collider != null)
                        {
                            allColliders.Add(collider);
                        }
                    }
                }
                return allColliders.Distinct().ToList();
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
            try
            {
                // Access the componentsSet field using cached reflection
                var componentsSet = cachedComponentsSetField.GetValue(mergePhysBoneComponent);
                if (componentsSet == null)
                {
                    return Array.Empty<VRCPhysBone>();
                }

                // Cache GetAsList method if not already cached
                if (cachedGetAsListMethod == null)
                {
                    cachedGetAsListMethod = componentsSet.GetType().GetMethod(GetAsListMethodName);
                    if (cachedGetAsListMethod == null)
                    {
                        return Array.Empty<VRCPhysBone>();
                    }
                }

                var listObj = cachedGetAsListMethod.Invoke(componentsSet, null);
                if (listObj == null)
                {
                    return Array.Empty<VRCPhysBone>();
                }

                // The list contains VRCPhysBoneBase, we need to filter to VRCPhysBone only
                var result = new List<VRCPhysBone>();
                if (listObj is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is VRCPhysBone physBone)
                        {
                            result.Add(physBone);
                        }
                    }
                }

                return result.ToArray();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is TargetException || ex is FieldAccessException || ex is NotSupportedException)
            {
                Debug.LogWarning($"Failed to access componentsSet from AAO MergePhysBone component: {ex.Message}");
            }

            return Array.Empty<VRCPhysBone>();
        }
    }
}
