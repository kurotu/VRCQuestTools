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
        internal override string OpenLabel => "Open";
        internal override string CloseLabel => "Close";
        internal override string DismissLabel => "Dismiss";
        internal override string YesLabel => "Yes";
        internal override string NoLabel => "No";
        internal override string AbortLabel => "Abort";
        internal override string RemoveLabel => "Remove";

        internal override string Maximum => "Maximum";

        internal override string IncompatibleSDK => "Incompatible VRChat SDK. Please report.";

        // Convert Avatar for Quest
        internal override string AvatarConverterSettingsEditorDescription => "Convert this avatar uploadable for Android platform.";
        internal override string AvatarConverterSettingsEditorDescriptionNDMF => "Convert this avatar uploadable for Android platform when the target platform is Android.";
        internal override string ExitPlayModeToEdit => "Exit play mode to edit.";
        internal override string BeingConvertSettingsButtonLabel => $"Begin Converter Settings";
        internal override string AvatarLabel => "Avatar";
        internal override string GenerateAndroidTexturesLabel => "Generate Textures for Android";
        internal override string GenerateAndroidTexturesTooltip => "By generating new textures which applying material's parameters not only main textures, get closer to PC version of the avatar";
        internal override string SupportedShadersLabel => "Supported Shaders";
        internal override string SaveToLabel => "Folder to Save";
        internal override string SelectButtonLabel => "Select";
        internal override string ConvertButtonLabel => "Convert";
        internal override string AssignButtonLabel => "Assign";
        internal override string AttachButtonLabel => "Attach";
        internal override string UpdateTexturesLabel => "Update Converted Android Textures";
        internal override string AdvancedConverterSettingsLabel => "Advanced Converter Settings";
        internal override string RemoveVertexColorLabel => "Remove Vertex Color from Meshes";
        internal override string RemoveVertexColorTooltip => "Usually you don't have to disable this option. You can disable this option to prevent unexpected behavior when you are using special shaders which require vertex colors in PC avatars.\nRestore from the avatar's \"VertexColorRemover\" component if vertex color is accidentally removed.";
        internal override string AnimationOverrideLabel => "Animation Override";
        internal override string AnimationOverrideTooltip => "Convert Animator Controllers with Animator Override Controller's animations.";
        internal override string GeneratingTexturesDialogMessage => "Generating textures...";
        internal override string MaterialExceptionDialogMessage => "An error occured when converting materials. Aborted.";
        internal override string AnimationClipExceptionDialogMessage => "An error occured when converting Animation Clips. Aborted.";
        internal override string AnimatorControllerExceptionDialogMessage => "An error occured when converting Animator Controllers. Aborted.";
        internal override string InfoForNdmfConversion => "You can non-destructively convert the avatar at build-time when the project has Non-Destructive Modular Framework (NDMF) package. Please use the Avatar Builder to avoid VRChat SDK limitation.";
        internal override string InfoForNetworkIdAssigner => "You can non-destructively assign Network IDs by attaching Network ID Assigner component to the avatar root object. Please upload again to apply IDs after attaching the component.";
        internal override string WarningForPerformance => $"Estimated performance rating is Very Poor. You can upload the converted avatar for Android platform, but there are following limitation.\n- Quest/PICO users see your fallback avatar by default, then need to change \"Avatar Display\" setting.\n- Android smartphone users can't see Very Poor avatars.";
        internal override string WarningForAppearance => "Texture's transparency doesn't make any effects, so this will be an issue for facial expression. In this case, please take steps by yourself (for example, by editing animation clips or deleting problematic meshes).\n" +
            "You should check converted avatar's appearance on PC by uploading with another Blueprint ID or using Avatars 3.0 local testing.";
        internal override string WarningForUnsupportedShaders => $"Following materials are using unsupported shaders. Textures might not properly be generated.\nDisabling \"{GenerateAndroidTexturesLabel}\" option changes only shader.";
        internal override string AlertForComponents => "Following unsupported components will be removed. Check avatar features after conversion.";
        internal override string AlertForDynamicBoneConversion => $"{VRCQuestTools.Name} doesn't convert Dynamic Bones to PhysBones. Please set up PhysBones before converting the avatar.";
        internal override string AlertForUnityConstraintsConversion => $"{VRCQuestTools.Name} doesn't convert Unity constraints to VRChat constraints. Please set up VRChat constraints before converting the avatar.";
        internal override string AlertForMissingNetIds => "There are PhysBones which don't have Network ID. To keep sync between PC and Android, assign Network IDs then re-upload the PC avatar.";
        internal override string AlertForAvatarDynamicsPerformance => "Avatar Dynamics (PhysBones and Contacts) performance rating will be \"Very Poor\", so you will not able to upload for Android.  Please keep \"Poor\" rating in avatar dynamics categories.";

        internal override string AvatarConverterMaterialConvertSettingsLabel => "Material Conversion Settings";
        internal override string AvatarConverterDefaultMaterialConvertSettingLabel => "Default Material Conversion Settings";
        internal override string AvatarConverterAdditionalMaterialConvertSettingsLabel => "Additional Material Conversion Settings";

        internal override string AvatarConverterAvatarDynamicsSettingsLabel => "Avatar Dynamics Settings";
        internal override string AvatarConverterPhysBonesTooltip => "Set PhysBones to keep while conversion.";
        internal override string AvatarConverterPhysBoneCollidersTooltip => "Set PhysBoneColliders to keep while conversion.";
        internal override string AvatarConverterContactsTooltip => "Set ContactSenders and ContactReceivers to keep while conversion.";

        // IMaterialConvertSettings
        internal override string IMaterialConvertSettingsTexturesSizeLimitLabel => "Textures Size Limit";
        internal override string IMaterialConvertSettingsMainTextureBrightnessLabel => "Main Texture Brightness";
        internal override string IMaterialConvertSettingsMainTextureBrightnessTooltip => "Tweak main texture color.";
        internal override string ToonLitConvertSettingsGenerateShadowFromNormalMapLabel => "Generate shadow from normal map";
        internal override string MatCapLitConvertSettingsMatCapTextureLabel => "MatCap Texture";
        internal override string MatCapLitConvertSettingsMatCapTextureWarning => "Set MatCap texture.";
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
        internal override string AvatarDynamicsPreventsUpload => "You can't upload this avatar for Android. At least Avatar Dynamics must keep \"Poor\" rating.";
        internal override string PhysBonesWillBeRemovedAtRunTime => "You can't upload this avatar for Android. Please reduce PhysBone components.";
        internal override string PhysBoneCollidersWillBeRemovedAtRunTime => "All PhysBone colliders will be removed at runtime on Android. Please reduce PhysBoneCollider components.";
        internal override string ContactsWillBeRemovedAtRunTime => "You can't upload this avatar for Android. Please reduce VRCContact components.";
        internal override string PhysBonesTransformsShouldBeReduced => "You can't upload this avatar for Android. Please reduce VRCPhysBone components or number of transforms in hierarchy under VRCPhysBone components.";

        internal override string PhysBonesCollisionCheckCountShouldBeReduced => "You can't upload this avatar for Android. Please reduce collision check count between VRCPhysBone components and VRCPhysBoneCollider components.";

        internal override string PhysBonesShouldHaveNetworkID => "To properly synchronize PhysBones, PhysBones must have same Network ID between PC and Android. Please assign Network IDs to both of PC version and Android version with Network ID Utility of VRCSDK, then re-upload both.";
        internal override string AlertForMultiplePhysBones => "There are multiple PhysBones in a single GameObject. They may not be properly synchronized between PC and Android after removing PhysBones.";
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
        internal override string RecommendedUnitySettingsForAndroid => "Recommended Settings for Android";
        internal override string TextureCompressionLabel => "Android Texture Compression";
        internal override string TextureCompressionHelp => "ASTC improves Android texture quality in exchange for long compression time";
        internal override string TextureCompressionButtonLabel => "Set texture compression to ASTC";
        internal override string ApplyAllButtonLabel => "Apply All Settings";
        internal override string ShowOnStartupLabel => "Show on startup";
        internal override string AllAppliedHelp => "OK, all recommended settings are applied.";

        // Check for Update
        internal override string GetUpdate => "Get Update";
        internal override string SeeChangelog => "Changelog";
        internal override string SkipThisVersion => "Skip This";
        internal override string NewVersionIsAvailable(string latestVersion) => $"New version {latestVersion} is available.";
        internal override string NewVersionHasBreakingChanges => $"This version might have breaking changes about compatibility.";

        // Validations
        internal override string MissingScripts => "There are \"missing\" scripts. Please check for assets or packages you forgot to import.";

        // Inspector
        internal override string AvatarRootComponentMustBeOnAvatarRoot => "This component must be attached to VRC_AvatarDescriptor GameObject.";

        // Vertex Color
        internal override string VertexColorRemoverEditorDescription => "Vertex color is automatically removed from this GameObject's mesh.";
        internal override string VertexColorRemoverEditorRemove => "Remove Vertex Color";
        internal override string VertexColorRemoverEditorRestore => "Restore Vertex Color";

        // Converted Avatar
        internal override string ConvertedAvatarEditorMessage => "This component indicates the avatar was converted by VRCQuestTools.";
        internal override string ConvertedAvatarEditorNDMFMessage => "Components which are not supported on Android will be removed in NDMF optimization phase.";

        // Network ID Assigner
        internal override string NetworkIDAssignerEditorDescription => "Assign Network IDs to the avatar's components such as PhysBones. IDs are determined by hash values of hierarchy paths from the avatar root.";

        // Platform Target Settings
        internal override string PlatformTargetSettingsEditorDescription => "Components are enforced to use a specific platform settings in NDMF build process.";
        internal override string PlatformTargetSettingsIsRequiredToEnforcePlatform => "PlatformTargetSettings component is required to the avatar root object in order to enforce a specific platform settings.";

        // Platform Component Remover
        internal override string ComponentRequiresNdmf => "Non-Destructive Modular Framework (NDMF) is required.";
        internal override string BuildTargetLabel => "Build Target";
        internal override string BuildTargetTooltip => "Select build target platform. Use Unity's target platform when Auto is selected.";
        internal override string PlatformComponentRemoverEditorDescription => "Components will be removed on build for unselected build targets.";
        internal override string PlatformComponentRemoverEditorComponentSettingsLabel => "Components to Keep";
        internal override string PlatformComponentRemoverEditorComponentSettingsTooltip => "Select platform to keep components.";
        internal override string PlatformComponentRemoverEditorCheckboxPCTooltip => "Keep selected components on PC build.";
        internal override string PlatformComponentRemoverEditorCheckboxAndroidTooltip => "Keep selected components on Android build.";
        internal override string ComponentLabel => "Component";

        // Platform GameObject Remover
        internal override string PlatformGameObjectRemoverEditorDescription => "This GameObject will be removed on build for unselected build targets.";
        internal override string PlatformGameObjectRemoverEditorKeepOnPCLabel => "Keep on PC";
        internal override string PlatformGameObjectRemoverEditorKeepOnAndroidLabel => "Keep on Android";

        // Avatar Builder
        internal override string AvatarBuilderWindowExitPlayMode => "Exit play mode to build the avatar.";
        internal override string AvatarBuilderWindowExitPrefabStage => "Exit prefab stage to build the avatar.";
        internal override string AvatarBuilderWindowNoActiveAvatarsFound => "No active avatars in the scene.";
        internal override string AvatarBuilderWindowSelectAvatar => "Select an avatar to build in VRChat SDK Control Panel.";
        internal override string AvatarBuilderWindowNoNdmfComponentsFound => "No VRCQuestTools components found in the avatar.";

        internal override string AvatarBuilderWindowSucceededBuild => "Succeeded to build the avatar.";
        internal override string AvatarBuilderWindowSucceededUpload => "Succeeded to upload the avatar.";
        internal override string AvatarBuilderWindowFailedBuild => "Failed to build the avatar. See console logs.";
        internal override string AvatarBuilderWindowRequiresControlPanel => "Need to open VRChat SDK Control Panel to build the avatar.";
        internal override string AvatarBuilderWindowOfflineTestingLabel => "Offline Testing (PC)";
        internal override string AvatarBuilderWindowOfflineTestingDescription(string name) => $"Build this avatar with Android build setting to test it on PC. The avatar will appear as \"SDK {name}\" in the Other category of the avatars menu.";
        internal override string AvatarBuilderWindowOnlinePublishingLabel => "Online Publishing (Android)";
        internal override string AvatarBuilderWindowOnlinePublishingDescription => "Upload this avatar to VRChat with Android build setting. The pre-build validation of VRChat SDK Control Panel will be skipped even if the avatar doesn't satisfy Android limitation.";
        internal override string AvatarBuilderWindowNdmfManualBakingLabel => "NDMF Manual Baking";
        internal override string AvatarBuilderWindowNdmfManualBakingDescription => "Execute the menu, Tools -> NDM Framework -> Manual bake avatar with Android build setting.";
        internal override string AvatarBuilderWindowRequiresAvatarNameAndThumb => "Need to set name and thumbnail in VRChat SDK Control Panel when uploading a new avatar.";

        // NDMF
        internal override string NdmfPluginRequiresNdmfUpdate(string version) => $"Non-Destructive Modular Framework (NDMF) {version} or later is required. Please update NDMF.";
        internal override string NdmfPluginRemovedUnsupportedComponent(string typeName, string objectName) => $"Removed unsupported component \"{typeName}\" from \"{objectName}\". Please test the avatar.";
    }
}
