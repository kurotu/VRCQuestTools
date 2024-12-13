﻿// <copyright file="VRChatAvatar.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
#if VQT_HAS_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDKBase.Validation.Performance.Stats;

#if VQT_HAS_VRCSDK_BASE
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
                    if (mesh.colors32 != null && mesh.colors32.Length > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the avatar has DynamicBone or DynamicBoneCollider components.
        /// </summary>
        internal bool HasDynamicBoneComponents
        {
            get
            {
                if (!AssetUtility.IsDynamicBoneImported())
                {
                    return false;
                }
                var dbs = GameObject.GetComponentsInChildren(AssetUtility.DynamicBoneType, true);
                if (dbs.Length > 0)
                {
                    return true;
                }
                var dbcs = GameObject.GetComponentsInChildren(AssetUtility.DynamicBoneColliderType, true);
                if (dbcs.Length > 0)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the avatar has Unity constraints.
        /// </summary>
        internal bool HasUnityConstraints => GameObject.GetComponentsInChildren<IConstraint>(true).Length > 0;

        /// <summary>
        /// Gets runtime animator controllers which are related.
        /// </summary>
        /// <returns>Controllers.</returns>
        internal RuntimeAnimatorController[] GetRuntimeAnimatorControllers()
        {
#if VQT_HAS_VRCSDK_BASE
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
#if VQT_HAS_MODULAR_AVATAR
            mergeAnimators = AvatarDescriptor.gameObject
                .GetComponentsInChildren<ModularAvatarMergeAnimator>(true)
                .Select(ma => ma.animator)
                .ToArray();
#endif
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

#if VQT_HAS_VRCSDK_LOCAL_CONTACT_RECEIVER
        /// <summary>
        /// Gets non-local ContactReceivers and ContactSenders.
        /// </summary>
        /// <returns>All attached non-local ContactReceivers and ContactSenders.</returns>
        internal ContactBase[] GetNonLocalContacts()
        {
            return new ContactBase[] { }
                .Concat(AvatarDescriptor.GetComponentsInChildren<ContactReceiver>(true).Where(c => !c.IsLocalOnly))
                .Concat(AvatarDescriptor.GetComponentsInChildren<ContactSender>(true))
                .ToArray();
        }

        /// <summary>
        /// Gets local ContactReceivers.
        /// </summary>
        /// <returns>All attaches local ContactReceivers.</returns>
        internal ContactReceiver[] GetLocalContactReceivers()
        {
            return AvatarDescriptor.GetComponentsInChildren<ContactReceiver>(true).Where(c => c.IsLocalOnly).ToArray();
        }
#endif

        /// <summary>
        /// Estimates performance stats.
        /// </summary>
        /// <param name="physbones">PhysBones to keep.</param>
        /// <param name="colliders">PhysBone colliders to keep.</param>
        /// <param name="contacts">Contacts to keep.</param>
        /// <param name="isMobile">true for mobile.</param>
        /// <returns>Estimated performance stats.</returns>
        internal AvatarPerformanceStats EstimatePerformanceStats(
            VRCSDKUtility.Reflection.PhysBone[] physbones,
            VRCSDKUtility.Reflection.PhysBoneCollider[] colliders,
            VRCSDKUtility.Reflection.ContactBase[] contacts,
            bool isMobile = true)
        {
            var stats = VRCSDKUtility.CalculatePerformanceStats(AvatarDescriptor.gameObject, isMobile);
            var dynaimcsStats = AvatarDynamics.CalculatePerformanceStats(AvatarDescriptor.gameObject, physbones, colliders, contacts);
            stats.physBone = new AvatarPerformanceStats.PhysBoneStats
            {
                componentCount = dynaimcsStats.PhysBonesCount,
                transformCount = dynaimcsStats.PhysBonesTransformCount,
                colliderCount = dynaimcsStats.PhysBonesColliderCount,
                collisionCheckCount = dynaimcsStats.PhysBonesCollisionCheckCount,
            };
            stats.contactCount = dynaimcsStats.ContactsCount;

            if (isMobile)
            {
                stats.audioSourceCount = null;
                stats.clothCount = null;
                stats.clothMaxVertices = null;
                stats.constraintsCount = null;
                stats.downloadSizeBytes = null;
#if !VQT_HAS_VRCSDK_CONSTRAINTS
                stats.dynamicBone = null;
#endif
                stats.lightCount = null;
                stats.physicsColliderCount = null;
                stats.physicsRigidbodyCount = null;
                stats.textureMegabytes = null;
                stats.uncompressedSizeBytes = null;
            }

            stats.CalculateAllPerformanceRatings(isMobile);
            return stats;
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
