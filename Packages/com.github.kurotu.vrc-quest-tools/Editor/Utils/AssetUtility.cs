// <copyright file="AssetUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for assets.
    /// </summary>
    internal static class AssetUtility
    {
        /// <summary>
        /// Gets version of lilToon.
        /// </summary>
        internal static readonly SemVer LilToonVersion;

        /// <summary>
        /// Type object of DynamicBone.
        /// </summary>
        internal static readonly Type DynamicBoneType = SystemUtility.GetTypeByName("DynamicBone");

        /// <summary>
        /// Type object of DynamicBoneCollider.
        /// </summary>
        internal static readonly Type DynamicBoneColliderType = SystemUtility.GetTypeByName("DynamicBoneCollider");

        private const string LilToonPackageJsonGUID = "397d2fa9e93fb5d44a9540d5f01437fc";

        private static readonly Lazy<Shader> LilToon2Ramp = new Lazy<Shader>(() => Shader.Find("Hidden/ltsother_bakeramp"));

        static AssetUtility()
        {
            if (IsLilToonImported())
            {
                try
                {
                    var path = AssetDatabase.GUIDToAssetPath(LilToonPackageJsonGUID);
                    var str = File.ReadAllText(path);
                    var package = JsonUtility.FromJson<PackageJson>(str);
                    LilToonVersion = new SemVer(package.version);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    EditorUtility.DisplayDialog(VRCQuestTools.Name, $"Error occurred when detecting lilToon version.\nPlease report this message and the console error log.\n\n{e.GetType().Name}: {e.Message}", "OK");
                    LilToonVersion = new SemVer("0.0.0");
                }
            }
            else
            {
                LilToonVersion = new SemVer("0.0.0");
            }
        }

        /// <summary>
        /// Gets whether Dynamic Bone is imported.
        /// </summary>
        /// <returns>true when Dynamic Bone is imported.</returns>
        internal static bool IsDynamicBoneImported()
        {
            return DynamicBoneType != null;
        }

        /// <summary>
        /// Gets whether lilToon is imported.
        /// </summary>
        /// <returns>true when lilToon shader and lilToonInspector are imported.</returns>
        internal static bool IsLilToonImported()
        {
            var shader = Shader.Find("lilToon");
            var inspector = SystemUtility.GetTypeByName("lilToon.lilToonInspector");
            return (shader != null) && (inspector != null);
        }

        /// <summary>
        /// Gets whether lilToon supports shadow ramp baking.
        /// </summary>
        /// <returns>true for lilToon 1.10.0 or later.</returns>
        internal static bool CanLilToonBakeShadowRamp()
        {
            return new SemVer(1, 10, 0) <= LilToonVersion;
        }

        /// <summary>
        /// Gets lilToon2Ramp shader.
        /// </summary>
        /// <returns>Shader object or null.</returns>
        internal static Shader GetLilToon2Ramp()
        {
            return LilToon2Ramp.Value;
        }

        /// <summary>
        /// Create a new asset. If the path already exists, it will be overwritten.
        /// </summary>
        /// <typeparam name="T">Type of asset.</typeparam>
        /// <param name="asset">Asset to save.</param>
        /// <param name="path">Path to save.</param>
        /// <param name="postCreateAction">Action to execute after AssetDatabase.CreateAsset() for further objects.</param>
        /// <returns>Created asset.</returns>
        /// <remarks>Do not use `asset` object after this method. Use the returned object.</remarks>
        internal static T CreateAsset<T>(T asset, string path, Action<T> postCreateAction = null)
            where T : UnityEngine.Object
        {
            if (!File.Exists(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                postCreateAction?.Invoke(asset);
                AssetDatabase.SaveAssets();
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }

            var file = Path.GetFileName(path);
            var tmpDir = $"Assets/tmp_vqt";
            var tmpPath = $"{tmpDir}/{file}";

            try
            {
                if (!Directory.Exists(tmpDir))
                {
                    Directory.CreateDirectory(tmpDir);
                }
                AssetDatabase.CreateAsset(asset, tmpPath);
                postCreateAction?.Invoke(asset);
                AssetDatabase.SaveAssets();
                File.Copy(tmpPath, path, true);
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            finally
            {
                if (File.Exists(tmpPath))
                {
                    File.Delete(tmpPath);
                }
                if (File.Exists($"{tmpPath}.meta"))
                {
                    File.Delete($"{tmpPath}.meta");
                }
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, true);
                }
                if (File.Exists($"{tmpDir}.meta"))
                {
                    File.Delete($"{tmpDir}.meta");
                }
            }

            AssetDatabase.Refresh();
            var newAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            return newAsset;
        }

        /// <summary>
        /// Get all object references in the object.
        /// </summary>
        /// <param name="o">Object.</param>
        /// <returns>All referenced objects.</returns>
        internal static UnityEngine.Object[] GetAllObjectReferences(UnityEngine.Object o)
        {
            var list = new HashSet<UnityEngine.Object>();
            GetAllObjectReferencesImpl(o, list);
            return list.ToArray();
        }

        /// <summary>
        /// Load asset by GUID.
        /// </summary>
        /// <typeparam name="T">Asset type.</typeparam>
        /// <param name="guid">Asset GUID.</param>
        /// <returns>Loaded asset.</returns>
        internal static T LoadAssetByGUID<T>(string guid)
            where T : UnityEngine.Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Failed to get asset path by GUID: {guid}");
                return null;
            }
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogError($"Failed to load asset by GUID: {guid}");
                return null;
            }
            return asset;
        }

        private static void GetAllObjectReferencesImpl(UnityEngine.Object o, HashSet<UnityEngine.Object> list)
        {
            var so = new SerializedObject(o);
            var itr = so.GetIterator();
            while (itr.Next(true))
            {
                if (itr.propertyType == SerializedPropertyType.ObjectReference)
                {
                    var obj = itr.objectReferenceValue;
                    if (obj != null && !list.Contains(obj))
                    {
                        list.Add(obj);
                        GetAllObjectReferencesImpl(obj, list);
                    }
                }
            }
        }

        [Serializable]
        private class PackageJson
        {
            /// <summary>
            /// package version.
            /// </summary>
            public string version;
        }
    }
}
