// Tests for VRChatAvatar.HasVertexColor property and related edge cases.

using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class VRChatAvatarHasVertexColorTests
    {
        [Test]
        public void HasVertexColor_NoRenderers_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_RendererWithoutMesh_ThrowsNullReference()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            child.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                // No mesh assigned - accessing HasVertexColor triggers NullReferenceException
                Assert.Throws<System.NullReferenceException>(() => { var _ = avatar.HasVertexColor; });
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_MeshWithoutColors_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            smr.sharedMesh = mesh;
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void HasVertexColor_MeshWithColors_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(0, 255, 0, 255),
                new Color32(0, 0, 255, 255),
            };
            smr.sharedMesh = mesh;
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void HasVertexColor_MeshFilter_WithColors_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Prop");
            child.transform.SetParent(go.transform);
            child.AddComponent<MeshRenderer>();
            var mf = child.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(128, 128, 128, 255),
                new Color32(128, 128, 128, 255),
                new Color32(128, 128, 128, 255),
            };
            mf.sharedMesh = mesh;
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void HasVertexColor_InactiveChild_StillChecked()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            child.SetActive(false);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.colors32 = new Color32[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(0, 255, 0, 255),
                new Color32(0, 0, 255, 255),
            };
            smr.sharedMesh = mesh;
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.IsTrue(avatar.HasVertexColor);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
            }
        }
    }
}
