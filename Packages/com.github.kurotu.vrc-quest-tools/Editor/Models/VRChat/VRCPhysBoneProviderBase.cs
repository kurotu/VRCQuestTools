// <copyright file="VRCPhysBoneProviderBase.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Base class for providing abstracted access to VRCPhysBone functionality.
    /// </summary>
    [Serializable]
    internal abstract class VRCPhysBoneProviderBase
    {
        /// <summary>
        /// Gets the underlying VRCPhysBone component.
        /// </summary>
        public abstract Component Component { get; }

        /// <summary>
        /// Gets the GameObject that contains the PhysBone component.
        /// </summary>
        public abstract GameObject GameObject { get; }

        /// <summary>
        /// Gets the root transform set by inspector.
        /// </summary>
        public abstract Transform RootTransform { get; }

        /// <summary>
        /// Gets the ignore transforms set by inspector.
        /// </summary>
        public abstract List<Transform> IgnoreTransforms { get; }

        /// <summary>
        /// Gets the endpoint position set by inspector.
        /// </summary>
        public abstract Vector3 EndpointPosition { get; }

        /// <summary>
        /// Gets the multi child type set by inspector.
        /// </summary>
        public abstract MultiChildType MultiChildType { get; }

        /// <summary>
        /// Gets the PhysBoneCollider instances.
        /// </summary>
        public abstract List<Component> Colliders { get; }

        /// <summary>
        /// Gets the radius value set by inspector.
        /// </summary>
        public abstract float Radius { get; }

        /// <summary>
        /// Gets the radius curve set by inspector.
        /// </summary>
        public abstract AnimationCurve RadiusCurve { get; }

        /// <summary>
        /// Sets null to PhysBoneCollider at index.
        /// </summary>
        /// <param name="index">Index to set null.</param>
        public abstract void ClearCollider(int index);
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