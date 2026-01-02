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
        private readonly AAOMergePhysBoneReflectionInfo reflectionInfo;
        private readonly Component mergePhysBoneComponent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AAOMergePhysBoneProvider"/> class.
        /// </summary>
        /// <param name="component">The AAO MergePhysBone component.</param>
        /// <param name="reflectionInfo">Reflection info for AAO MergePhysBone. Uses defaults when null.</param>
        internal AAOMergePhysBoneProvider(Component component, AAOMergePhysBoneReflectionInfo reflectionInfo = null)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            this.reflectionInfo = reflectionInfo ?? AAOMergePhysBoneReflectionInfo.Default;
            if (this.reflectionInfo == null)
            {
                throw new ArgumentException("AAO MergePhysBone reflection info is not available.", nameof(component));
            }

            mergePhysBoneComponent = component;

            if (this.reflectionInfo.MergePhysBoneType == null || !this.reflectionInfo.MergePhysBoneType.IsInstanceOfType(component))
            {
                throw new ArgumentException($"Component is not an instance of {this.reflectionInfo.MergePhysBoneType.FullName}", nameof(component));
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
                // Access the componentsSet field using injected reflection info
                var componentsSet = reflectionInfo.ComponentsSetField.GetValue(mergePhysBoneComponent);
                if (componentsSet == null)
                {
                    return Array.Empty<VRCPhysBone>();
                }

                var listObj = reflectionInfo.GetAsListMethod.Invoke(componentsSet, null);
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

        /// <summary>
        /// Reflection cache for AAO MergePhysBone members.
        /// </summary>
        internal sealed class AAOMergePhysBoneReflectionInfo
        {
            internal static readonly AAOMergePhysBoneReflectionInfo Default = CreateDefault();

            private const string ComponentsSetFieldName = "componentsSet";
            private const string GetAsListMethodName = "GetAsList";
            private const string MergePhysBoneTypeName = "Anatawa12.AvatarOptimizer.MergePhysBone";
            private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            /// <summary>
            /// Initializes a new instance of the <see cref="AAOMergePhysBoneReflectionInfo"/> class.
            /// </summary>
            /// <param name="mergePhysBoneTypeName">Fully qualified type name of the MergePhysBone component.</param>
            /// <param name="componentsSetFieldName">Field name storing the underlying components set.</param>
            /// <param name="getAsListMethodName">Method name that converts the components set to a list.</param>
            internal AAOMergePhysBoneReflectionInfo(string mergePhysBoneTypeName, string componentsSetFieldName, string getAsListMethodName)
            {
                if (string.IsNullOrEmpty(mergePhysBoneTypeName))
                {
                    throw new ArgumentNullException(nameof(mergePhysBoneTypeName));
                }

                if (string.IsNullOrEmpty(componentsSetFieldName))
                {
                    throw new ArgumentNullException(nameof(componentsSetFieldName));
                }

                if (string.IsNullOrEmpty(getAsListMethodName))
                {
                    throw new ArgumentNullException(nameof(getAsListMethodName));
                }

                MergePhysBoneType = Utils.SystemUtility.GetTypeByName(mergePhysBoneTypeName) ?? throw new ArgumentException($"Type {mergePhysBoneTypeName} not found.", nameof(mergePhysBoneTypeName));

                ComponentsSetField = MergePhysBoneType.GetField(componentsSetFieldName, MemberBindingFlags) ?? throw new ArgumentException($"Field {componentsSetFieldName} not found on {mergePhysBoneTypeName}.", nameof(componentsSetFieldName));

                GetAsListMethod = ComponentsSetField.FieldType.GetMethod(getAsListMethodName, MemberBindingFlags) ?? throw new ArgumentException($"Method {getAsListMethodName} not found on {ComponentsSetField.FieldType.FullName}.", nameof(getAsListMethodName));
            }

            /// <summary>
            /// Gets the MergePhysBone type resolved from the supplied name.
            /// </summary>
            internal Type MergePhysBoneType { get; }

            /// <summary>
            /// Gets the field that contains the set of merged components.
            /// </summary>
            internal FieldInfo ComponentsSetField { get; }

            /// <summary>
            /// Gets the method that converts the components set to a list.
            /// </summary>
            internal MethodInfo GetAsListMethod { get; }

            private static AAOMergePhysBoneReflectionInfo CreateDefault()
            {
                try
                {
                    return new AAOMergePhysBoneReflectionInfo(MergePhysBoneTypeName, ComponentsSetFieldName, GetAsListMethodName);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
        }
    }
}
