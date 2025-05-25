using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// NDMF phase for AvatarConverterSettings.
    /// </summary>
    public enum AvatarConverterNdmfPhase
    {
        /// <summary>Transforming phase.</summary>
        Transforming,

        /// <summary>Optimizing phase.</summary>
        Optimizing,

        /// <summary>Auto.</summary>
        Auto,
    }

    /// <summary>
    /// Extension methods for AvatarConverterNdmfPhase.
    /// </summary>
#pragma warning disable SA1649 // SA1649 File name should match first type name
    public static class AvatarConverterNdmfPhaseExtension
#pragma warning restore SA1649
    {
#if VQT_VRCFURY
        private static Lazy<Type> VRCFuryComponentType = new Lazy<Type>(() =>
        {
            var type = GetTypeByName("VF.Component.VRCFuryComponent") ?? GetTypeByName("VF.Model.VRCFuryComponent");
            if (type == null)
            {
                Debug.LogError("[VRCQuestTools] VRCFuryComponent type not found. VRCFury may have made breaking changes.");
            }
            return type;
        });
#endif

        /// <summary>
        /// Resolve the NDMF phase for project configuration.
        /// </summary>
        /// <param name="phase">This object.</param>
        /// <param name="avatarRoot">Avatar root game object.</param>
        /// <returns>Resolved phase other than Auto.</returns>
        public static AvatarConverterNdmfPhase Resolve(this AvatarConverterNdmfPhase phase, GameObject avatarRoot)
        {
            if (phase == AvatarConverterNdmfPhase.Auto)
            {
                if (avatarRoot == null)
                {
                    return AvatarConverterNdmfPhase.Optimizing;
                }

#if VQT_VRCFURY
                if (VRCFuryComponentType.Value == null)
                {
                    // VRCFury exists, but the type not found. => VRCFury made breaking changes.
                    return AvatarConverterNdmfPhase.Transforming;
                }

                if (avatarRoot.GetComponentInChildren(VRCFuryComponentType.Value, true) != null)
                {
                    return AvatarConverterNdmfPhase.Transforming;
                }
#endif

                return AvatarConverterNdmfPhase.Optimizing;
            }
            return phase;
        }

        private static Type GetTypeByName(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
    }
}
