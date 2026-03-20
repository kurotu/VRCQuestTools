// Tests for AvatarConverter.CreateMaterialConvertSettingsMap exception paths and logic.

using System;
using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRCQuestToolsTextureUtility = KRT.VRCQuestTools.Utils.TextureUtility;
using VRCQuestToolsVRCSDKUtility = KRT.VRCQuestTools.Utils.VRCSDKUtility;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class AvatarConverterCreateSettingsMapTests
    {
        private AvatarConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
        }

        [Test]
        public void CreateSettingsMap_EmptyGameObject_ReturnsEmptyDictionary()
        {
            var root = new GameObject("Avatar");
            try
            {
                var result = converter.CreateMaterialConvertSettingsMap(root, new Material[0]);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CreateSettingsMap_NoComponents_ReturnsEmptyDictionary()
        {
            var root = new GameObject("Avatar");
            var mat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            try
            {
                var result = converter.CreateMaterialConvertSettingsMap(root, new[] { mat });
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialSwapWithNullOriginal_ThrowsInvalidMaterialSwapNull()
        {
            var root = new GameObject("Avatar");
            var swap = root.AddComponent<MaterialSwap>();
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping { originalMaterial = null, replacementMaterial = null },
            };
            try
            {
                Assert.Throws<InvalidMaterialSwapNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(root, new Material[0]);
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialSwapWithNullReplacement_ThrowsInvalidMaterialSwapNull()
        {
            var root = new GameObject("Avatar");
            var swap = root.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping { originalMaterial = originalMat, replacementMaterial = null },
            };
            try
            {
                Assert.Throws<InvalidMaterialSwapNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(root, new Material[0]);
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(originalMat);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialSwapWithValidMapping_AddsMaterialReplace()
        {
            var root = new GameObject("Avatar");
            var swap = root.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            var replacementMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping { originalMaterial = originalMat, replacementMaterial = replacementMat },
            };
            try
            {
                var result = converter.CreateMaterialConvertSettingsMap(root, new[] { originalMat });
                Assert.IsTrue(result.ContainsKey(originalMat));
                Assert.IsInstanceOf<MaterialReplaceSettings>(result[originalMat]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(originalMat);
                UnityEngine.Object.DestroyImmediate(replacementMat);
            }
        }

        [Test]
        public void CreateSettingsMap_OmitsUnusedMaterials()
        {
            var root = new GameObject("Avatar");
            var swap = root.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            var replacementMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            var usedMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping { originalMaterial = originalMat, replacementMaterial = replacementMat },
            };
            try
            {
                // usedMat is the only material on avatar, but originalMat is what's being swapped
                var result = converter.CreateMaterialConvertSettingsMap(root, new[] { usedMat });
                Assert.IsFalse(result.ContainsKey(originalMat));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(originalMat);
                UnityEngine.Object.DestroyImmediate(replacementMat);
                UnityEngine.Object.DestroyImmediate(usedMat);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialConversionSettingsWithNullTarget_ThrowsTargetMaterialNull()
        {
            var root = new GameObject("Avatar");
            var settings = root.AddComponent<MaterialConversionSettings>();
            settings.additionalMaterialConvertSettings = new[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };
            try
            {
                Assert.Throws<TargetMaterialNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(root, new Material[0]);
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CreateSettingsMap_AvatarConverterSettingsWithNullTarget_ThrowsTargetMaterialNull()
        {
            var root = new GameObject("Avatar");
            var settings = root.AddComponent<AvatarConverterSettings>();
            settings.additionalMaterialConvertSettings = new[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };
            try
            {
                Assert.Throws<TargetMaterialNullException>(() =>
                {
                    converter.CreateMaterialConvertSettingsMap(root, new Material[0]);
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CreateSettingsMap_WithAvatarConverterSettings_AddsDefaultSettings()
        {
            var root = new GameObject("Avatar");
            var converterSettings = root.AddComponent<AvatarConverterSettings>();
            converterSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[0];

            var mat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            try
            {
                var result = converter.CreateMaterialConvertSettingsMap(root, new[] { mat });
                // With AvatarConverterSettings but no additional settings and no primary conversion component,
                // the default settings from the converter should be applied
                Assert.IsNotNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CreateSettingsMap_MaterialConversionSettingsWithValidTarget_AddsToMap()
        {
            var root = new GameObject("Avatar");
            var settings = root.AddComponent<MaterialConversionSettings>();
            var targetMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            var convertSettings = new ToonLitConvertSettings();
            settings.additionalMaterialConvertSettings = new[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = targetMat,
                    materialConvertSettings = convertSettings,
                },
            };
            try
            {
                var result = converter.CreateMaterialConvertSettingsMap(root, new[] { targetMat });
                Assert.IsTrue(result.ContainsKey(targetMat));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(targetMat);
            }
        }
    }

    [TestFixture]
    internal class TextureUtilityGetBestPlatformOverrideTests
    {
        [Test]
        public void GetBestPlatformOverrideSettings_NullTextures_ReturnsNull()
        {
            var result = VRCQuestToolsTextureUtility.GetBestPlatformOverrideSettings(null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_EmptyArray_ReturnsNull()
        {
            var result = VRCQuestToolsTextureUtility.GetBestPlatformOverrideSettings(new Texture[0]);
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_NullElements_ReturnsNull()
        {
            var result = VRCQuestToolsTextureUtility.GetBestPlatformOverrideSettings(null, null, null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_RuntimeTexture_ReturnsNull()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var result = VRCQuestToolsTextureUtility.GetBestPlatformOverrideSettings(tex);
                Assert.IsNull(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }
    }

    [TestFixture]
    internal class VRCSDKUtilityPerformanceIconTests
    {
        [Test]
        public void LoadPerformanceIcon_Excellent_ReturnsTexture()
        {
            var icon = VRCQuestToolsVRCSDKUtility.LoadPerformanceIcon(VRC.SDKBase.Validation.Performance.PerformanceRating.Excellent);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Good_ReturnsTexture()
        {
            var icon = VRCQuestToolsVRCSDKUtility.LoadPerformanceIcon(VRC.SDKBase.Validation.Performance.PerformanceRating.Good);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Medium_ReturnsTexture()
        {
            var icon = VRCQuestToolsVRCSDKUtility.LoadPerformanceIcon(VRC.SDKBase.Validation.Performance.PerformanceRating.Medium);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_Poor_ReturnsTexture()
        {
            var icon = VRCQuestToolsVRCSDKUtility.LoadPerformanceIcon(VRC.SDKBase.Validation.Performance.PerformanceRating.Poor);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_VeryPoor_ReturnsTexture()
        {
            var icon = VRCQuestToolsVRCSDKUtility.LoadPerformanceIcon(VRC.SDKBase.Validation.Performance.PerformanceRating.VeryPoor);
            Assert.IsNotNull(icon);
        }

        [Test]
        public void LoadPerformanceIcon_None_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                VRCQuestToolsVRCSDKUtility.LoadPerformanceIcon(VRC.SDKBase.Validation.Performance.PerformanceRating.None);
            });
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_ReturnsNonNull()
        {
            var result = VRCQuestToolsVRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            Assert.IsNotNull(result);
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Desktop_ReturnsNonNull()
        {
            var result = VRCQuestToolsVRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(false);
            Assert.IsNotNull(result);
        }
    }
}
