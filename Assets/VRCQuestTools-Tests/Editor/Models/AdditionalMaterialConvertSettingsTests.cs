// <copyright file="AdditionalMaterialConvertSettingsTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="AdditionalMaterialConvertSettings"/>.
    /// </summary>
    public class AdditionalMaterialConvertSettingsTests
    {
        [Test]
        public void GetCacheKey_NullMaterial_ContainsNull()
        {
            var settings = new AdditionalMaterialConvertSettings();
            settings.targetMaterial = null;
            var key = settings.GetCacheKey();
            Assert.IsTrue(key.StartsWith("null_"));
        }

        [Test]
        public void GetCacheKey_WithMaterial_ContainsInstanceId()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var settings = new AdditionalMaterialConvertSettings();
                settings.targetMaterial = mat;
                var key = settings.GetCacheKey();
                Assert.IsTrue(key.Contains(mat.GetInstanceID().ToString()));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetCacheKey_IncludesConvertSettingsKey()
        {
            var settings = new AdditionalMaterialConvertSettings();
            settings.materialConvertSettings = new ToonLitConvertSettings();
            var key = settings.GetCacheKey();
            var innerKey = settings.materialConvertSettings.GetCacheKey();
            Assert.IsTrue(key.Contains(innerKey));
        }

        [Test]
        public void DefaultConvertSettings_IsToonLit()
        {
            var settings = new AdditionalMaterialConvertSettings();
            Assert.IsInstanceOf<ToonLitConvertSettings>(settings.materialConvertSettings);
        }

        [Test]
        public void LoadDefaultAssets_DoesNotThrow()
        {
            var settings = new AdditionalMaterialConvertSettings();
            Assert.DoesNotThrow(() => settings.LoadDefaultAssets());
        }
    }
}
