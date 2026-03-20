// <copyright file="AvatarConverterSettingsTests.cs" company="kurotu">
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
    /// Tests for <see cref="AvatarConverterSettings"/>.
    /// </summary>
    public class AvatarConverterSettingsTests
    {
        [Test]
        public void DefaultMaterialConvertSettings_ReturnsToonStandard()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                Assert.IsNotNull(settings.DefaultMaterialConvertSettings);
                Assert.IsInstanceOf<ToonStandardConvertSettings>(settings.DefaultMaterialConvertSettings);
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
                var settings = go.AddComponent<AvatarConverterSettings>();
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
                var settings = go.AddComponent<AvatarConverterSettings>();
                var additional = new AdditionalMaterialConvertSettings[]
                {
                    new AdditionalMaterialConvertSettings(),
                };
                settings.AdditionalMaterialConvertSettings = additional;
                Assert.AreEqual(1, settings.AdditionalMaterialConvertSettings.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetMaterialConvertSettings_NoAdditional_ReturnsDefault()
        {
            var go = new GameObject("TestAvatar");
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var result = settings.GetMaterialConvertSettings(mat);
                Assert.AreSame(settings.DefaultMaterialConvertSettings, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMaterialConvertSettings_MatchingAdditional_ReturnsAdditionalSettings()
        {
            var go = new GameObject("TestAvatar");
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var toonLit = new ToonLitConvertSettings();
                var additional = new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat,
                    materialConvertSettings = toonLit,
                };
                settings.AdditionalMaterialConvertSettings = new[] { additional };

                var result = settings.GetMaterialConvertSettings(mat);
                Assert.AreSame(toonLit, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetMaterialConvertSettings_NonMatchingAdditional_ReturnsDefault()
        {
            var go = new GameObject("TestAvatar");
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var additional = new AdditionalMaterialConvertSettings
                {
                    targetMaterial = mat1,
                    materialConvertSettings = new ToonLitConvertSettings(),
                };
                settings.AdditionalMaterialConvertSettings = new[] { additional };

                var result = settings.GetMaterialConvertSettings(mat2);
                Assert.AreSame(settings.DefaultMaterialConvertSettings, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_DefaultTrue()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
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
                var settings = go.AddComponent<AvatarConverterSettings>();
                Assert.AreEqual(AvatarConverterNdmfPhase.Auto, settings.NdmfPhase);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsPrimaryRoot_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                Assert.IsTrue(settings.IsPrimaryRoot);
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
                var settings = go.AddComponent<AvatarConverterSettings>();
                Assert.IsTrue(settings.EnableMaterialPreview);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DefaultFields_HaveExpectedValues()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                Assert.IsTrue(settings.removeAvatarDynamics);
                Assert.IsTrue(settings.removeVertexColor);
                Assert.IsTrue(settings.compressExpressionsMenuIcons);
                Assert.AreEqual(0, settings.physBonesToKeep.Length);
                Assert.AreEqual(0, settings.physBoneCollidersToKeep.Length);
                Assert.AreEqual(0, settings.contactsToKeep.Length);
                Assert.AreEqual(0, settings.animatorOverrideControllers.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
