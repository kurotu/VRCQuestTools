// <copyright file="VRCPhysBoneProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Default implementation of VRCPhysBoneProviderBase that wraps a VRCPhysBone component.
    /// </summary>
    internal class VRCPhysBoneProvider : VRCPhysBoneProviderBase
    {
        private readonly VRCPhysBone physBone;

        /// <summary>
        /// Initializes a new instance of the <see cref="VRCPhysBoneProvider"/> class.
        /// </summary>
        /// <param name="component">The VRCPhysBone component to wrap.</param>
        internal VRCPhysBoneProvider(VRCPhysBone component)
        {
            physBone = component;
        }

        /// <inheritdoc/>
        public override Component Component => physBone;

        /// <inheritdoc/>
        public override GameObject GameObject => physBone.gameObject;

        /// <inheritdoc/>
        public override Transform RootTransform => physBone.rootTransform == null ? physBone.gameObject.transform : physBone.rootTransform;

        /// <inheritdoc/>
        public override List<Transform> IgnoreTransforms => physBone.ignoreTransforms;

        /// <inheritdoc/>
        public override Vector3 EndpointPosition => physBone.endpointPosition;

        /// <inheritdoc/>
        public override MultiChildType MultiChildType => (MultiChildType)(int)physBone.multiChildType;

        /// <inheritdoc/>
        public override List<Component> Colliders => physBone.colliders.Cast<Component>().ToList();

        /// <inheritdoc/>
        public override float Radius => physBone.radius;

        /// <inheritdoc/>
        public override AnimationCurve RadiusCurve => physBone.radiusCurve;

        /// <inheritdoc/>
        public override void ClearCollider(int index)
        {
            if (index >= 0 && index < physBone.colliders.Count)
            {
                physBone.colliders[index] = null;
            }
        }

        /// <inheritdoc/>
        public override VRCPhysBone[] GetPhysBones()
        {
            if (physBone == null)
            {
                return new VRCPhysBone[] { };
            }
            return new VRCPhysBone[] { physBone };
        }
    }
}
