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

            bool IsKeptForNew<T>(T item, int index)
            {
                return !indicesToRemove.Contains(index);
            }

            T[] ShrinkArray<T>(T[] array)
            {
                if (array == null)
                {
                    return new T[0];
                }
                if (array.Length != oldVertices.Length)
                {
                    return new T[0];
                }
                return array.Where(IsKeptForNew).ToArray();
            }

            var oldToNewMap = oldVertices
                .Select((vertex, oldIndex) => (vertex, oldIndex))
                .Where(item => !indicesToRemove.Contains(item.oldIndex))
                .Select((item, newIndex) => (item.oldIndex, newIndex))
                .ToDictionary(item => item.oldIndex, item => item.newIndex);

            var newMesh = new Mesh();
            newMesh.vertices = ShrinkArray(oldVertices);
            newMesh.normals = ShrinkArray(mesh.normals);
            newMesh.tangents = ShrinkArray(mesh.tangents);
            newMesh.colors = ShrinkArray(mesh.colors);
            newMesh.uv = ShrinkArray(mesh.uv);
            newMesh.uv2 = ShrinkArray(mesh.uv2);
            newMesh.uv3 = ShrinkArray(mesh.uv3);
            newMesh.uv4 = ShrinkArray(mesh.uv4);
            newMesh.uv5 = ShrinkArray(mesh.uv5);
            newMesh.uv6 = ShrinkArray(mesh.uv6);
            newMesh.uv7 = ShrinkArray(mesh.uv7);
            newMesh.uv8 = ShrinkArray(mesh.uv8);
            newMesh.boneWeights = ShrinkArray(mesh.boneWeights);

            var bindposes = mesh.bindposes;
            if (bindposes != null && bindposes.Length > 0)
            {
                newMesh.bindposes = bindposes;
            }

            // Sub meshes
            newMesh.subMeshCount = mesh.subMeshCount;
            for (var sub = 0; sub < mesh.subMeshCount; sub++)
            {
                var oldTris = mesh.GetTriangles(sub);
                var newTris = new List<int>();
                for (var i = 0; i < oldTris.Length; i += 3)
                {
                    var i0 = oldTris[i];
                    var i1 = oldTris[i + 1];
                    var i2 = oldTris[i + 2];

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
            var blendShapeCount = mesh.blendShapeCount;
            for (var shapeIndex = 0; shapeIndex < blendShapeCount; shapeIndex++)
            {
                string shapeName = mesh.GetBlendShapeName(shapeIndex);
                var frameCount = mesh.GetBlendShapeFrameCount(shapeIndex);

                for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    float weight = mesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    var deltaVertices = new Vector3[oldVertices.Length];
                    var deltaNormals = new Vector3[oldVertices.Length];
                    var deltaTangents = new Vector3[oldVertices.Length];

                    mesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    var newDeltaVertices = deltaVertices.Where(IsKeptForNew).ToArray();
                    var newDeltaNormals = deltaNormals.Where(IsKeptForNew).ToArray();
                    var newDeltaTangents = deltaTangents.Where(IsKeptForNew).ToArray();

                    newMesh.AddBlendShapeFrame(
                        shapeName,
                        weight,
                        newDeltaVertices,
                        newDeltaNormals,
                        newDeltaTangents);
                }
            }

            newMesh.RecalculateBounds();
            if (newMesh.normals.Length != newMesh.vertices.Length)
            {
                newMesh.RecalculateNormals();
            }

            return newMesh;
        }
    }
}
