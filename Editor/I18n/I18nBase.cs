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

        // Convert Avatar for Quest
        internal abstract string OverwriteWarningDialogTitle { get; }
        internal abstract string OverwriteWarningDialogMessage(string artifactsDir);
        internal abstract string OverwriteWarningDialogButtonOK { get; }
        internal abstract string OverwriteWarningDialogButtonCancel { get; }
        internal abstract string OverwriteWarningDialogButtonUseAltDir(string altDir);
        internal abstract string AvatarLabel { get; }
        internal abstract string GenerateQuestTexturesLabel { get; }
        internal abstract string QuestTexturesDescription { get; }
        internal abstract string VerifiedShadersLabel { get; }
        internal abstract string SaveToLabel { get; }
        internal abstract string SelectButtonLabel { get; }
        internal abstract string ConvertButtonLabel { get; }
        internal abstract string UpdateTexturesLabel { get; }
        internal abstract string ConvertingMaterialsDialogMessage { get; }
        internal abstract string GeneratingTexturesDialogMessage { get; }
        internal abstract string MaterialExceptionDialogMessage { get; }
        internal abstract string AnimationClipExceptionDialogMessage { get; }
        internal abstract string AnimatorControllerExceptionDialogMessage { get; }
        internal abstract string WarningForPerformance { get; }
        internal abstract string WarningForAppearance { get; }
        internal abstract string WarningForUnverifiedShaders { get; }
        internal abstract string TexturesSizeLimitLabel { get; }
        internal abstract string AlertForComponents { get; }
        internal abstract string AlertForMaterialAnimation { get; }
        internal abstract string CompletedDialogMessage(string originalName);

        // Remove Missing Components
        internal abstract string NoMissingComponentsMessage(string objectName);
        internal abstract string MissingRemoverConfirmationMessage(string objectName);
        internal abstract string UnpackPrefabMessage { get; }

        // BlendShapes Copy
        internal abstract string SourceMeshLabel { get; }
        internal abstract string TargetMeshLabel { get; }
        internal abstract string CopyButtonLabel { get; }
        internal abstract string SwitchButtonLabel { get; }

        // Remove Unsupported Components
        internal abstract string NoUnsupportedComponentsMessage(string objectName);
        internal abstract string UnsupportedRemoverConfirmationMessage(string objectName);

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
        internal abstract string NewVersionIsAvailable(string latestVersion);
        internal abstract string ThereIsNoUpdate { get; }
    }
}
