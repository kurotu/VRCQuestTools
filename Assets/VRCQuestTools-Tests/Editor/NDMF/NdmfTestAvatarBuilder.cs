// <copyright file="NdmfTestAvatarBuilder.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Builds a minimal, non-prefab avatar GameObject for driving NDMF passes directly
    /// (via <see cref="nadena.dev.ndmf.BuildContext"/>) in EditMode tests.
    /// </summary>
    internal class NdmfTestAvatarBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NdmfTestAvatarBuilder"/> class.
        /// </summary>
        /// <param name="name">Name of the root GameObject.</param>
        internal NdmfTestAvatarBuilder(string name = "NdmfTestAvatar")
        {
            Root = new GameObject(name);
            AvatarDescriptor = Root.AddComponent<VRCAvatarDescriptor>();
            AvatarDescriptor.customizeAnimationLayers = true;
            AvatarDescriptor.baseAnimationLayers = new[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer { type = VRCAvatarDescriptor.AnimLayerType.Base, isDefault = true },
                new VRCAvatarDescriptor.CustomAnimLayer { type = VRCAvatarDescriptor.AnimLayerType.Additive, isDefault = true },
                new VRCAvatarDescriptor.CustomAnimLayer { type = VRCAvatarDescriptor.AnimLayerType.Gesture, isDefault = true },
                new VRCAvatarDescriptor.CustomAnimLayer { type = VRCAvatarDescriptor.AnimLayerType.Action, isDefault = true },
                new VRCAvatarDescriptor.CustomAnimLayer { type = VRCAvatarDescriptor.AnimLayerType.FX, isDefault = true },
            };
            AvatarDescriptor.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
        }

        /// <summary>
        /// Gets the root GameObject of the avatar. A plain scene object, not a prefab instance,
        /// so it can be passed to <see cref="nadena.dev.ndmf.BuildContext"/> directly.
        /// </summary>
        internal GameObject Root { get; }

        /// <summary>
        /// Gets the VRCAvatarDescriptor attached to <see cref="Root"/>.
        /// </summary>
        internal VRCAvatarDescriptor AvatarDescriptor { get; }

        /// <summary>
        /// Adds a child GameObject with a SkinnedMeshRenderer.
        /// </summary>
        /// <param name="name">Name of the child GameObject.</param>
        /// <param name="mesh">Mesh to assign.</param>
        /// <param name="materials">Materials to assign.</param>
        /// <returns>The added SkinnedMeshRenderer.</returns>
        internal SkinnedMeshRenderer AddSkinnedMeshRenderer(string name, Mesh mesh, params Material[] materials)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root.transform, false);
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh;
            if (materials.Length > 0)
            {
                smr.sharedMaterials = materials;
            }
            return smr;
        }

        /// <summary>
        /// Destroys the avatar root and all of its children and components.
        /// </summary>
        internal void Destroy()
        {
            if (Root != null)
            {
                Object.DestroyImmediate(Root);
            }
        }
    }
}
