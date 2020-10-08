// <copyright file="VRCAvatarQuestConverterI18n.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

namespace VRCQuestTools
{
    static class VRCAvatarQuestConverterI18n
    {
        public static VRCAvatarQuestConverterI18nBase Create()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "ja-JP")
            {
                return new VRCAvatarQuestConverterI18nJapanese();
            }
            else
            {
                return new VRCAvatarQuestConverterI18nEnglish();
            }
        }
    }

    abstract class VRCAvatarQuestConverterI18nBase
    {
        public abstract string OverwriteWarningDialogTitle { get; }
        public abstract string OverwriteWarningDialogMessage(string artifactsDir);
        public abstract string OverwriteWarningDialogButtonOK { get; }
        public abstract string OverwriteWarningDialogButtonCancel { get; }
        public abstract string OverwriteWarningDialogButtonUseAltDir(string altDir);
        public abstract string ConvertSettingsLabel { get; }
        public abstract string AvatarLabel { get; }
        public abstract string ExperimentalSettingsLabel { get; }
        public abstract string CombineEmissionLabel { get; }
        public abstract string SupportedShadersLabel { get; }
        public abstract string OutputSettingsLabel { get; }
        public abstract string SaveToLabel { get; }
        public abstract string SelectButtonLabel { get; }
        public abstract string ConvertButtonLabel { get; }
        public abstract string ConvertingMaterialsDialogMessage { get; }
    }

    class VRCAvatarQuestConverterI18nEnglish : VRCAvatarQuestConverterI18nBase
    {
        public override string OverwriteWarningDialogButtonCancel => "Cancel";

        public override string OverwriteWarningDialogButtonOK => "OK";

        public override string OverwriteWarningDialogButtonUseAltDir(string altDir)
        {
            return $"Use \"{altDir}\"";
        }

        public override string OverwriteWarningDialogMessage(string artifactsDir)
        {
            return $"\"{artifactsDir}\" already exists. Do you want to overwrite?";
        }

        public override string OverwriteWarningDialogTitle => "VRCAvatarQuestConverter Warning";

        public override string ConvertSettingsLabel => "Convert Settings";

        public override string AvatarLabel => "Avatar";

        public override string ExperimentalSettingsLabel => "Experimental Settings";

        public override string CombineEmissionLabel => "Combine Emission";

        public override string SupportedShadersLabel => "Supported Shaders";

        public override string OutputSettingsLabel => "Output Settings";

        public override string SaveToLabel => "Save to";

        public override string SelectButtonLabel => "Select";

        public override string ConvertButtonLabel => "Convert";

        public override string ConvertingMaterialsDialogMessage => "Converting materials...";
    }

    class VRCAvatarQuestConverterI18nJapanese : VRCAvatarQuestConverterI18nBase
    {
        public override string OverwriteWarningDialogButtonCancel => "キャンセル";

        public override string OverwriteWarningDialogButtonOK => "OK";

        public override string OverwriteWarningDialogButtonUseAltDir(string altDir)
        {
            return $"\"{altDir}\" を使用する";
        }

        public override string OverwriteWarningDialogMessage(string artifactsDir)
        {
            return $"\"{artifactsDir}\" が既に存在します。上書きしますか？";
        }

        public override string OverwriteWarningDialogTitle => "VRCAvatarQuestConverter 警告";

        public override string ConvertSettingsLabel => "変換設定";

        public override string AvatarLabel => "アバター";

        public override string ExperimentalSettingsLabel => "実験的機能";

        public override string CombineEmissionLabel => "Emissionを合成する";

        public override string SupportedShadersLabel => "対応シェーダー";

        public override string OutputSettingsLabel => "保存設定";

        public override string SaveToLabel => "保存先";

        public override string SelectButtonLabel => "選択";

        public override string ConvertButtonLabel => "変換";

        public override string ConvertingMaterialsDialogMessage => "マテリアルを変換中";
    }
}
