using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Utility for avatar converter pass.
    /// </summary>
    internal static class AvatarConverterPassUtility
    {
        /// <summary>
        /// Check if the avatar has material conversion components.
        /// </summary>
        /// <param name="avatarRoot">Root game object.</param>
        /// <returns>True if the avatar has material conversion components.</returns></returns>
        internal static bool HasMaterialOperatorComponents(GameObject avatarRoot)
        {
            return avatarRoot.GetComponentsInChildren<IMaterialOperatorComponent>(true).Length > 0;
        }

        /// <summary>
        /// Resolve the NDMF phase from the avatar conversion.
        /// </summary>
        /// <param name="avatarRoot">Root game object.</param>
        /// <returns>NDMF Phase.</returns>
        internal static AvatarConverterNdmfPhase ResolveAvatarConverterNdmfPhase(GameObject avatarRoot)
        {
            var primaryRoot = avatarRoot.GetComponents<IMaterialConversionComponent>().FirstOrDefault(c => c.IsPrimaryRoot);
            if (primaryRoot != null)
            {
                return primaryRoot.NdmfPhase.Resolve(avatarRoot);
            }

            return AvatarConverterNdmfPhase.Auto.Resolve(avatarRoot);
        }

        /// <summary>
        /// Convert the avatar with AvatarConverterSettings component in NDMF.
        /// </summary>
        /// <param name="context">BuildContext.</param>
        internal static void ConvertAvatarInPass(BuildContext context)
        {
            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            if (buildTarget != Models.BuildTarget.Android)
            {
                return;
            }

            var settings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (settings != null)
            {
                context.GetState<NdmfState>().compressExpressionsMenuIcons = settings.compressExpressionsMenuIcons;
            }

            try
            {
                TrackObjectRegistryForMaterialSwaps(context);
                TrackObjectRegistryForMaterialConversion(context);

                var avatar = new VRChatAvatar(context.AvatarDescriptor);
                var objectRegistry = context.GetState<NdmfObjectRegistry>();
                VRCQuestTools.AvatarConverter.ConvertForQuestInPlace(avatar, VRCQuestTools.ComponentRemover, false, null, new Models.VRChat.AvatarConverter.ProgressCallback()
                {
                    onTextureProgress = (_, __, original, converted) =>
                    {
                        // Register converted material to ObjectRegistry when it is not an asset..
                        if (converted != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(converted)))
                        {
                            objectRegistry.RegisterReplacedObject(original, converted);
                        }
                    },
                    onAnimationClipProgress = (_, __, original, converted) =>
                    {
                        if (converted != null)
                        {
                            objectRegistry.RegisterReplacedObject(original, converted);
                        }
                    },
                    onRuntimeAnimatorProgress = (_, __, original, converted) =>
                    {
                        if (converted != null)
                        {
                            objectRegistry.RegisterReplacedObject(original, converted);
                        }
                    },
                });
            }
            catch (System.Exception exception)
            {
                NdmfPluginUtility.HandleConversionException(exception);
            }
        }

        /// <summary>
        /// Track object registry then add as new settings.
        /// </summary>
        private static void TrackObjectRegistryForMaterialConversion(BuildContext context)
        {
            var avatar = new VRChatAvatar(context.AvatarDescriptor);
            var currentMaterials = avatar.Materials;

            var conversions = context.AvatarRootObject.GetComponentsInChildren<IMaterialConversionComponent>(true);
            foreach (var conversion in conversions)
            {
                // Create new additional material settings by tracking object registry.
                var newAdditionalMappings = new List<AdditionalMaterialConvertSettings>();
                foreach (var material in currentMaterials)
                {
                    var original = (Material)NdmfObjectRegistry.GetReference(material).Object;
                    if (material == original)
                    {
                        continue;
                    }

                    var setting = conversion.AdditionalMaterialConvertSettings.FirstOrDefault(s => s.targetMaterial == original);
                    if (setting == null)
                    {
                        continue;
                    }
                    var newSetting = new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = material,
                        materialConvertSettings = setting.materialConvertSettings,
                    };
                    newAdditionalMappings.Add(newSetting);
                }
                conversion.AdditionalMaterialConvertSettings = conversion.AdditionalMaterialConvertSettings.Concat(newAdditionalMappings).ToArray();
            }
        }

        private static void TrackObjectRegistryForMaterialSwaps(BuildContext context)
        {
            var avatar = new VRChatAvatar(context.AvatarDescriptor);
            var currentMaterials = avatar.Materials;

            var swaps = context.AvatarRootObject.GetComponentsInChildren<MaterialSwap>(true);
            foreach (var swap in swaps)
            {
                var newMappings = new List<MaterialSwap.MaterialMapping>();
                foreach (var material in currentMaterials)
                {
                    var original = (Material)NdmfObjectRegistry.GetReference(material).Object;
                    if (material == original)
                    {
                        continue;
                    }

                    var mapping = swap.materialMappings.FirstOrDefault(m => m.originalMaterial == original);
                    if (mapping == null)
                    {
                        continue;
                    }
                    var newMapping = new MaterialSwap.MaterialMapping
                    {
                        originalMaterial = material,
                        replacementMaterial = mapping.replacementMaterial,
                    };
                    newMappings.Add(newMapping);
                }
                swap.materialMappings = swap.materialMappings.Concat(newMappings).ToList();
            }
        }
    }
}
