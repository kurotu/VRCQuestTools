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
        internal abstract string CloseLabel { get; }
        internal abstract string DismissLabel { get; }
        internal abstract string YesLabel { get; }
        internal abstract string NoLabel { get; }
        internal abstract string AbortLabel { get; }

        internal abstract string Maximum { get; }

        // Convert Avatar for Quest
        internal abstract string OverwriteWarningDialogTitle { get; }
        internal abstract string OverwriteWarningDialogMessage(string artifactsDir);
        internal abstract string OverwriteWarningDialogButtonOK { get; }
        internal abstract string OverwriteWarningDialogButtonCancel { get; }
        internal abstract string OverwriteWarningDialogButtonUseAltDir(string altDir);
        internal abstract string AvatarLabel { get; }
        internal abstract string GenerateQuestTexturesLabel { get; }
        internal abstract string QuestTexturesDescription { get; }
        internal abstract string SupportedShadersLabel { get; }
        internal abstract string SaveToLabel { get; }
        internal abstract string InvalidCharsInOutputPath { get; }
        internal abstract string SelectButtonLabel { get; }
        internal abstract string ConvertButtonLabel { get; }
        internal abstract string AssignButtonLabel { get; }
        internal abstract string UpdateTexturesLabel { get; }
        internal abstract string AdvancedConverterSettingsLabel { get; }
        internal abstract string RemoveVertexColorLabel { get; }
        internal abstract string RemoveVertexColorTooltip { get; }
        internal abstract string AnimationOverrideLabel { get; }
        internal abstract string AnimationOverrideTooltip { get; }
        internal abstract string AnimationOverrideMaterialErrorMessage { get; }
        internal abstract string ConvertingMaterialsDialogMessage { get; }
        internal abstract string GeneratingTexturesDialogMessage { get; }
        internal abstract string MaterialExceptionDialogMessage { get; }
        internal abstract string AnimationClipExceptionDialogMessage { get; }
        internal abstract string AnimatorControllerExceptionDialogMessage { get; }
        internal abstract string WarningForPerformance { get; }
        internal abstract string WarningForAppearance { get; }
        internal abstract string WarningForUnsupportedShaders { get; }
        internal abstract string TexturesSizeLimitLabel { get; }
        internal abstract string MainTextureBrightnessLabel { get; }
        internal abstract string MainTextureBrightnessTooltip { get; }
        internal abstract string AlertForDynamicBoneConversion { get; }
        internal abstract string AlertForMissingNetIds { get; }
        internal abstract string AlertForMultiplePhysBones { get; }
        internal abstract string AlertForAvatarDynamicsPerformance { get; }
        internal abstract string AlertForComponents { get; }
        internal abstract string AlertForMaterialAnimation { get; }
        internal abstract string CompletedDialogMessage(string originalName);

        internal abstract string AvatarConverterMustBeChildrenOfAvatar { get; }
        internal abstract string AvatarConverterMaterialConvertSettingLabel { get; }
        internal abstract string AvatarConverterAvatarDynamicsSettingLabel { get; }
        internal abstract string AvatarConverterPhysBonesTooltip { get; }
        internal abstract string AvatarConverterPhysBoneCollidersTooltip { get; }
        internal abstract string AvatarConverterContactsTooltip { get; }

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

        internal abstract string PhysBonesOrderMustMatchWithPC { get; }
        internal abstract string PhysBonesShouldHaveNetworkID { get; }
        internal abstract string EstimatedPerformanceStats { get; }
        internal abstract string DeleteUnselectedComponents { get; }

        // Metallic Smoothness
        internal abstract string TextureLabel { get; }
        internal abstract string InvertLabel { get; }
        internal abstract string SaveFileDialogTitle(string thing);
        internal abstract string SaveFileDialogMessage { get; }
        internal abstract string GenerateButtonLabel { get; }

        // Unity Settings
        internal abstract string CacheServerModeLabel { get; }
        internal abstract string CacheServerHelp { get; }
        internal abstract string CacheServerButtonLabel { get; }
        internal abstract string TextureCompressionLabel { get; }
        internal abstract string TextureCompressionHelp { get; }
        internal abstract string TextureCompressionButtonLabel { get; }
        internal abstract string ApplyAllButtonLabel { get; }
        internal abstract string ShowOnStartupLabel { get; }
        internal abstract string AllAppliedHelp { get; }

        // Check for Update
        internal abstract string CheckLater { get; }
        internal abstract string GetUpdate { get; }
        internal abstract string SeeChangelog { get; }
        internal abstract string SkipThisVersion { get; }
        internal abstract string NewVersionIsAvailable(string latestVersion);
        internal abstract string NewVersionHasBreakingChanges { get; }
        internal abstract string ThereIsNoUpdate { get; }

        // Validations
        internal abstract string DeactivateAvatar { get; }
        internal abstract string IncompatibleForQuest { get; }
        internal abstract string MissingScripts { get; }
        internal abstract string ValidatorAlertsProhibitedShaders(string shaderName, string[] materialNames);
        internal abstract string ValidatorAlertsUnsupportedComponents(string componentName, string objectName);
        internal abstract string ValidatorAlertsVeryPoorPhysBones(int count);
        internal abstract string ValidatorAlertsVeryPoorPhysBoneColliders(int count);
        internal abstract string ValidatorAlertsVeryPoorContacts(int count);

        // Vertex Color
        internal abstract string VertexColorRemoverEditorRemove { get; }
        internal abstract string VertexColorRemoverEditorRestore { get; }
        internal abstract string VertexColorRemoverDialogTitle { get; }
        internal abstract string VertexColorRemoverDialogMessage(string name);
    }
}
