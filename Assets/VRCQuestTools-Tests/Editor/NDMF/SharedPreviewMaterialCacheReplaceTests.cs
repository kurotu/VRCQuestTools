// <copyright file="SharedPreviewMaterialCacheReplaceTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="SharedPreviewMaterialCache.ReplaceTextureReferences"/>, used by
    /// <see cref="PreviewTextureCompressionQueue"/> (in the main VRCQuestTools-Editor assembly, via a registered
    /// callback -- see <c>RegisterWithPreviewTextureCompressionQueue</c>) to swap a progressive preview
    /// placeholder texture for its background-compressed replacement once compression finishes.
    /// </summary>
    public class SharedPreviewMaterialCacheReplaceTests
    {
        /// <summary>
        /// Verifies that <see cref="SharedPreviewMaterialCache.ReplaceTextureReferences"/> swaps a texture
        /// property on every cached converted material that references it, returns how many properties were
        /// replaced, and finds nothing left to replace on a second call once the old texture is no longer
        /// referenced by any cached material.
        /// </summary>
        [Test]
        public void ReplaceTextureReferences_SwapsTexturePropertyAndReturnsCount()
        {
            var shader = Shader.Find("Standard");
            Assert.IsNotNull(shader, "Standard shader should exist for this test.");

            var source = new Material(shader);
            var oldTex = TextureUtility.CreateColorTexture(new Color32(255, 0, 0, 255));
            var newTex = TextureUtility.CreateColorTexture(new Color32(0, 0, 255, 255));
            Material converted = null;

            var settingsMap = new Dictionary<Material, IMaterialConvertSettings>
            {
                [source] = new DummySettings(),
            };

            var lease = SharedPreviewMaterialCache.Acquire(settingsMap, m =>
            {
                converted = new Material(shader);
                converted.SetTexture("_MainTex", oldTex);
                return new Dictionary<Material, Material> { [source] = converted };
            });

            try
            {
                Assert.IsNotNull(converted, "Acquire should have produced a converted material via the convertFunc.");
                Assert.AreSame(oldTex, converted.GetTexture("_MainTex"));

                var replacedCount = SharedPreviewMaterialCache.ReplaceTextureReferences(oldTex, newTex);
                Assert.GreaterOrEqual(replacedCount, 1, "At least the _MainTex property on `converted` should have been replaced.");
                Assert.AreSame(newTex, converted.GetTexture("_MainTex"), "The converted material's texture property should now be the replacement.");

                var replacedAgain = SharedPreviewMaterialCache.ReplaceTextureReferences(oldTex, newTex);
                Assert.AreEqual(0, replacedAgain, "Nothing should reference the old texture anymore after the first replace.");
            }
            finally
            {
                // Releasing the lease drops the cache entry's refcount to zero, which destroys `converted` and,
                // since it is not backed by an asset, every texture it still references -- i.e. newTex. Do not
                // destroy newTex separately here, or this would double-destroy it.
                lease.Release();
                UnityEngine.Object.DestroyImmediate(source);
                UnityEngine.Object.DestroyImmediate(oldTex);
            }
        }

        /// <summary>
        /// Verifies that replacing a texture no cached material references at all (e.g. every lease was already
        /// released) is a safe no-op that reports zero replacements.
        /// </summary>
        [Test]
        public void ReplaceTextureReferences_NoCachedMaterialReferencesTexture_ReturnsZero()
        {
            var unreferenced = TextureUtility.CreateColorTexture(new Color32(10, 20, 30, 255));
            var replacement = TextureUtility.CreateColorTexture(new Color32(40, 50, 60, 255));
            try
            {
                var replacedCount = SharedPreviewMaterialCache.ReplaceTextureReferences(unreferenced, replacement);
                Assert.AreEqual(0, replacedCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(unreferenced);
                UnityEngine.Object.DestroyImmediate(replacement);
            }
        }

        private class DummySettings : IMaterialConvertSettings
        {
            private readonly string key = Guid.NewGuid().ToString("N");

            public MobileTextureFormat MobileTextureFormat => MobileTextureFormat.ASTC_6x6;

            public void LoadDefaultAssets()
            {
            }

            public string GetCacheKey() => key;
        }
    }
}
