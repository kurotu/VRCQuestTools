using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace KRT.VRCQuestTools.ScriptedAssets
{
    public static class MeshSerializer
    {
        public static SerializableMesh GetSerializableMesh(Mesh mesh)
        {
            SerializableMesh serializableMesh = new SerializableMesh();

            // ���_�A�@���A�^���W�F���g�AUV�A�O�p�`�C���f�b�N�X�̏����擾
            serializableMesh.vertices = mesh.vertices;
            serializableMesh.normals = mesh.normals;
            serializableMesh.tangents = mesh.tangents;
            serializableMesh.colors32 = mesh.colors32;
            serializableMesh.uv = mesh.uv;
            serializableMesh.uv2 = mesh.uv2;
            serializableMesh.uv3 = mesh.uv3;
            serializableMesh.uv4 = mesh.uv4;
            serializableMesh.uv5 = mesh.uv5;
            serializableMesh.uv6 = mesh.uv6;
            serializableMesh.uv7 = mesh.uv7;
            serializableMesh.uv8 = mesh.uv8;
            serializableMesh.bindposes = mesh.bindposes;

            // �T�u���b�V���̎O�p�`�C���f�b�N�X���擾
            serializableMesh.submeshTriangles = Enumerable.Range(0, mesh.subMeshCount)
                .Select(i => new SubmeshTriangle() { triangles = mesh.GetTriangles(i) })
                .ToArray();
            //new List<int[]>();
            //for (int i = 0; i < mesh.subMeshCount; i++)
            //{
            //    int[] triangles = mesh.GetTriangles(i);
            //    serializableMesh.submeshTriangles.Add(triangles);
            //}

            // �u�����h�V�F�C�v�̏����擾
            serializableMesh.blendShapes = new BlendShape[mesh.blendShapeCount];
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                BlendShape blendShape = new BlendShape();
                blendShape.shapeName = mesh.GetBlendShapeName(i);

                // �e�u�����h�V�F�C�v�̒��_�f�[�^���擾
                int frameCount = mesh.GetBlendShapeFrameCount(i);
                blendShape.frames = new BlendShapeFrame[frameCount];
                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    // �w�肵���t���[���ł̒��_�f�[�^���擾
                    Vector3[] deltaVertices = new Vector3[mesh.vertexCount];
                    Vector3[] deltaNormals = new Vector3[mesh.vertexCount];
                    Vector3[] deltaTangents = new Vector3[mesh.vertexCount];
                    mesh.GetBlendShapeFrameVertices(i, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    blendShape.frames[frameIndex] = new BlendShapeFrame
                    {
                        frameWeight = mesh.GetBlendShapeFrameWeight(i, frameIndex),
                        deltaVertices = deltaVertices,
                        deltaNormals = deltaNormals,
                        deltaTangents = deltaTangents,
                    };
                }

                serializableMesh.blendShapes[i] = blendShape;
            }

            // �E�F�C�g
            serializableMesh.boneWeights = mesh.boneWeights;

            return serializableMesh;
        }

        public static Mesh GetMesh(SerializableMesh serializableMesh)
        {
            Mesh mesh = new Mesh
            {
                vertices = serializableMesh.vertices,
                normals = serializableMesh.normals,
                tangents = serializableMesh.tangents,
                colors32 = serializableMesh.colors32,
                uv = serializableMesh.uv,
                uv2 = serializableMesh.uv2,
                uv3 = serializableMesh.uv3,
                uv4 = serializableMesh.uv4,
                uv5 = serializableMesh.uv5,
                uv6 = serializableMesh.uv6,
                uv7 = serializableMesh.uv7,
                uv8 = serializableMesh.uv8,
                bindposes = serializableMesh.bindposes
            };

            // �T�u���b�V���̎O�p�`�C���f�b�N�X��ݒ�
            mesh.subMeshCount = serializableMesh.submeshTriangles.Length;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                mesh.SetTriangles(serializableMesh.submeshTriangles[i].triangles, i);
            }

            // �u�����h�V�F�C�v�̏���ݒ�
            for (int i = 0; i < serializableMesh.blendShapes.Length; i++)
            {
                BlendShape blendShape = serializableMesh.blendShapes[i];
                for (int frameIndex = 0; frameIndex < blendShape.frames.Length; frameIndex++)
                {
                    BlendShapeFrame frame = blendShape.frames[frameIndex];
                    mesh.AddBlendShapeFrame(blendShape.shapeName, frame.frameWeight, frame.deltaVertices, frame.deltaNormals, frame.deltaTangents);
                }
            }

            // �E�F�C�g
            if (serializableMesh.boneWeights != null && serializableMesh.boneWeights.Length > 0)
            {
                mesh.boneWeights = serializableMesh.boneWeights;
            }

            mesh.RecalculateBounds();

            return mesh;
        }

        [System.Serializable]
        public class SerializableMesh
        {
            public int version = 1;
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector4[] tangents;
            public Color32[] colors32;
            public Vector2[] uv;
            public Vector2[] uv2;
            public Vector2[] uv3;
            public Vector2[] uv4;
            public Vector2[] uv5;
            public Vector2[] uv6;
            public Vector2[] uv7;
            public Vector2[] uv8;
            public Matrix4x4[] bindposes;
            public SubmeshTriangle[] submeshTriangles;
            public BlendShape[] blendShapes;
            public BoneWeight[] boneWeights;
        }

        [System.Serializable]
        public class SubmeshTriangle
        {
            public int[] triangles;
        }

        [System.Serializable]
        public class BlendShape
        {
            public int version = 1;
            public string shapeName;
            public BlendShapeFrame[] frames;
        }

        [System.Serializable]
        public class BlendShapeFrame
        {
            public int version = 1;
            public float frameWeight;
            public Vector3[] deltaVertices;
            public Vector3[] deltaNormals;
            public Vector3[] deltaTangents;
        }
    }
}
