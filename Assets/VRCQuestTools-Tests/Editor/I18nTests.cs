// <copyright file="I18nTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.I18n;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="I18nBase"/> and its subclasses.
    /// </summary>
    public class I18nTests
    {
        /// <summary>
        /// Test I18n.GetI18n returns non-null instance.
        /// </summary>
        [Test]
        public void GetI18n_ReturnsNonNull()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotNull(i18n);
        }

        /// <summary>
        /// Test common UI labels are non-empty.
        /// </summary>
        [Test]
        public void CommonLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.CancelLabel);
            Assert.IsNotEmpty(i18n.OpenLabel);
            Assert.IsNotEmpty(i18n.CloseLabel);
            Assert.IsNotEmpty(i18n.DismissLabel);
            Assert.IsNotEmpty(i18n.ExitLabel);
            Assert.IsNotEmpty(i18n.YesLabel);
            Assert.IsNotEmpty(i18n.NoLabel);
            Assert.IsNotEmpty(i18n.AbortLabel);
            Assert.IsNotEmpty(i18n.AddLabel);
            Assert.IsNotEmpty(i18n.RemoveLabel);
            Assert.IsNotEmpty(i18n.FixLabel);
            Assert.IsNotEmpty(i18n.Maximum);
        }

        /// <summary>
        /// Test avatar converter labels are non-empty.
        /// </summary>
        [Test]
        public void AvatarConverterLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.AvatarConverterSettingsEditorDescription);
            Assert.IsNotEmpty(i18n.AvatarConverterSettingsEditorDescriptionNDMF);
            Assert.IsNotEmpty(i18n.ExitPlayModeToEdit);
            Assert.IsNotEmpty(i18n.BeginConvertSettingsButtonLabel);
            Assert.IsNotEmpty(i18n.BeginConvertSettingsButtonDescription);
            Assert.IsNotEmpty(i18n.AvatarLabel);
            Assert.IsNotEmpty(i18n.GenerateMobileTexturesLabel);
            Assert.IsNotEmpty(i18n.GenerateMobileTexturesTooltip);
            Assert.IsNotEmpty(i18n.SupportedShadersLabel);
            Assert.IsNotEmpty(i18n.SaveToLabel);
            Assert.IsNotEmpty(i18n.SelectButtonLabel);
            Assert.IsNotEmpty(i18n.ConvertButtonLabel);
            Assert.IsNotEmpty(i18n.AssignButtonLabel);
            Assert.IsNotEmpty(i18n.AttachButtonLabel);
            Assert.IsNotEmpty(i18n.UpdateTexturesLabel);
            Assert.IsNotEmpty(i18n.AdvancedConverterSettingsLabel);
            Assert.IsNotEmpty(i18n.RemoveVertexColorLabel);
            Assert.IsNotEmpty(i18n.RemoveVertexColorTooltip);
            Assert.IsNotEmpty(i18n.RemoveExtraMaterialSlotsLabel);
            Assert.IsNotEmpty(i18n.RemoveExtraMaterialSlotsTooltip);
            Assert.IsNotEmpty(i18n.CompressExpressionsMenuIconsLabel);
            Assert.IsNotEmpty(i18n.CompressExpressionsMenuIconsTooltip);
            Assert.IsNotEmpty(i18n.AnimationOverrideLabel);
            Assert.IsNotEmpty(i18n.AnimationOverrideTooltip);
            Assert.IsNotEmpty(i18n.NdmfPhaseLabel);
            Assert.IsNotEmpty(i18n.NdmfPhaseTooltip);
            Assert.IsNotEmpty(i18n.EnableMaterialPreviewLabel);
            Assert.IsNotEmpty(i18n.EnableMaterialPreviewTooltip);
        }

        /// <summary>
        /// Test dialog messages are non-empty.
        /// </summary>
        [Test]
        public void DialogMessages_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.GeneratingTexturesDialogMessage);
            Assert.IsNotEmpty(i18n.AvatarConverterFailedDialogMessage);
            Assert.IsNotEmpty(i18n.MaterialExceptionDialogMessage);
            Assert.IsNotEmpty(i18n.AnimationClipExceptionDialogMessage);
            Assert.IsNotEmpty(i18n.AnimatorControllerExceptionDialogMessage);
            Assert.IsNotEmpty(i18n.InvalidReplacementMaterialExceptionDialogMessage);
            Assert.IsNotEmpty(i18n.InfoForNdmfConversion);
            Assert.IsNotEmpty(i18n.InfoForNdmfConversion2);
            Assert.IsNotEmpty(i18n.InfoForNetworkIdAssigner);
            Assert.IsNotEmpty(i18n.NetworkIdAssignerAttached);
        }

        /// <summary>
        /// Test warning and alert messages are non-empty.
        /// </summary>
        [Test]
        public void WarningAndAlertMessages_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.IncompatibleSDK);
            Assert.IsNotEmpty(i18n.WarningForPerformance);
            Assert.IsNotEmpty(i18n.WarningForAppearance);
            Assert.IsNotEmpty(i18n.WarningForUnsupportedShaders);
            Assert.IsNotEmpty(i18n.AlertForDynamicBoneConversion);
            Assert.IsNotEmpty(i18n.AlertForMAConvertConstraints);
            Assert.IsNotEmpty(i18n.AlertForUnityConstraintsConversion);
            Assert.IsNotEmpty(i18n.AlertForMultiplePhysBones);
            Assert.IsNotEmpty(i18n.AlertForAvatarDynamicsPerformance);
            Assert.IsNotEmpty(i18n.AlertForComponents);
            Assert.IsNotEmpty(i18n.ErrorForPrefabStage);
            Assert.IsNotEmpty(i18n.ExitPrefabStageButtonLabel);
            Assert.IsNotEmpty(i18n.ManualConversionWarning);
        }

        /// <summary>
        /// Test material convert settings labels are non-empty.
        /// </summary>
        [Test]
        public void MaterialConvertSettingsLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.AvatarConverterMaterialConvertSettingsLabel);
            Assert.IsNotEmpty(i18n.AvatarConverterDefaultMaterialConvertSettingLabel);
            Assert.IsNotEmpty(i18n.AvatarConverterAdditionalMaterialConvertSettingsLabel);
            Assert.IsNotEmpty(i18n.IMaterialConvertSettingsTexturesSizeLimitLabel);
            Assert.IsNotEmpty(i18n.IMaterialConvertSettingsMobileTextureFormatLabel);
            Assert.IsNotEmpty(i18n.IMaterialConvertSettingsMainTextureBrightnessLabel);
            Assert.IsNotEmpty(i18n.IMaterialConvertSettingsMainTextureBrightnessTooltip);
            Assert.IsNotEmpty(i18n.ToonLitConvertSettingsGenerateShadowFromNormalMapLabel);
            Assert.IsNotEmpty(i18n.MatCapLitConvertSettingsMatCapTextureLabel);
            Assert.IsNotEmpty(i18n.MatCapLitConvertSettingsMatCapTextureWarning);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsGenerateShadowRampLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsFallbackShadowRampLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsCustomFallbackShadowRampLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsFeaturesLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsFeaturesNormalMapLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsFeaturesEmissionLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsFeaturesOcclusionLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsFeaturesSpecularLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsFeaturesMatcapLabel);
            Assert.IsNotEmpty(i18n.ToonStandardConvertSettingsFeaturesRimLightingLabel);
        }

        /// <summary>
        /// Test material popup and replacement labels are non-empty.
        /// </summary>
        [Test]
        public void MaterialPopupLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.MaterialConvertTypePopupLabelToonLit);
            Assert.IsNotEmpty(i18n.MaterialConvertTypePopupLabelMatCapLit);
            Assert.IsNotEmpty(i18n.MaterialConvertTypePopupLabelToonStandard);
            Assert.IsNotEmpty(i18n.MaterialConvertTypePopupLabelMaterialReplace);
            Assert.IsNotEmpty(i18n.AdditionalMaterialConvertSettingsTargetMaterialLabel);
            Assert.IsNotEmpty(i18n.AdditionalMaterialConvertSettingsSelectMaterialLabel);
            Assert.IsNotEmpty(i18n.MaterialReplaceSettingsMaterialLabel);
            Assert.IsNotEmpty(i18n.MaterialReplaceSettingsMaterialTooltip);
            Assert.IsNotEmpty(i18n.MaterialReplaceSettingsMaterialWarning);
        }

        /// <summary>
        /// Test avatar dynamics labels are non-empty.
        /// </summary>
        [Test]
        public void AvatarDynamicsLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.AvatarConverterAvatarDynamicsSettingsLabel);
            Assert.IsNotEmpty(i18n.AvatarConverterRemoveAvatarDynamicsLabel);
            Assert.IsNotEmpty(i18n.AvatarConverterRemoveAvatarDynamicsTooltip);
            Assert.IsNotEmpty(i18n.AvatarConverterPhysBonesTooltip);
            Assert.IsNotEmpty(i18n.AvatarConverterPhysBoneCollidersTooltip);
            Assert.IsNotEmpty(i18n.AvatarConverterContactsTooltip);
            Assert.IsNotEmpty(i18n.ManualConvertButtonLabel);
            Assert.IsNotEmpty(i18n.ConfirmationForUnityConstraints);
            Assert.IsNotEmpty(i18n.ConfirmationForMAConvertConstraints);
        }

        /// <summary>
        /// Test PhysBone labels are non-empty.
        /// </summary>
        [Test]
        public void PhysBoneLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.SelectComponentsToKeep);
            Assert.IsNotEmpty(i18n.PhysBonesListTooltip);
            Assert.IsNotEmpty(i18n.KeepAll);
            Assert.IsNotEmpty(i18n.AvatarDynamicsPreventsUpload);
            Assert.IsNotEmpty(i18n.PhysBonesWillBeRemovedAtRunTime);
            Assert.IsNotEmpty(i18n.PhysBoneCollidersWillBeRemovedAtRunTime);
            Assert.IsNotEmpty(i18n.ContactsWillBeRemovedAtRunTime);
            Assert.IsNotEmpty(i18n.PhysBonesTransformsShouldBeReduced);
            Assert.IsNotEmpty(i18n.PhysBonesCollisionCheckCountShouldBeReduced);
            Assert.IsNotEmpty(i18n.PhysBonesShouldHaveNetworkID);
            Assert.IsNotEmpty(i18n.EstimatedPerformanceStats);
            Assert.IsNotEmpty(i18n.DeleteUnselectedComponents);
            Assert.IsNotEmpty(i18n.SelectAllButtonLabel);
            Assert.IsNotEmpty(i18n.DeselectAllButtonLabel);
            Assert.IsNotEmpty(i18n.ApplyButtonLabel);
        }

        /// <summary>
        /// Test texture and format labels are non-empty.
        /// </summary>
        [Test]
        public void TextureLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.TextureFormatHighQuality);
            Assert.IsNotEmpty(i18n.TextureFormatHighCompression);
            Assert.IsNotEmpty(i18n.TextureLabel);
            Assert.IsNotEmpty(i18n.InvertLabel);
            Assert.IsNotEmpty(i18n.SaveFileDialogMessage);
            Assert.IsNotEmpty(i18n.GenerateButtonLabel);
            Assert.IsNotEmpty(i18n.TextureCompressionLabel);
            Assert.IsNotEmpty(i18n.TextureCompressionHelp);
            Assert.IsNotEmpty(i18n.TextureCompressionButtonLabel);
        }

        /// <summary>
        /// Test Unity settings labels are non-empty.
        /// </summary>
        [Test]
        public void UnitySettingsLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.RecommendedUnitySettingsForAndroid);
            Assert.IsNotEmpty(i18n.AndroidBuildSupportButtonLabel);
            Assert.IsNotEmpty(i18n.AndroidBuildSupportHelp);
            Assert.IsNotEmpty(i18n.ApplyAllButtonLabel);
            Assert.IsNotEmpty(i18n.ShowOnStartupLabel);
            Assert.IsNotEmpty(i18n.AllAppliedHelp);
        }

        /// <summary>
        /// Test check for update labels are non-empty.
        /// </summary>
        [Test]
        public void UpdateLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.GetUpdate);
            Assert.IsNotEmpty(i18n.SeeChangelog);
            Assert.IsNotEmpty(i18n.SkipThisVersion);
            Assert.IsNotEmpty(i18n.NewVersionHasBreakingChanges);
        }

        /// <summary>
        /// Test validation labels are non-empty.
        /// </summary>
        [Test]
        public void ValidationLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.MissingScripts);
            Assert.IsNotEmpty(i18n.ExperimentalComponentWarning);
            Assert.IsNotEmpty(i18n.AvatarRootComponentMustBeOnAvatarRoot);
            Assert.IsNotEmpty(i18n.ComponentRequiresNdmf);
            Assert.IsNotEmpty(i18n.FeatureRequiresNdmf);
            Assert.IsNotEmpty(i18n.PlatformTargetSettingsIsRequiredToEnforcePlatform);
        }

        /// <summary>
        /// Test component-specific labels are non-empty.
        /// </summary>
        [Test]
        public void ComponentLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.VertexColorRemoverEditorDescription);
            Assert.IsNotEmpty(i18n.VertexColorRemoverEditorRemove);
            Assert.IsNotEmpty(i18n.VertexColorRemoverEditorRestore);
            Assert.IsNotEmpty(i18n.ConvertedAvatarEditorMessage);
            Assert.IsNotEmpty(i18n.ConvertedAvatarEditorNDMFMessage);
            Assert.IsNotEmpty(i18n.NetworkIDAssignerEditorDescription);
            Assert.IsNotEmpty(i18n.PlatformTargetSettingsEditorDescription);
            Assert.IsNotEmpty(i18n.BuildTargetLabel);
            Assert.IsNotEmpty(i18n.BuildTargetTooltip);
            Assert.IsNotEmpty(i18n.PlatformComponentRemoverEditorDescription);
            Assert.IsNotEmpty(i18n.PlatformComponentRemoverEditorComponentSettingsLabel);
            Assert.IsNotEmpty(i18n.PlatformComponentRemoverEditorComponentSettingsTooltip);
            Assert.IsNotEmpty(i18n.PlatformComponentRemoverEditorCheckboxPCTooltip);
            Assert.IsNotEmpty(i18n.PlatformComponentRemoverEditorCheckboxMobileTooltip);
            Assert.IsNotEmpty(i18n.ComponentLabel);
            Assert.IsNotEmpty(i18n.PlatformGameObjectRemoverEditorDescription);
            Assert.IsNotEmpty(i18n.PlatformGameObjectRemoverEditorKeepOnPCLabel);
            Assert.IsNotEmpty(i18n.PlatformGameObjectRemoverEditorKeepOnMobileLabel);
        }

        /// <summary>
        /// Test mesh flipper labels are non-empty.
        /// </summary>
        [Test]
        public void MeshFlipperLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.MeshFlipperEditorDescription);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorDirectionLabel);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorUseMaskLabel);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorMaskTextureLabel);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorMaskModeLabel);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorProcessingPhaseLabel);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorEnabledOnPCLabel);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorEnabledOnMobileLabel);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorEnabledOnPCWarning);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorEnabledOnMobileWarning);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorMaskTextureMissingError);
            Assert.IsNotEmpty(i18n.MeshFlipperEditorMaskTextureNotReadableError);
            Assert.IsNotEmpty(i18n.MeshFlipperMeshDirectionFlip);
            Assert.IsNotEmpty(i18n.MeshFlipperMeshDirectionDoubleSide);
        }

        /// <summary>
        /// Test material swap labels are non-empty.
        /// </summary>
        [Test]
        public void MaterialSwapLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.MaterialSwapEditorDescription);
            Assert.IsNotEmpty(i18n.MaterialSwapEditorMaterialMappingsLabel);
            Assert.IsNotEmpty(i18n.MaterialSwapEditorOriginalMaterialLabel);
            Assert.IsNotEmpty(i18n.MaterialSwapEditorReplacementMaterialLabel);
            Assert.IsNotEmpty(i18n.MaterialSwapEditorSelectMaterialLabel);
            Assert.IsNotEmpty(i18n.MaterialSwapEditorReplacementMaterialError);
        }

        /// <summary>
        /// Test material conversion settings labels are non-empty.
        /// </summary>
        [Test]
        public void MaterialConversionSettingsLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.MaterialConversionSettingsEditorDescription);
            Assert.IsNotEmpty(i18n.MaterialConversionSettingsEditorDefaultConversionWarning);
        }

        /// <summary>
        /// Test menu icon resizer labels are non-empty.
        /// </summary>
        [Test]
        public void MenuIconResizerLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.MenuIconResizerEditorDescription);
            Assert.IsNotEmpty(i18n.MenuIconResizerEditorResizeModePCLabel);
            Assert.IsNotEmpty(i18n.MenuIconResizerEditorResizeModeMobileLabel);
            Assert.IsNotEmpty(i18n.MenuIconResizerEditorCompressTexturesLabel);
            Assert.IsNotEmpty(i18n.MenuIconResizerEditorMobileTextureFormatLabel);
        }

        /// <summary>
        /// Test miscellaneous labels are non-empty.
        /// </summary>
        [Test]
        public void MiscLabels_AreNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.FallbackAvatarEditorDescription);
            Assert.IsNotEmpty(i18n.BuildAndTestRequiresSdkControlPanel);
            Assert.IsNotEmpty(i18n.UnpackPrefabMessage);
            Assert.IsNotEmpty(i18n.SourceMeshLabel);
            Assert.IsNotEmpty(i18n.TargetMeshLabel);
            Assert.IsNotEmpty(i18n.CopyButtonLabel);
            Assert.IsNotEmpty(i18n.SwitchButtonLabel);
        }

        /// <summary>
        /// Test parameterized messages return non-empty results.
        /// </summary>
        [Test]
        public void ParameterizedMessages_ReturnNonEmpty()
        {
            var i18n = I18n.I18n.GetI18n();
            Assert.IsNotEmpty(i18n.BuildAndTestFailed("error msg"));
            Assert.IsNotEmpty(i18n.BuildAndTestSucceeded("avatar"));
            Assert.IsNotEmpty(i18n.LegacyPackageExceptionMessage("pkg", "1.0"));
            Assert.IsNotEmpty(i18n.BreakingPackageExceptionMessage("pkg", "2.0"));
            Assert.IsNotEmpty(i18n.NewVersionIsAvailable("1.2.3"));
            Assert.IsNotEmpty(i18n.SaveFileDialogTitle("thing"));
            Assert.IsNotEmpty(i18n.NoMissingComponentsMessage("obj"));
            Assert.IsNotEmpty(i18n.MissingRemoverConfirmationMessage("obj"));
            Assert.IsNotEmpty(i18n.MissingRemoverOnBuildDialogMessage("obj"));
            Assert.IsNotEmpty(i18n.NoUnsupportedComponentsMessage("obj"));
            Assert.IsNotEmpty(i18n.UnsupportedRemoverConfirmationMessage("obj"));
        }

        /// <summary>
        /// Test English I18n instance.
        /// </summary>
        [Test]
        public void I18nEnglish_IsCreatable()
        {
            var en = new I18nEnglish();
            Assert.IsNotNull(en);
            Assert.IsNotEmpty(en.CancelLabel);
        }

        /// <summary>
        /// Test Japanese I18n instance.
        /// </summary>
        [Test]
        public void I18nJapanese_IsCreatable()
        {
            var ja = new I18nJapanese();
            Assert.IsNotNull(ja);
            Assert.IsNotEmpty(ja.CancelLabel);
        }

        /// <summary>
        /// Test I18nRussian can be created and returns non-empty strings.
        /// </summary>
        [Test]
        public void I18nRussian_IsCreatable()
        {
            var ru = new I18nRussian();
            Assert.IsNotNull(ru);
            Assert.IsNotEmpty(ru.CancelLabel);
        }
    }
}
