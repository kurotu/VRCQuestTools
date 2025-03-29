using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Utility for NDMF plugin.
    /// </summary>
    internal static class NdmfPluginUtility
    {
        /// <summary>
        /// Manual bake with Android buold target settings.
        /// </summary>
        /// <param name="avatar">Target avatar.</param>
        /// <returns>Baked avatar.</returns>
        internal static GameObject ManualBakeWithAndroidSettings(GameObject avatar)
        {
            SetBuildTarget(Models.BuildTarget.Android);
            try
            {
                return AvatarProcessor.ProcessAvatarUI(avatar);
            }
            finally
            {
                SetBuildTarget(Models.BuildTarget.Auto);
            }
        }

        /// <summary>
        /// Convert the avatar with AvatarConverterSettings component in NDMF.
        /// </summary>
        /// <param name="context">BuildContext.</param>
        /// <param name="settings">settings component.</param>
        internal static void ConvertAvatarInPass(BuildContext context, AvatarConverterSettings settings)
        {
            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            if (buildTarget != Models.BuildTarget.Android)
            {
                return;
            }

            context.GetState<NdmfState>().compressExpressionsMenuIcons = settings.compressExpressionsMenuIcons;
            var objectRegistry = context.GetState<NdmfObjectRegistry>();

            try
            {
                TrackObjectRegistryForMaterialSwaps(objectRegistry, context.AvatarRootObject);
                TrackObjectRegistryForConverterSettings(context, settings);

                VRCQuestTools.AvatarConverter.ConvertForQuestInPlace(settings, VRCQuestTools.ComponentRemover, false, null, new Models.VRChat.AvatarConverter.ProgressCallback()
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
                HandleConversionException(exception);
            }
        }

        private static void SetBuildTarget(Models.BuildTarget target)
        {
            NdmfSessionState.BuildTarget = target;
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

        private static void HandleConversionException(System.Exception exception)
        {
            var message = exception.Message;
            var dialogException = exception;
            switch (exception)
            {
                case MaterialConversionException e:
                    {
                        var matRef = NdmfObjectRegistry.GetReference(e.source);
                        var error = new MaterialConversionError(matRef, e);
                        ErrorReport.ReportError(error);
                    }
                    break;
                case AnimationClipConversionException e:
                    {
                        var animRef = NdmfObjectRegistry.GetReference(e.source);
                        var error = new ObjectConversionError(animRef, e);
                    }
                    break;
                case AnimatorControllerConversionException e:
                    {
                        var animRef = NdmfObjectRegistry.GetReference(e.source);
                        var error = new ObjectConversionError(animRef, e);
                    }
                    break;
                case InvalidReplacementMaterialException e:
                    {
                        var error = new ReplacementMaterialError(e.component, e.replacementMaterial);
                        ErrorReport.ReportError(error);
                    }
                    break;
            }
            if (exception.InnerException != null)
            {
                Debug.LogException(exception.InnerException);
            }
            Debug.LogException(exception);
        }
    }
}
