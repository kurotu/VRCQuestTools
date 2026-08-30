// <copyright file="ObjectIdentity.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Provides session-local identifiers for Unity objects.
    /// </summary>
    internal static class ObjectIdentity
    {
        /// <summary>
        /// Gets a session-local identifier for the object.
        /// Unity 6000.4 deprecated Object.GetInstanceID() in favor of Object.GetEntityId(),
        /// and 6000.5 turned it into a compile error.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <returns>Identifier which is unique within the current session.</returns>
        internal static ulong GetId(UnityEngine.Object obj)
        {
#if UNITY_6000_4_OR_NEWER
            return UnityEngine.EntityId.ToULong(obj.GetEntityId());
#else
            return unchecked((uint)obj.GetInstanceID());
#endif
        }
    }
}
