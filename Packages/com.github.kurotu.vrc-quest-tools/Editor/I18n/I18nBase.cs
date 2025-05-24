// <copyright file="I18nBase.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1516 // Elements should be separated by blank line
#pragma warning disable SA1600 // Elements should be documented

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.I18n
{
    /// <summary>
    /// Base class for i18n object.
    /// </summary>
    internal abstract class I18nBase
    {
        /// <summary>
        /// Gets localization asset for this i18n.
        /// </summary>
        private readonly Lazy<LocalizationAsset> localizationAsset;

        /// <summary>
        /// Gets fallback to en-US.
        /// </summary>
        private readonly Lazy<LocalizationAsset> fallback = new Lazy<LocalizationAsset>(() =>
            AssetDatabase.LoadAssetAtPath<LocalizationAsset>(AssetDatabase.GUIDToAssetPath("a85787afc553be3449497e3beaf403cf")));

        internal I18nBase(string assetGuid)
        {
            localizationAsset = new Lazy<LocalizationAsset>(() =>
                AssetDatabase.LoadAssetAtPath<LocalizationAsset>(AssetDatabase.GUIDToAssetPath(assetGuid)));
        }

        internal string GetText(string key)
        {
            var str = localizationAsset.Value.GetLocalizedString(key);
            if ((key == str) && (localizationAsset.Value != fallback.Value))
            {
                return fallback.Value.GetLocalizedString(key);
            }
            return str;
        }

        private string GetText(string key, params object[] args)
        {
            var str = GetText(key);
            return string.Format(str, args);
        }

        internal string CancelLabel => GetText("CancelLabel");
        internal string OpenLabel => GetText("OpenLabel");
        internal string CloseLabel => GetText("CloseLabel");
        internal string DismissLabel => GetText("DismissLabel");
        internal string ExitLabel => GetText("ExitLabel");
        internal string YesLabel => GetText("YesLabel");
        internal string NoLabel => GetText("NoLabel");
        internal string AbortLabel => GetText("AbortLabel");
        internal string AddLabel => GetText("AddLabel");
        internal string RemoveLabel => GetText("RemoveLabel");
        internal string FixLabel => GetText("FixLabel");

        internal string Maximum => GetText("Maximum");

        internal string IncompatibleSDK => GetText("IncompatibleSDK");

        internal string TextureFormatHighQuality => GetText("TextureFormatHighQuality");
        internal string TextureFormatHighCompression => GetText("TextureFormatHighCompression");

        // Convert Avatar for Quest
        internal string AvatarConverterSettingsEditorDescription => GetText("AvatarConverterSettingsEditorDescription");
        internal string AvatarConverterSettingsEditorDescriptionNDMF => GetText("AvatarConverterSettingsEditorDescriptionNDMF");
        internal string ExitPlayModeToEdit => GetText("ExitPlayModeToEdit");
        internal string BeginConvertSettingsButtonLabel => GetText("BeginConvertSettingsButtonLabel");
        internal string BeginConvertSettingsButtonDescription => GetText("BeginConvertSettingsButtonDescription");

        internal string AvatarLabel => GetText("AvatarLabel");
        internal string GenerateAndroidTexturesLabel => GetText("GenerateAndroidTexturesLabel");
        internal string GenerateAndroidTexturesTooltip => GetText("GenerateAndroidTexturesTooltip");
        internal string SupportedShadersLabel => GetText("SupportedShadersLabel");
        internal string SaveToLabel => GetText("SaveToLabel");
        internal string SelectButtonLabel => GetText("SelectButtonLabel");
        internal string ConvertButtonLabel => GetText("ConvertButtonLabel");
        internal string AssignButtonLabel => GetText("AssignButtonLabel");
        internal string AttachButtonLabel => GetText("AttachButtonLabel");
        internal string UpdateTexturesLabel => GetText("UpdateTexturesLabel");
        internal string AdvancedConverterSettingsLabel => GetText("AdvancedConverterSettingsLabel");
        internal string RemoveVertexColorLabel => GetText("RemoveVertexColorLabel");
        internal string RemoveVertexColorTooltip => GetText("RemoveVertexColorTooltip");
        internal string RemoveExtraMaterialSlotsLabel => GetText("RemoveExtraMaterialSlotsLabel");
        internal string RemoveExtraMaterialSlotsTooltip => GetText("RemoveExtraMaterialSlotsTooltip");
        internal string CompressExpressionsMenuIconsLabel => GetText("CompressExpressionsMenuIconsLabel");
        internal string CompressExpressionsMenuIconsTooltip => GetText("CompressExpressionsMenuIconsTooltip");
        internal string AnimationOverrideLabel => GetText("AnimationOverrideLabel");
        internal string AnimationOverrideTooltip => GetText("AnimationOverrideTooltip");
        internal string NdmfPhaseLabel => GetText("NdmfPhaseLabel");
        internal string NdmfPhaseTooltip => GetText("NdmfPhaseTooltip");
        internal string GeneratingTexturesDialogMessage => GetText("GeneratingTexturesDialogMessage");
        internal string AvatarConverterFailedDialogMessage => GetText("AvatarConverterFailedDialogMessage");
        internal string MaterialExceptionDialogMessage => GetText("MaterialExceptionDialogMessage");
        internal string AnimationClipExceptionDialogMessage => GetText("AnimationClipExceptionDialogMessage");
        internal string AnimatorControllerExceptionDialogMessage => GetText("AnimatorControllerExceptionDialogMessage");
        internal string InvalidReplacementMaterialExceptionDialogMessage => GetText("InvalidReplacementMaterialExceptionDialogMessage");
        internal string InfoForNdmfConversion => GetText("InfoForNdmfConversion");
        internal string InfoForNdmfConversion2 => GetText("InfoForNdmfConversion2");

        internal string InfoForNetworkIdAssigner => GetText("InfoForNetworkIdAssigner");
        internal string NetworkIdAssignerAttached => GetText("NetworkIdAssignerAttached");
        internal string WarningForPerformance => GetText("WarningForPerformance");
        internal string WarningForAppearance => GetText("WarningForAppearance");
        internal string WarningForUnsupportedShaders => GetText("WarningForUnsupportedShaders");
        internal string AlertForDynamicBoneConversion => GetText("AlertForDynamicBoneConversion");
        internal string AlertForMAConvertConstraints => GetText("AlertForMAConvertConstraints");
        internal string AlertForUnityConstraintsConversion => GetText("AlertForUnityConstraintsConversion");
        internal string AlertForMultiplePhysBones => GetText("AlertForMultiplePhysBones");
        internal string AlertForAvatarDynamicsPerformance => GetText("AlertForAvatarDynamicsPerformance");
        internal string AlertForComponents => GetText("AlertForComponents");
        internal string ErrorForPrefabStage => GetText("ErrorForPrefabStage");
        internal string ExitPrefabStageButtonLabel => GetText("ExitPrefabStageButtonLabel");

        internal string AvatarConverterMaterialConvertSettingsLabel => GetText("AvatarConverterMaterialConvertSettingsLabel");
        internal string AvatarConverterDefaultMaterialConvertSettingLabel => GetText("AvatarConverterDefaultMaterialConvertSettingLabel");
        internal string AvatarConverterAdditionalMaterialConvertSettingsLabel => GetText("AvatarConverterAdditionalMaterialConvertSettingsLabel");

        internal string AvatarConverterAvatarDynamicsSettingsLabel => GetText("AvatarConverterAvatarDynamicsSettingsLabel");
        internal string AvatarConverterRemoveAvatarDynamicsLabel => GetText("AvatarConverterRemoveAvatarDynamicsLabel");
        internal string AvatarConverterRemoveAvatarDynamicsTooltip => GetText("AvatarConverterRemoveAvatarDynamicsTooltip");
        internal string AvatarConverterPhysBonesTooltip => GetText("AvatarConverterPhysBonesTooltip");
        internal string AvatarConverterPhysBoneCollidersTooltip => GetText("AvatarConverterPhysBoneCollidersTooltip");
        internal string AvatarConverterContactsTooltip => GetText("AvatarConverterContactsTooltip");
        internal string OpenAvatarBuilder => GetText("OpenAvatarBuilder");
        internal string ManualConversionWarning => GetText("ManualConversionWarning");
        internal string ManualConvertButtonLabel => GetText("ManualConvertButtonLabel");
        internal string ConfirmationForUnityConstraints => GetText("ConfirmationForUnityConstraints");
        internal string ConfirmationForMAConvertConstraints => GetText("ConfirmationForMAConvertConstraints");

        // IMaterialConvertSettings
        internal string IMaterialConvertSettingsTexturesSizeLimitLabel => GetText("IMaterialConvertSettingsTexturesSizeLimitLabel");
        internal string IMaterialConvertSettingsMobileTextureFormatLabel => GetText("IMaterialConvertSettingsMobileTextureFormatLabel");
        internal string IMaterialConvertSettingsMainTextureBrightnessLabel => GetText("IMaterialConvertSettingsMainTextureBrightnessLabel");
        internal string IMaterialConvertSettingsMainTextureBrightnessTooltip => GetText("IMaterialConvertSettingsMainTextureBrightnessTooltip");
        internal string ToonLitConvertSettingsGenerateShadowFromNormalMapLabel => GetText("ToonLitConvertSettingsGenerateShadowFromNormalMapLabel");
        internal string MatCapLitConvertSettingsMatCapTextureLabel => GetText("MatCapLitConvertSettingsMatCapTextureLabel");
        internal string MatCapLitConvertSettingsMatCapTextureWarning => GetText("MatCapLitConvertSettingsMatCapTextureWarning");
        internal string ToonStandardConvertSettingsFallbackShadowRampLabel => GetText("ToonStandardConvertSettingsFallbackShadowRampLabel");
        internal string ToonStandardConvertSettingsCustomFallbackShadowRampLabel => GetText("ToonStandardConvertSettingsCustomFallbackShadowRampLabel");
        internal string AdditionalMaterialConvertSettingsTargetMaterialLabel => GetText("AdditionalMaterialConvertSettingsTargetMaterialLabel");
        internal string AdditionalMaterialConvertSettingsSelectMaterialLabel => GetText("AdditionalMaterialConvertSettingsSelectMaterialLabel");
        internal string MaterialConvertTypePopupLabelToonLit => GetText("MaterialConvertTypePopupLabelToonLit");
        internal string MaterialConvertTypePopupLabelMatCapLit => GetText("MaterialConvertTypePopupLabelMatCapLit");
        internal string MaterialConvertTypePopupLabelToonStandard => GetText("MaterialConvertTypePopupLabelToonStandard");
        internal string MaterialConvertTypePopupLabelMaterialReplace => GetText("MaterialConvertTypePopupLabelMaterialReplace");
        internal string MaterialReplaceSettingsMaterialLabel => GetText("MaterialReplaceSettingsMaterialLabel");
        internal string MaterialReplaceSettingsMaterialTooltip => GetText("MaterialReplaceSettingsMaterialTooltip");
        internal string MaterialReplaceSettingsMaterialWarning => GetText("MaterialReplaceSettingsMaterialWarning");

        // Remove Missing Components
        internal string NoMissingComponentsMessage(string objectName) => GetText("NoMissingComponentsMessage", objectName);
        internal string MissingRemoverConfirmationMessage(string objectName) => GetText("MissingRemoverConfirmationMessage", objectName);
        internal string UnpackPrefabMessage => GetText("UnpackPrefabMessage");
        internal string MissingRemoverOnBuildDialogMessage(string objectName) => GetText("MissingRemoverOnBuildDialogMessage", objectName);

        // BlendShapes Copy
        internal string SourceMeshLabel => GetText("SourceMeshLabel");
        internal string TargetMeshLabel => GetText("TargetMeshLabel");
        internal string CopyButtonLabel => GetText("CopyButtonLabel");
        internal string SwitchButtonLabel => GetText("SwitchButtonLabel");

        // Remove Unsupported Components
        internal string NoUnsupportedComponentsMessage(string objectName) => GetText("NoUnsupportedComponentsMessage", objectName);
        internal string UnsupportedRemoverConfirmationMessage(string objectName) => GetText("UnsupportedRemoverConfirmationMessage", objectName);

        // Remove PhysBones
        internal string PhysBonesSDKRequired => GetText("PhysBonesSDKRequired");
        internal string SelectComponentsToKeep => GetText("SelectComponentsToKeep");
        internal string PhysBonesListTooltip => GetText("PhysBonesListTooltip");
        internal string KeepAll => GetText("KeepAll");
        internal string AvatarDynamicsPreventsUpload => GetText("AvatarDynamicsPreventsUpload");
        internal string PhysBonesWillBeRemovedAtRunTime => GetText("PhysBonesWillBeRemovedAtRunTime");
        internal string PhysBoneCollidersWillBeRemovedAtRunTime => GetText("PhysBoneCollidersWillBeRemovedAtRunTime");
        internal string ContactsWillBeRemovedAtRunTime => GetText("ContactsWillBeRemovedAtRunTime");
        internal string PhysBonesTransformsShouldBeReduced => GetText("PhysBonesTransformsShouldBeReduced");
        internal string PhysBonesCollisionCheckCountShouldBeReduced => GetText("PhysBonesCollisionCheckCountShouldBeReduced");

        internal string PhysBonesShouldHaveNetworkID => GetText("PhysBonesShouldHaveNetworkID");
        internal string EstimatedPerformanceStats => GetText("EstimatedPerformanceStats");
        internal string DeleteUnselectedComponents => GetText("DeleteUnselectedComponents");

        // Avatar Dynamics Selector
        internal string SelectAllButtonLabel => GetText("SelectAllButtonLabel");
        internal string DeselectAllButtonLabel => GetText("DeselectAllButtonLabel");
        internal string ApplyButtonLabel => GetText("ApplyButtonLabel");

        // Metallic Smoothness
        internal string TextureLabel => GetText("TextureLabel");
        internal string InvertLabel => GetText("InvertLabel");
        internal string SaveFileDialogTitle(string thing) => GetText("SaveFileDialogTitle", thing);
        internal string SaveFileDialogMessage => GetText("SaveFileDialogMessage");
        internal string GenerateButtonLabel => GetText("GenerateButtonLabel");

        // Unity Settings
        internal string RecommendedUnitySettingsForAndroid => GetText("RecommendedUnitySettingsForAndroid");
        internal string AndroidBuildSupportButtonLabel => GetText("AndroidBuildSupportButtonLabel");
        internal string AndroidBuildSupportHelp => GetText("AndroidBuildSupportHelp");
        internal string TextureCompressionLabel => GetText("TextureCompressionLabel");
        internal string TextureCompressionHelp => GetText("TextureCompressionHelp");
        internal string TextureCompressionButtonLabel => GetText("TextureCompressionButtonLabel");
        internal string ApplyAllButtonLabel => GetText("ApplyAllButtonLabel");
        internal string ShowOnStartupLabel => GetText("ShowOnStartupLabel");
        internal string AllAppliedHelp => GetText("AllAppliedHelp");

        // Check for Update
        internal string GetUpdate => GetText("GetUpdate");
        internal string SeeChangelog => GetText("SeeChangelog");
        internal string SkipThisVersion => GetText("SkipThisVersion");
        internal string NewVersionIsAvailable(string latestVersion) => GetText("NewVersionIsAvailable", latestVersion);
        internal string NewVersionHasBreakingChanges => GetText("NewVersionHasBreakingChanges");

        // Validations
        internal string MissingScripts => GetText("MissingScripts");

        // Inspector
        internal string ExperimentalComponentWarning => GetText("ExperimentalComponentWarning");
        internal string AvatarRootComponentMustBeOnAvatarRoot => GetText("AvatarRootComponentMustBeOnAvatarRoot");

        // Vertex Color
        internal string VertexColorRemoverEditorDescription => GetText("VertexColorRemoverEditorDescription");
        internal string VertexColorRemoverEditorRemove => GetText("VertexColorRemoverEditorRemove");
        internal string VertexColorRemoverEditorRestore => GetText("VertexColorRemoverEditorRestore");

        // Converted Avatar
        internal string ConvertedAvatarEditorMessage => GetText("ConvertedAvatarEditorMessage");
        internal string ConvertedAvatarEditorNDMFMessage => GetText("ConvertedAvatarEditorNDMFMessage");

        // Network ID Assigner
        internal string NetworkIDAssignerEditorDescription => GetText("NetworkIDAssignerEditorDescription");

        // Platform Target Settings
        internal string PlatformTargetSettingsEditorDescription => GetText("PlatformTargetSettingsEditorDescription");
        internal string PlatformTargetSettingsIsRequiredToEnforcePlatform => GetText("PlatformTargetSettingsIsRequiredToEnforcePlatform");

        // Platform Component Remover
        internal string ComponentRequiresNdmf => GetText("ComponentRequiresNdmf");
        internal string BuildTargetLabel => GetText("BuildTargetLabel");
        internal string BuildTargetTooltip => GetText("BuildTargetTooltip");
        internal string PlatformComponentRemoverEditorDescription => GetText("PlatformComponentRemoverEditorDescription");
        internal string PlatformComponentRemoverEditorComponentSettingsLabel => GetText("PlatformComponentRemoverEditorComponentSettingsLabel");
        internal string PlatformComponentRemoverEditorComponentSettingsTooltip => GetText("PlatformComponentRemoverEditorComponentSettingsTooltip");
        internal string PlatformComponentRemoverEditorCheckboxPCTooltip => GetText("PlatformComponentRemoverEditorCheckboxPCTooltip");
        internal string PlatformComponentRemoverEditorCheckboxAndroidTooltip => GetText("PlatformComponentRemoverEditorCheckboxAndroidTooltip");
        internal string ComponentLabel => GetText("ComponentLabel");

        // Platform GameObject Remover
        internal string PlatformGameObjectRemoverEditorDescription => GetText("PlatformGameObjectRemoverEditorDescription");
        internal string PlatformGameObjectRemoverEditorKeepOnPCLabel => GetText("PlatformGameObjectRemoverEditorKeepOnPCLabel");
        internal string PlatformGameObjectRemoverEditorKeepOnAndroidLabel => GetText("PlatformGameObjectRemoverEditorKeepOnAndroidLabel");

        // Mesh Flipper
        internal string MeshFlipperEditorDescription => GetText("MeshFlipperEditorDescription");
        internal string MeshFlipperEditorDirectionLabel => GetText("MeshFlipperEditorDirectionLabel");
        internal string MeshFlipperEditorUseMaskLabel => GetText("MeshFlipperEditorUseMaskLabel");
        internal string MeshFlipperEditorMaskTextureLabel => GetText("MeshFlipperEditorMaskTextureLabel");
        internal string MeshFlipperEditorMaskModeLabel => GetText("MeshFlipperEditorMaskModeLabel");
        internal string MeshFlipperEditorProcessingPhaseLabel => GetText("MeshFlipperEditorProcessingPhaseLabel");
        internal string MeshFlipperEditorEnabledOnPCLabel => GetText("MeshFlipperEditorEnabledOnPCLabel");
        internal string MeshFlipperEditorEnabledOnAndroidLabel => GetText("MeshFlipperEditorEnabledOnAndroidLabel");
        internal string MeshFlipperEditorEnabledOnPCWarning => GetText("MeshFlipperEditorEnabledOnPCWarning");
        internal string MeshFlipperEditorEnabledOnAndroidWarning => GetText("MeshFlipperEditorEnabledOnAndroidWarning");
        internal string MeshFlipperEditorMaskTextureMissingError => GetText("MeshFlipperEditorMaskTextureMissingError");
        internal string MeshFlipperEditorMaskTextureNotReadableError => GetText("MeshFlipperEditorMaskTextureNotReadableError");
        internal string MeshFlipperMeshDirectionFlip => GetText("MeshFlipperMeshDirectionFlip");
        internal string MeshFlipperMeshDirectionDoubleSide => GetText("MeshFlipperMeshDirectionDoubleSide");

        // Material Swap
        internal string MaterialSwapEditorDescription => GetText("MaterialSwapEditorDescription");
        internal string MaterialSwapEditorMaterialMappingsLabel => GetText("MaterialSwapEditorMaterialMappingsLabel");
        internal string MaterialSwapEditorOriginalMaterialLabel => GetText("MaterialSwapEditorOriginalMaterialLabel");
        internal string MaterialSwapEditorReplacementMaterialLabel => GetText("MaterialSwapEditorReplacementMaterialLabel");
        internal string MaterialSwapEditorSelectMaterialLabel => GetText("MaterialSwapEditorSelectMaterialLabel");
        internal string MaterialSwapEditorReplacementMaterialError => GetText("MaterialSwapEditorReplacementMaterialError");

        // Material Conversion Settings
        internal string MaterialConversionSettingsEditorDescription => GetText("MaterialConversionSettingsEditorDescription");
        internal string MaterialConversionSettingsEditorDefaultConversionWarning => GetText("MaterialConversionSettingsEditorDefaultConversionWarning");

        // Menu Icons Resizer
        internal string MenuIconResizerEditorDescription => GetText("MenuIconResizerEditorDescription");
        internal string MenuIconResizerEditorResizeModePCLabel => GetText("MenuIconResizerEditorResizeModePCLabel");
        internal string MenuIconResizerEditorResizeModeAndroidLabel => GetText("MenuIconResizerEditorResizeModeAndroidLabel");
        internal string MenuIconResizerEditorCompressTexturesLabel => GetText("MenuIconResizerEditorCompressTexturesLabel");
        internal string MenuIconResizerEditorMobileTextureFormatLabel => GetText("MenuIconResizerEditorMobileTextureFormatLabel");

        // Avatar Builder
        internal string AvatarBuilderWindowExitPlayMode => GetText("AvatarBuilderWindowExitPlayMode");
        internal string AvatarBuilderWindowExitPrefabStage => GetText("AvatarBuilderWindowExitPrefabStage");
        internal string AvatarBuilderWindowNoActiveAvatarsFound => GetText("AvatarBuilderWindowNoActiveAvatarsFound");
        internal string AvatarBuilderWindowSelectAvatar => GetText("AvatarBuilderWindowSelectAvatar");
        internal string AvatarBuilderWindowNoNdmfComponentsFound => GetText("AvatarBuilderWindowNoNdmfComponentsFound");
        internal string AvatarBuilderWindowSucceededBuild => GetText("AvatarBuilderWindowSucceededBuild");
        internal string AvatarBuilderWindowSucceededUpload => GetText("AvatarBuilderWindowSucceededUpload");
        internal string AvatarBuilderWindowFailedBuild => GetText("AvatarBuilderWindowFailedBuild");
        internal string AvatarBuilderWindowRequiresControlPanel => GetText("AvatarBuilderWindowRequiresControlPanel");
        internal string AvatarBuilderWindowBuildingProgressLabel => GetText("AvatarBuilderWindowBuildingProgressLabel");
        internal string AvatarBuilderWindowUploadingProgressLabel => GetText("AvatarBuilderWindowUploadingProgressLabel");
        internal string AvatarBuilderWindowCopyrightAgreementHelp => GetText("AvatarBuilderWindowCopyrightAgreementHelp");
        internal string AvatarBuilderWindowOfflineTestingLabel(string platformName) => GetText("AvatarBuilderWindowOfflineTestingLabel", platformName);
        internal string AvatarBuilderWindowOfflineTestingDescription(string name) => GetText("AvatarBuilderWindowOfflineTestingDescription", name);
        internal string AvatarBuilderWindowOnlinePublishingLabel(string platformName) => GetText("AvatarBuilderWindowOnlinePublishingLabel", platformName);
        internal string AvatarBuilderWindowOnlinePublishingDescription => GetText("AvatarBuilderWindowOnlinePublishingDescription");
        internal string AvatarBuilderWindowSdkNoPrecheck => GetText("AvatarBuilderWindowSdkNoPrecheck");
        internal string AvatarBuilderWindowSetAsFallbackIfPossible => GetText("AvatarBuilderWindowSetAsFallbackIfPossible");
        internal string AvatarBuilderWindowSetAsFallbackIfPossibleTooltip => GetText("AvatarBuilderWindowSetAsFallbackIfPossibleTooltip");
        internal string AvatarBuilderWindowFallbackNotAllowed(string rating) => GetText("AvatarBuilderWindowFallbackNotAllowed", rating);
        internal string AvatarBuilderWindowNdmfManualBakingLabel => GetText("AvatarBuilderWindowNdmfManualBakingLabel");
        internal string AvatarBuilderWindowNdmfManualBakingDescription => GetText("AvatarBuilderWindowNdmfManualBakingDescription");
        internal string AvatarBuilderWindowRequiresAvatarNameAndThumb => GetText("AvatarBuilderWindowRequiresAvatarNameAndThumb");

        // NDMF
        internal string FeatureRequiresNdmf => GetText("FeatureRequiresNdmf");
        internal string NdmfPluginRequiresNdmfUpdate(string requiredVersion) => GetText("NdmfPluginRequiresNdmfUpdate", requiredVersion);
        internal string NdmfPluginRemovedUnsupportedComponent(string typeName, string objectName) => GetText("NdmfPluginRemovedUnsupportedComponent", typeName, objectName);
    }
}
