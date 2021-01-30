// <copyright file="VRCSDKUtils.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    static class VRCSDKUtils
    {
        internal static bool IsAvatar(GameObject obj)
        {
            if (obj == null) { return false; }
            if (obj.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>() == null) { return false; }
            return true;
        }

        internal static void RemoveUnsupportedComponentsInChildren(GameObject gameObject, bool includeInactive, bool canUndo = false)
        {
            var types = new System.Type[] {
                GetType("DynamicBoneColliderBase"), GetType("DynamicBone"), // DynamicBone may be missing
                typeof(Cloth),
                typeof(Camera),
                typeof(Light),
                typeof(AudioSource),
                typeof(Joint), typeof(Rigidbody),typeof(Collider),
                typeof(UnityEngine.Animations.IConstraint)
            }.Where(e => e != null);
            foreach (var type in types)
            {
                var components = gameObject.GetComponentsInChildren(type, includeInactive);
                foreach (var c in components)
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
    }
}
