// Tests for NDMF Preview Filter inner node classes via reflection.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class MaterialConversionFilterNodeTests
    {
        private Type nodeType;

        [SetUp]
        public void SetUp()
        {
            nodeType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionFilter+MaterialConversionFilterNode");
            if (nodeType == null) Assert.Ignore("MaterialConversionFilterNode not found");
        }

        [Test]
        public void Constructor_WithEmptyMap_CreatesInstance()
        {
            var map = new Dictionary<Material, Material>();
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);
            Assert.IsNotNull(instance);
        }

        [Test]
        public void WhatChanged_ReturnsMaterial()
        {
            var map = new Dictionary<Material, Material>();
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);
            var whatChanged = NdmfTestHelper.GetProperty(instance, "WhatChanged");
            Assert.IsNotNull(whatChanged);
        }

        [Test]
        public void OnFrame_EmptyMap_DoesNotModifyProxy()
        {
            var map = new Dictionary<Material, Material>();
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var go = new GameObject("TestRenderer");
            var meshFilter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            var originalMat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new[] { originalMat };

            try
            {
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", renderer, renderer);
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
                Assert.AreEqual(originalMat, renderer.sharedMaterials[0]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(originalMat);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnFrame_WithMaterialMap_ReplacesMaterial()
        {
            var originalMat = new Material(Shader.Find("Standard"));
            originalMat.name = "Original";
            var convertedMat = new Material(Shader.Find("Standard"));
            convertedMat.name = "Converted";

            var map = new Dictionary<Material, Material> { { originalMat, convertedMat } };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var go = new GameObject("TestRenderer");
            go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { originalMat };

            try
            {
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", renderer, renderer);
                Assert.AreEqual(convertedMat, renderer.sharedMaterials[0]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(originalMat);
                UnityEngine.Object.DestroyImmediate(convertedMat);
            }
        }

        [Test]
        public void OnFrame_WithUnmappedMaterial_KeepsOriginal()
        {
            var mappedMat = new Material(Shader.Find("Standard"));
            var convertedMat = new Material(Shader.Find("Standard"));
            var unmappedMat = new Material(Shader.Find("Standard"));

            var map = new Dictionary<Material, Material> { { mappedMat, convertedMat } };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var go = new GameObject("TestRenderer");
            go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { unmappedMat };

            try
            {
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", renderer, renderer);
                Assert.AreEqual(unmappedMat, renderer.sharedMaterials[0]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(mappedMat);
                UnityEngine.Object.DestroyImmediate(convertedMat);
                UnityEngine.Object.DestroyImmediate(unmappedMat);
            }
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var mat = new Material(Shader.Find("Standard"));
            var convertedMat = new Material(Shader.Find("Standard"));
            var map = new Dictionary<Material, Material> { { mat, convertedMat } };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var disposeMethod = instance.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));

            UnityEngine.Object.DestroyImmediate(mat);
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var map = new Dictionary<Material, Material>();
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var disposeMethod = instance.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            disposeMethod.Invoke(instance, null);
            Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
        }

        [Test]
        public void OnFrame_WithRemoveExtraMaterialSlots_True()
        {
            var originalMat = new Material(Shader.Find("Standard"));
            var convertedMat = new Material(Shader.Find("Standard"));

            var map = new Dictionary<Material, Material> { { originalMat, convertedMat } };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, true);

            var go = new GameObject("TestRenderer");
            var mf = go.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            mesh.subMeshCount = 1;
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.SetTriangles(new int[] { 0, 1, 2 }, 0);
            mf.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { originalMat, originalMat };

            try
            {
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", renderer, renderer);
                // With removeExtraMaterialSlots=true and 1 submesh, should only keep 1 material
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(originalMat);
                UnityEngine.Object.DestroyImmediate(convertedMat);
            }
        }
    }

    [TestFixture]
    internal class MeshFlipperFilterNodeTests
    {
        private Type nodeType;

        [SetUp]
        public void SetUp()
        {
            nodeType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperFilter+MeshFlipperFilterNode");
            if (nodeType == null) Assert.Ignore("MeshFlipperFilterNode not found");
        }

        [Test]
        public void Constructor_CreatesInstance()
        {
            var mesh = new Mesh();
            try
            {
                var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);
                Assert.IsNotNull(instance);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void WhatChanged_ReturnsMesh()
        {
            var mesh = new Mesh();
            try
            {
                var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);
                var whatChanged = NdmfTestHelper.GetProperty(instance, "WhatChanged");
                Assert.IsNotNull(whatChanged);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void OnFrame_SkinnedMeshRenderer_SetsMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            var flippedMesh = new Mesh();
            flippedMesh.vertices = new Vector3[] { Vector3.up, Vector3.one, Vector3.zero };

            var go = new GameObject("TestSMR");
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh;

            try
            {
                var instance = NdmfTestHelper.CreateInstance(nodeType, flippedMesh);
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", smr, smr);
                Assert.AreEqual(flippedMesh, smr.sharedMesh);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(mesh);
                UnityEngine.Object.DestroyImmediate(flippedMesh);
            }
        }

        [Test]
        public void OnFrame_MeshRenderer_SetsMeshViaFilter()
        {
            var mesh = new Mesh();
            var flippedMesh = new Mesh();

            var go = new GameObject("TestMR");
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();

            try
            {
                var instance = NdmfTestHelper.CreateInstance(nodeType, flippedMesh);
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", mr, mr);
                Assert.AreEqual(flippedMesh, mf.sharedMesh);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(mesh);
                UnityEngine.Object.DestroyImmediate(flippedMesh);
            }
        }

        [Test]
        public void OnFrame_MeshRendererWithoutFilter_DoesNotThrow()
        {
            var flippedMesh = new Mesh();
            var go = new GameObject("TestMR_NoFilter");
            var mr = go.AddComponent<MeshRenderer>();

            try
            {
                var instance = NdmfTestHelper.CreateInstance(nodeType, flippedMesh);
                Assert.DoesNotThrow(() => NdmfTestHelper.InvokeMethod(instance, "OnFrame", mr, mr));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(flippedMesh);
            }
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var mesh = new Mesh();
            var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);
            var disposeMethod = instance.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var mesh = new Mesh();
            var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);
            var disposeMethod = instance.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            disposeMethod.Invoke(instance, null);
            Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
        }
    }

    [TestFixture]
    internal class VertexColorRemoverFilterNodeTests
    {
        private Type nodeType;

        [SetUp]
        public void SetUp()
        {
            nodeType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.VertexColorRemoverFilter+VertexColorRemoverFilterNode");
            if (nodeType == null) Assert.Ignore("VertexColorRemoverFilterNode not found");
        }

        [Test]
        public void Constructor_CreatesInstance()
        {
            var mesh = new Mesh();
            try
            {
                var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);
                Assert.IsNotNull(instance);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void WhatChanged_ReturnsMesh()
        {
            var mesh = new Mesh();
            try
            {
                var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);
                var whatChanged = NdmfTestHelper.GetProperty(instance, "WhatChanged");
                Assert.IsNotNull(whatChanged);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void OnFrame_SkinnedMeshRenderer_SetsMesh()
        {
            var originalMesh = new Mesh();
            originalMesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };

            var colorlessMesh = new Mesh();
            colorlessMesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };

            var go = new GameObject("TestSMR");
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = originalMesh;

            try
            {
                var instance = NdmfTestHelper.CreateInstance(nodeType, colorlessMesh);
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", smr, smr);
                Assert.AreEqual(colorlessMesh, smr.sharedMesh);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(originalMesh);
                UnityEngine.Object.DestroyImmediate(colorlessMesh);
            }
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var mesh = new Mesh();
            var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);
            var disposeMethod = instance.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var mesh = new Mesh();
            var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);
            var disposeMethod = instance.GetType().GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            disposeMethod.Invoke(instance, null);
            Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
        }
    }

    [TestFixture]
    internal class MaterialConversionFilterConstructorTests
    {
        private Type filterType;

        [SetUp]
        public void SetUp()
        {
            filterType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionFilter");
            if (filterType == null) Assert.Ignore("MaterialConversionFilter not found");
        }

        [Test]
        public void Constructor_Optimizing_CreatesInstance()
        {
            var phaseType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterNdmfPhase");
            if (phaseType == null) Assert.Ignore("AvatarConverterNdmfPhase not found");
            var optimizing = Enum.Parse(phaseType, "Optimizing");
            var instance = NdmfTestHelper.CreateInstance(filterType, optimizing);
            Assert.IsNotNull(instance);
        }

        [Test]
        public void GetPreviewControlNodes_Optimizing_ReturnsNodes()
        {
            var phaseType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterNdmfPhase");
            if (phaseType == null) Assert.Ignore("AvatarConverterNdmfPhase not found");
            var optimizing = Enum.Parse(phaseType, "Optimizing");
            var instance = NdmfTestHelper.CreateInstance(filterType, optimizing);
            var nodes = NdmfTestHelper.InvokeMethod(instance, "GetPreviewControlNodes");
            Assert.IsNotNull(nodes);
        }

        [Test]
        public void GetPreviewControlNodes_NonOptimizing_ReturnsEmpty()
        {
            var phaseType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.AvatarConverterNdmfPhase");
            if (phaseType == null) Assert.Ignore("AvatarConverterNdmfPhase not found");
            var transforming = Enum.Parse(phaseType, "Transforming");
            var instance = NdmfTestHelper.CreateInstance(filterType, transforming);
            var method = filterType.GetMethod("GetPreviewControlNodes");
            if (method == null) Assert.Ignore("GetPreviewControlNodes not found");
            var result = method.Invoke(instance, null);
            var enumerable = result as System.Collections.IEnumerable;
            int count = 0;
            foreach (var _ in enumerable)
            {
                count++;
            }
            Assert.AreEqual(0, count);
        }
    }

    [TestFixture]
    internal class MeshFlipperFilterConstructorTests
    {
        private Type filterType;

        [SetUp]
        public void SetUp()
        {
            filterType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperFilter");
            if (filterType == null) Assert.Ignore("MeshFlipperFilter not found");
        }

        [Test]
        public void Constructor_CreatesInstance()
        {
            var phaseType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperProcessingPhase");
            if (phaseType == null) Assert.Ignore("MeshFlipperProcessingPhase not found");
            var phase = Enum.GetValues(phaseType).GetValue(0);
            var instance = NdmfTestHelper.CreateInstance(filterType, phase);
            Assert.IsNotNull(instance);
        }

        [Test]
        public void GetPreviewControlNodes_BeforePolygonReduction_ReturnsNodes()
        {
            var phaseType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperProcessingPhase");
            if (phaseType == null) Assert.Ignore("MeshFlipperProcessingPhase not found");
            var phase = Enum.Parse(phaseType, "BeforePolygonReduction");
            var instance = NdmfTestHelper.CreateInstance(filterType, phase);
            var nodes = NdmfTestHelper.InvokeMethod(instance, "GetPreviewControlNodes");
            Assert.IsNotNull(nodes);
        }
    }

    [TestFixture]
    internal class VertexColorRemoverFilterConstructorTests
    {
        private Type filterType;

        [SetUp]
        public void SetUp()
        {
            filterType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.VertexColorRemoverFilter");
            if (filterType == null) Assert.Ignore("VertexColorRemoverFilter not found");
        }

        [Test]
        public void Constructor_CreatesInstance()
        {
            var instance = Activator.CreateInstance(filterType);
            Assert.IsNotNull(instance);
        }

        [Test]
        public void GetPreviewControlNodes_ReturnsNodes()
        {
            var instance = Activator.CreateInstance(filterType);
            var nodes = NdmfTestHelper.InvokeMethod(instance, "GetPreviewControlNodes");
            Assert.IsNotNull(nodes);
        }
    }

    [TestFixture]
    internal class RemoveVertexColorPassTests
    {
        private Type passType;

        [SetUp]
        public void SetUp()
        {
            passType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.RemoveVertexColorPass");
            if (passType == null) Assert.Ignore("RemoveVertexColorPass not found");
        }

        [Test]
        public void DisplayName_ReturnsExpectedValue()
        {
            var instance = Activator.CreateInstance(passType);
            var displayName = NdmfTestHelper.GetProperty(instance, "DisplayName");
            Assert.AreEqual("Remove vertex color", displayName);
        }

        [Test]
        public void ShouldRemoveVertexColor_NullMesh_ReturnsFalse()
        {
            var instance = Activator.CreateInstance(passType);
            var method = passType.GetMethod("ShouldRemoveVertexColor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) Assert.Ignore("ShouldRemoveVertexColor not found");
            var result = (bool)method.Invoke(instance, new object[] { null });
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldRemoveVertexColor_NoColors_ReturnsFalse()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            try
            {
                var instance = Activator.CreateInstance(passType);
                var method = passType.GetMethod("ShouldRemoveVertexColor", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null) Assert.Ignore("ShouldRemoveVertexColor not found");
                var result = (bool)method.Invoke(instance, new object[] { mesh });
                Assert.IsFalse(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ShouldRemoveVertexColor_AllWhite_ReturnsFalse()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[] { new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255) };
            try
            {
                var instance = Activator.CreateInstance(passType);
                var method = passType.GetMethod("ShouldRemoveVertexColor", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null) Assert.Ignore("ShouldRemoveVertexColor not found");
                var result = (bool)method.Invoke(instance, new object[] { mesh });
                Assert.IsFalse(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ShouldRemoveVertexColor_NonWhiteColors_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[] { new Color32(128, 0, 0, 255), new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255) };
            try
            {
                var instance = Activator.CreateInstance(passType);
                var method = passType.GetMethod("ShouldRemoveVertexColor", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null) Assert.Ignore("ShouldRemoveVertexColor not found");
                var result = (bool)method.Invoke(instance, new object[] { mesh });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ShouldRemoveVertexColor_NonWhiteAlpha_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[] { new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255) };
            try
            {
                var instance = Activator.CreateInstance(passType);
                var method = passType.GetMethod("ShouldRemoveVertexColor", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null) Assert.Ignore("ShouldRemoveVertexColor not found");
                var result = (bool)method.Invoke(instance, new object[] { mesh });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }
    }
}
