// Tests for RemoveVertexColorPass.ShouldRemoveVertexColor private method via reflection.

using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class RemoveVertexColorPassShouldRemoveTests
    {
        private Type passType;
        private object passInstance;
        private MethodInfo shouldRemoveMethod;

        [SetUp]
        public void SetUp()
        {
            passType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.RemoveVertexColorPass");
            if (passType == null) { Assert.Ignore("RemoveVertexColorPass not found."); return; }
            passInstance = Activator.CreateInstance(passType);
            shouldRemoveMethod = passType.GetMethod("ShouldRemoveVertexColor", BindingFlags.Instance | BindingFlags.NonPublic);
            if (shouldRemoveMethod == null) { Assert.Ignore("ShouldRemoveVertexColor method not found."); }
        }

        private bool InvokeShouldRemove(Mesh mesh)
        {
            return (bool)shouldRemoveMethod.Invoke(passInstance, new object[] { mesh });
        }

        [Test]
        public void ShouldRemoveVertexColor_NullMesh_ReturnsFalse()
        {
            Assert.IsFalse(InvokeShouldRemove(null));
        }

        [Test]
        public void ShouldRemoveVertexColor_EmptyColors_ReturnsFalse()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            try
            {
                Assert.IsFalse(InvokeShouldRemove(mesh));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ShouldRemoveVertexColor_AllWhiteColors_ReturnsFalse()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255),
            };
            try
            {
                Assert.IsFalse(InvokeShouldRemove(mesh));
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
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(0, 255, 0, 255),
                new Color32(0, 0, 255, 255),
            };
            try
            {
                Assert.IsTrue(InvokeShouldRemove(mesh));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ShouldRemoveVertexColor_MixedColors_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 255, 255, 255),
                new Color32(128, 128, 128, 255),
                new Color32(255, 255, 255, 255),
            };
            try
            {
                Assert.IsTrue(InvokeShouldRemove(mesh));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ShouldRemoveVertexColor_AlphaNotFull_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 255, 255, 128),
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255),
            };
            try
            {
                Assert.IsTrue(InvokeShouldRemove(mesh));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ShouldRemoveVertexColor_SingleVertex_NonWhite_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero };
            mesh.colors32 = new Color32[] { new Color32(0, 0, 0, 255) };
            try
            {
                Assert.IsTrue(InvokeShouldRemove(mesh));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void ShouldRemoveVertexColor_SingleVertex_White_ReturnsFalse()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero };
            mesh.colors32 = new Color32[] { new Color32(255, 255, 255, 255) };
            try
            {
                Assert.IsFalse(InvokeShouldRemove(mesh));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }
    }
}
