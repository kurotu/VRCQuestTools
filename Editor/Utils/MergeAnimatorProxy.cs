// <copyright file="MergeAnimatorProxy.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Proxy object for ModularAvatarMergeAnimator.
    /// </summary>
    internal class MergeAnimatorProxy
    {
        private readonly Component component;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeAnimatorProxy"/> class.
        /// </summary>
        /// <param name="component">ModularAvatarMergeAnimator Component.</param>
        internal MergeAnimatorProxy(Component component)
        {
            this.component = component;
        }

        /// <summary>
        /// Gets or sets RuntimeAnimatorController to be merged.
        /// </summary>
        internal RuntimeAnimatorController Animator
        {
            get => (RuntimeAnimatorController)AnimatorField.GetValue(component);
            set => AnimatorField.SetValue(component, value);
        }

        private static FieldInfo AnimatorField
        {
            get
            {
                if (ModularAvatarUtility.MergeAnimatorType == null)
                {
                    throw new InvalidProgramException("ModularAvatarMergeAnimator not found. Unsupported version of Modular Avatar.");
                }
                var field = ModularAvatarUtility.MergeAnimatorType.GetField("animator");
                if (field == null)
                {
                    throw new InvalidProgramException("animator field not found. Unsupported version of Modular Avatar.");
                }
                return field;
            }
        }
    }
}
