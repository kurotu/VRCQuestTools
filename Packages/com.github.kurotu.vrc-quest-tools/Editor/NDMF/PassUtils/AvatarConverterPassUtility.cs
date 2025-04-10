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
        internal static bool HasMaterialConversionComponents(GameObject avatarRoot)
        {
            return avatarRoot.GetComponentsInChildren<IMaterialConversionComponent>(true).Length > 0;
        }

        /// <summary>
        /// Resolve the NDMF phase from the avatar conversion.
        /// </summary>
        /// <param name="avatarRoot">Root game object.</param>
        /// <returns>NDMF Phase.</returns>
        internal static AvatarConverterNdmfPhase ResolveAvatarConverterNdmfPhase(GameObject avatarRoot)
        {
            var avatarConverterSettings = avatarRoot.GetComponent<AvatarConverterSettings>();
            if (avatarConverterSettings != null)
            {
                return avatarConverterSettings.ndmfPhase;
            }

            return AvatarConverterNdmfPhase.Transforming;
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

            var objectRegistry = context.GetState<NdmfObjectRegistry>();

            try
            {
                TrackObjectRegistryForMaterialSwaps(objectRegistry, context.AvatarRootObject);
                if (settings != null)
                {
                    TrackObjectRegistryForConverterSettings(context, settings);
                }

                var avatar = new VRChatAvatar(context.AvatarDescriptor);
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
        private static void TrackObjectRegistryForConverterSettings(BuildContext context, AvatarConverterSettings settings)
        {
            var avatar = new VRChatAvatar(context.AvatarDescriptor);
            var currentMaterials = avatar.Materials;

            // Create new additional material settings by tracking object registry.
            var newAdditionalMappings = new List<AdditionalMaterialConvertSettings>();
            foreach (var material in currentMaterials)
            {
                var original = (Material)NdmfObjectRegistry.GetReference(material).Object;
                var setting = settings.additionalMaterialConvertSettings.FirstOrDefault(s => s.targetMaterial == original);
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

            settings.additionalMaterialConvertSettings = settings.additionalMaterialConvertSettings.Concat(newAdditionalMappings).ToArray();
        }

        private static void TrackObjectRegistryForMaterialSwaps(NdmfObjectRegistry objectRegistry, GameObject avatarRoot)
        {
            var swaps = avatarRoot.GetComponentsInChildren<MaterialSwap>();
            var mappings = swaps.SelectMany(s => s.materialMappings);
            var map = new Dictionary<Material, Material>();

            // Create global material mapping.
            foreach (var mapping in mappings)
            {
                if (mapping.originalMaterial == null)
                {
                    continue;
                }
                if (map.ContainsKey(mapping.originalMaterial))
                {
                    continue;
                }
                map[mapping.originalMaterial] = mapping.replacementMaterial;
            }

            // Register material mapping to root swap and ObjectRegistry.
            var materials = new VRChatAvatar(avatarRoot.GetComponent<VRC_AvatarDescriptor>()).Materials;
            var rootSwap = VRC.Core.ExtensionMethods.GetOrAddComponent<MaterialSwap>(avatarRoot);
            foreach (var material in materials)
            {
                var original = (Material)NdmfObjectRegistry.GetReference(material).Object;
                if (map.ContainsKey(original))
                {
                    objectRegistry.RegisterReplacedObject(material, map[original]);
                    rootSwap.materialMappings.Add(new MaterialSwap.MaterialMapping
                    {
                        originalMaterial = material,
                        replacementMaterial = map[original],
                    });
                }
            }
        }
    }
}
