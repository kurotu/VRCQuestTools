// <copyright file="MeshFlipperPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="MeshFlipperPass"/> and <see cref="MeshFlipperAfterPolygonReductionPass"/>
    /// (both backed by <see cref="MeshFlipperPassUtility"/>) driven through NDMF's <see cref="BuildContext"/>.
    /// </summary>
    public class MeshFlipperPassTests
    {
        private NdmfTestAvatarBuilder builder;
        private Mesh originalMesh;

        /// <summary>
        /// Cleans up objects created during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            builder?.Destroy();
            builder = null;

            if (originalMesh != null)
            {
                Object.DestroyImmediate(originalMesh);
                originalMesh = null;
            }
        }

        /// <summary>
        /// The pass should duplicate the mesh, leave the original untouched, and register the
        /// replacement in ObjectRegistry.
        /// </summary>
        [Test]
        public void Execute_FlipsMeshAndTracksObjectRegistry()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            originalMesh = new Mesh();
            originalMesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            originalMesh.triangles = new[] { 0, 1, 2 };

            var smr = builder.AddSkinnedMeshRenderer("Body", originalMesh);
            var flipper = smr.gameObject.AddComponent<MeshFlipper>();
            flipper.direction = MeshFlipperMeshDirection.Flip;
            flipper.processingPhase = MeshFlipperProcessingPhase.BeforePolygonReduction;

            var context = new BuildContext(builder.Root, null);
            using (new ObjectRegistryScope(context.ObjectRegistry))
            {
                MeshFlipperPass.Instance.RunForTest(context);

                var reference = NdmfObjectRegistry.GetReference(smr.sharedMesh);
                Assert.AreSame(originalMesh, reference.Object, "ObjectRegistry should trace the flipped mesh back to the original.");
            }

            Assert.AreNotSame(originalMesh, smr.sharedMesh, "Pass should replace the mesh with a flipped duplicate, not mutate it in place.");
            Assert.AreEqual(new[] { 2, 1, 0 }, smr.sharedMesh.triangles, "Flipped mesh should have reversed winding order.");
            Assert.AreEqual(new[] { 0, 1, 2 }, originalMesh.triangles, "Original mesh asset must remain untouched.");

            Object.DestroyImmediate(smr.sharedMesh);
        }

        /// <summary>
        /// Two MeshFlippers referencing the same original mesh must resolve to the same flipped
        /// mesh instance instead of each producing its own duplicate.
        /// </summary>
        [Test]
        public void Execute_DeduplicatesFlippedMesh_ForSharedOriginal()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            originalMesh = new Mesh();
            originalMesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            originalMesh.triangles = new[] { 0, 1, 2 };

            var smrA = builder.AddSkinnedMeshRenderer("BodyA", originalMesh);
            var flipperA = smrA.gameObject.AddComponent<MeshFlipper>();
            flipperA.direction = MeshFlipperMeshDirection.Flip;
            flipperA.processingPhase = MeshFlipperProcessingPhase.BeforePolygonReduction;

            var smrB = builder.AddSkinnedMeshRenderer("BodyB", originalMesh);
            var flipperB = smrB.gameObject.AddComponent<MeshFlipper>();
            flipperB.direction = MeshFlipperMeshDirection.Flip;
            flipperB.processingPhase = MeshFlipperProcessingPhase.BeforePolygonReduction;

            var context = new BuildContext(builder.Root, null);
            MeshFlipperPass.Instance.RunForTest(context);

            Assert.AreSame(smrA.sharedMesh, smrB.sharedMesh, "Renderers sharing the original mesh must resolve to the same flipped mesh instance.");

            Object.DestroyImmediate(smrA.sharedMesh);
        }

        /// <summary>
        /// MeshFlipperAfterPolygonReductionPass must only process MeshFlippers configured for the
        /// AfterPolygonReduction phase, ignoring ones configured for BeforePolygonReduction.
        /// </summary>
        [Test]
        public void Execute_AfterPolygonReductionPass_IgnoresBeforePhaseFlippers()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            originalMesh = new Mesh();
            originalMesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            originalMesh.triangles = new[] { 0, 1, 2 };

            var smr = builder.AddSkinnedMeshRenderer("Body", originalMesh);
            var flipper = smr.gameObject.AddComponent<MeshFlipper>();
            flipper.direction = MeshFlipperMeshDirection.Flip;
            flipper.processingPhase = MeshFlipperProcessingPhase.BeforePolygonReduction;

            var context = new BuildContext(builder.Root, null);
            MeshFlipperAfterPolygonReductionPass.Instance.RunForTest(context);

            Assert.AreSame(originalMesh, smr.sharedMesh, "A BeforePolygonReduction-phase flipper must not be processed by the AfterPolygonReduction pass.");
        }
    }
}
