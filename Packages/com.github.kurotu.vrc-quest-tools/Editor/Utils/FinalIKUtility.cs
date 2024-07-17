// <copyright file="FinalIKUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// FinalIK Utility.
    /// </summary>
    internal static class FinalIKUtility
    {
        /// <summary>
        /// Gets FinalIK component types.
        /// </summary>
        internal static IEnumerable<Type> ComponentTypes => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(IsFinalIKComponent);

        /// <summary>
        /// Whether a component is from FinalIK.
        /// </summary>
        /// <param name="component">Component to check.</param>
        /// <returns>FinalIK: true.</returns>
        internal static bool IsFinalIKComponent(Component component)
        {
            return IsFinalIKComponent(component.GetType());
        }

        /// <summary>
        /// Whether a type is from FinalIK.
        /// </summary>
        /// <param name="type">Component type to check.</param>
        /// <returns>FinalIK: true.</returns>
        internal static bool IsFinalIKComponent(System.Type type)
        {
            if (type.FullName.StartsWith("RootMotion.FinalIK."))
            {
                return true;
            }
            return false;
        }
    }
}
