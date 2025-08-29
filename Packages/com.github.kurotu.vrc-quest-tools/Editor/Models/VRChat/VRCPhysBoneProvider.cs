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
    /// Default implementation of IVRCPhysBoneProvider that wraps a VRCPhysBone component.
    /// </summary>
    internal class VRCPhysBoneProvider : IVRCPhysBoneProvider
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
        public Component Component => physBone;

        /// <inheritdoc/>
        public GameObject GameObject => physBone.gameObject;

        /// <inheritdoc/>
        public Transform RootTransform => physBone.rootTransform;

        /// <inheritdoc/>
        public List<Transform> IgnoreTransforms => physBone.ignoreTransforms;

        /// <inheritdoc/>
        public Vector3 EndpointPosition => physBone.endpointPosition;

        /// <inheritdoc/>
        public MultiChildType MultiChildType => (MultiChildType)(int)physBone.multiChildType;

        /// <inheritdoc/>
        public List<Component> Colliders => physBone.colliders.Cast<Component>().ToList();

        /// <inheritdoc/>
        public void ClearCollider(int index)
        {
            if (index >= 0 && index < physBone.colliders.Count)
            {
                physBone.colliders[index] = null;
            }
        }
    }
}