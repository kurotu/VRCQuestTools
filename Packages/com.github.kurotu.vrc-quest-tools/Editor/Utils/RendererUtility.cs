// <copyright file="RendererUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Represents SkinnedMeshRenderer.
    /// </summary>
    internal static class RendererUtility
    {
        /// <summary>
        /// Copy blend shapes weights by blend shape name.
        /// </summary>
        /// <param name="source">Source mesh.</param>
        /// <param name="target">Target mesh.</param>
        internal static void CopyBlendShapesWeights(SkinnedMeshRenderer source, SkinnedMeshRenderer target)
        {
            var count = source.sharedMesh.blendShapeCount;
            for (var i = 0; i < count; i++)
            {
                var name = source.sharedMesh.GetBlendShapeName(i);
                var weight = source.GetBlendShapeWeight(i);
                var targetIndex = target.sharedMesh.GetBlendShapeIndex(name);
                if (targetIndex < 0)
                {
                    continue;
                }
                target.SetBlendShapeWeight(targetIndex, weight);
            }
        }

        /// <summary>
        /// Gets sharedMesh from a renderer.
        /// </summary>
        /// <param name="renderer">MeshRenderer or SkinnedMeshRenderer.</param>
        /// <returns>sharedMesh.</returns>
        internal static Mesh GetSharedMesh(Renderer renderer)
        {
            var type = renderer.GetType();
            if (type == typeof(SkinnedMeshRenderer))
            {
                return ((SkinnedMeshRenderer)renderer).sharedMesh;
            }

            if (type == typeof(MeshRenderer) && renderer.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                return meshFilter.sharedMesh;
            }

            return null;
        }

        /// <summary>
        /// Remove vertex color from renderer's sharedMesh.
        /// </summary>
        /// <param name="renderer">MeshRenderer or SkinnedMeshRenderer.</param>
        internal static void RemoveVertexColor(Renderer renderer)
        {
            var mesh = GetSharedMesh(renderer);
            if (mesh == null)
            {
                return;
            }
            if (mesh.colors32.Length > 0)
            {
                mesh.colors32 = null;
            }
        }
    }
}
