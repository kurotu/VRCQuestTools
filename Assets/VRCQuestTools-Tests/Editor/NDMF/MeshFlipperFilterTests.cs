// <copyright file="MeshFlipperFilterTests.cs" company="kurotu">
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
    /// Tests for <see cref="MeshFlipperFilter"/> nodes driven directly through
    /// <see cref="IRenderFilter.Instantiate"/> with a manually constructed proxy renderer.
    /// </summary>
    public class MeshFlipperFilterTests
    {
        private NdmfTestAvatarBuilder builder;
        private GameObject proxyObject;
        private Mesh originalMesh;
        private Mesh proxyMesh;
        private IRenderFilterNode node;

        /// <summary>
        /// Cleans up objects created during the test. Disposing the node first destroys any
        /// flipped mesh it owns, so only the source meshes need explicit destruction here.
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
        /// Instantiate must return a usable no-op node instead of null when the renderer has no mesh.
        /// NDMF's NodeController does not null-check Instantiate's result, so a null node would throw
        /// and take down the whole preview pipeline build.
        /// </summary>
        [Test]
        public void Instantiate_ReturnsNoOpNode_WhenMeshIsNull()
        {
            var original = CreateAvatarRenderer(mesh: null, addMeshFlipper: true, enabledOnPC: true);
            var proxy = CreateProxyRenderer(mesh: null);

            node = Instantiate(original, proxy);

            Assert.IsNotNull(node, "Instantiate must return a no-op node, not null, when the mesh is missing.");
            node.OnFrame(original, proxy);
            Assert.IsNull(proxy.sharedMesh, "A no-op node must leave the proxy mesh unchanged.");
        }

        /// <summary>
        /// Instantiate must return a usable no-op node instead of null when the MeshFlipper component
        /// is missing (e.g. removed between GetTargetGroups and Instantiate).
        /// </summary>
        [Test]
        public void Instantiate_ReturnsNoOpNode_WhenComponentMissing()
        {
            var original = CreateAvatarRenderer(CreateTriangleMesh(), addMeshFlipper: false, enabledOnPC: false);
            var proxy = CreateProxyRenderer(CreateProxyMesh());

            node = Instantiate(original, proxy);

            Assert.IsNotNull(node, "Instantiate must return a no-op node, not null, when the component is missing.");
            node.OnFrame(original, proxy);
            Assert.AreSame(proxyMesh, proxy.sharedMesh, "A no-op node must leave the proxy mesh unchanged.");
        }

        /// <summary>
        /// The flipped mesh only depends on the proxy's mesh, so blendshape, material and texture changes
        /// upstream must reuse the node while a mesh change or an invalidation of the node's own
        /// observations (updatedAspects == 0) must force a rebuild.
        /// </summary>
        [Test]
        public void FilterNode_Refresh_ReusesForShapesMaterialTexture_RebuildsForMeshAndZero()
        {
            var original = CreateAvatarRenderer(CreateTriangleMesh(), addMeshFlipper: true, enabledOnPC: true);
            var proxy = CreateProxyRenderer(CreateProxyMesh());
            node = Instantiate(original, proxy);
            var proxyPairs = new (Renderer, Renderer)[0];

            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Shapes), "Blendshape/bone updates cannot change the flipped geometry, so the node must be reusable.");
            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Material), "Material changes cannot change the flipped geometry, so the node must be reusable.");
            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Texture), "Texture changes cannot change the flipped geometry, so the node must be reusable.");
            Assert.AreSame(node, Refresh(node, proxyPairs, RenderAspects.Shapes | RenderAspects.Material | RenderAspects.Texture), "RenderAspects is a flag set; a combination of reusable aspects must still be reusable.");
            Assert.IsNull(Refresh(node, proxyPairs, RenderAspects.Mesh), "A mesh change upstream invalidates the flipped mesh and must force a rebuild.");
            Assert.IsNull(Refresh(node, proxyPairs, RenderAspects.Mesh | RenderAspects.Shapes), "A reusable aspect combined with a non-reusable one must still force a rebuild.");
            Assert.IsNull(Refresh(node, proxyPairs, 0), "Zero means this node's own context was invalidated, which must force a rebuild.");
        }

        /// <summary>
        /// When the flipper does not process (disabled for the resolved PC target), the node passes the
        /// source mesh through and Dispose must not destroy it: it may be an in-memory mesh owned by an
        /// upstream preview node.
        /// </summary>
        [Test]
        public void Dispose_DoesNotDestroySourceMesh_WhenNotProcessing()
        {
            var original = CreateAvatarRenderer(CreateTriangleMesh(), addMeshFlipper: true, enabledOnPC: false);
            var proxy = CreateProxyRenderer(CreateProxyMesh());
            node = Instantiate(original, proxy);

            node.OnFrame(original, proxy);
            Assert.AreSame(proxyMesh, proxy.sharedMesh, "A pass-through node must reuse the source mesh as-is.");

            node.Dispose();

            Assert.IsTrue(proxyMesh != null, "Dispose must not destroy a source mesh the node does not own.");
        }

        /// <summary>
        /// When the flipper processes, the node owns the flipped mesh and Dispose must destroy it while
        /// leaving the source mesh alive.
        /// </summary>
        [Test]
        public void Dispose_DestroysFlippedMesh_WhenProcessing()
        {
            var original = CreateAvatarRenderer(CreateTriangleMesh(), addMeshFlipper: true, enabledOnPC: true);
            var proxy = CreateProxyRenderer(CreateProxyMesh());
            node = Instantiate(original, proxy);

            node.OnFrame(original, proxy);
            var flippedMesh = proxy.sharedMesh;
            Assert.AreNotSame(proxyMesh, flippedMesh, "A processing node must assign a newly created flipped mesh to the proxy.");

            node.Dispose();

            Assert.IsTrue(flippedMesh == null, "Dispose must destroy the flipped mesh the node owns.");
            Assert.IsTrue(proxyMesh != null, "Dispose must leave the source mesh alive.");
        }

        private static IRenderFilterNode Refresh(IRenderFilterNode node, (Renderer Original, Renderer Proxy)[] proxyPairs, RenderAspects updatedAspects)
        {
            var task = node.Refresh(proxyPairs, new ComputeContext($"test {updatedAspects}"), updatedAspects);
            Assert.IsTrue(task.IsCompleted, "MeshFlipperFilterNode.Refresh is synchronous and must complete immediately.");
            return task.Result;
        }

        private static Mesh CreateTriangleMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up };
            mesh.triangles = new[] { 0, 1, 2 };
            return mesh;
        }

        private static IRenderFilterNode Instantiate(Renderer original, Renderer proxy)
        {
            var group = RenderGroup.For(original);
            var pairs = new[] { (original, proxy) };
            var task = new MeshFlipperFilter(MeshFlipperProcessingPhase.AfterPolygonReduction).Instantiate(group, pairs, new ComputeContext("test"));
            Assert.IsTrue(task.IsCompleted, "MeshFlipperFilter.Instantiate is synchronous and must complete immediately.");
            return task.Result;
        }

        private SkinnedMeshRenderer CreateAvatarRenderer(Mesh mesh, bool addMeshFlipper, bool enabledOnPC)
        {
            originalMesh = mesh;
            builder = new NdmfTestAvatarBuilder();

            // Pin the resolved build target so the test does not depend on the editor's active build target.
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.PC;
            var smr = builder.AddSkinnedMeshRenderer("Body", mesh);
            if (addMeshFlipper)
            {
                var meshFlipper = smr.gameObject.AddComponent<MeshFlipper>();
                meshFlipper.processingPhase = MeshFlipperProcessingPhase.AfterPolygonReduction;
                meshFlipper.enabledOnPC = enabledOnPC;
            }
            return smr;
        }

        private Mesh CreateProxyMesh()
        {
            proxyMesh = Object.Instantiate(originalMesh);
            return proxyMesh;
        }

        private SkinnedMeshRenderer CreateProxyRenderer(Mesh mesh)
        {
            proxyObject = new GameObject("Proxy");
            var smr = proxyObject.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh;
            return smr;
        }
    }
}
