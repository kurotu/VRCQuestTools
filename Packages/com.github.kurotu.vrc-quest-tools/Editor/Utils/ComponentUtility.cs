using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility class for working with VRCQuestTools components.
    /// </summary>
    internal static class ComponentUtility
    {
        /// <summary>
        /// Gets the primary material conversion component from the specified GameObject.
        /// </summary>
        /// <param name="rootObject">The root GameObject to search for the material conversion component.</param>
        /// <returns>The primary material conversion component if found; otherwise, <c>null</c>.</returns>
        public static IMaterialConversionComponent GetPrimaryMaterialConversionComponent(GameObject rootObject)
        {
            var components = rootObject.GetComponents<IMaterialConversionComponent>();
            return components.FirstOrDefault(c => c.IsPrimaryRoot);
        }

        /// <summary>
        /// Gets whether the avatar for the specified GameObject has a PlatformTargetSettings component.
        /// </summary>
        /// <param name="gameObject">GameObject of a component that may be placed on a child of the avatar root.</param>
        /// <returns><c>true</c> if the avatar root has a PlatformTargetSettings component.</returns>
        public static bool AvatarHasPlatformTargetSettings(GameObject gameObject)
        {
            return GetAvatarRoot(gameObject).GetComponent<PlatformTargetSettings>() != null;
        }

        /// <summary>
        /// Gets whether the resolved build target for the avatar of the specified GameObject is Android (Mobile).
        /// </summary>
        /// <param name="gameObject">GameObject of a component that may be placed on a child of the avatar root.</param>
        /// <returns><c>true</c> if the resolved build target is Android.</returns>
        public static bool IsMobileBuildTarget(GameObject gameObject)
        {
            var targetSettings = GetAvatarRoot(gameObject).GetComponent<PlatformTargetSettings>();
            var buildTarget = targetSettings != null ? targetSettings.buildTarget : Models.BuildTarget.Auto;
            if (buildTarget == Models.BuildTarget.Auto)
            {
                buildTarget = EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS
                    ? Models.BuildTarget.Android
                    : Models.BuildTarget.PC;
            }

            return buildTarget == Models.BuildTarget.Android;
        }

        // PlatformTargetSettings is looked up on the avatar root rather than gameObject itself,
        // since platform-dependent components are commonly placed on child objects.
        private static GameObject GetAvatarRoot(GameObject gameObject)
        {
            var avatarDescriptor = gameObject.GetComponentInParent<VRC_AvatarDescriptor>(true);
            return avatarDescriptor != null ? avatarDescriptor.gameObject : gameObject;
        }
    }
}
