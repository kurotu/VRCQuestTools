// <copyright file="MeshFlipperTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="MeshFlipper"/>.
    /// </summary>
    public class MeshFlipperTests
    {
        private static Mesh CreateSimpleMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
            };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.RecalculateNormals();
            return mesh;
        }

        /// <summary>
        /// Test GetSharedMesh returns mesh from SkinnedMeshRenderer.
        /// </summary>
        [Test]
        public void GetSharedMesh_SkinnedMeshRenderer_ReturnsMesh()
        {
            var go = new GameObject("TestMF");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = CreateSimpleMesh();
                smr.sharedMesh = mesh;

                var mf = go.AddComponent<MeshFlipper>();
                Assert.AreEqual(mesh, mf.GetSharedMesh());

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test GetSharedMesh returns mesh from MeshFilter.
        /// </summary>
        [Test]
        public void GetSharedMesh_MeshFilter_ReturnsMesh()
        {
            var go = new GameObject("TestMF");
            try
            {
                var filter = go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                var mesh = CreateSimpleMesh();
                filter.sharedMesh = mesh;

                var mf = go.AddComponent<MeshFlipper>();
                Assert.AreEqual(mesh, mf.GetSharedMesh());

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test GetSharedMesh returns null when no renderer.
        /// </summary>
        [Test]
        public void GetSharedMesh_NoRenderer_ReturnsNull()
        {
            var go = new GameObject("TestMF");
            try
            {
                var mf = go.AddComponent<MeshFlipper>();
                Assert.IsNull(mf.GetSharedMesh());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test SetSharedMesh sets mesh on SkinnedMeshRenderer.
        /// </summary>
        [Test]
        public void SetSharedMesh_SkinnedMeshRenderer_SetsMesh()
        {
            var go = new GameObject("TestMF");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = CreateSimpleMesh();

                var mf = go.AddComponent<MeshFlipper>();
                mf.SetSharedMesh(mesh);
                Assert.AreEqual(mesh, smr.sharedMesh);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test SetSharedMesh sets mesh on MeshFilter.
        /// </summary>
        [Test]
        public void SetSharedMesh_MeshFilter_SetsMesh()
        {
            var go = new GameObject("TestMF");
            try
            {
                var filter = go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                var mesh = CreateSimpleMesh();

                var mf = go.AddComponent<MeshFlipper>();
                mf.SetSharedMesh(mesh);
                Assert.AreEqual(mesh, filter.sharedMesh);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CreateFlippedMesh in Flip mode reverses triangles.
        /// </summary>
        [Test]
        public void CreateFlippedMesh_FlipMode_ReversesTriangles()
        {
            var go = new GameObject("TestMF");
            try
            {
                var originalMesh = CreateSimpleMesh();

                var mf = go.AddComponent<MeshFlipper>();
                mf.direction = MeshFlipperMeshDirection.Flip;
                mf.useMask = false;

                var flipped = MeshFlipper.CreateFlippedMesh(mf, originalMesh);
                Assert.IsNotNull(flipped);
                Assert.AreEqual(3, flipped.triangles.Length);
                Assert.IsTrue(flipped.name.Contains("flipped"));

                // Verify triangle winding order is reversed
                var origTris = originalMesh.GetTriangles(0);
                var flipTris = flipped.GetTriangles(0);
                Assert.AreEqual(origTris[0], flipTris[2]);
                Assert.AreEqual(origTris[2], flipTris[0]);

                Object.DestroyImmediate(originalMesh);
                Object.DestroyImmediate(flipped);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CreateFlippedMesh in BothSides mode doubles triangles.
        /// </summary>
        [Test]
        public void CreateFlippedMesh_BothSidesMode_DoublesTriangles()
        {
            var go = new GameObject("TestMF");
            try
            {
                var originalMesh = CreateSimpleMesh();

                var mf = go.AddComponent<MeshFlipper>();
                mf.direction = MeshFlipperMeshDirection.BothSides;
                mf.useMask = false;

                var bothSides = MeshFlipper.CreateFlippedMesh(mf, originalMesh);
                Assert.IsNotNull(bothSides);
                Assert.AreEqual(6, bothSides.triangles.Length);
                Assert.IsTrue(bothSides.name.Contains("bothSides"));

                Object.DestroyImmediate(originalMesh);
                Object.DestroyImmediate(bothSides);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CreateFlippedMesh with mask in Flip mode, mask missing throws.
        /// </summary>
        [Test]
        public void CreateFlippedMesh_FlipWithMaskMissing_Throws()
        {
            var go = new GameObject("TestMF");
            try
            {
                var originalMesh = CreateSimpleMesh();

                var mf = go.AddComponent<MeshFlipper>();
                mf.direction = MeshFlipperMeshDirection.Flip;
                mf.useMask = true;
                mf.maskTexture = null;

                Assert.Throws<MeshFlipperMaskMissingException>(() =>
                    MeshFlipper.CreateFlippedMesh(mf, originalMesh));

                Object.DestroyImmediate(originalMesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CreateFlippedMesh with mask in BothSides mode, mask missing throws.
        /// </summary>
        [Test]
        public void CreateFlippedMesh_BothSidesWithMaskMissing_Throws()
        {
            var go = new GameObject("TestMF");
            try
            {
                var originalMesh = CreateSimpleMesh();

                var mf = go.AddComponent<MeshFlipper>();
                mf.direction = MeshFlipperMeshDirection.BothSides;
                mf.useMask = true;
                mf.maskTexture = null;

                Assert.Throws<MeshFlipperMaskMissingException>(() =>
                    MeshFlipper.CreateFlippedMesh(mf, originalMesh));

                Object.DestroyImmediate(originalMesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CreateFlippedMesh with readable mask in Flip mode.
        /// </summary>
        [Test]
        public void CreateFlippedMesh_FlipWithReadableMask_Succeeds()
        {
            var go = new GameObject("TestMF");
            try
            {
                var originalMesh = CreateSimpleMesh();

                var maskTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                maskTex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
                maskTex.Apply();

                var mf = go.AddComponent<MeshFlipper>();
                mf.direction = MeshFlipperMeshDirection.Flip;
                mf.useMask = true;
                mf.maskTexture = maskTex;
                mf.maskMode = MeshFlipperMaskMode.FlipWhite;

                var flipped = MeshFlipper.CreateFlippedMesh(mf, originalMesh);
                Assert.IsNotNull(flipped);

                Object.DestroyImmediate(originalMesh);
                Object.DestroyImmediate(flipped);
                Object.DestroyImmediate(maskTex);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CreateFlippedMesh with readable mask in BothSides mode.
        /// </summary>
        [Test]
        public void CreateFlippedMesh_BothSidesWithReadableMask_Succeeds()
        {
            var go = new GameObject("TestMF");
            try
            {
                var originalMesh = CreateSimpleMesh();

                var maskTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                maskTex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
                maskTex.Apply();

                var mf = go.AddComponent<MeshFlipper>();
                mf.direction = MeshFlipperMeshDirection.BothSides;
                mf.useMask = true;
                mf.maskTexture = maskTex;
                mf.maskMode = MeshFlipperMaskMode.FlipWhite;

                var bothSides = MeshFlipper.CreateFlippedMesh(mf, originalMesh);
                Assert.IsNotNull(bothSides);

                Object.DestroyImmediate(originalMesh);
                Object.DestroyImmediate(bothSides);
                Object.DestroyImmediate(maskTex);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CreateFlippedMesh with black mask mode.
        /// </summary>
        [Test]
        public void CreateFlippedMesh_FlipBlackMode_Succeeds()
        {
            var go = new GameObject("TestMF");
            try
            {
                var originalMesh = CreateSimpleMesh();

                var maskTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                maskTex.SetPixels(new Color[] { Color.black, Color.black, Color.black, Color.black });
                maskTex.Apply();

                var mf = go.AddComponent<MeshFlipper>();
                mf.direction = MeshFlipperMeshDirection.Flip;
                mf.useMask = true;
                mf.maskTexture = maskTex;
                mf.maskMode = MeshFlipperMaskMode.FlipBlack;

                var flipped = MeshFlipper.CreateFlippedMesh(mf, originalMesh);
                Assert.IsNotNull(flipped);

                Object.DestroyImmediate(originalMesh);
                Object.DestroyImmediate(flipped);
                Object.DestroyImmediate(maskTex);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CreateMesh uses the component's settings.
        /// </summary>
        [Test]
        public void CreateMesh_UsesComponentSettings()
        {
            var go = new GameObject("TestMF");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = CreateSimpleMesh();
                smr.sharedMesh = mesh;

                var mf = go.AddComponent<MeshFlipper>();
                mf.direction = MeshFlipperMeshDirection.Flip;
                mf.useMask = false;

                var result = mf.CreateMesh();
                Assert.IsNotNull(result);
                Assert.IsTrue(result.name.Contains("flipped"));

                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test default field values.
        /// </summary>
        [Test]
        public void DefaultValues_AreCorrect()
        {
            var go = new GameObject("TestMF");
            try
            {
                var mf = go.AddComponent<MeshFlipper>();
                Assert.AreEqual(MeshFlipperMeshDirection.BothSides, mf.direction);
                Assert.IsFalse(mf.enabledOnPC);
                Assert.IsTrue(mf.enabledOnAndroid);
                Assert.IsFalse(mf.useMask);
                Assert.IsNull(mf.maskTexture);
                Assert.AreEqual(MeshFlipperMaskMode.FlipWhite, mf.maskMode);
                Assert.AreEqual(MeshFlipperProcessingPhase.AfterPolygonReduction, mf.processingPhase);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
