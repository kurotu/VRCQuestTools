// <copyright file="MaterialConversionSettingsTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="MaterialConversionSettings"/>.
    /// </summary>
    public class MaterialConversionSettingsTests
    {
        [Test]
        public void DefaultMaterialConvertSettings_ReturnsToonLit()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                Assert.IsNotNull(settings.DefaultMaterialConvertSettings);
                Assert.IsInstanceOf<ToonLitConvertSettings>(settings.DefaultMaterialConvertSettings);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AdditionalMaterialConvertSettings_DefaultEmpty()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                Assert.IsNotNull(settings.AdditionalMaterialConvertSettings);
                Assert.AreEqual(0, settings.AdditionalMaterialConvertSettings.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AdditionalMaterialConvertSettings_CanSet()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                var additional = new AdditionalMaterialConvertSettings[]
                {
                    new AdditionalMaterialConvertSettings(),
                    new AdditionalMaterialConvertSettings(),
                };
                settings.AdditionalMaterialConvertSettings = additional;
                Assert.AreEqual(2, settings.AdditionalMaterialConvertSettings.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_DefaultTrue()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                Assert.IsTrue(settings.RemoveExtraMaterialSlots);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void NdmfPhase_DefaultAuto()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                Assert.AreEqual(AvatarConverterNdmfPhase.Auto, settings.NdmfPhase);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnableMaterialPreview_DefaultTrue()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                Assert.IsTrue(settings.EnableMaterialPreview);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsPrimaryRoot_WithoutAvatarDescriptor_ReturnsFalse()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<MaterialConversionSettings>();
                // No VRC_AvatarDescriptor so IsPrimaryRoot should be false
                Assert.IsFalse(settings.IsPrimaryRoot);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
