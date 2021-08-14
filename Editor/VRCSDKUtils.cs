// <copyright file="VRCSDKUtils.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KRT.VRCQuestTools
{
    static class VRCSDKUtils
    {
        static readonly System.Type[] UnsupportedComponentTypes = new System.Type[] {
            GetType("DynamicBoneColliderBase"), GetType("DynamicBone"), // DynamicBone may be missing
            typeof(Cloth),
            typeof(Camera),
            typeof(Light),
            typeof(AudioSource),
            typeof(Joint), typeof(Rigidbody),typeof(Collider),
            typeof(UnityEngine.Animations.IConstraint)
        }.Where(e => e != null).ToArray();

        internal static bool IsAvatar(GameObject obj)
        {
            if (obj == null) { return false; }
            if (obj.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>() == null) { return false; }
            return true;
        }

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

        internal static Component[] GetUnsupportedComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            return UnsupportedComponentTypes.SelectMany(type => gameObject.GetComponentsInChildren(type, includeInactive)).ToArray();
        }

        internal static void RemoveUnsupportedComponentsInChildren(GameObject gameObject, bool includeInactive, bool canUndo = false)
        {
            foreach (var c in GetUnsupportedComponentsInChildren(gameObject, includeInactive))
            {
                var message = $"[VRCQuestTools] Removed {c.GetType().Name} from {c.gameObject.name}";
                if (canUndo)
                {
                    Undo.DestroyObjectImmediate(c);
                }
                else
                {
                    Object.DestroyImmediate(c);
                }
                Debug.Log(message);
            }
        }

        private static System.Type GetType(string fullName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.FullName == fullName)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        internal static void RemoveMissingComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            var children = gameObject.GetComponentsInChildren<Transform>(includeInactive).Select(t => t.gameObject);
            foreach (var c in children)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(c);
            }
        }

        internal static int CountMissingComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            var children = gameObject.GetComponentsInChildren<Transform>(includeInactive).Select(t => t.gameObject);
            return children.Sum(c => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(c));
        }

        internal static VRC.SDKBase.VRC_AvatarDescriptor[] GetAvatarsFromScene(Scene scene) {
            var avatars = new List<VRC.SDKBase.VRC_AvatarDescriptor>();
            var rootGameObjects = scene.GetRootGameObjects();
            foreach (var obj in rootGameObjects)
            {
                avatars.AddRange(obj.GetComponentsInChildren<VRC.SDKBase.VRC_AvatarDescriptor>());
            }
            return avatars.ToArray();
        }
    }
}
