// <copyright file="SkinnedMeshRenderer.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Represents SkinnedMeshRenderer.
    /// </summary>
    internal static class SkinnedMeshRendererUtility
    {
        /// <summary>
        /// Copy blend shapes weights by blend shape name.
        /// </summary>
        /// <param name="source">Source mesh.</param>
        /// <param name="target">Target mesh.</param>
        internal static void CopyBlensShapesWeights(SkinnedMeshRenderer source, SkinnedMeshRenderer target)
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
    }
}
