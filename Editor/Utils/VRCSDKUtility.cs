// <copyright file="VRCSDKUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// VRCSDK Utility.
    /// </summary>
    internal static class VRCSDKUtility
    {
        /// <summary>
        /// Limit for Quest Poor.
        /// </summary>
        internal const int PoorPhysBonesCountLimit = 8;

        /// <summary>
        /// Limit for Quest Poor.
        /// </summary>
        internal const int PoorPhysBoneCollidersCountLimit = 16;

        /// <summary>
        /// Limit for Quest Poor.
        /// </summary>
        internal const int PoorContactsCountLimit = 16;

        /// <summary>
        /// Types which is not allowed for Quest avatars.
        /// </summary>
        internal static readonly System.Type[] UnsupportedComponentTypes = new System.Type[]
        {
            SystemUtility.GetTypeByName("DynamicBoneColliderBase"), SystemUtility.GetTypeByName("DynamicBone"), // DynamicBone may be missing
            typeof(Cloth),
            typeof(Camera),
            typeof(Light),
            typeof(AudioSource),
            typeof(Joint), typeof(Rigidbody), typeof(Collider),
            typeof(UnityEngine.Animations.IConstraint),
        }.Where(e => e != null).ToArray();

        /// <summary>
        /// Type object of VRCPhysBone.
        /// </summary>
        internal static readonly System.Type PhysBoneType = SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

        /// <summary>
        /// Type object of VRCPhysBoneCollider.
        /// </summary>
        internal static readonly System.Type PhysBoneColliderType = SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider");

        /// <summary>
        /// Type object of VRCContactReceiver.
        /// </summary>
        internal static readonly System.Type ContactReceiverType = SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver");

        /// <summary>
        /// Type object of VRCContactSender.
        /// </summary>
        internal static readonly System.Type ContactSenderType = SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.Contact.Components.VRCContactSender");

        /// <summary>
        /// Whether the game object is a VRC avatar root.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <returns>true when the object has VRC_AvatarDescriptor.</returns>
        internal static bool IsAvatarRoot(GameObject obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetComponent<VRC_AvatarDescriptor>() == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Whether the animation clips is a proxy animation.
        /// </summary>
        /// <param name="animationClip">Animation clip.</param>
        /// <returns>true when the clips exists in proxy animation folder.</returns>
        internal static bool IsProxyAnimationClip(AnimationClip animationClip)
        {
            var path = AssetDatabase.GetAssetPath(animationClip);

            // SDK3
            if (path.StartsWith("Assets/VRCSDK/Examples3/Animation/ProxyAnim/"))
            {
                return true;
            }

            // SDK2
            if (path.StartsWith("Assets/VRChat Examples/Examples2/Animation/"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Whether the asset object is an example from VRCSDK.
        /// </summary>
        /// <param name="obj">Asset object.</param>
        /// <returns>true when the object exists in VRCSDK examples folder.</returns>
        internal static bool IsExampleAsset(Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            return IsExampleAsset(path);
        }

        /// <summary>
        /// Whether the asset path is an example from VRCSDK.
        /// </summary>
        /// <param name="path">Asset path.</param>
        /// <returns>true when the path exists in VRCSDK examples folder.</returns>
        internal static bool IsExampleAsset(string path)
        {
            return path.StartsWith("Assets/VRCSDK/Examples3/");
        }

        /// <summary>
        /// Whether a component type is unsupported for Quest.
        /// </summary>
        /// <param name="type">Compoent type to check.</param>
        /// <returns>true when unsupported.</returns>
        internal static bool IsUnsupportedComponentType(System.Type type)
        {
            return UnsupportedComponentTypes.FirstOrDefault(t =>
            {
                return t.IsAssignableFrom(type);
            }) != null;
        }

        /// <summary>
        /// Remove "Missing" script components.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        internal static void RemoveMissingComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            var children = gameObject.GetComponentsInChildren<Transform>(includeInactive).Select(t => t.gameObject);
            foreach (var c in children)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(c);
            }
        }

        /// <summary>
        /// Count number of "Missing" script components.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <param name="includeInactive">Whether to incldue inactive objects.</param>
        /// <returns>Number of missing components.</returns>
        internal static int CountMissingComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            var children = gameObject.GetComponentsInChildren<Transform>(includeInactive).Select(t => t.gameObject);
            return children.Sum(c => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(c));
        }

        /// <summary>
        /// Gets avatar root objects from the scene.
        /// </summary>
        /// <param name="scene">Target scene.</param>
        /// <returns>Avatar root objects.</returns>
        internal static VRC_AvatarDescriptor[] GetAvatarsFromScene(Scene scene)
        {
            var avatars = new List<VRC_AvatarDescriptor>();
            var rootGameObjects = scene.GetRootGameObjects();
            foreach (var obj in rootGameObjects)
            {
                avatars.AddRange(obj.GetComponentsInChildren<VRC_AvatarDescriptor>());
            }
            return avatars.ToArray();
        }

        /// <summary>
        /// Whether the material is allowed for Quest avatar.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <returns>true when the material is allowed.</returns>
        internal static bool IsMaterialAllowedForQuestAvatar(Material material)
        {
            var usableShaders = new string[]
            {
                "Standard Lite", "Bumped Diffuse", "Bumped Mapped Specular", "Diffuse",
                "MatCap Lit", "Toon Lit", "Particles/Additive", "Particles/Multiply",
            }.Select(s => $"VRChat/Mobile/{s}");
            return usableShaders.Contains(material.shader.name);
        }

        /// <summary>
        /// Whether VRCSDK has PhysBones.
        /// </summary>
        /// <returns>true when PhysBones exists in the project.</returns>
        internal static bool IsPhysBonesImported()
        {
            return PhysBoneType != null;
        }

        /// <summary>
        /// Reflection to use VRCSDK features.
        /// </summary>
        internal class Reflection
        {
            /// <summary>
            /// Reflection wrapper for VRCPhysBone.
            /// </summary>
            internal class PhysBone
            {
                private static readonly FieldInfo RootTransformField = PhysBoneType?.GetField("rootTransform");
                private readonly Component component;

                /// <summary>
                /// Initializes a new instance of the <see cref="PhysBone"/> class.
                /// </summary>
                /// <param name="component">Component to wrap.</param>
                internal PhysBone(Component component)
                {
                    this.component = component;
                }

                /// <summary>
                /// Gets root tansform set by inspector.
                /// </summary>
                internal Transform RootTransform => (Transform)RootTransformField.GetValue(component);
            }

            /// <summary>
            /// Reflection wrapper for VRCPhysBoneCollider.
            /// </summary>
            internal class PhysBoneCollider
            {
                private static readonly FieldInfo RootTransformField = PhysBoneColliderType?.GetField("rootTransform");
                private readonly Component component;

                /// <summary>
                /// Initializes a new instance of the <see cref="PhysBoneCollider"/> class.
                /// </summary>
                /// <param name="component">Component to wrap.</param>
                internal PhysBoneCollider(Component component)
                {
                    this.component = component;
                }

                /// <summary>
                /// Gets root tansform set by inspector.
                /// </summary>
                internal Transform RootTransform => (Transform)RootTransformField.GetValue(component);
            }

            /// <summary>
            /// Reflection wrapper for ContactBase.
            /// </summary>
            internal class ContactBase
            {
                private static readonly System.Type ContactBaseType = SystemUtility.GetTypeByName("VRC.Dynamics.ContactBase");
                private static readonly FieldInfo RootTransformField = ContactBaseType?.GetField("rootTransform");
                private readonly Component component;

                /// <summary>
                /// Initializes a new instance of the <see cref="ContactBase"/> class.
                /// </summary>
                /// <param name="component">Component to wrap.</param>
                internal ContactBase(Component component)
                {
                    this.component = component;
                }

                /// <summary>
                /// Gets root tansform set by inspector.
                /// </summary>
                internal Transform RootTransform => (Transform)RootTransformField.GetValue(component);
            }
        }
    }
}
