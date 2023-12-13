// <copyright file="I18nEnglish.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1516 // Elements should be separated by blank line
#pragma warning disable SA1600 // Elements should be documented

namespace KRT.VRCQuestTools.I18n
{
    /// <summary>
    /// English strings.
    /// </summary>
    internal class I18nEnglish : I18nBase
    {
        internal override string CancelLabel => "Cancel";
        internal override string CloseLabel => "Close";
        internal override string DismissLabel => "Dismiss";
        internal override string YesLabel => "Yes";
        internal override string NoLabel => "No";
        internal override string AbortLabel => "Abort";

        internal override string Maximum => "Maximum";

        // Convert Avatar for Quest
        internal override string ExitPlayModeToEdit => "Exit play mode to edit.";
        internal override string AddAvatarConverterButtonLabel(string name) => $"Add VQT Avatar Converter to {name}";
        internal override string OverwriteWarningDialogButtonCancel => "Cancel";
        internal override string OverwriteWarningDialogButtonOK => "OK";
        internal override string OverwriteWarningDialogButtonUseAltDir(string altDir) => $"Use \"{altDir}\"";
        internal override string OverwriteWarningDialogMessage(string artifactsDir) => $"\"{artifactsDir}\" already exists. Do you want to overwrite?";
        internal override string OverwriteWarningDialogTitle => $"{VRCQuestTools.Name} Warning";
        internal override string AvatarLabel => "Avatar";
        internal override string GenerateQuestTexturesLabel => "Generate Textures for Quest";
        internal override string QuestTexturesDescription => "By generating new textures which applying material's parameters not only main textures, get closer to PC version of the avatar";
        internal override string SupportedShadersLabel => "Supported Shaders";
        internal override string SaveToLabel => "Folder to Save";
        internal override string InvalidCharsInOutputPath => "Folder name has invalid characters.";
        internal override string SelectButtonLabel => "Select";
        internal override string ConvertButtonLabel => "Convert";
        internal override string AssignButtonLabel => "Assign";
        internal override string UpdateTexturesLabel => "Update Only Quest Textures";
        internal override string AdvancedConverterSettingsLabel => "Advanced Converter Settings";
        internal override string RemoveVertexColorLabel => "Remove Vertex Color from Meshes";
        internal override string RemoveVertexColorTooltip => "Usually you don't have to disable this option. When you are using special shaders which require vertex colors in PC avatars, you can disable this option to prevent unexpected behavior.\nIf vertex color is accidentally removed, restore from the avatar's \"VertexColorRemover\" component.";
        internal override string AnimationOverrideLabel => "Animation Override";
        internal override string AnimationOverrideTooltip => "Convert Animator Controllers with Animator Override Controller's animations.";
        internal override string AnimationOverrideMaterialErrorMessage => "Animator Override Controllers contain animated materials which uses unsupported shaders for Quest.";
        internal override string ConvertingMaterialsDialogMessage => "Converting materials...";
        internal override string GeneratingTexturesDialogMessage => "Generating textures...";
        internal override string MaterialExceptionDialogMessage => "An error occured when converting materials. Aborted.";
        internal override string AnimationClipExceptionDialogMessage => "An error occured when converting Animation Clips. Aborted.";
        internal override string AnimatorControllerExceptionDialogMessage => "An error occured when converting Animator Controllers. Aborted.";
        internal override string WarningForPerformance => $"{VRCQuestTools.Name} never optimize performance rank such as polygon reduction. In most cases, the converted avatar's performance rank will be \"Very Poor\" for Quest.\n- Quest users see your fallback avatar by default, and need to change \"Avatar Display\" setting.\n- You can't use and see Very Poor avatars on Android smartphone.";
        internal override string WarningForAppearance => "Texture's transparency doesn't make any effects, so this will be an issue for facial expression. In this case, please take steps by yourself (for example, by editing animation clips or deleting problematic meshes).\n\n" +
            "You should check converted avatar's appearance on PC by uploading with another Blueprint ID or using Avatars 3.0 local testing.";
        internal override string WarningForUnsupportedShaders => $"Following materials are using unsupported shaders. Textures might not properly be generated.\nDisabling \"{GenerateQuestTexturesLabel}\" option changes only shader.";
        internal override string AlertForComponents => "Following unsupported components will be removed. Check avatar features after conversion.";
        internal override string AlertForMaterialAnimation => "There are Animation clips which change avatar's materials. Animator Controllers and Animation clips will be duplicated then converted for Quest.";
        internal override string AlertForDynamicBoneConversion => $"{VRCQuestTools.Name} doesn't convert Dynamic Bones to PhysBones. Please set up PhysBones before converting the avatar.";
        internal override string AlertForMissingNetIds => "There are PhysBones which don't have Network ID. To keep sync between PC and Quest, assign Network IDs then re-upload the PC avatar.";
        internal override string AlertForAvatarDynamicsPerformance => "Avatar Dynamics components exceed \"Poor\" limits (Very Poor). Please keep \"Poor\" rating by removing them.";
        internal override string CompletedDialogMessage(string originalName) => $"{originalName} has been converted for Quest.\nTest your avatar such as facial expression then upload it for Android platform by using same Blueprint ID as PC version.";

        internal override string AvatarConverterMustBeOnAvatarRoot => "This component must be attached to VRC_AvatarDescriptor GameObject.";
        internal override string AvatarConverterMaterialConvertSettingLabel => "Material Conversion Setting";
        internal override string AvatarConverterDefaultMaterialConvertSettingLabel => "Default Material Conversion Settings";
        internal override string AvatarConverterAdditionalMaterialConvertSettingsLabel => "Additional Material Conversion Settings";

        internal override string AvatarConverterAvatarDynamicsSettingLabel => "Avatar Dynamics Setting";
        internal override string AvatarConverterPhysBonesTooltip => "Set PhysBones to keep while conversion.";
        internal override string AvatarConverterPhysBoneCollidersTooltip => "Set PhysBoneColliders to keep while conversion.";
        internal override string AvatarConverterContactsTooltip => "Set ContactSenders and ContactReceivers to keep while conversion.";

        // IMaterialConvertSettings
        internal override string IMaterialConvertSettingsTexturesSizeLimitLabel => "Textures Size Limit";
        internal override string IMaterialConvertSettingsMainTextureBrightnessLabel => "Main Texture Brightness";
        internal override string IMaterialConvertSettingsMainTextureBrightnessTooltip => "Tweak main texture color.";
        internal override string ToonLitConvertSettingsGenerateShadowFromNormalMapLabel => "Generate shadow from normal map";
        internal override string MatCapLitConvertSettingsMatCapTextureLabel => "MatCap Texture";
        internal override string AdditionalMaterialConvertSettingsTargetMaterialLabel => "Target Material";
        internal override string AdditionalMaterialConvertSettingsSelectMaterialLabel => "Select Material";
        internal override string MaterialConvertTypePopupLabelToonLit => "Toon Lit";
        internal override string MaterialConvertTypePopupLabelMatCapLit => "MatCap Lit";
        internal override string MaterialConvertTypePopupLabelMaterialReplace => "Material Replacement";
        internal override string MaterialReplaceSettingsMaterialLabel => "Replaced Material";
        internal override string MaterialReplaceSettingsMaterialTooltip => "Target material will be replaced by this material.";

        // Remove Missing Components
        internal override string NoMissingComponentsMessage(string objectName) => $"There are no \"missing\" components in {objectName}.";
        internal override string MissingRemoverConfirmationMessage(string objectName) => $"Remove \"missing\" components from {objectName}.";
        internal override string UnpackPrefabMessage => "This also executes \"Unpack Prefab\" operation.";
        internal override string MissingRemoverOnBuildDialogMessage(string objectName) => $"{objectName} has \"missing\" components. Would you like to continue building the avatar without such components?";

        // BlendShapes Copy
        internal override string SourceMeshLabel => "Source Mesh";
        internal override string TargetMeshLabel => "Target Mesh";
        internal override string CopyButtonLabel => "Copy BlendShape Weights";
        internal override string SwitchButtonLabel => "Switch Source/Target";

        // Remove Unsupported Components
        internal override string NoUnsupportedComponentsMessage(string objectName) => $"There are no unsupported components in {objectName}.";
        internal override string UnsupportedRemoverConfirmationMessage(string objectName) => $"Remove following unsupported components from {objectName}.";

        // Remove PhysBones
        internal override string PhysBonesSDKRequired => "VRCSDK which supports Avatar Dynamics is required.";
        internal override string SelectComponentsToKeep => "Select components to keep.";
        internal override string PhysBonesListTooltip => "The list of components and their root transforms.";
        internal override string KeepAll => "Keep All";
        internal override string AvatarDynamicsPreventsUpload => "You can't upload this avatar for Quest. At least Avatar Dynamics must keep \"Poor\" rating.";
        internal override string PhysBonesWillBeRemovedAtRunTime => "You can't upload this avatar for Quest. Please reduce PhysBone components.";
        internal override string PhysBoneCollidersWillBeRemovedAtRunTime => "All PhysBone colliders will be removed at runtime on Quest. Please reduce PhysBoneCollider components.";
        internal override string ContactsWillBeRemovedAtRunTime => "You can't upload this avatar for Quest. Please reduce VRCContact components.";
        internal override string PhysBonesTransformsShouldBeReduced => "You can't upload this avatar for Quest. Please reduce VRCPhysBone components or number of transforms in hierarchy under VRCPhysBone components.";

        internal override string PhysBonesCollisionCheckCountShouldBeReduced => "You can't upload this avatar for Quest. Please reduce collision check count between VRCPhysBone components and VRCPhysBoneCollider components.";

        internal override string PhysBonesOrderMustMatchWithPC => "To properly synchronize PhysBones, the order of PhysBones' Network IDs must match with the PC avatar. Please select PhysBones from the top of the list.";
        internal override string PhysBonesShouldHaveNetworkID => "To properly synchronize PhysBones, PhysBones must have same Network ID between PC and Quest. Please assign Network IDs to both of PC version and Quest version with Network ID Utility of VRCSDK, then re-upload both.";
        internal override string AlertForMultiplePhysBones => "There are multiple PhysBones in a single GameObject. When removing PhysBones for Quest after conversion, they may not be properly synchronized between PC and Quest.";
        internal override string EstimatedPerformanceStats => "Estimated Performance Stats";
        internal override string DeleteUnselectedComponents => "Delete Unselected Components";

        // Avatar Dynamics Selector
        internal override string SelectAllButtonLabel => "Select All";
        internal override string DeselectAllButtonLabel => "Deselect All";
        internal override string ApplyButtonLabel => "Apply";

        // Metallic Smoothness
        internal override string TextureLabel => "Texture";
        internal override string InvertLabel => "Invert";
        internal override string SaveFileDialogTitle(string thing) => $"Save {thing}";
        internal override string SaveFileDialogMessage => "Please enter a file name to save the texture to";
        internal override string GenerateButtonLabel => "Generate Metallic Smoothness";

        // Unity Settings
        internal override string CacheServerModeLabel => "Cache Server Mode";
        internal override string CacheServerHelp => "By enabling the local cache server, you can save time for such as texture compression from the next. In default preferences, the server takes 10 GB from C drive at maximum.";
        internal override string CacheServerButtonLabel => "Enable Local Cache Server";
        internal override string TextureCompressionLabel => "Android Texture Compression";
        internal override string TextureCompressionHelp => "ASTC improves Quest texture quality in exchange for long compression time";
        internal override string TextureCompressionButtonLabel => "Set texture compression to ASTC";
        internal override string ApplyAllButtonLabel => "Apply All Settings";
        internal override string ShowOnStartupLabel => "Show on startup";
        internal override string AllAppliedHelp => "OK, all recommended settings are applied.";

        // Check for Update
        internal override string CheckLater => "Check Later";
        internal override string GetUpdate => "Get Update";
        internal override string SeeChangelog => "Changelog";
        internal override string SkipThisVersion => "Skip This";
        internal override string NewVersionIsAvailable(string latestVersion) => $"New version {latestVersion} is available.";
        internal override string NewVersionHasBreakingChanges => $"This version might have breaking changes about compatibility.";
        internal override string ThereIsNoUpdate => "There is no update.";

        // Validations
        internal override string DeactivateAvatar => "Deactivate avatar";
        internal override string IncompatibleForQuest => "On Android build target, you can't upload Quest avatars because this avatar can't be uploaded for Quest. Please deactivate such avatars or switch platfrom back to PC.";
        internal override string MissingScripts => "There are \"missing\" scripts. Please check for assets or packages you forgot to import.";
        internal override string ValidatorAlertsProhibitedShaders(string shaderName, string[] materialNames) =>
            $"Shader \"{shaderName}\" (Materials: {string.Join(", ", materialNames)}) is not allowed for Quest.";
        internal override string ValidatorAlertsUnsupportedComponents(string componentName, string objectName) =>
            $"Component \"{componentName}\" ({objectName}) is not allowed for Quest.";
        internal override string ValidatorAlertsVeryPoorPhysBones(int count) =>
            $"Too many PhysBones: {count} (Very Poor).";
        internal override string ValidatorAlertsVeryPoorPhysBoneColliders(int count) =>
            $"Too many PhysBoneColliders: {count} (Very Poor).";
        internal override string ValidatorAlertsVeryPoorContacts(int count) =>
            $"Too many ContactSenders and ContactReceivers: {count} (Very Poor).";

        // Vertex Color
        internal override string VertexColorRemoverEditorRemove => "Remove Vertex Color";
        internal override string VertexColorRemoverEditorRestore => "Restore Vertex Color";
        internal override string VertexColorRemoverDialogTitle => $"Remove Vertex Color - {VRCQuestTools.Name}";
        internal override string VertexColorRemoverDialogMessage(string name) =>
            $"\"{name}\" has vertex colors in its meshes. Would you like to remove vertex colors to properly apply texture color?\n\nUsually you can choose \"{YesLabel}\". If you are using special shaders which require vertex color for PC avatars, you can choose \"{NoLabel}\"";
    }
}
