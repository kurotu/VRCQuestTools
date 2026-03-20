// <copyright file="RendererUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="RendererUtility"/>.
    /// </summary>
    public class RendererUtilityTests
    {
        /// <summary>
        /// Test GetSharedMesh returns mesh from SkinnedMeshRenderer.
        /// </summary>
        [Test]
        public void GetSharedMesh_SkinnedMeshRenderer_ReturnsMesh()
        {
            var go = new GameObject("TestSMR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.name = "TestMesh";
                smr.sharedMesh = mesh;

                var result = RendererUtility.GetSharedMesh(smr);
                Assert.AreEqual(mesh, result);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test GetSharedMesh returns mesh from MeshRenderer with MeshFilter.
        /// </summary>
        [Test]
        public void GetSharedMesh_MeshRenderer_ReturnsMeshFromFilter()
        {
            var go = new GameObject("TestMR");
            try
            {
                var mf = go.AddComponent<MeshFilter>();
                var mr = go.AddComponent<MeshRenderer>();
                var mesh = new Mesh();
                mesh.name = "TestMesh";
                mf.sharedMesh = mesh;

                var result = RendererUtility.GetSharedMesh(mr);
                Assert.AreEqual(mesh, result);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test SetSharedMesh for SkinnedMeshRenderer.
        /// </summary>
        [Test]
        public void SetSharedMesh_SkinnedMeshRenderer_SetsMesh()
        {
            var go = new GameObject("TestSMR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.name = "TestMesh";

                RendererUtility.SetSharedMesh(smr, mesh);
                Assert.AreEqual(mesh, smr.sharedMesh);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test SetSharedMesh for MeshRenderer.
        /// </summary>
        [Test]
        public void SetSharedMesh_MeshRenderer_SetsMeshOnFilter()
        {
            var go = new GameObject("TestMR");
            try
            {
                var mf = go.AddComponent<MeshFilter>();
                var mr = go.AddComponent<MeshRenderer>();
                var mesh = new Mesh();
                mesh.name = "TestMesh";

                RendererUtility.SetSharedMesh(mr, mesh);
                Assert.AreEqual(mesh, mf.sharedMesh);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test GetSharedMeshSubMeshCount returns correct count.
        /// </summary>
        [Test]
        public void GetSharedMeshSubMeshCount_ReturnsMeshSubMeshCount()
        {
            var go = new GameObject("TestSMR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                mesh.subMeshCount = 1;
                mesh.SetTriangles(new int[] { 0, 1, 2 }, 0);
                smr.sharedMesh = mesh;

                var result = RendererUtility.GetSharedMeshSubMeshCount(smr);
                Assert.AreEqual(1, result);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test GetSharedMeshSubMeshCount returns 0 when no mesh.
        /// </summary>
        [Test]
        public void GetSharedMeshSubMeshCount_NoMesh_ReturnsZero()
        {
            var go = new GameObject("TestSMR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMesh = null;

                var result = RendererUtility.GetSharedMeshSubMeshCount(smr);
                Assert.AreEqual(0, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test RemoveVertexColor removes colors from mesh.
        /// </summary>
        [Test]
        public void RemoveVertexColor_RemovesColors()
        {
            var go = new GameObject("TestSMR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                mesh.colors32 = new Color32[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 0, 255, 255) };
                smr.sharedMesh = mesh;

                Assert.AreEqual(3, mesh.colors32.Length);
                RendererUtility.RemoveVertexColor(smr);
                Assert.AreEqual(0, mesh.colors32.Length);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test RemoveVertexColor does nothing when mesh has no colors.
        /// </summary>
        [Test]
        public void RemoveVertexColor_NoColors_DoesNothing()
        {
            var go = new GameObject("TestSMR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                smr.sharedMesh = mesh;

                Assert.DoesNotThrow(() => RendererUtility.RemoveVertexColor(smr));

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test RemoveVertexColor does nothing when no mesh.
        /// </summary>
        [Test]
        public void RemoveVertexColor_NullMesh_DoesNotThrow()
        {
            var go = new GameObject("TestSMR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMesh = null;

                Assert.DoesNotThrow(() => RendererUtility.RemoveVertexColor(smr));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test CopyBlendShapesWeights copies weights by name.
        /// </summary>
        [Test]
        public void CopyBlendShapesWeights_CopiesWeightsByName()
        {
            var go1 = new GameObject("Source");
            var go2 = new GameObject("Target");
            try
            {
                var smr1 = go1.AddComponent<SkinnedMeshRenderer>();
                var smr2 = go2.AddComponent<SkinnedMeshRenderer>();

                var sourceMesh = new Mesh();
                sourceMesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                var delta = new Vector3[] { Vector3.right, Vector3.right, Vector3.right };
                var normals = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
                var tangents = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
                sourceMesh.AddBlendShapeFrame("Shape1", 100f, delta, normals, tangents);
                smr1.sharedMesh = sourceMesh;
                smr1.SetBlendShapeWeight(0, 75f);

                var targetMesh = new Mesh();
                targetMesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                targetMesh.AddBlendShapeFrame("Shape1", 100f, delta, normals, tangents);
                smr2.sharedMesh = targetMesh;

                RendererUtility.CopyBlendShapesWeights(smr1, smr2);
                Assert.AreEqual(75f, smr2.GetBlendShapeWeight(0), 0.001f);

                Object.DestroyImmediate(sourceMesh);
                Object.DestroyImmediate(targetMesh);
            }
            finally
            {
                Object.DestroyImmediate(go1);
                Object.DestroyImmediate(go2);
            }
        }
    }
}
