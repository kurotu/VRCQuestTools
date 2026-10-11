// <copyright file="MaterialReplaceSettingsTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests replacement settings and the component cache key observed by NDMF previews.
    /// </summary>
    internal class MaterialReplaceSettingsTests
    {
        private Material replacement;

        /// <summary>
        /// Creates a replacement material for each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            var shader = Shader.Find("Standard");
            Assert.IsNotNull(shader);
            replacement = new Material(shader);
        }

        /// <summary>
        /// Destroys the replacement material after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(replacement);
        }

        /// <summary>
        /// An unset replacement must have a stable key instead of breaking preview initialization.
        /// </summary>
        [Test]
        public void GetCacheKey_NullReplacement_IsStable()
        {
            var settings = new MaterialReplaceSettings();
            var key = settings.GetCacheKey();

            Assert.That(key, Does.Contain("MaterialReplaceSettings_null"));
            Assert.AreEqual(key, settings.GetCacheKey());
        }

        /// <summary>
        /// Unchanged replacement materials must not continuously invalidate the preview.
        /// </summary>
        [Test]
        public void GetCacheKey_UnchangedReplacement_IsStable()
        {
            var settings = new MaterialReplaceSettings { material = replacement };
            var key = settings.GetCacheKey();

            Assert.IsNotEmpty(key);
            Assert.AreEqual(key, settings.GetCacheKey());
        }

        /// <summary>
        /// Switching to a different material with identical properties must rebuild the preview.
        /// </summary>
        [Test]
        public void GetCacheKey_DifferentReplacementWithSameContent_ChangesKey()
        {
            var other = new Material(replacement);
            try
            {
                var settings = new MaterialReplaceSettings { material = replacement };
                var key = settings.GetCacheKey();
                settings.material = other;

                Assert.AreNotEqual(key, settings.GetCacheKey());
            }
            finally
            {
                Object.DestroyImmediate(other);
            }
        }

        /// <summary>
        /// Editing the replacement material must invalidate the observed component settings.
        /// </summary>
        [Test]
        public void GetCacheKey_ReplacementColorChanges_ChangesKey()
        {
            var settings = new MaterialReplaceSettings { material = replacement };
            replacement.color = Color.red;
            var key = settings.GetCacheKey();
            replacement.color = Color.blue;

            Assert.AreNotEqual(key, settings.GetCacheKey());
        }

        /// <summary>
        /// Clearing a material must invalidate its previous valid key without throwing.
        /// </summary>
        [Test]
        public void GetCacheKey_ReplacementCleared_ChangesKey()
        {
            var settings = new MaterialReplaceSettings { material = replacement };
            var key = settings.GetCacheKey();
            settings.material = null;

            Assert.AreNotEqual(key, settings.GetCacheKey());
        }

        /// <summary>
        /// The additional settings path used by material preview must accept replacement settings.
        /// </summary>
        /// <param name="nullReplacement">Whether the replacement material is unset.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void GetCacheKey_AdditionalReplacement_DoesNotThrow(bool nullReplacement)
        {
            var settings = new AdditionalMaterialConvertSettings
            {
                targetMaterial = replacement,
                materialConvertSettings = new MaterialReplaceSettings
                {
                    material = nullReplacement ? null : replacement,
                },
            };

            Assert.DoesNotThrow(() => settings.GetCacheKey());
        }

        /// <summary>
        /// The aggregate key observed by MaterialConversionFilter must support replacements.
        /// Default replacements can be assigned programmatically even though the inspector excludes them.
        /// </summary>
        /// <param name="defaultReplacement">Whether to put the replacement in the default settings.</param>
        /// <param name="nullReplacement">Whether the replacement material is unset.</param>
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void GetCacheKey_ComponentReplacement_DoesNotThrow(bool defaultReplacement, bool nullReplacement)
        {
            var replacementSettings = new MaterialReplaceSettings
            {
                material = nullReplacement ? null : replacement,
            };
            IMaterialConversionComponent component = new TestMaterialConversionComponent
            {
                DefaultMaterialConvertSettings = defaultReplacement ? replacementSettings : (IMaterialConvertSettings)new TestConvertSettings(),
                AdditionalMaterialConvertSettings = defaultReplacement
                    ? new AdditionalMaterialConvertSettings[0]
                    : new[]
                    {
                        new AdditionalMaterialConvertSettings
                        {
                            targetMaterial = replacement,
                            materialConvertSettings = replacementSettings,
                        },
                    },
            };

            Assert.DoesNotThrow(() => component.GetCacheKey());
            var key = component.GetCacheKey();
            replacementSettings.material = nullReplacement ? replacement : null;
            Assert.AreNotEqual(key, component.GetCacheKey());
        }

        /// <summary>
        /// Supplies a non-replacement default for additional replacement tests.
        /// </summary>
        private class TestConvertSettings : IMaterialConvertSettings
        {
            /// <inheritdoc/>
            public MobileTextureFormat MobileTextureFormat => MobileTextureFormat.NoOverride;

            /// <inheritdoc/>
            public void LoadDefaultAssets()
            {
            }

            /// <inheritdoc/>
            public string GetCacheKey() => "default";
        }

        /// <summary>
        /// Uses the production interface's aggregate cache key without creating scene objects.
        /// </summary>
        private class TestMaterialConversionComponent : IMaterialConversionComponent
        {
            /// <inheritdoc/>
            public IMaterialConvertSettings DefaultMaterialConvertSettings { get; set; }

            /// <inheritdoc/>
            public AdditionalMaterialConvertSettings[] AdditionalMaterialConvertSettings { get; set; }

            /// <inheritdoc/>
            public bool RemoveExtraMaterialSlots => true;

            /// <inheritdoc/>
            public AvatarConverterNdmfPhase NdmfPhase => AvatarConverterNdmfPhase.Optimizing;

            /// <inheritdoc/>
            public bool IsPrimaryRoot => true;

            /// <inheritdoc/>
            public bool EnableMaterialPreview => true;

            /// <inheritdoc/>
            public bool ForceMaterialPreview => false;
        }
    }
}
