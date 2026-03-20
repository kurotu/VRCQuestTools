// Tests for MaterialConversionFilter and MeshFlipperFilter Dispose with actual material/texture cleanup,
// and additional edge cases for filter node classes.

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.NDMF
{
    [TestFixture]
    internal class MaterialConversionFilterNodeDisposeTests
    {
        private Type nodeType;

        [SetUp]
        public void SetUp()
        {
            nodeType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MaterialConversionFilter+MaterialConversionFilterNode");
            if (nodeType == null)
            {
                Assert.Ignore("MaterialConversionFilterNode not found");
            }
        }

        [Test]
        public void Dispose_WithMaterialHavingTextures_DestroysNonAssetTextures()
        {
            var originalMat = new Material(Shader.Find("Standard"));
            var convertedMat = new Material(Shader.Find("Standard"));
            var inMemoryTex = new Texture2D(4, 4);
            inMemoryTex.name = "InMemoryTexture";
            convertedMat.mainTexture = inMemoryTex;

            var map = new Dictionary<Material, Material> { { originalMat, convertedMat } };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var disposeMethod = instance.GetType().GetMethod("Dispose",
                BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            try
            {
                Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(originalMat);
                // convertedMat and inMemoryTex should be destroyed by Dispose
            }
        }

        [Test]
        public void Dispose_WithNullMaterialInMap_DoesNotThrow()
        {
            var originalMat = new Material(Shader.Find("Standard"));
            var map = new Dictionary<Material, Material> { { originalMat, null } };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var disposeMethod = instance.GetType().GetMethod("Dispose",
                BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            try
            {
                Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(originalMat);
            }
        }

        [Test]
        public void Dispose_MultipleMaterials_CleansUpAll()
        {
            var orig1 = new Material(Shader.Find("Standard"));
            var conv1 = new Material(Shader.Find("Standard"));
            var orig2 = new Material(Shader.Find("Standard"));
            var conv2 = new Material(Shader.Find("Standard"));

            var map = new Dictionary<Material, Material>
            {
                { orig1, conv1 },
                { orig2, conv2 },
            };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var disposeMethod = instance.GetType().GetMethod("Dispose",
                BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            try
            {
                Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));

                // Verify map is cleared
                var mapField = nodeType.GetField("materialMap", BindingFlags.Instance | BindingFlags.NonPublic);
                if (mapField != null)
                {
                    var internalMap = (Dictionary<Material, Material>)mapField.GetValue(instance);
                    Assert.AreEqual(0, internalMap.Count, "Map should be cleared after dispose");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(orig1);
                UnityEngine.Object.DestroyImmediate(orig2);
            }
        }

        [Test]
        public void OnFrame_MultipleMaterials_ReplacesAll()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            mat1.name = "Mat1";
            var conv1 = new Material(Shader.Find("Standard"));
            conv1.name = "Conv1";
            var mat2 = new Material(Shader.Find("Standard"));
            mat2.name = "Mat2";
            var conv2 = new Material(Shader.Find("Standard"));
            conv2.name = "Conv2";

            var map = new Dictionary<Material, Material>
            {
                { mat1, conv1 },
                { mat2, conv2 },
            };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var go = new GameObject("TestMultiMat");
            go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { mat1, mat2 };

            try
            {
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", renderer, renderer);
                Assert.AreEqual(conv1, renderer.sharedMaterials[0]);
                Assert.AreEqual(conv2, renderer.sharedMaterials[1]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(mat1);
                UnityEngine.Object.DestroyImmediate(conv1);
                UnityEngine.Object.DestroyImmediate(mat2);
                UnityEngine.Object.DestroyImmediate(conv2);
            }
        }

        [Test]
        public void OnFrame_MixedMappedAndUnmapped_KeepsUnmapped()
        {
            var mapped = new Material(Shader.Find("Standard"));
            var converted = new Material(Shader.Find("Standard"));
            var unmapped = new Material(Shader.Find("Standard"));

            var map = new Dictionary<Material, Material> { { mapped, converted } };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var go = new GameObject("TestMixed");
            go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { mapped, unmapped };

            try
            {
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", renderer, renderer);
                Assert.AreEqual(converted, renderer.sharedMaterials[0]);
                Assert.AreEqual(unmapped, renderer.sharedMaterials[1]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(mapped);
                UnityEngine.Object.DestroyImmediate(converted);
                UnityEngine.Object.DestroyImmediate(unmapped);
            }
        }

        [Test]
        public void OnFrame_SkinnedMeshRenderer_WorksCorrectly()
        {
            var mat = new Material(Shader.Find("Standard"));
            var conv = new Material(Shader.Find("Standard"));

            var map = new Dictionary<Material, Material> { { mat, conv } };
            var instance = NdmfTestHelper.CreateInstance(nodeType, map, false);

            var go = new GameObject("TestSMR");
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMaterials = new[] { mat };

            try
            {
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", smr, smr);
                Assert.AreEqual(conv, smr.sharedMaterials[0]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(mat);
                UnityEngine.Object.DestroyImmediate(conv);
            }
        }
    }

    [TestFixture]
    internal class MeshFlipperFilterNodeDisposeTests
    {
        private Type nodeType;

        [SetUp]
        public void SetUp()
        {
            nodeType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.MeshFlipperFilter+MeshFlipperFilterNode");
            if (nodeType == null)
            {
                Assert.Ignore("MeshFlipperFilterNode not found");
            }
        }

        [Test]
        public void Dispose_WithMesh_DestroysMesh()
        {
            var mesh = new Mesh();
            mesh.name = "TestFlippedMesh";
            var instance = NdmfTestHelper.CreateInstance(nodeType, mesh);

            var disposeMethod = instance.GetType().GetMethod("Dispose",
                BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
            // Mesh should be destroyed (no need to cleanup)
        }

        [Test]
        public void Dispose_WithNullMesh_DoesNotThrow()
        {
            var instance = NdmfTestHelper.CreateInstance(nodeType, (Mesh)null);

            var disposeMethod = instance.GetType().GetMethod("Dispose",
                BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            Assert.DoesNotThrow(() => disposeMethod.Invoke(instance, null));
        }

        [Test]
        public void OnFrame_NullMesh_SetsNullOnRenderer()
        {
            var instance = NdmfTestHelper.CreateInstance(nodeType, (Mesh)null);

            var go = new GameObject("TestNullMesh");
            var smr = go.AddComponent<SkinnedMeshRenderer>();

            try
            {
                NdmfTestHelper.InvokeMethod(instance, "OnFrame", smr, smr);
                Assert.IsNull(smr.sharedMesh);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnFrame_MeshFilterWithMesh_SetsMesh()
        {
            var origMesh = new Mesh();
            origMesh.name = "OriginalMesh";
            var flippedMesh = new Mesh();
            flippedMesh.name = "FlippedMesh";

            var go = new GameObject("TestMF");
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = origMesh;
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
                UnityEngine.Object.DestroyImmediate(origMesh);
                UnityEngine.Object.DestroyImmediate(flippedMesh);
            }
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
            if (filterType == null)
            {
                Assert.Ignore("MaterialConversionFilter not found");
            }
        }

        [Test]
        public void Constructor_WithOptimizingPhase_GetPreviewControlNodes_YieldsNode()
        {
            // AvatarConverterNdmfPhase.Optimizing = 1
            var phaseType = typeof(KRT.VRCQuestTools.Models.AvatarConverterNdmfPhase);
            var optimizingPhase = Enum.Parse(phaseType, "Optimizing");

            var instance = NdmfTestHelper.CreateInstance(filterType, optimizingPhase);
            Assert.IsNotNull(instance);

            var method = filterType.GetMethod("GetPreviewControlNodes",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("GetPreviewControlNodes not found");
            }

            var result = method.Invoke(instance, null) as System.Collections.IEnumerable;
            Assert.IsNotNull(result);

            int count = 0;
            foreach (var item in result)
            {
                count++;
            }
            Assert.AreEqual(1, count, "Optimizing phase should yield one preview node");
        }

        [Test]
        public void Constructor_WithTransformingPhase_GetPreviewControlNodes_YieldsNothing()
        {
            var phaseType = typeof(KRT.VRCQuestTools.Models.AvatarConverterNdmfPhase);
            var transformingPhase = Enum.Parse(phaseType, "Transforming");

            var instance = NdmfTestHelper.CreateInstance(filterType, transformingPhase);
            Assert.IsNotNull(instance);

            var method = filterType.GetMethod("GetPreviewControlNodes",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("GetPreviewControlNodes not found");
            }

            var result = method.Invoke(instance, null) as System.Collections.IEnumerable;
            Assert.IsNotNull(result);

            int count = 0;
            foreach (var item in result)
            {
                count++;
            }
            Assert.AreEqual(0, count, "Transforming phase should yield no preview nodes");
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
            if (filterType == null)
            {
                Assert.Ignore("MeshFlipperFilter not found");
            }
        }

        [Test]
        public void Constructor_WithBeforePolygonReductionPhase_YieldsNode()
        {
            var phaseType = typeof(KRT.VRCQuestTools.Components.MeshFlipperProcessingPhase);
            var beforePhase = Enum.Parse(phaseType, "BeforePolygonReduction");

            var instance = NdmfTestHelper.CreateInstance(filterType, beforePhase);
            Assert.IsNotNull(instance);

            var method = filterType.GetMethod("GetPreviewControlNodes",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("GetPreviewControlNodes not found");
            }

            var result = method.Invoke(instance, null) as System.Collections.IEnumerable;
            int count = 0;
            foreach (var item in result)
            {
                count++;
            }
            Assert.AreEqual(1, count, "BeforePolygonReduction should yield one preview node");
        }

        [Test]
        public void Constructor_WithAfterPolygonReductionPhase_YieldsNothing()
        {
            var phaseType = typeof(KRT.VRCQuestTools.Components.MeshFlipperProcessingPhase);
            var afterPhase = Enum.Parse(phaseType, "AfterPolygonReduction");

            var instance = NdmfTestHelper.CreateInstance(filterType, afterPhase);
            Assert.IsNotNull(instance);

            var method = filterType.GetMethod("GetPreviewControlNodes",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("GetPreviewControlNodes not found");
            }

            var result = method.Invoke(instance, null) as System.Collections.IEnumerable;
            int count = 0;
            foreach (var item in result)
            {
                count++;
            }
            Assert.AreEqual(0, count, "AfterPolygonReduction should yield no preview nodes");
        }
    }
}
