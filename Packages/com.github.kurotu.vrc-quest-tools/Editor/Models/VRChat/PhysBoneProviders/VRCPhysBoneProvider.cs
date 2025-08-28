// <copyright file="VRCPhysBoneProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Utils;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Models.VRChat.PhysBoneProviders
{
    /// <summary>
    /// Default implementation of IVRCPhysBoneProvider that wraps a VRCPhysBone component.
    /// </summary>
    internal class VRCPhysBoneProvider : IVRCPhysBoneProvider
    {
        private static readonly FieldInfo RootTransformField = VRCSDKUtility.PhysBoneType?.GetField("rootTransform");
        private static readonly FieldInfo IgnoreTransformsField = VRCSDKUtility.PhysBoneType?.GetField("ignoreTransforms");
        private static readonly FieldInfo EndpointPositionField = VRCSDKUtility.PhysBoneType?.GetField("endpointPosition");
        private static readonly FieldInfo MultiChildTypeField = VRCSDKUtility.PhysBoneType?.GetField("multiChildType");
        private static readonly FieldInfo CollidersField = VRCSDKUtility.PhysBoneType?.GetField("colliders");

        /// <summary>
        /// Initializes a new instance of the <see cref="VRCPhysBoneProvider"/> class.
        /// </summary>
        /// <param name="component">The VRCPhysBone component to wrap.</param>
        internal VRCPhysBoneProvider(VRCPhysBone component)
        {
            Component = component;
        }

        /// <inheritdoc/>
        public Component Component { get; }

        /// <inheritdoc/>
        public GameObject GameObject => Component.gameObject;

        /// <inheritdoc/>
        public Transform RootTransform => (Transform)RootTransformField?.GetValue(Component);

        /// <inheritdoc/>
        public List<Transform> IgnoreTransforms => (List<Transform>)IgnoreTransformsField?.GetValue(Component);

        /// <inheritdoc/>
        public Vector3 EndpointPosition => (Vector3)(EndpointPositionField?.GetValue(Component) ?? Vector3.zero);

        /// <inheritdoc/>
        public MultiChildType MultiChildType
        {
            get
            {
                var value = MultiChildTypeField?.GetValue(Component);
                if (value == null)
                {
                    return MultiChildType.Ignore;
                }

                // Convert from VRCPhysBoneBase.MultiChildType to our enum
                return (MultiChildType)(int)value;
            }
        }

        /// <inheritdoc/>
        public List<Component> Colliders
        {
            get
            {
                var colliders = CollidersField?.GetValue(Component);
                if (colliders == null)
                {
                    return new List<Component>();
                }

                dynamic[] c = Enumerable.ToArray((dynamic)colliders);
                return c.Cast<Component>().ToList();
            }
        }

        /// <inheritdoc/>
        public void ClearCollider(int index)
        {
            var colliders = CollidersField?.GetValue(Component);
            if (colliders != null)
            {
                ((dynamic)colliders)[index] = null;
            }
        }
    }
}