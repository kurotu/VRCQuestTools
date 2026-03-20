// <copyright file="MaterialSwapTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="MaterialSwap"/>.
    /// </summary>
    public class MaterialSwapTests
    {
        /// <summary>
        /// Test ApplyMaterialSwaps replaces materials.
        /// </summary>
        [Test]
        public void ApplyMaterialSwaps_ReplacesMaterials()
        {
            var go = new GameObject("TestSwap");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var originalMat = new Material(Shader.Find("Standard"));
                originalMat.name = "Original";
                var replacementMat = new Material(Shader.Find("Standard"));
                replacementMat.name = "Replacement";
                smr.sharedMaterials = new Material[] { originalMat };

                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings.Add(new MaterialSwap.MaterialMapping
                {
                    originalMaterial = originalMat,
                    replacementMaterial = replacementMat,
                });

                swap.ApplyMaterialSwaps();

                Assert.AreEqual(replacementMat, smr.sharedMaterials[0]);

                Object.DestroyImmediate(originalMat);
                Object.DestroyImmediate(replacementMat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test ApplyMaterialSwaps with no matching material does nothing.
        /// </summary>
        [Test]
        public void ApplyMaterialSwaps_NoMatch_DoesNotChange()
        {
            var go = new GameObject("TestSwap");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                mat.name = "Original";
                var otherMat = new Material(Shader.Find("Standard"));
                otherMat.name = "Other";
                var replacementMat = new Material(Shader.Find("Standard"));
                smr.sharedMaterials = new Material[] { mat };

                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings.Add(new MaterialSwap.MaterialMapping
                {
                    originalMaterial = otherMat,
                    replacementMaterial = replacementMat,
                });

                swap.ApplyMaterialSwaps();

                Assert.AreEqual(mat, smr.sharedMaterials[0]);

                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(otherMat);
                Object.DestroyImmediate(replacementMat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test ApplyMaterialSwaps works with children.
        /// </summary>
        [Test]
        public void ApplyMaterialSwaps_WorksWithChildren()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            try
            {
                var smr = child.AddComponent<SkinnedMeshRenderer>();
                var originalMat = new Material(Shader.Find("Standard"));
                var replacementMat = new Material(Shader.Find("Standard"));
                smr.sharedMaterials = new Material[] { originalMat };

                var swap = parent.AddComponent<MaterialSwap>();
                swap.materialMappings.Add(new MaterialSwap.MaterialMapping
                {
                    originalMaterial = originalMat,
                    replacementMaterial = replacementMat,
                });

                swap.ApplyMaterialSwaps();

                Assert.AreEqual(replacementMat, smr.sharedMaterials[0]);

                Object.DestroyImmediate(originalMat);
                Object.DestroyImmediate(replacementMat);
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        /// <summary>
        /// Test ApplyMaterialSwaps with null replacement does not swap.
        /// </summary>
        [Test]
        public void ApplyMaterialSwaps_NullReplacement_DoesNotSwap()
        {
            var go = new GameObject("TestSwap");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var originalMat = new Material(Shader.Find("Standard"));
                smr.sharedMaterials = new Material[] { originalMat };

                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings.Add(new MaterialSwap.MaterialMapping
                {
                    originalMaterial = originalMat,
                    replacementMaterial = null,
                });

                swap.ApplyMaterialSwaps();

                Assert.AreEqual(originalMat, smr.sharedMaterials[0]);

                Object.DestroyImmediate(originalMat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test ApplyMaterialSwaps with empty mappings does nothing.
        /// </summary>
        [Test]
        public void ApplyMaterialSwaps_EmptyMappings_DoesNotThrow()
        {
            var go = new GameObject("TestSwap");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                smr.sharedMaterials = new Material[] { mat };

                var swap = go.AddComponent<MaterialSwap>();
                Assert.DoesNotThrow(() => swap.ApplyMaterialSwaps());

                Object.DestroyImmediate(mat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test default materialMappings is empty.
        /// </summary>
        [Test]
        public void DefaultMaterialMappings_IsEmpty()
        {
            var go = new GameObject("TestSwap");
            try
            {
                var swap = go.AddComponent<MaterialSwap>();
                Assert.IsNotNull(swap.materialMappings);
                Assert.AreEqual(0, swap.materialMappings.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test MaterialMapping class can be constructed.
        /// </summary>
        [Test]
        public void MaterialMapping_DefaultValues()
        {
            var mapping = new MaterialSwap.MaterialMapping();
            Assert.IsNull(mapping.originalMaterial);
            Assert.IsNull(mapping.replacementMaterial);
        }

        /// <summary>
        /// Test ApplyMaterialSwaps skips null materials in renderer.
        /// </summary>
        [Test]
        public void ApplyMaterialSwaps_NullMaterialInSlot_Skips()
        {
            var go = new GameObject("TestSwap");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMaterials = new Material[] { null };

                var swap = go.AddComponent<MaterialSwap>();
                Assert.DoesNotThrow(() => swap.ApplyMaterialSwaps());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
