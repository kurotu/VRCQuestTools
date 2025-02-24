using System.Linq;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Avatar Dynamics.
    /// </summary>
    internal static class AvatarDynamics
    {
        /// <summary>
        /// Calculate performance stats for Avatar Dynamics.
        /// </summary>
        /// <param name="root">Avatar root object (VRCAvatarDescriptor).</param>
        /// <param name="physbones">PhysBone GameObjects.</param>
        /// <param name="colliders">PhysBoneCollider GameObjects.</param>
        /// <param name="contacts">ContactSender and ContactReceiver GameObjects.</param>
        /// <returns>Calculated performance stats.</returns>
        internal static PerformanceStats CalculatePerformanceStats(
            GameObject root,
            VRCSDKUtility.Reflection.PhysBone[] physbones,
            VRCSDKUtility.Reflection.PhysBoneCollider[] colliders,
            VRCSDKUtility.Reflection.ContactBase[] contacts)
        {
            return new PerformanceStats()
            {
                PhysBonesCount = CalculatePhysBonesCount(root, physbones),
                PhysBonesTransformCount = CalculatePhysBonesTransformCount(root, physbones),
                PhysBonesColliderCount = CalculatePhysBonesColliderCount(root, physbones, colliders),
                PhysBonesCollisionCheckCount = CalculatePhysBonesCollisionCheckCount(root, physbones, colliders),
                ContactsCount = CalculateContactsCount(contacts),
            };
        }

        private static int CalculatePhysBonesCount(GameObject root, VRCSDKUtility.Reflection.PhysBone[] physbones)
        {
            return GetActualPhysBones(root, physbones).Count();
        }

        private static int CalculatePhysBonesTransformCount(GameObject root, VRCSDKUtility.Reflection.PhysBone[] physbones)
        {
            // exclude editor only physbones
            var actual = GetActualPhysBones(root, physbones);
            return actual.Sum(pb => CalculatePhysBoneTransformCount(pb));
        }

        private static int CalculatePhysBonesColliderCount(GameObject root, VRCSDKUtility.Reflection.PhysBone[] physbones, VRCSDKUtility.Reflection.PhysBoneCollider[] colliders)
        {
            var actualPbs = GetActualPhysBones(root, physbones);
            var actual = colliders.Where((collider) =>
            {
                return actualPbs.FirstOrDefault(pb => IsColliderReferencedByPhysBone(collider, pb)) != null;
            });
            return actual.Count();
        }

        private static int CalculatePhysBonesCollisionCheckCount(GameObject root, VRCSDKUtility.Reflection.PhysBone[] physbones, VRCSDKUtility.Reflection.PhysBoneCollider[] colliders)
        {
            // exclude editor only physbones
            var actualPbs = physbones.Where((obj) => !IsFinallyEditorOnly(root, obj.GameObject));
            var collisions = actualPbs.Select((pb) =>
            {
                var transformCount = CalculatePhysBoneTransformCount(pb) - 1; // ignore itself.
                var rootTrans = pb.RootTransform == null ? pb.GameObject.transform : pb.RootTransform;

                var multiChildRoots = rootTrans.GetComponentsInChildren<Transform>(true)
                    .Where(t => !IsFinallyEditorOnly(root, t.gameObject))
                    .Where(t => IsMultiChildRoot(pb, t))
                    .ToArray();
                transformCount -= multiChildRoots.Sum(t => t.childCount);

                if (pb.MultiChildType != VRCSDKUtility.Reflection.PhysBone.MultiChildTypeEnum.Ignore)
                {
                    transformCount += multiChildRoots.Length;
                }

                if (pb.EndpointPosition.magnitude > 0)
                {
                    var ignore = pb.IgnoreTransforms.ToArray();
                    var endpoints = rootTrans.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.childCount == 0)
                        .Where(t => !IsIgnoredTransform(t, ignore))
                        .Where(t => !IsFinallyEditorOnly(root, t.gameObject));
                    transformCount += endpoints.Count();
                }

                var colliderCount = pb.Colliders
                    .Distinct()
                    .Where(c => c != null)
                    .Where(c => colliders.FirstOrDefault(cc => cc.Component == c) != null)
                    .Count();
                return transformCount * colliderCount;
            });
            return collisions.Sum();
        }

        private static int CalculateContactsCount(VRCSDKUtility.Reflection.ContactBase[] contacts)
        {
            return contacts.Count(c => !c.IsLocalOnly);
        }

        private static VRCSDKUtility.Reflection.PhysBone[] GetActualPhysBones(GameObject root, VRCSDKUtility.Reflection.PhysBone[] physbones)
        {
            // exclude editor only physbones
            return physbones.Where(obj => !IsFinallyEditorOnly(root, obj.GameObject)).ToArray();
        }

        private static bool IsColliderReferencedByPhysBone(VRCSDKUtility.Reflection.PhysBoneCollider collider, VRCSDKUtility.Reflection.PhysBone physBone)
        {
            return physBone.Colliders.Contains(collider.Component);
        }

        private static bool IsFinallyEditorOnly(GameObject root, GameObject obj)
        {
            if (obj.tag == "EditorOnly")
            {
                return true;
            }
            if (obj.transform.parent == null || obj.transform.parent.gameObject == root)
            {
                return false;
            }
            return IsFinallyEditorOnly(root, obj.transform.parent.gameObject);
        }

        private static bool IsIgnoredTransform(Transform transform, Transform[] ignoreTransforms)
        {
            ignoreTransforms
                .Where(t => t != null)
                .SelectMany(t => t.GetComponentsInChildren<Transform>(true))
                .ToArray();
            return ignoreTransforms.Contains(transform);
        }

        private static bool IsMultiChildRoot(VRCSDKUtility.Reflection.PhysBone physBone, Transform transform)
        {
            if (transform.childCount <= 1)
            {
                return false;
            }

            // get all direct child transform.
            var children = transform.GetComponentsInChildren<Transform>(true)
                .Where(t => t.parent == transform)
                .ToArray();
            return children.Where(t => !IsIgnoredTransform(t, physBone.IgnoreTransforms.ToArray())).Count() > 1;
        }

        private static int CalculatePhysBoneTransformCount(VRCSDKUtility.Reflection.PhysBone physbone)
        {
            var rootTransform = physbone.RootTransform == null ? physbone.GameObject.transform : physbone.RootTransform;
            return CountChildrenRecursive(rootTransform, physbone.IgnoreTransforms.ToArray()) + 1; // count root itself.
        }

        private static int CountChildrenRecursive(Transform transform, Transform[] ignoreTransforms)
        {
            var count = 0;
            foreach (Transform child in transform)
            {
                if (ignoreTransforms.Contains(child))
                {
                    continue;
                }
                count++;
                count += CountChildrenRecursive(child, ignoreTransforms);
            }
            return count;
        }

        /// <summary>
        /// Performance stats for Avatar Dynamics.
        /// </summary>
        internal class PerformanceStats
        {
            /// <summary>
            /// PhysBones Components count.
            /// </summary>
            internal int PhysBonesCount;

            /// <summary>
            /// PhysBones Affected Transforms count.
            /// </summary>
            internal int PhysBonesTransformCount;

            /// <summary>
            /// PhysBonesColliders Components count.
            /// </summary>
            internal int PhysBonesColliderCount;

            /// <summary>
            /// PhysBones Collision Check count.
            /// </summary>
            internal int PhysBonesCollisionCheckCount;

            /// <summary>
            /// Avatar Dynamics Contacts count.
            /// </summary>
            internal int ContactsCount;
        }
    }
}
