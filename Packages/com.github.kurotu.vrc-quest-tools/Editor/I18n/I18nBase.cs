// <copyright file="I18nBase.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1516 // Elements should be separated by blank line
#pragma warning disable SA1600 // Elements should be documented

namespace KRT.VRCQuestTools.I18n
{
    /// <summary>
    /// Base class for i18n object.
    /// </summary>
    internal abstract class I18nBase
    {
        internal abstract string CancelLabel { get; }
        internal abstract string OpenLabel { get; }
        internal abstract string CloseLabel { get; }
        internal abstract string DismissLabel { get; }
        internal abstract string YesLabel { get; }
        internal abstract string NoLabel { get; }
        internal abstract string AbortLabel { get; }
        internal abstract string RemoveLabel { get; }

        internal abstract string Maximum { get; }

        // Convert Avatar for Quest
        internal abstract string ExitPlayModeToEdit { get; }
        internal abstract string BeingConvertSettingsButtonLabel { get; }
        internal abstract string AvatarLabel { get; }
        internal abstract string GenerateAndroidTexturesLabel { get; }
        internal abstract string GenerateAndroidTexturesTooltip { get; }
        internal abstract string SupportedShadersLabel { get; }
        internal abstract string SaveToLabel { get; }
        internal abstract string SelectButtonLabel { get; }
        internal abstract string ConvertButtonLabel { get; }
        internal abstract string AssignButtonLabel { get; }
        internal abstract string AttachButtonLabel { get; }
        internal abstract string UpdateTexturesLabel { get; }
        internal abstract string AdvancedConverterSettingsLabel { get; }
        internal abstract string RemoveVertexColorLabel { get; }
        internal abstract string RemoveVertexColorTooltip { get; }
        internal abstract string AnimationOverrideLabel { get; }
        internal abstract string AnimationOverrideTooltip { get; }
        internal abstract string GeneratingTexturesDialogMessage { get; }
        internal abstract string MaterialExceptionDialogMessage { get; }
        internal abstract string AnimationClipExceptionDialogMessage { get; }
        internal abstract string AnimatorControllerExceptionDialogMessage { get; }
        internal abstract string InfoForNdmfConversion { get; }
        internal abstract string InfoForNetworkIdAssigner { get; }
        internal abstract string WarningForPerformance { get; }
        internal abstract string WarningForAppearance { get; }
        internal abstract string WarningForUnsupportedShaders { get; }
        internal abstract string AlertForDynamicBoneConversion { get; }
        internal abstract string AlertForMissingNetIds { get; }
        internal abstract string AlertForMultiplePhysBones { get; }
        internal abstract string AlertForAvatarDynamicsPerformance { get; }
        internal abstract string AlertForComponents { get; }

        internal abstract string AvatarConverterMustBeOnAvatarRoot { get; }
        internal abstract string AvatarConverterMaterialConvertSettingsLabel { get; }
        internal abstract string AvatarConverterDefaultMaterialConvertSettingLabel { get; }
        internal abstract string AvatarConverterAdditionalMaterialConvertSettingsLabel { get; }

        internal abstract string AvatarConverterAvatarDynamicsSettingsLabel { get; }
        internal abstract string AvatarConverterPhysBonesTooltip { get; }
        internal abstract string AvatarConverterPhysBoneCollidersTooltip { get; }
        internal abstract string AvatarConverterContactsTooltip { get; }

        // IMaterialConvertSettings
        internal abstract string IMaterialConvertSettingsTexturesSizeLimitLabel { get; }
        internal abstract string IMaterialConvertSettingsMainTextureBrightnessLabel { get; }
        internal abstract string IMaterialConvertSettingsMainTextureBrightnessTooltip { get; }
        internal abstract string ToonLitConvertSettingsGenerateShadowFromNormalMapLabel { get; }
        internal abstract string MatCapLitConvertSettingsMatCapTextureLabel { get; }
        internal abstract string MatCapLitConvertSettingsMatCapTextureWarning { get; }
        internal abstract string AdditionalMaterialConvertSettingsTargetMaterialLabel { get; }
        internal abstract string AdditionalMaterialConvertSettingsSelectMaterialLabel { get; }
        internal abstract string MaterialConvertTypePopupLabelToonLit { get; }
        internal abstract string MaterialConvertTypePopupLabelMatCapLit { get; }
        internal abstract string MaterialConvertTypePopupLabelMaterialReplace { get; }
        internal abstract string MaterialReplaceSettingsMaterialLabel { get; }
        internal abstract string MaterialReplaceSettingsMaterialTooltip { get; }

        // Remove Missing Components
        internal abstract string NoMissingComponentsMessage(string objectName);
        internal abstract string MissingRemoverConfirmationMessage(string objectName);
        internal abstract string UnpackPrefabMessage { get; }
        internal abstract string MissingRemoverOnBuildDialogMessage(string objectName);

        // BlendShapes Copy
        internal abstract string SourceMeshLabel { get; }
        internal abstract string TargetMeshLabel { get; }
        internal abstract string CopyButtonLabel { get; }
        internal abstract string SwitchButtonLabel { get; }

        // Remove Unsupported Components
        internal abstract string NoUnsupportedComponentsMessage(string objectName);
        internal abstract string UnsupportedRemoverConfirmationMessage(string objectName);

        // Remove PhysBones
        internal abstract string PhysBonesSDKRequired { get; }
        internal abstract string SelectComponentsToKeep { get; }
        internal abstract string PhysBonesListTooltip { get; }
        internal abstract string KeepAll { get; }
        internal abstract string AvatarDynamicsPreventsUpload { get; }
        internal abstract string PhysBonesWillBeRemovedAtRunTime { get; }
        internal abstract string PhysBoneCollidersWillBeRemovedAtRunTime { get; }
        internal abstract string ContactsWillBeRemovedAtRunTime { get; }
        internal abstract string PhysBonesTransformsShouldBeReduced { get; }
        internal abstract string PhysBonesCollisionCheckCountShouldBeReduced { get; }

        internal abstract string PhysBonesShouldHaveNetworkID { get; }
        internal abstract string EstimatedPerformanceStats { get; }
        internal abstract string DeleteUnselectedComponents { get; }

        // Avatar Dynamics Selector
        internal abstract string SelectAllButtonLabel { get; }
        internal abstract string DeselectAllButtonLabel { get; }
        internal abstract string ApplyButtonLabel { get; }

        // Metallic Smoothness
        internal abstract string TextureLabel { get; }
        internal abstract string InvertLabel { get; }
        internal abstract string SaveFileDialogTitle(string thing);
        internal abstract string SaveFileDialogMessage { get; }
        internal abstract string GenerateButtonLabel { get; }

        // Unity Settings
        internal abstract string RecommendedUnitySettingsForAndroid { get; }
        internal abstract string TextureCompressionLabel { get; }
        internal abstract string TextureCompressionHelp { get; }
        internal abstract string TextureCompressionButtonLabel { get; }
        internal abstract string ApplyAllButtonLabel { get; }
        internal abstract string ShowOnStartupLabel { get; }
        internal abstract string AllAppliedHelp { get; }

        // Check for Update
        internal abstract string GetUpdate { get; }
        internal abstract string SeeChangelog { get; }
        internal abstract string SkipThisVersion { get; }
        internal abstract string NewVersionIsAvailable(string latestVersion);
        internal abstract string NewVersionHasBreakingChanges { get; }

        // Validations
        internal abstract string MissingScripts { get; }

        // Vertex Color
        internal abstract string VertexColorRemoverEditorDescription { get; }
        internal abstract string VertexColorRemoverEditorRemove { get; }
        internal abstract string VertexColorRemoverEditorRestore { get; }

        // Converted Avatar
        internal abstract string ConvertedAvatarEditorMessage { get; }
        internal abstract string ConvertedAvatarEditorNDMFMessage { get; }

        // Network ID Assigner
        internal abstract string NetworkIDAssignerEditorDescription { get; }
        internal abstract string NetworkIDAssignerEditorAssignmentMethodLabel { get; }
        internal abstract string NetworkIDAssignerEditorAssignmentMethodHierachyHashLabel { get; }
        internal abstract string NetworkIDAssignerEditorAssignmentMethodHierachyHashTooltip { get; }
        internal abstract string NetworkIDAssignerEditorAssignmentMethodVRChatSDKLabel { get; }
        internal abstract string NetworkIDAssignerEditorAssignmentMethodVRChatSDKTooltip { get; }

        // Platform Target Settings
        internal abstract string PlatformTargetSettingsEditorDescription { get; }
        internal abstract string PlatformTargetSettingsShouldBeAttachedToAvatarRoot { get; }
        internal abstract string PlatformTargetSettingsIsRequiredToEnforcePlatform { get; }

        // Platform Component Remover
        internal abstract string ComponentRequiresNdmf { get; }
        internal abstract string BuildTargetLabel { get; }
        internal abstract string BuildTargetTooltip { get; }
        internal abstract string PlatformComponentRemoverEditorDescription { get; }
        internal abstract string PlatformComponentRemoverEditorComponentSettingsLabel { get; }
        internal abstract string PlatformComponentRemoverEditorComponentSettingsTooltip { get; }
        internal abstract string ComponentLabel { get; }

        // Platform GameObject Remover
        internal abstract string PlatformGameObjectRemoverEditorDescription { get; }
        internal abstract string PlatformGameObjectRemoverEditorRemoveOnPCLabel { get; }
        internal abstract string PlatformGameObjectRemoverEditorRemoveOnAndroidLabel { get; }

        // Avatar Builder
        internal abstract string AvatarBuilderWindowExitPlayMode { get; }
        internal abstract string AvatarBuilderWindowExitPrefabStage { get; }
        internal abstract string AvatarBuilderWindowNoActiveAvatarsFound { get; }
        internal abstract string AvatarBuilderWindowNoNdmfComponentsFound { get; }
        internal abstract string AvatarBuilderWindowSucceededBuild { get; }
        internal abstract string AvatarBuilderWindowSucceededUpload { get; }
        internal abstract string AvatarBuilderWindowFailedBuild { get; }
        internal abstract string AvatarBuilderWindowRequiresControlPanel { get; }
        internal abstract string AvatarBuilderWindowOfflineTestingLabel { get; }
        internal abstract string AvatarBuilderWindowOfflineTestingDescription(string name);
        internal abstract string AvatarBuilderWindowOnlinePublishingLabel { get; }
        internal abstract string AvatarBuilderWindowOnlinePublishingDescription { get; }
        internal abstract string AvatarBuilderWindowNdmfManualBakingLabel { get; }
        internal abstract string AvatarBuilderWindowNdmfManualBakingDescription { get; }
        internal abstract string AvatarBuilderWindowRequiresAvatarNameAndThumb { get; }

        // NDMF
        internal abstract string NdmfPluginRequiresNdmfUpdate(string requiredVersion);
        internal abstract string NdmfPluginRemovedUnsupportedComponent(string typeName, string objectName);
    }
}
