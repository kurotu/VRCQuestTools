// Tests for RemoveVertexColorPass.ShouldRemoveVertexColor via reflection.

using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.NDMF
{
    [TestFixture]
    internal class RemoveVertexColorPassTests
    {
        private Type passType;
        private MethodInfo shouldRemoveMethod;
        private object passInstance;

        [SetUp]
        public void SetUp()
        {
            passType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.RemoveVertexColorPass");
            if (passType == null)
            {
                Assert.Ignore("RemoveVertexColorPass not found");
            }

            shouldRemoveMethod = passType.GetMethod(
                "ShouldRemoveVertexColor",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (shouldRemoveMethod == null)
            {
                Assert.Ignore("ShouldRemoveVertexColor method not found");
            }

            passInstance = NdmfTestHelper.CreateInstance(passType);
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
        public void ShouldRemoveVertexColor_MeshWithNoColors_ReturnsFalse()
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
        public void ShouldRemoveVertexColor_SingleNonWhitePixel_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255),
                new Color32(254, 255, 255, 255),
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
        public void ShouldRemoveVertexColor_AllBlack_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(0, 0, 0, 0),
                new Color32(0, 0, 0, 0),
                new Color32(0, 0, 0, 0),
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
        public void ShouldRemoveVertexColor_AlphaDifference_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 255, 255, 254),
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
        public void ShouldRemoveVertexColor_EmptyColors32_ReturnsFalse()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            // Setting colors32 to empty array
            mesh.colors32 = new Color32[0];
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
        public void ShouldRemoveVertexColor_RedOnly_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero };
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 0, 0, 255),
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
        public void ShouldRemoveVertexColor_GreenOnly_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero };
            mesh.colors32 = new Color32[]
            {
                new Color32(0, 255, 0, 255),
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
        public void ShouldRemoveVertexColor_BlueOnly_ReturnsTrue()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero };
            mesh.colors32 = new Color32[]
            {
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
        public void DisplayName_ReturnsNonEmptyString()
        {
            var prop = passType.GetProperty("DisplayName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null)
            {
                Assert.Ignore("DisplayName property not found");
            }

            var displayName = (string)prop.GetValue(passInstance);
            Assert.IsNotNull(displayName);
            Assert.IsNotEmpty(displayName);
        }
    }
}
