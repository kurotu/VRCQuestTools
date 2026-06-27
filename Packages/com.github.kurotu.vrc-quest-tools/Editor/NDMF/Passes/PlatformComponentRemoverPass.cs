using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Remove components by PlatformComponentRemover in NDMF.
    /// </summary>
    internal class PlatformComponentRemoverPass : Pass<PlatformComponentRemoverPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Remove platform components";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            var pcrs = context.AvatarRootObject.GetComponentsInChildren<PlatformComponentRemover>(true);

            var componentsToRemove = pcrs
                .SelectMany(pcr => pcr.componentSettings)
                .Where(s => s.component != null)
                .Where(s => IsDecendant(s.component.gameObject, context.AvatarRootObject))
                .Where(s => (buildTarget == Models.BuildTarget.PC && s.removeOnPC) ||
                            (buildTarget == Models.BuildTarget.Android && s.removeOnAndroid))
                .Select(s => s.component)
                .ToArray();

            // Remove PhysBones first.
            foreach (var physBone in componentsToRemove.OfType<VRCPhysBone>())
            {
                Logger.Log($"Remove {physBone.GetType().Name} for {buildTarget} platform", physBone.gameObject);
                Object.DestroyImmediate(physBone);
            }

            // Remove colliders, clearing references from remaining PhysBones first.
            var remainingPhysBones = context.AvatarRootObject.GetComponentsInChildren<VRCPhysBone>(true);
            foreach (var collider in componentsToRemove.OfType<VRCPhysBoneCollider>())
            {
                foreach (var bone in remainingPhysBones)
                {
                    for (var i = 0; i < bone.colliders.Count; i++)
                    {
                        if (bone.colliders[i] == collider)
                        {
                            bone.colliders[i] = null;
                        }
                    }
                }

                Logger.Log($"Remove {collider.GetType().Name} for {buildTarget} platform", collider.gameObject);
                Object.DestroyImmediate(collider);
            }

            // Remove contacts and other components.
            foreach (var component in componentsToRemove.Where(c => !(c is VRCPhysBone) && !(c is VRCPhysBoneCollider)))
            {
                Logger.Log($"Remove {component.GetType().Name} for {buildTarget} platform", component.gameObject);
                Object.DestroyImmediate(component);
            }

            // Strip orphaned NetworkIDs left by removed PhysBones.
            if (componentsToRemove.OfType<VRCPhysBone>().Any())
            {
                var avatarDescriptor = context.AvatarRootObject.GetComponent<VRC_AvatarDescriptor>();
                if (avatarDescriptor != null)
                {
                    VRCSDKUtility.StripeUnusedNetworkIds(avatarDescriptor);
                }
            }
        }

        private bool IsDecendant(GameObject decendant, GameObject root)
        {
            if (decendant == null)
            {
                return false;
            }
            if (root == null)
            {
                return false;
            }
            if (decendant == root)
            {
                return true;
            }
            var parent = decendant.transform.parent;
            if (parent == null)
            {
                return false;
            }
            return IsDecendant(parent.gameObject, root);
        }
    }
}
