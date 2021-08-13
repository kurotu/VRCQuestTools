﻿// <copyright file="BlendShapesCopyViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for BlendShapesCopyWindow.
    /// </summary>
    internal class BlendShapesCopyViewModel
    {
        /// <summary>
        /// Source skinned mesh to copy weights.
        /// </summary>
        internal SkinnedMeshRenderer sourceMesh;

        /// <summary>
        /// Target skinned mesh to paste weights.
        /// </summary>
        internal SkinnedMeshRenderer targetMesh;

        /// <summary>
        /// Gets a value indicating whether a window disables a copy button.
        /// </summary>
        internal bool ShouldDisableCopyButton => sourceMesh == null || targetMesh == null;

        /// <summary>
        /// Executes copy blendshape weights.
        /// </summary>
        internal void CopyBlendShapesCopy()
        {
            Undo.RecordObject(targetMesh, "Copy BlendShape Weights");
            SkinnedMeshRendererUtility.CopyBlensShapesWeights(sourceMesh, targetMesh);
        }

        /// <summary>
        /// Switch source and target.
        /// </summary>
        internal void SwitchMeshes()
        {
            var tmp = sourceMesh;
            sourceMesh = targetMesh;
            targetMesh = tmp;
        }
    }
}
