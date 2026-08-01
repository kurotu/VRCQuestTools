// <copyright file="MaterialConversionFilterGroupTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_NDMF_HAS_PROP_CACHE_DEBUG
using System.Linq;
using KRT.VRCQuestTools.Models;
#endif
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf.preview;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for the render groups built by <see cref="MaterialConversionFilter.GetTargetGroups"/>,
    /// which attach the primary conversion component as typed group data so that a change of the
    /// primary component's identity makes the groups compare unequal.
    /// </summary>
    public class MaterialConversionFilterGroupTests
    {
        private NdmfTestAvatarBuilder builder;
        private GameObject rendererObject;
        private Mesh mesh;
        private Material material;

        /// <summary>
        /// Cleans up objects created during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            builder?.Destroy();
            builder = null;

            if (rendererObject != null)
            {
                Object.DestroyImmediate(rendererObject);
                rendererObject = null;
            }

            if (mesh != null)
            {
                Object.DestroyImmediate(mesh);
                mesh = null;
            }

            if (material != null)
            {
                Object.DestroyImmediate(material);
                material = null;
            }
        }

        /// <summary>
        /// Canary for the NDMF equality semantics MaterialConversionFilter relies on: groups over the
        /// same renderers must compare equal when the attached data is the same component and unequal
        /// when it is a different component. If NDMF ever changed this, stale nodes would be reused
        /// with the wrong settings component.
        /// </summary>
        [Test]
        public void RenderGroup_WithData_ChangesEquality_WhenPrimaryComponentDiffers()
        {
            rendererObject = new GameObject("Renderer");
            var renderer = rendererObject.AddComponent<SkinnedMeshRenderer>();
            var a = rendererObject.AddComponent<MaterialConversionSettings>();
            var b = rendererObject.AddComponent<MaterialConversionSettings>();

            var groupA1 = RenderGroup.For(renderer).WithData<Component>(a);
            var groupA2 = RenderGroup.For(renderer).WithData<Component>(a);
            var groupB = RenderGroup.For(renderer).WithData<Component>(b);

            Assert.AreEqual(groupA1, groupA2, "Groups over the same renderers with the same data component must be equal.");
            Assert.AreNotEqual(groupA1, groupB, "Groups over the same renderers with different data components must not be equal.");
        }

#if VQT_NDMF_HAS_PROP_CACHE_DEBUG
        /// <summary>
        /// GetTargetGroups must attach the primary conversion component as group data, and switching the
        /// primary component (adding AvatarConverterSettings next to MaterialConversionSettings) must
        /// produce an unequal group even though the renderer set is unchanged, so NDMF discards the prior
        /// node instead of silently reusing it with the stale settings.
        /// </summary>
        [Test]
        public void GetTargetGroups_AttachesPrimaryComponentAsGroupData()
        {
            builder = new NdmfTestAvatarBuilder();
            var conversionSettings = builder.Root.AddComponent<MaterialConversionSettings>();
            conversionSettings.ndmfPhase = AvatarConverterNdmfPhase.Optimizing;

            mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            material = new Material(Shader.Find("Standard"));
            var renderer = builder.AddSkinnedMeshRenderer("Body", mesh, material);

            var firstGroup = GetGroupFor(renderer);
            Assert.AreSame(conversionSettings, firstGroup.GetData<Component>(), "The primary conversion component must be attached as group data.");

            // AvatarConverterSettings on the root takes over as the primary component
            // (MaterialConversionSettings.IsPrimaryRoot turns false next to it).
            var converterSettings = builder.Root.AddComponent<AvatarConverterSettings>();
            converterSettings.ndmfPhase = AvatarConverterNdmfPhase.Optimizing;

            var secondGroup = GetGroupFor(renderer);
            Assert.AreSame(converterSettings, secondGroup.GetData<Component>(), "The added AvatarConverterSettings must take over as the primary component.");
            Assert.IsTrue(firstGroup.Renderers.SequenceEqual(secondGroup.Renderers), "The renderer set must be unchanged by the primary component switch.");
            Assert.AreNotEqual(firstGroup, secondGroup, "Groups must compare unequal when the primary component's identity changed, so NDMF rebuilds the node.");
        }

        private static RenderGroup GetGroupFor(Renderer renderer)
        {
            // Global queries are cached and don't observe objects created inside a synchronous test body.
            // PropCacheDebug.InvalidateAllCaches() itself requires NDMF 1.10.0+ (see VQT_NDMF_HAS_PROP_CACHE_DEBUG),
            // so GetTargetGroups() is only exercised here.
            PropCacheDebug.InvalidateAllCaches();
            var groups = new MaterialConversionFilter(AvatarConverterNdmfPhase.Optimizing).GetTargetGroups(new ComputeContext("test"));
            return groups.Single(g => g.Renderers.Contains(renderer));
        }
#endif
    }
}
