// <copyright file="MaterialConversionGUITests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Tests for MaterialConversionGUI.IsHandledMaterial method.
    /// This method is used to determine if a material is already handled by conversion or replacement settings,
    /// preventing unnecessary warnings for materials with custom rules (issue#80).
    /// 
    /// The method should return true when:
    /// 1. Material has AdditionalMaterialConvertSettings (except MaterialReplaceSettings with null replacement)
    /// 2. Material has valid MaterialReplaceSettings with non-null replacement material
    /// 3. Material is mapped in MaterialSwap component with non-null replacement material
    /// </summary>
    public class MaterialConversionGUITests
    {
        private GameObject testAvatarObject;
        private AvatarConverterSettings avatarConverterSettings;
        private MaterialConversionSettings materialConversionSettings;
        private MaterialSwap materialSwap;
        private Material testMaterial1;
        private Material testMaterial2;
        private Material replacementMaterial;

        /// <summary>
        /// Setup for each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // Create test avatar GameObject
            testAvatarObject = new GameObject("TestAvatar");
            testAvatarObject.AddComponent<VRCAvatarDescriptor>();

            // Create test materials
            testMaterial1 = new Material(Shader.Find("Standard"))
            {
                name = "TestMaterial1"
            };
            testMaterial2 = new Material(Shader.Find("Standard"))
            {
                name = "TestMaterial2"
            };
            replacementMaterial = new Material(Shader.Find("VRChat/Mobile/Toon Lit"))
            {
                name = "ReplacementMaterial"
            };
        }

        /// <summary>
        /// Cleanup for each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (testAvatarObject != null)
            {
                Object.DestroyImmediate(testAvatarObject);
            }
            if (testMaterial1 != null)
            {
                Object.DestroyImmediate(testMaterial1);
            }
            if (testMaterial2 != null)
            {
                Object.DestroyImmediate(testMaterial2);
            }
            if (replacementMaterial != null)
            {
                Object.DestroyImmediate(replacementMaterial);
            }
        }

        /// <summary>
        /// Test that null material returns false.
        /// </summary>
        [Test]
        public void IsHandledMaterial_NullMaterial_ReturnsFalse()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();

            // Act
            bool result = InvokeIsHandledMaterial(null, avatarConverterSettings);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test that material without any handling settings returns false.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialNotHandled_ReturnsFalse()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, avatarConverterSettings);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test that material with AdditionalMaterialConvertSettings returns true.
        /// This ensures materials with custom conversion settings don't show warnings.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialInAdditionalSettings_ReturnsTrue()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();
            avatarConverterSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = testMaterial1,
                    materialConvertSettings = new ToonLitConvertSettings()
                }
            };

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, avatarConverterSettings);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test that material with MaterialReplaceSettings with null replacement returns false.
        /// This ensures materials with incomplete replacement settings still show warnings.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialReplaceSettingsWithNullReplacement_ReturnsFalse()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();
            avatarConverterSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = testMaterial1,
                    materialConvertSettings = new MaterialReplaceSettings
                    {
                        material = null
                    }
                }
            };

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, avatarConverterSettings);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test that material with MaterialReplaceSettings with valid replacement returns true.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialReplaceSettingsWithValidReplacement_ReturnsTrue()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();
            avatarConverterSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = testMaterial1,
                    materialConvertSettings = new MaterialReplaceSettings
                    {
                        material = replacementMaterial
                    }
                }
            };

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, avatarConverterSettings);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test that material handled by MaterialSwap returns true.
        /// This ensures materials replaced via MaterialSwap component don't show warnings.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialInMaterialSwap_ReturnsTrue()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();
            materialSwap = testAvatarObject.AddComponent<MaterialSwap>();
            materialSwap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = testMaterial1,
                    replacementMaterial = replacementMaterial
                }
            };

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, avatarConverterSettings);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test that material in MaterialSwap with null replacement returns false.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialInMaterialSwapWithNullReplacement_ReturnsFalse()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();
            materialSwap = testAvatarObject.AddComponent<MaterialSwap>();
            materialSwap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = testMaterial1,
                    replacementMaterial = null
                }
            };

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, avatarConverterSettings);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test that MaterialConversionSettings component also works.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialConversionSettingsComponent_ReturnsTrue()
        {
            // Arrange
            materialConversionSettings = testAvatarObject.AddComponent<MaterialConversionSettings>();
            materialConversionSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = testMaterial1,
                    materialConvertSettings = new ToonLitConvertSettings()
                }
            };

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, materialConversionSettings);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test that MaterialSwap in child object is detected.
        /// This ensures the method searches the entire hierarchy for MaterialSwap components.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialSwapInChildObject_ReturnsTrue()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();

            var childObject = new GameObject("ChildObject");
            childObject.transform.SetParent(testAvatarObject.transform);

            materialSwap = childObject.AddComponent<MaterialSwap>();
            materialSwap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = testMaterial1,
                    replacementMaterial = replacementMaterial
                }
            };

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, avatarConverterSettings);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test that material is detected when MaterialConversionSettings exists in child objects.
        /// This ensures the method searches child objects for material conversion components.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialConversionSettingsInChildObject_ReturnsTrue()
        {
            // Arrange
            avatarConverterSettings = testAvatarObject.AddComponent<AvatarConverterSettings>();

            var childObject = new GameObject("ChildObject");
            childObject.transform.SetParent(testAvatarObject.transform);

            var childMaterialConversionSettings = childObject.AddComponent<MaterialConversionSettings>();
            childMaterialConversionSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = testMaterial1,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, avatarConverterSettings);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test that material is detected when MaterialConversionSettings exists in parent objects.
        /// This ensures the method searches parent objects for material conversion components.
        /// </summary>
        [Test]
        public void IsHandledMaterial_MaterialConversionSettingsInParentObject_ReturnsTrue()
        {
            // Arrange
            // Create a parent object with MaterialConversionSettings
            var parentObject = new GameObject("ParentObject");
            var parentMaterialConversionSettings = parentObject.AddComponent<MaterialConversionSettings>();
            parentMaterialConversionSettings.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = testMaterial1,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            // Create a child object with AvatarConverterSettings (target component)
            var childObject = new GameObject("ChildObject");
            childObject.transform.SetParent(parentObject.transform);
            var childMaterialConversionSettings = childObject.AddComponent<MaterialConversionSettings>();

            // Act
            bool result = InvokeIsHandledMaterial(testMaterial1, childMaterialConversionSettings);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Invokes the private IsHandledMaterial method using reflection.
        /// </summary>
        /// <param name="material">The material to check.</param>
        /// <param name="targetComponent">The target component.</param>
        /// <returns>The result of IsHandledMaterial.</returns>
        private static bool InvokeIsHandledMaterial(Material material, Component targetComponent)
        {
            var type = typeof(MaterialConversionGUI);
            var method = type.GetMethod("IsHandledMaterial", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "IsHandledMaterial method should exist");

            return (bool)method.Invoke(null, new object[] { material, targetComponent });
        }
    }
}
