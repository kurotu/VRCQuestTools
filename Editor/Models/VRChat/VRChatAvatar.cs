// <copyright file="VRChatAvatar.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Represents and wraps VRChat avatar.
    /// </summary>
    internal class VRChatAvatar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VRChatAvatar"/> class with an existing avatar.
        /// </summary>
        /// <param name="avatar">VRChat avatar.</param>
        internal VRChatAvatar(VRC_AvatarDescriptor avatar)
        {
            AvatarDescriptor = avatar;
        }

        /// <summary>
        /// Gets VRC_AvatarDescriptor.
        /// </summary>
        internal VRC_AvatarDescriptor AvatarDescriptor { get; }

        /// <summary>
        /// Gets GameObject of AvatarDescriptor.
        /// </summary>
        internal GameObject GameObject => AvatarDescriptor.gameObject;

        /// <summary>
        /// Gets all related materials.
        /// </summary>
        internal Material[] Materials => GetRendererMaterials()
                    .Concat(GetAnimatedMaterials())
                    .Distinct()
                    .ToArray();

        /// <summary>
        /// Gets a value indicating whether the avatar has materials which are changed by animation clips.
        /// </summary>
        internal bool HasAnimatedMaterials => GetAnimatedMaterials().Length > 0;

        /// <summary>
        /// Gets a value indicating whether the avatar has vertex color in childrens' renderers.
        /// </summary>
        internal bool HasVertexColor
        {
            get
            {
                Renderer[] skinnedMeshRenderers = AvatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                Renderer[] meshRenderers = AvatarDescriptor.GetComponentsInChildren<MeshRenderer>(true);
                var renderers = skinnedMeshRenderers.Concat(meshRenderers);

                foreach (var r in renderers)
                {
                    var mesh = RendererUtility.GetSharedMesh(r);
                    if (mesh.colors32 == null || mesh.colors32.Length == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets runtime animator controllers which are related.
        /// </summary>
        /// <returns>Controllers.</returns>
        internal RuntimeAnimatorController[] GetRuntimeAnimatorControllers()
        {
#if VRC_SDK_VRCSDK3
            // AV3 Playable Layers
            RuntimeAnimatorController[] layers = AvatarDescriptor.gameObject
                .GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>()
                .baseAnimationLayers
                .Where(obj => !obj.isDefault)
                .Select(obj => obj.animatorController)
                .ToArray();
#else
            RuntimeAnimatorController[] layers = { };
#endif

            // Animator Controller
            RuntimeAnimatorController[] avatercontrollers = AvatarDescriptor.gameObject
                .GetComponentsInChildren<Animator>()
                .Select(obj => obj.runtimeAnimatorController)
                .ToArray();

            // Modular Avatar Merge Animator
            RuntimeAnimatorController[] mergeAnimators = { };
            if (ModularAvatarUtility.IsModularAvatarImported)
            {
                mergeAnimators = AvatarDescriptor.gameObject
                    .GetComponentsInChildren(ModularAvatarUtility.MergeAnimatorType, true)
                    .Select(c => new MergeAnimatorProxy(c))
                    .Select(ma => ma.Animator)
                    .ToArray();
            }

            return layers.Concat(avatercontrollers).Concat(mergeAnimators).Where(c => c != null).Distinct().ToArray();
        }

        /// <summary>
        /// Gets materials which are refered by Renderers.
        /// </summary>
        /// <returns>Renderers' materials.</returns>
        internal Material[] GetRendererMaterials()
        {
            var renderers = AvatarDescriptor.GetComponentsInChildren<Renderer>(true);
            return renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
        }

        /// <summary>
        /// Gets PhysBones.
        /// </summary>
        /// <returns>All attached PhysBones.</returns>
        internal Component[] GetPhysBones()
        {
            if (VRCSDKUtility.IsPhysBonesImported())
            {
                return AvatarDescriptor.GetComponentsInChildren(VRCSDKUtility.PhysBoneType, true);
            }
            return new Component[] { };
        }

        /// <summary>
        /// Gets PhysBoneColliders.
        /// </summary>
        /// <returns>All attached PhysBoneColliders.</returns>
        internal Component[] GetPhysBoneColliders()
        {
            if (VRCSDKUtility.IsPhysBonesImported())
            {
                return AvatarDescriptor.GetComponentsInChildren(VRCSDKUtility.PhysBoneColliderType, true);
            }
            return new Component[] { };
        }

        /// <summary>
        /// Gets ContactReceivers and ContactSenders.
        /// </summary>
        /// <returns>All attached ContactReceivers and ContactSenders.</returns>
        internal Component[] GetContacts()
        {
            if (VRCSDKUtility.IsPhysBonesImported())
            {
                return AvatarDescriptor.GetComponentsInChildren(VRCSDKUtility.ContactReceiverType, true)
                    .Concat(AvatarDescriptor.GetComponentsInChildren(VRCSDKUtility.ContactSenderType, true))
                    .ToArray();
            }
            return new Component[] { };
        }

        private Material[] GetAnimatedMaterials()
        {
            var animMats = GetRuntimeAnimatorControllers()
                .SelectMany(controller => UnityAnimationUtility.GetMaterials(controller))
                .Distinct()
                .ToArray();
            return animMats;
        }
    }
}
