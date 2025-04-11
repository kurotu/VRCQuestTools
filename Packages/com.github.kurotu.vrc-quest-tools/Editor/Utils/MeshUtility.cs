using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Mesh utility.
    /// </summary>
    internal static class MeshUtility
    {
        /// <summary>
        /// Remove transparent part of the mesh.
        /// </summary>
        /// <param name="mesh">Mesh.</param>
        /// <param name="materials">Materials to render.</param>
        /// <returns>Modified mesh. If not modified, returns null.</returns>
        internal static Mesh RemoveTransparentPart(Mesh mesh, Material[] materials)
        {
            var indicesToRemove = GetVertexIndicesToRemoveTransparentPart(mesh, materials);
            if (indicesToRemove.Count == 0)
            {
                return null;
            }

            var newMesh = RemoveVertices(mesh, indicesToRemove);
            newMesh.name = mesh.name;
            return newMesh;
        }

        private static HashSet<int> GetVertexIndicesToRemoveTransparentPart(Mesh mesh, Material[] materials)
        {
            var renderedTextures = materials
                .Take(mesh.subMeshCount)
                .Select(m =>
                {
                    var mainTexture = m.GetTexture("_MainTex");
                    var width = 1024;
                    var height = 1024;
                    if (mainTexture != null)
                    {
                        width = System.Math.Min(width, mainTexture.width);
                        height = System.Math.Min(height, mainTexture.height);
                    }

                    var output = AssetUtility.RenderMaterialToTexture2D(m, width, height, false);
                    return output;
                })
                .ToArray();

            // Determine which vertices to keep
            var vertexIndexToKeep = new List<int>();
            for (var sub = 0; sub < mesh.subMeshCount; sub++)
            {
                var triangles = mesh.GetTriangles(sub);
                var triangleGroups = triangles.Select((value, index) => (value, index))
                    .GroupBy(x => x.index / 3)
                    .Select(g => (g.ElementAt(0).value, g.ElementAt(1).value, g.ElementAt(2).value))
                    .ToArray();
                var texture = renderedTextures[sub];
                var newTriangles = triangleGroups
                    .Where(t =>
                    {
                        var isTransparent =
                            texture.GetPixelBilinear(mesh.uv[t.Item1].x, mesh.uv[t.Item1].y).a < 1f
                            && texture.GetPixelBilinear(mesh.uv[t.Item2].x, mesh.uv[t.Item2].y).a < 1f
                            && texture.GetPixelBilinear(mesh.uv[t.Item3].x, mesh.uv[t.Item3].y).a < 1f;
                        return !isTransparent;
                    })
                    .SelectMany(t => new[] { t.Item1, t.Item2, t.Item3 })
                    .ToArray();
                vertexIndexToKeep.AddRange(newTriangles);
            }
            vertexIndexToKeep = vertexIndexToKeep.Distinct().ToList();

            var indices = Enumerable.Range(0, mesh.vertexCount - 1).
                Where(i => !vertexIndexToKeep.Contains(i));
            return new HashSet<int>(indices);
        }

        private static Mesh RemoveVertices(Mesh mesh, HashSet<int> indicesToRemove)
        {
            Vector3[] oldVertices = mesh.vertices;
            Vector3[] oldNormals = mesh.normals;
            Vector4[] oldTangents = mesh.tangents;
            Color[] oldColors = mesh.colors;
            Vector2[] oldUV = mesh.uv;
            Vector2[] oldUV2 = mesh.uv2;
            Vector2[] oldUV3 = mesh.uv3;
            Vector2[] oldUV4 = mesh.uv4;
            BoneWeight[] oldBoneWeights = mesh.boneWeights;
            Matrix4x4[] bindposes = mesh.bindposes;

            var newVertices = new List<Vector3>();
            var newNormals = new List<Vector3>();
            var newTangents = new List<Vector4>();
            var newColors = new List<Color>();
            var newUV = new List<Vector2>();
            var newUV2 = new List<Vector2>();
            var newUV3 = new List<Vector2>();
            var newUV4 = new List<Vector2>();
            var newBoneWeights = new List<BoneWeight>();

            var oldToNewMap = new Dictionary<int, int>();
            for (int i = 0; i < oldVertices.Length; i++)
            {
                if (!indicesToRemove.Contains(i))
                {
                    int newIndex = newVertices.Count;
                    oldToNewMap[i] = newIndex;

                    newVertices.Add(oldVertices[i]);
                    if (oldNormals.Length == oldVertices.Length)
                    {
                        newNormals.Add(oldNormals[i]);
                    }

                    if (oldTangents.Length == oldVertices.Length)
                    {
                        newTangents.Add(oldTangents[i]);
                    }

                    if (oldColors.Length == oldVertices.Length)
                    {
                        newColors.Add(oldColors[i]);
                    }

                    if (oldUV.Length == oldVertices.Length)
                    {
                        newUV.Add(oldUV[i]);
                    }

                    if (oldUV2.Length == oldVertices.Length)
                    {
                        newUV2.Add(oldUV2[i]);
                    }

                    if (oldUV3.Length == oldVertices.Length)
                    {
                        newUV3.Add(oldUV3[i]);
                    }

                    if (oldUV4.Length == oldVertices.Length)
                    {
                        newUV4.Add(oldUV4[i]);
                    }

                    if (oldBoneWeights.Length == oldVertices.Length)
                    {
                        newBoneWeights.Add(oldBoneWeights[i]);
                    }
                }
            }

            var newMesh = new Mesh();
            newMesh.vertices = newVertices.ToArray();
            if (newNormals.Count == newVertices.Count)
            {
                newMesh.normals = newNormals.ToArray();
            }

            if (newTangents.Count == newVertices.Count)
            {
                newMesh.tangents = newTangents.ToArray();
            }

            if (newColors.Count == newVertices.Count)
            {
                newMesh.colors = newColors.ToArray();
            }

            if (newUV.Count == newVertices.Count)
            {
                newMesh.uv = newUV.ToArray();
            }

            if (newUV2.Count == newVertices.Count)
            {
                newMesh.uv2 = newUV2.ToArray();
            }

            if (newUV3.Count == newVertices.Count)
            {
                newMesh.uv3 = newUV3.ToArray();
            }

            if (newUV4.Count == newVertices.Count)
            {
                newMesh.uv4 = newUV4.ToArray();
            }

            if (newBoneWeights.Count == newVertices.Count)
            {
                newMesh.boneWeights = newBoneWeights.ToArray();
            }

            if (bindposes != null && bindposes.Length > 0)
            {
                newMesh.bindposes = bindposes;
            }

            // Sub meshes
            newMesh.subMeshCount = mesh.subMeshCount;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                var oldTris = mesh.GetTriangles(sub);
                var newTris = new List<int>();
                for (int i = 0; i < oldTris.Length; i += 3)
                {
                    int i0 = oldTris[i];
                    int i1 = oldTris[i + 1];
                    int i2 = oldTris[i + 2];

                    if (indicesToRemove.Contains(i0) || indicesToRemove.Contains(i1) || indicesToRemove.Contains(i2))
                    {
                        continue;
                    }

                    newTris.Add(oldToNewMap[i0]);
                    newTris.Add(oldToNewMap[i1]);
                    newTris.Add(oldToNewMap[i2]);
                }
                newMesh.SetTriangles(newTris, sub);
            }

            // BlendShapes
            int blendShapeCount = mesh.blendShapeCount;
            for (int shapeIndex = 0; shapeIndex < blendShapeCount; shapeIndex++)
            {
                string shapeName = mesh.GetBlendShapeName(shapeIndex);
                int frameCount = mesh.GetBlendShapeFrameCount(shapeIndex);

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    float weight = mesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    Vector3[] deltaVertices = new Vector3[oldVertices.Length];
                    Vector3[] deltaNormals = new Vector3[oldVertices.Length];
                    Vector3[] deltaTangents = new Vector3[oldVertices.Length];

                    mesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    var newDeltaVertices = new List<Vector3>();
                    var newDeltaNormals = new List<Vector3>();
                    var newDeltaTangents = new List<Vector3>();

                    for (int i = 0; i < oldVertices.Length; i++)
                    {
                        if (!indicesToRemove.Contains(i))
                        {
                            newDeltaVertices.Add(deltaVertices[i]);
                            newDeltaNormals.Add(deltaNormals[i]);
                            newDeltaTangents.Add(deltaTangents[i]);
                        }
                    }

                    newMesh.AddBlendShapeFrame(
                        shapeName,
                        weight,
                        newDeltaVertices.ToArray(),
                        newDeltaNormals.ToArray(),
                        newDeltaTangents.ToArray());
                }
            }

            newMesh.RecalculateBounds();
            if (newNormals.Count != newVertices.Count)
            {
                newMesh.RecalculateNormals();
            }

            return newMesh;
        }

        private class Vector3Object
        {
            public readonly Vector3 vector3;

            public Vector3Object(Vector3 vector3)
            {
                this.vector3 = vector3;
            }
        }
    }
}
