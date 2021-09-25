// <copyright file="TestUtils.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using ImageMagick;
using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Test utility.
    /// </summary>
    internal static class TestUtils
    {
        /// <summary>
        /// Gets Test fixtures folder.
        /// </summary>
        internal static string FixturesFolder => VRCQuestTools.AssetRoot + "/Tests/Fixtures";

        /// <summary>
        /// Gets Materials folder.
        /// </summary>
        internal static string MaterialsFolder => FixturesFolder + "/Materials";

        /// <summary>
        /// Gets Textures folder.
        /// </summary>
        internal static string TexturesFolder => FixturesFolder + "/Textures";

        /// <summary>
        /// Load material from materials folder.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>Material.</returns>
        internal static Material LoadMaterial(string file)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/" + file);
            Assert.NotNull(material);
            return material;
        }

        /// <summary>
        /// Load MaterialBase from materials folder.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>MaterialBase.</returns>
        internal static MaterialBase LoadMaterialWrapper(string file)
        {
            var material = LoadMaterial(file);
            var wrapper = MaterialBase.Create(material);
            Assert.NotNull(wrapper);
            return wrapper;
        }

        /// <summary>
        /// Load MagickImage from textures folder.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>MagickImage.</returns>
        internal static MagickImage LoadMagickImage(string file)
        {
            var image = new MagickImage(TexturesFolder + "/" + file);
            Assert.NotNull(image);
            return image;
        }

        /// <summary>
        /// Get GUID for an asset.
        /// </summary>
        /// <param name="obj">Object of target asset.</param>
        /// <returns>GUID.</returns>
        internal static string GetAssetGUID(Object obj)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localId);
            return guid;
        }
    }
}
