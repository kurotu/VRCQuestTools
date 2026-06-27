// <copyright file="VRCPhysBoneColliderProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Provider for VRCPhysBoneCollider components with abstracted access.
    /// </summary>
    internal class VRCPhysBoneColliderProvider : IVRCAvatarDynamicsProvider
    {
        private readonly VRCPhysBoneCollider collider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VRCPhysBoneColliderProvider"/> class.
        /// </summary>
        /// <param name="component">The VRCPhysBoneCollider component to wrap.</param>
        internal VRCPhysBoneColliderProvider(VRCPhysBoneCollider component)
        {
            collider = component;
        }

        /// <inheritdoc/>
        public Component Component => collider;

        /// <inheritdoc/>
        public GameObject GameObject => collider.gameObject;

        /// <inheritdoc/>
        public AvatarDynamicsComponentType ComponentType => AvatarDynamicsComponentType.PhysBoneCollider;

        /// <summary>
        /// Gets the shape type of the collider.
        /// </summary>
        public VRCPhysBoneCollider.ShapeType ShapeType => collider.shapeType;

        /// <summary>
        /// Gets the radius of the collider.
        /// </summary>
        public float Radius => collider.radius;

        /// <summary>
        /// Gets the height of the collider (for capsule type).
        /// </summary>
        public float Height => collider.height;
    }
}