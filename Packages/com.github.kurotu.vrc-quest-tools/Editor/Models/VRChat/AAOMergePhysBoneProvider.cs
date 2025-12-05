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
        private const string MergePhysBoneTypeName = "Anatawa12.AvatarOptimizer.MergePhysBone";
        private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly Component mergePhysBoneComponent;
        private readonly FieldInfo componentsSetField;

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
            var mergePhysBoneType = Utils.SystemUtility.GetTypeByName(MergePhysBoneTypeName);

            if (mergePhysBoneType == null || !mergePhysBoneType.IsInstanceOfType(component))
            {
                throw new ArgumentException($"Component is not an instance of {MergePhysBoneTypeName}", nameof(component));
            }

            // Cache reflection members and verify this is an AAO MergePhysBone component
            componentsSetField = mergePhysBoneType.GetField(ComponentsSetFieldName, MemberBindingFlags);
            if (componentsSetField == null)
            {
                throw new ArgumentException($"Component does not have a '{ComponentsSetFieldName}' field", nameof(component));
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
                // Access the componentsSet field using reflection
                var componentsSet = componentsSetField.GetValue(mergePhysBoneComponent);
                if (componentsSet == null)
                {
                    return Array.Empty<VRCPhysBone>();
                }

                // Try to get the GetEnumerator method to iterate through the set
                var getEnumeratorMethod = componentsSet.GetType().GetMethod("GetEnumerator");
                if (getEnumeratorMethod == null)
                {
                    return Array.Empty<VRCPhysBone>();
                }

                var enumeratorObj = getEnumeratorMethod.Invoke(componentsSet, null);
                if (enumeratorObj is not System.Collections.IEnumerator enumerator)
                {
                    return Array.Empty<VRCPhysBone>();
                }

                var result = new List<VRCPhysBone>();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current is VRCPhysBone physBone)
                    {
                        result.Add(physBone);
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
