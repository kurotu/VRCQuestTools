// <copyright file="VRCSDKUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
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
        private static readonly System.Type[] UnsupportedComponentTypes = new System.Type[]
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
    }
}
