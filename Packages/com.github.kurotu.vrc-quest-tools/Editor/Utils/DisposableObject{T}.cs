// <copyright file="DisposableObjectT.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEditor;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// UnityEngine.Object wrapped with IDisposable.
    /// </summary>
    /// <typeparam name="T">UnityEngine.Object to wrap.</typeparam>
    internal class DisposableObject<T> : System.IDisposable
        where T : UnityEngine.Object
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableObject{T}"/> class.
        /// </summary>
        /// <param name="obj">object to wrap.</param>
        internal DisposableObject(T obj)
        {
            this.Object = obj;
        }

        /// <summary>
        /// Gets the wrapped object.
        /// </summary>
        internal T Object { get; }

        /// <summary>
        /// DestroyImmediate the object.
        /// </summary>
        public void Dispose()
        {
            if (this.Object != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this.Object)))
            {
                UnityEngine.Object.DestroyImmediate(this.Object);
            }
        }
    }
}
