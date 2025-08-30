// <copyright file="IVRCPhysBoneProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Interface for providing abstracted access to VRCPhysBone functionality.
    /// </summary>
    [Serializable]
    internal interface IVRCPhysBoneProvider
    {
        /// <summary>
        /// Gets the underlying VRCPhysBone component.
        /// </summary>
        Component Component { get; }

        /// <summary>
        /// Gets the GameObject that contains the PhysBone component.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Gets the root transform set by inspector.
        /// </summary>
        Transform RootTransform { get; }

        /// <summary>
        /// Gets the ignore transforms set by inspector.
        /// </summary>
        List<Transform> IgnoreTransforms { get; }

        /// <summary>
        /// Gets the endpoint position set by inspector.
        /// </summary>
        Vector3 EndpointPosition { get; }

        /// <summary>
        /// Gets the multi child type set by inspector.
        /// </summary>
        MultiChildType MultiChildType { get; }

        /// <summary>
        /// Gets the PhysBoneCollider instances.
        /// </summary>
        List<Component> Colliders { get; }

        /// <summary>
        /// Sets null to PhysBoneCollider at index.
        /// </summary>
        /// <param name="index">Index to set null.</param>
        void ClearCollider(int index);
    }

    /// <summary>
    /// Multi child type enumeration matching VRCPhysBoneBase.MultiChildType.
    /// </summary>
    internal enum MultiChildType
    {
        /// <summary>
        /// VRCPhysBoneBase.MultiChildType.Ignore.
        /// </summary>
        Ignore,

        /// <summary>
        /// VRCPhysBoneBase.MultiChildType.First.
        /// </summary>
        First,

        /// <summary>
        /// VRCPhysBoneBase.MultiChildType.Average.
        /// </summary>
        Average,
    }
}