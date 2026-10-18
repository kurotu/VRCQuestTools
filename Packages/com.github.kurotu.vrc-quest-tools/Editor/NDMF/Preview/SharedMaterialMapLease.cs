// <copyright file="SharedMaterialMapLease.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Represents a lease for a shared mapping of source materials to converted preview materials.
    /// The lease keeps a reference to the cache entries that were acquired; calling <see cref="Release"/> will decrement
    /// the underlying cache reference counts and schedule any unreferenced converted materials for destruction.
    /// </summary>
    internal class SharedMaterialMapLease
    {
        private readonly Dictionary<string, SharedPreviewMaterialCache.CacheEntry> acquiredEntries;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedMaterialMapLease"/> class.
        /// </summary>
        /// <param name="materialMap">Mapping from original materials to converted (shared) preview materials.</param>
        /// <param name="acquiredEntries">The cache entries owned by this lease, indexed by their keys.</param>
        public SharedMaterialMapLease(Dictionary<Material, Material> materialMap, Dictionary<string, SharedPreviewMaterialCache.CacheEntry> acquiredEntries)
        {
            MaterialMap = materialMap;
            this.acquiredEntries = acquiredEntries;
        }

        /// <summary>
        /// Gets mapping from original <see cref="Material"/> to the converted (shared) <see cref="Material"/> instance.
        /// </summary>
        public Dictionary<Material, Material> MaterialMap { get; private set; }

        /// <summary>
        /// Release this lease. Idempotent: multiple calls have no further effect.
        /// After release, the underlying cache reference counts are decremented and the local material map is cleared.
        /// </summary>
        public void Release()
        {
            if (disposed)
            {
                return;
            }

            SharedPreviewMaterialCache.Release(acquiredEntries);
            MaterialMap.Clear();
            disposed = true;
        }
    }
}
