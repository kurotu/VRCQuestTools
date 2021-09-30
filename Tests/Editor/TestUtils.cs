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
        /// Gets a value indicating whether DynamicBone is imported.
        /// </summary>
        internal static bool HasDynamicBone => AssetDatabase.GUIDToAssetPath("f9ac8d30c6a0d9642a11e5be4c440740") != string.Empty; // DynamicBone.cs

        /// <summary>
        /// Gets a value indicating whether FinalIK is imported.
        /// </summary>
        internal static bool HasFinalIK => AssetDatabase.GUIDToAssetPath("e8ad84abaddc346b9a51365d3dc292e7") != string.Empty; // IK.cs

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

        /// <summary>
        /// Check the shader exists. If the shader is missing, test is ignored.
        /// </summary>
        /// <param name="name">Shader name.</param>
        internal static void AssertIgnoreOnMissingShader(string name)
        {
            var shader = Shader.Find(name);
            if (shader == null)
            {
                Assert.Ignore($"\"{name}\" shader not found");
            }
        }
    }
}
