// <copyright file="DisposableObject.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Static class of DisposableObject.
    /// </summary>
    internal static class DisposableObject
    {
        /// <summary>
        /// Create a new instance of DisposableObject.
        /// </summary>
        /// <typeparam name="T">Type of UnityEngine.Object.</typeparam>
        /// <param name="obj">Object to wrap.</param>
        /// <returns>Wrapped object.</returns>
        internal static DisposableObject<T> New<T>(T obj)
            where T : UnityEngine.Object
        {
            return new DisposableObject<T>(obj);
        }
    }
}
