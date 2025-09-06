// <copyright file="VRCContactBaseProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;
using VRC.Dynamics;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Provider for ContactBase components with abstracted access.
    /// </summary>
    internal class VRCContactBaseProvider : IVRCAvatarDynamicsProvider
    {
        private readonly ContactBase contact;

        /// <summary>
        /// Initializes a new instance of the <see cref="VRCContactBaseProvider"/> class.
        /// </summary>
        /// <param name="component">The ContactBase component to wrap.</param>
        internal VRCContactBaseProvider(ContactBase component)
        {
            contact = component;
        }

        /// <inheritdoc/>
        public Component Component => contact;

        /// <inheritdoc/>
        public GameObject GameObject => contact.gameObject;

        /// <inheritdoc/>
        public AvatarDynamicsComponentType ComponentType => AvatarDynamicsComponentType.Contact;

        /// <summary>
        /// Gets the radius of the contact.
        /// </summary>
        public float Radius => contact.radius;
    }
}