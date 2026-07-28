// <copyright file="MaterialConversionFilterCacheTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using nadena.dev.ndmf.preview;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for NDMF preview material cache in <c>MaterialConversionFilter</c>.
    /// </summary>
    public class MaterialConversionFilterCacheTests
    {
        private const string SharedCacheTypeName = "KRT.VRCQuestTools.Ndmf.SharedPreviewMaterialCache, VRCQuestTools-Editor-Ndmf";

        /// <summary>
        /// GetSettingsCacheKey should not throw for MaterialReplaceSettings when replacement material is null.
        /// </summary>
        [Test]
        public void GetSettingsCacheKey_MaterialReplaceSettingsWithNullMaterial_DoesNotThrow()
        {
            var method = GetSharedCacheMethod("GetSettingsCacheKey");
            if (method == null)
            {
                Assert.Ignore("NDMF preview cache type was not found.");
            }

            var settings = new MaterialReplaceSettings
            {
                material = null,
            };

            Assert.DoesNotThrow(() =>
            {
                var key = (string)method.Invoke(null, new object[] { settings });
                Assert.That(key, Does.Contain("MaterialReplaceSettings_null"));
            });
        }

        /// <summary>
        /// GetSettingsCacheKey should include replacement material content for MaterialReplaceSettings.
        /// </summary>
        [Test]
        public void GetSettingsCacheKey_MaterialReplaceSettingsWithMaterial_ContainsMaterialIdentity()
        {
            var method = GetSharedCacheMethod("GetSettingsCacheKey");
            if (method == null)
            {
                Assert.Ignore("NDMF preview cache type was not found.");
            }

            var replacement = new Material(Shader.Find("Standard"));
            try
            {
                var settings = new MaterialReplaceSettings
                {
                    material = replacement,
                };

                var key = (string)method.Invoke(null, new object[] { settings });
                Assert.That(key, Does.Contain("MaterialReplaceSettings"));
                Assert.That(key, Does.Contain(replacement.GetInstanceID().ToString()));
            }
            finally
            {
                if (replacement != null)
                {
                    Object.DestroyImmediate(replacement);
                }
            }
        }

        /// <summary>
        /// Shared preview cache should keep converted material alive until all leases are released.
        /// </summary>
        [Test]
        public void AcquireRelease_TwoLeases_DestroyAfterLastRelease()
        {
            var acquireMethod = GetSharedCacheMethod("Acquire");
            if (acquireMethod == null)
            {
                Assert.Ignore("NDMF preview cache type was not found.");
            }

            var shader = Shader.Find("Standard");
            Assert.IsNotNull(shader, "Standard shader should exist for this test.");

            var original1 = new Material(shader);
            var original2 = new Material(shader);
            var replacement = new Material(shader);
            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            System.Func<Dictionary<Material, IMaterialConvertSettings>, Dictionary<Material, Material>> convertFunc = m => converter.ConvertMaterialsForMobile(m, false, string.Empty, null, false);
            
            object lease1 = null;
            object lease2 = null;
            try
            {
                var settings1 = new MaterialReplaceSettings
                {
                    material = replacement,
                };
                var settings2 = new MaterialReplaceSettings
                {
                    material = replacement,
                };

                var map1 = new Dictionary<Material, IMaterialConvertSettings>
                {
                    [original1] = settings1,
                };
                var map2 = new Dictionary<Material, IMaterialConvertSettings>
                {
                    [original2] = settings2,
                };

                lease1 = acquireMethod.Invoke(null, new object[] { map1, convertFunc });
                lease2 = acquireMethod.Invoke(null, new object[] { map2, convertFunc });

                var materialMap1 = GetLeaseMaterialMap(lease1);
                var materialMap2 = GetLeaseMaterialMap(lease2);

                Assert.IsTrue(materialMap1.ContainsKey(original1));
                Assert.IsTrue(materialMap2.ContainsKey(original2));
                Assert.AreSame(replacement, materialMap1[original1]);
                Assert.AreSame(replacement, materialMap2[original2]);

                ReleaseLease(lease1);
                lease1 = null;
                Assert.IsNotNull(replacement, "Replacement material should remain while another lease still references it.");

                ReleaseLease(lease2);
                lease2 = null;
                Assert.IsTrue(replacement == null, "Replacement material should be destroyed after the last lease is released.");
            }
            finally
            {
                if (lease1 != null)
                {
                    ReleaseLease(lease1);
                }

                if (lease2 != null)
                {
                    ReleaseLease(lease2);
                }

                if (original1 != null)
                {
                    Object.DestroyImmediate(original1);
                }

                if (original2 != null)
                {
                    Object.DestroyImmediate(original2);
                }

                if (replacement != null)
                {
                    Object.DestroyImmediate(replacement);
                }
            }
        }

        /// <summary>
        /// With extra material slots kept, OnFrame takes proxy.sharedMaterials.Length slots, so no mesh change
        /// can move which slots are remapped and Mesh is reusable alongside Shapes. Material and Texture never
        /// are -- returning the node for those would keep a stale conversion on screen. Zero is included because
        /// NDMF passes it when the node's own ComputeContext was invalidated, i.e. exactly when the convert
        /// settings or a source material changed.
        /// </summary>
        [Test]
        public void FilterNode_Refresh_KeepingExtraSlots_ReusesForShapeAndMeshChanges()
        {
            var node = CreateFilterNode(removeExtraMaterialSlots: false);
            var proxyPairs = new (Renderer, Renderer)[0];

            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Shapes), "Blendshape/bone updates cannot change material assignment, so the node must be reusable.");
            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Mesh), "With extra material slots kept, the slot count comes from sharedMaterials.Length, so a mesh change cannot invalidate the conversion.");
            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Shapes | RenderAspects.Mesh), "RenderAspects is a flag set; a combination of reusable aspects must still be reusable.");
            Assert.IsNull(Refresh(node, proxyPairs, RenderAspects.Material), "A material change upstream must force a rebuild.");
            Assert.IsNull(Refresh(node, proxyPairs, RenderAspects.Texture), "A texture change upstream must force a rebuild.");
            Assert.IsNull(Refresh(node, proxyPairs, RenderAspects.Mesh | RenderAspects.Material), "A reusable aspect combined with a non-reusable one must still force a rebuild.");
            Assert.IsNull(Refresh(node, proxyPairs, 0), "Zero means this node's own context was invalidated, which must force a rebuild.");
        }

        /// <summary>
        /// With extra material slots removed, OnFrame takes as many slots as the mesh has submeshes. An upstream
        /// node that raises that count would reach a slot that was an extra slot when Instantiate ran, whose
        /// material was therefore never converted, so a mesh change has to force a rebuild here.
        /// </summary>
        [Test]
        public void FilterNode_Refresh_RemovingExtraSlots_RebuildsForMeshChanges()
        {
            var node = CreateFilterNode(removeExtraMaterialSlots: true);
            var proxyPairs = new (Renderer, Renderer)[0];

            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Shapes), "Blendshape/bone updates stay reusable regardless of the slot handling.");
            Assert.IsNull(Refresh(node, proxyPairs, RenderAspects.Mesh), "The submesh count decides the slot count here, so a mesh change must force a rebuild.");
            Assert.IsNull(Refresh(node, proxyPairs, RenderAspects.Shapes | RenderAspects.Mesh), "A reusable aspect combined with a non-reusable one must still force a rebuild.");
        }

        /// <summary>
        /// A node returned by one of MaterialConversionFilter.Instantiate's early exits (wrong phase, non-mobile
        /// target, or a conversion that threw) holds no lease, and must always rebuild so the condition that made
        /// it a no-op is re-evaluated.
        /// </summary>
        [Test]
        public void FilterNode_Refresh_NoOpNodeWithoutLease_AlwaysRebuilds()
        {
            var nodeType = typeof(MaterialConversionFilter).GetNestedType("MaterialConversionFilterNode", BindingFlags.NonPublic);
            Assert.IsNotNull(nodeType, "MaterialConversionFilter.MaterialConversionFilterNode was not found.");

            var node = (IRenderFilterNode)System.Activator.CreateInstance(
                nodeType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { new Dictionary<Material, Material>(), false, null, null, null, null },
                null);

            Assert.IsNull(Refresh(node, new (Renderer, Renderer)[0], RenderAspects.Shapes));
        }

        private static IRenderFilterNode CreateFilterNode(bool removeExtraMaterialSlots)
        {
            var nodeType = typeof(MaterialConversionFilter).GetNestedType("MaterialConversionFilterNode", BindingFlags.NonPublic);
            Assert.IsNotNull(nodeType, "MaterialConversionFilter.MaterialConversionFilterNode was not found.");

            var lease = new SharedMaterialMapLease(new Dictionary<Material, Material>(), new HashSet<string>());
            return (IRenderFilterNode)System.Activator.CreateInstance(
                nodeType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { new Dictionary<Material, Material>(), removeExtraMaterialSlots, lease, null, null, new Material[0] },
                null);
        }

        private static IRenderFilterNode Refresh(IRenderFilterNode node, IEnumerable<(Renderer Original, Renderer Proxy)> proxyPairs, RenderAspects updatedAspects)
        {
            var task = node.Refresh(proxyPairs, new ComputeContext($"test {updatedAspects}"), updatedAspects);
            Assert.IsTrue(task.IsCompleted, "MaterialConversionFilterNode.Refresh is synchronous and must complete immediately.");
            return task.Result;
        }

        private static MethodInfo GetSharedCacheMethod(string methodName)
        {
            var type = System.Type.GetType(SharedCacheTypeName);
            return type?.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        private static Dictionary<Material, Material> GetLeaseMaterialMap(object lease)
        {
            var property = lease.GetType().GetProperty("MaterialMap", BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(lease) as Dictionary<Material, Material>;
        }

        private static void ReleaseLease(object lease)
        {
            var method = lease.GetType().GetMethod("Release", BindingFlags.Public | BindingFlags.Instance);
            method?.Invoke(lease, null);
        }
    }
}
