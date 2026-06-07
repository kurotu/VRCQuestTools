// <copyright file="SharedPreviewMaterialCache.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

#pragma warning disable SA1414 // tuple element names

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Shared cache of preview materials for NDMF conversions.
    /// Holds converted materials keyed by build target + conversion settings + material content key.
    /// Uses reference counting so multiple requesters can share converted materials; callers must release the
    /// returned <see cref="SharedMaterialMapLease"/> to decrement reference counts and allow cleanup.
    /// </summary>
    internal static class SharedPreviewMaterialCache
    {
        private static readonly object SyncRoot = new();
        private static readonly Dictionary<string, CacheEntry> Entries = new();

        /// <summary>
        /// Acquire a lease for converted preview materials corresponding to <paramref name="settingsMap"/>.
        /// Existing cached conversions are reused (reference count incremented). For entries not yet cached,
        /// <paramref name="convertFunc"/> is invoked to perform a batch conversion. The returned <see cref="SharedMaterialMapLease"/>
        /// must be released to decrement reference counts and allow cleanup.
        /// </summary>
        /// <param name="settingsMap">Map from source <see cref="Material"/> to conversion settings.</param>
        /// <param name="convertFunc">Delegate that performs batch material conversion and returns a mapping from source material to converted material.</param>
        /// <returns>A <see cref="SharedMaterialMapLease"/> containing the <see cref="MaterialMap"/> and mechanisms to release the lease.</returns>
        internal static SharedMaterialMapLease Acquire(Dictionary<Material, IMaterialConvertSettings> settingsMap, System.Func<Dictionary<Material, IMaterialConvertSettings>, Dictionary<Material, Material>> convertFunc)
        {
            var groupedOriginalMaterials = new Dictionary<string, List<Material>>();
            var representativeMaterials = new Dictionary<string, (Material Material, IMaterialConvertSettings Settings)>();
            foreach (var (material, settings) in settingsMap)
            {
                if (VRCSDKUtility.IsMaterialAllowedForQuestAvatar(material))
                {
                    continue;
                }

                var key = CreateKey(material, settings);
                if (!groupedOriginalMaterials.TryGetValue(key, out var groupedMaterials))
                {
                    groupedMaterials = new List<Material>();
                    groupedOriginalMaterials[key] = groupedMaterials;
                }
                groupedMaterials.Add(material);
                if (!representativeMaterials.ContainsKey(key))
                {
                    representativeMaterials[key] = (material, settings);
                }
            }

            var materialMap = new Dictionary<Material, Material>();
            var acquiredKeys = new HashSet<string>();
            var materialsToConvert = new Dictionary<Material, IMaterialConvertSettings>();
            var keyByRepresentative = new Dictionary<Material, string>();

            lock (SyncRoot)
            {
                foreach (var (key, value) in representativeMaterials)
                {
                    if (TryGetAliveEntry(key, out var cachedEntry))
                    {
                        cachedEntry.ReferenceCount++;
                        acquiredKeys.Add(key);
                        var srcNames = string.Join(", ", groupedOriginalMaterials[key].Select(m => m != null ? $"{m.name} (id:{m.GetInstanceID()})" : "<null>"));
                        var convertedIdStr = cachedEntry.Material != null ? cachedEntry.Material.GetInstanceID().ToString() : "<null>";
                        Logger.LogDebug($"Reusing cached preview material for {groupedOriginalMaterials[key].Count} source material(s): {srcNames}. Converted: {(cachedEntry.Material != null ? cachedEntry.Material.name : "<null>")} (id:{convertedIdStr}).");
                        foreach (var original in groupedOriginalMaterials[key])
                        {
                            materialMap[original] = cachedEntry.Material;
                        }

                        continue;
                    }

                    materialsToConvert[value.Material] = value.Settings;
                    keyByRepresentative[value.Material] = key;
                }
            }

            var generatedMaterialsToDestroy = new List<Material>();
            try
            {
                if (materialsToConvert.Count > 0)
                {
                    var convertedMaterials = convertFunc(materialsToConvert);
                    lock (SyncRoot)
                    {
                        foreach (var (representativeMaterial, _) in materialsToConvert)
                        {
                            var key = keyByRepresentative[representativeMaterial];
                            if (TryGetAliveEntry(key, out var existingEntry))
                            {
                                existingEntry.ReferenceCount++;
                                acquiredKeys.Add(key);
                                var srcNames = string.Join(", ", groupedOriginalMaterials[key].Select(m => m != null ? $"{m.name} (id:{m.GetInstanceID()})" : "<null>"));
                                var existingConvertedIdStr = existingEntry.Material != null ? existingEntry.Material.GetInstanceID().ToString() : "<null>";
                                Logger.LogDebug($"Reusing cached preview material after conversion merge for {groupedOriginalMaterials[key].Count} source material(s): {srcNames}. Converted: {(existingEntry.Material != null ? existingEntry.Material.name : "<null>")} (id:{existingConvertedIdStr}).");
                                foreach (var original in groupedOriginalMaterials[key])
                                {
                                    materialMap[original] = existingEntry.Material;
                                }

                                if (convertedMaterials.TryGetValue(representativeMaterial, out var generatedMaterial) && generatedMaterial != null && generatedMaterial != existingEntry.Material)
                                {
                                    generatedMaterialsToDestroy.Add(generatedMaterial);
                                }

                                continue;
                            }

                            if (!convertedMaterials.TryGetValue(representativeMaterial, out var convertedMaterial) || convertedMaterial == null)
                            {
                                continue;
                            }

                            Entries[key] = new CacheEntry(convertedMaterial);
                            acquiredKeys.Add(key);
                            foreach (var original in groupedOriginalMaterials[key])
                            {
                                materialMap[original] = convertedMaterial;
                            }
                        }
                    }
                }
            }
            catch
            {
                Release(acquiredKeys);
                throw;
            }
            finally
            {
                foreach (var material in generatedMaterialsToDestroy)
                {
                    DestroyConvertedMaterial(material);
                }
            }

            return new SharedMaterialMapLease(materialMap, acquiredKeys);
        }

        /// <summary>
        /// Release previously acquired cache entries.
        /// Decrements the reference count for each key in <paramref name="acquiredKeys"/>; entries with zero references are removed
        /// from the cache and scheduled for destruction. This method is idempotent for a given set of keys.
        /// </summary>
        /// <param name="acquiredKeys">Set of cache keys acquired from <see cref="Acquire"/>.</param>
        internal static void Release(HashSet<string> acquiredKeys)
        {
            if (acquiredKeys.Count == 0)
            {
                return;
            }

            var materialsToDestroy = new List<Material>();
            lock (SyncRoot)
            {
                foreach (var key in acquiredKeys)
                {
                    if (!Entries.TryGetValue(key, out var entry))
                    {
                        continue;
                    }

                    entry.ReferenceCount--;
                    if (entry.ReferenceCount <= 0)
                    {
                        Entries.Remove(key);
                        materialsToDestroy.Add(entry.Material);
                        var materialName = entry.Material != null ? entry.Material.name : "<null>";
                        var materialId = entry.Material != null ? entry.Material.GetInstanceID().ToString() : "<null>";
                        Logger.LogDebug($"Cache entry released for converted material: {materialName} (id:{materialId}). Reference count reached zero; scheduled for destruction.");
                    }
                }
            }

            foreach (var material in materialsToDestroy)
            {
                var matName = material != null ? material.name : "<null>";
                var matId = material != null ? material.GetInstanceID().ToString() : "<null>";
                var assetPath = material != null ? AssetDatabase.GetAssetPath(material) : string.Empty;
                Logger.LogDebug($"Destroying converted material: {matName} (id:{matId}). AssetPath: {assetPath}");
                DestroyConvertedMaterial(material);
                Logger.LogDebug($"Destroyed cached converted material: {matName} (id:{matId})");
            }

            acquiredKeys.Clear();
        }

        /// <summary>
        /// Try to retrieve a live cache entry for the specified key.
        /// Removes and returns false if the entry exists but the converted material has been garbage-collected or destroyed.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="entry">Out parameter for the cache entry when found and alive.</param>
        /// <returns>True when a live entry is found; otherwise false.</returns>
        private static bool TryGetAliveEntry(string key, out CacheEntry entry)
        {
            if (!Entries.TryGetValue(key, out entry))
            {
                return false;
            }

            if (entry.Material == null)
            {
                Entries.Remove(key);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create a unique cache key for the given material and conversion settings.
        /// The key encodes the active build target, a settings-derived key, and a content-derived key for the material.
        /// </summary>
        private static string CreateKey(Material material, IMaterialConvertSettings settings)
        {
            return $"{EditorUserBuildSettings.activeBuildTarget}_{GetSettingsCacheKey(settings)}_{CacheUtility.GetContentCacheKey(material)}";
        }

        /// <summary>
        /// Compute a cache key for the given material conversion settings.
        /// Special-cases <see cref="MaterialReplaceSettings"/> to include the replacement material identity when present.
        /// </summary>
        private static string GetSettingsCacheKey(IMaterialConvertSettings settings)
        {
            if (settings is MaterialReplaceSettings replaceSettings)
            {
                if (replaceSettings.material == null)
                {
                    return $"{nameof(MaterialReplaceSettings)}_null";
                }

                return $"{nameof(MaterialReplaceSettings)}_{replaceSettings.material.GetInstanceID()}_{CacheUtility.GetContentCacheKey(replaceSettings.material)}";
            }

            return $"{settings.GetType().FullName}_{settings.GetCacheKey()}";
        }

        /// <summary>
        /// Destroy an in-memory converted material and its non-asset textures.
        /// Only destroys objects that are not referenced by assets on disk (AssetDatabase.GetAssetPath returns empty).
        /// </summary>
        private static void DestroyConvertedMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            foreach (var propertyName in material.GetTexturePropertyNames())
            {
                var texture = material.GetTexture(propertyName);
                if (texture != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture)))
                {
                    Object.DestroyImmediate(texture);
                }
            }

            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(material)))
            {
                Object.DestroyImmediate(material);
            }
        }

        /// <summary>
        /// Internal cache entry storing converted material and its reference count.
        /// </summary>
        private class CacheEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CacheEntry"/> class.
            /// Reference count starts at 1 for a newly created entry.
            /// </summary>
            /// <param name="material">Converted material instance.</param>
            public CacheEntry(Material material)
            {
                Material = material;
                ReferenceCount = 1;
            }

            /// <summary>
            /// Gets the converted (shared) material instance.
            /// </summary>
            public Material Material { get; private set; }

            /// <summary>
            /// Gets or sets the number of active leases holding this converted material.
            /// </summary>
            public int ReferenceCount { get; set; }
        }
    }
}
