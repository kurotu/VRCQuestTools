// <copyright file="IVRCAvatarDynamicsProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Interface for providing abstracted access to Avatar Dynamics components.
    /// </summary>
    internal interface IVRCAvatarDynamicsProvider
    {
        /// <summary>
        /// Gets the underlying component.
        /// </summary>
        Component Component { get; }

        /// <summary>
        /// Gets the GameObject that contains the component.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Gets the component type for preview rendering.
        /// </summary>
        AvatarDynamicsComponentType ComponentType { get; }
    }

    /// <summary>
    /// Enumeration for Avatar Dynamics component types.
    /// </summary>
    internal enum AvatarDynamicsComponentType
    {
        /// <summary>
        /// VRCPhysBone component type.
        /// </summary>
        PhysBone,

        /// <summary>
        /// VRCPhysBoneCollider component type.
        /// </summary>
        PhysBoneCollider,

        /// <summary>
        /// ContactBase component type.
        /// </summary>
        Contact,
    }
}