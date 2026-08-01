// <copyright file="VertexColorRemoverFilterTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf.preview;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="VertexColorRemoverFilter"/> nodes driven directly through
    /// <see cref="IRenderFilter.Instantiate"/> with a manually constructed proxy renderer.
    /// </summary>
    public class VertexColorRemoverFilterTests
    {
        private NdmfTestAvatarBuilder builder;
        private GameObject proxyObject;
        private Mesh originalMesh;
        private Mesh proxyMesh;
        private IRenderFilterNode node;

        /// <summary>
        /// Cleans up objects created during the test. Disposing the node first destroys any
        /// colorless mesh it owns, so only the source meshes need explicit destruction here.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            node?.Dispose();
            node = null;

            builder?.Destroy();
            builder = null;

            if (proxyObject != null)
            {
                Object.DestroyImmediate(proxyObject);
                proxyObject = null;
            }

            if (originalMesh != null)
            {
                Object.DestroyImmediate(originalMesh);
                originalMesh = null;
            }

            if (proxyMesh != null)
            {
                Object.DestroyImmediate(proxyMesh);
                proxyMesh = null;
            }
        }

        /// <summary>
        /// Instantiate must return a usable no-op node instead of null when the avatar has no
        /// AvatarConverterSettings (e.g. removed between GetTargetGroups and Instantiate).
        /// NDMF's NodeController does not null-check Instantiate's result, so a null node would
        /// throw and take down the whole preview pipeline build.
        /// </summary>
        [Test]
        public void Instantiate_ReturnsNoOpNode_WhenSettingsMissing()
        {
            var original = CreateAvatarRenderer(addConverterSettings: false, removeVertexColor: false, target: Models.BuildTarget.Android);
            var proxy = CreateProxyRenderer();

            node = Instantiate(original, proxy);

            Assert.IsNotNull(node, "Instantiate must return a no-op node, not null, when the settings component is missing.");
            node.OnFrame(original, proxy);
            Assert.AreSame(proxyMesh, proxy.sharedMesh, "A no-op node must leave the proxy mesh unchanged.");
        }

        /// <summary>
        /// When vertex colors are not removed (removeVertexColor disabled and PC target), the node passes
        /// the source mesh through and Dispose must not destroy it: it may be an in-memory mesh owned by an
        /// upstream preview node.
        /// </summary>
        [Test]
        public void Dispose_DoesNotDestroySourceMesh_WhenNotRemoving()
        {
            var original = CreateAvatarRenderer(addConverterSettings: true, removeVertexColor: false, target: Models.BuildTarget.PC);
            var proxy = CreateProxyRenderer();
            node = Instantiate(original, proxy);

            node.OnFrame(original, proxy);
            Assert.AreSame(proxyMesh, proxy.sharedMesh, "A pass-through node must reuse the source mesh as-is.");

            node.Dispose();

            Assert.IsTrue(proxyMesh != null, "Dispose must not destroy a source mesh the node does not own.");
        }

        /// <summary>
        /// When vertex colors are removed, the node owns the cloned colorless mesh and Dispose must
        /// destroy it while leaving the source mesh alive.
        /// </summary>
        [Test]
        public void Dispose_DestroysColorlessMesh_WhenRemoving()
        {
            var original = CreateAvatarRenderer(addConverterSettings: true, removeVertexColor: true, target: Models.BuildTarget.Android);
            var proxy = CreateProxyRenderer();
            node = Instantiate(original, proxy);

            node.OnFrame(original, proxy);
            var colorlessMesh = proxy.sharedMesh;
            Assert.AreNotSame(proxyMesh, colorlessMesh, "A removing node must assign a newly cloned colorless mesh to the proxy.");
            Assert.IsTrue(colorlessMesh.colors32 == null || colorlessMesh.colors32.Length == 0, "The cloned mesh must have its vertex colors removed.");
            Assert.IsTrue(proxyMesh.colors32 != null && proxyMesh.colors32.Length > 0, "The source mesh must keep its vertex colors.");

            node.Dispose();

            Assert.IsTrue(colorlessMesh == null, "Dispose must destroy the colorless mesh the node owns.");
            Assert.IsTrue(proxyMesh != null, "Dispose must leave the source mesh alive.");
        }

        /// <summary>
        /// The colorless mesh only depends on the proxy's mesh, so blendshape, material and texture changes
        /// upstream must reuse the node while a mesh change or an invalidation of the node's own
        /// observations (updatedAspects == 0) must force a rebuild.
        /// </summary>
        [Test]
        public void FilterNode_Refresh_ReusesForShapesMaterialTexture_RebuildsForMeshAndZero()
        {
            var original = CreateAvatarRenderer(addConverterSettings: true, removeVertexColor: true, target: Models.BuildTarget.Android);
            var proxy = CreateProxyRenderer();
            node = Instantiate(original, proxy);
            var proxyPairs = new (Renderer, Renderer)[0];

            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Shapes), "Blendshape/bone updates cannot affect vertex colors, so the node must be reusable.");
            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Material), "Material changes cannot affect vertex colors, so the node must be reusable.");
            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Texture), "Texture changes cannot affect vertex colors, so the node must be reusable.");
            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Shapes | RenderAspects.Material | RenderAspects.Texture), "RenderAspects is a flag set; a combination of reusable aspects must still be reusable.");
            Assert.IsNull(Refresh(node, proxyPairs, RenderAspects.Mesh), "A mesh change upstream invalidates the colorless mesh and must force a rebuild.");
            Assert.IsNull(Refresh(node, proxyPairs, 0), "Zero means this node's own context was invalidated, which must force a rebuild.");
        }

        private static IRenderFilterNode Refresh(IRenderFilterNode node, (Renderer Original, Renderer Proxy)[] proxyPairs, RenderAspects updatedAspects)
        {
            var task = node.Refresh(proxyPairs, new ComputeContext($"test {updatedAspects}"), updatedAspects);
            Assert.IsTrue(task.IsCompleted, "VertexColorRemoverFilterNode.Refresh is synchronous and must complete immediately.");
            return task.Result;
        }

        private static Mesh CreateColoredTriangleMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.colors32 = new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255),
            };
            return mesh;
        }

        private static IRenderFilterNode Instantiate(Renderer original, Renderer proxy)
        {
            var group = RenderGroup.For(original);
            var pairs = new[] { (original, proxy) };
            var task = new VertexColorRemoverFilter().Instantiate(group, pairs, new ComputeContext("test"));
            Assert.IsTrue(task.IsCompleted, "VertexColorRemoverFilter.Instantiate is synchronous and must complete immediately.");
            return task.Result;
        }

        private SkinnedMeshRenderer CreateAvatarRenderer(bool addConverterSettings, bool removeVertexColor, Models.BuildTarget target)
        {
            originalMesh = CreateColoredTriangleMesh();
            builder = new NdmfTestAvatarBuilder();

            // Pin the resolved build target so the test does not depend on the editor's active build target.
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = target;
            if (addConverterSettings)
            {
                var settings = builder.Root.AddComponent<AvatarConverterSettings>();
                settings.removeVertexColor = removeVertexColor;
            }
            return builder.AddSkinnedMeshRenderer("Body", originalMesh);
        }

        private SkinnedMeshRenderer CreateProxyRenderer()
        {
            proxyMesh = Object.Instantiate(originalMesh);
            proxyObject = new GameObject("Proxy");
            var smr = proxyObject.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = proxyMesh;
            return smr;
        }
    }
}
