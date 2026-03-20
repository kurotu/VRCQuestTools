// <copyright file="VertexColorRemoverTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="VertexColorRemover"/>.
    /// </summary>
    public class VertexColorRemoverTests
    {
        /// <summary>
        /// Test RemoveVertexColor removes vertex colors from SkinnedMeshRenderer.
        /// </summary>
        [Test]
        public void RemoveVertexColor_RemovesFromSkinnedMeshRenderer()
        {
            var go = new GameObject("TestVCR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                mesh.colors32 = new Color32[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 0, 255, 255) };
                smr.sharedMesh = mesh;

                var vcr = go.AddComponent<VertexColorRemover>();
                vcr.includeChildren = false;
                LogAssert.ignoreFailingMessages = true;
                vcr.RemoveVertexColor();
                LogAssert.ignoreFailingMessages = false;

                Assert.AreEqual(0, mesh.colors32.Length);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test RemoveVertexColor does nothing when disabled.
        /// </summary>
        [Test]
        public void RemoveVertexColor_WhenDisabled_ReturnsEarly()
        {
            var go = new GameObject("TestVCR");
            try
            {
                var vcr = go.AddComponent<VertexColorRemover>();
                vcr.enabled = false;

                // Should not throw even when no renderer is present
                Assert.DoesNotThrow(() => vcr.RemoveVertexColor());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test RemoveVertexColor with includeChildren removes from children.
        /// </summary>
        [Test]
        public void RemoveVertexColor_IncludeChildren_RemovesFromChildren()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            try
            {
                var smr = child.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                mesh.colors32 = new Color32[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 0, 255, 255) };
                smr.sharedMesh = mesh;

                var vcr = parent.AddComponent<VertexColorRemover>();
                vcr.includeChildren = true;
                LogAssert.ignoreFailingMessages = true;
                vcr.RemoveVertexColor();
                LogAssert.ignoreFailingMessages = false;

                Assert.AreEqual(0, mesh.colors32.Length);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        /// <summary>
        /// Test RemoveVertexColor with MeshRenderer.
        /// </summary>
        [Test]
        public void RemoveVertexColor_MeshRenderer_Removes()
        {
            var go = new GameObject("TestVCR");
            try
            {
                var mf = go.AddComponent<MeshFilter>();
                var mr = go.AddComponent<MeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                mesh.colors32 = new Color32[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 0, 255, 255) };
                mf.sharedMesh = mesh;

                var vcr = go.AddComponent<VertexColorRemover>();
                vcr.includeChildren = false;
                LogAssert.ignoreFailingMessages = true;
                vcr.RemoveVertexColor();
                LogAssert.ignoreFailingMessages = false;

                Assert.AreEqual(0, mesh.colors32.Length);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test OnAfterDeserialize migration from old version.
        /// </summary>
        [Test]
        public void OnAfterDeserialize_DoesNotThrow()
        {
            var go = new GameObject("TestVCR");
            try
            {
                var vcr = go.AddComponent<VertexColorRemover>();
                Assert.DoesNotThrow(() => vcr.OnAfterDeserialize());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test OnBeforeSerialize does not throw.
        /// </summary>
        [Test]
        public void OnBeforeSerialize_DoesNotThrow()
        {
            var go = new GameObject("TestVCR");
            try
            {
                var vcr = go.AddComponent<VertexColorRemover>();
                Assert.DoesNotThrow(() => vcr.OnBeforeSerialize());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test RestoreVertexColor does not throw.
        /// </summary>
        [Test]
        public void RestoreVertexColor_DoesNotThrow()
        {
            var go = new GameObject("TestVCR");
            try
            {
                var vcr = go.AddComponent<VertexColorRemover>();
                Assert.DoesNotThrow(() => vcr.RestoreVertexColor());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test RestoreVertexColor with child renderers does not throw.
        /// </summary>
        [Test]
        public void RestoreVertexColor_WithChildRenderers_DoesNotThrow()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            try
            {
                var smr = child.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                smr.sharedMesh = mesh;

                var vcr = parent.AddComponent<VertexColorRemover>();
                vcr.includeChildren = true;
                Assert.DoesNotThrow(() => vcr.RestoreVertexColor());

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        /// <summary>
        /// Test RemoveVertexColor with mesh that has no vertex colors.
        /// </summary>
        [Test]
        public void RemoveVertexColor_NoVertexColors_DoesNothing()
        {
            var go = new GameObject("TestVCR");
            try
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                smr.sharedMesh = mesh;

                var vcr = go.AddComponent<VertexColorRemover>();
                LogAssert.ignoreFailingMessages = true;
                Assert.DoesNotThrow(() => vcr.RemoveVertexColor());
                LogAssert.ignoreFailingMessages = false;
                Assert.AreEqual(0, mesh.colors32.Length);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Test RemoveVertexColor includeChildren=true with MeshRenderer children.
        /// </summary>
        [Test]
        public void RemoveVertexColor_IncludeChildren_MeshRenderer()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            try
            {
                var mf = child.AddComponent<MeshFilter>();
                child.AddComponent<MeshRenderer>();
                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
                mesh.colors32 = new Color32[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(0, 0, 255, 255) };
                mf.sharedMesh = mesh;

                var vcr = parent.AddComponent<VertexColorRemover>();
                vcr.includeChildren = true;
                LogAssert.ignoreFailingMessages = true;
                vcr.RemoveVertexColor();
                LogAssert.ignoreFailingMessages = false;

                Assert.AreEqual(0, mesh.colors32.Length);

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }
    }
}
