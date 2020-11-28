// <copyright file="VRCAvatarQuestConverterI18n.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

namespace KRT.VRCQuestTools
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
        public abstract string GenerateQuestTexturesLabel { get; }
        public abstract string SupportedShadersLabel { get; }
        public abstract string OutputSettingsLabel { get; }
        public abstract string SaveToLabel { get; }
        public abstract string SelectButtonLabel { get; }
        public abstract string ConvertButtonLabel { get; }
        public abstract string ConvertingMaterialsDialogMessage { get; }
        public abstract string MaterialExceptionDialogMessage { get; }
        public abstract string WarningForPerformance { get; }
        public abstract string InfoForAppearance { get; }
        public abstract string ResizeTexturesLabel { get; }
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

        public override string GenerateQuestTexturesLabel => "Generate Textures for Quest";

        public override string SupportedShadersLabel => "Supported Shaders";

        public override string OutputSettingsLabel => "Output Settings";

        public override string SaveToLabel => "Save to";

        public override string SelectButtonLabel => "Select";

        public override string ConvertButtonLabel => "Convert";

        public override string ConvertingMaterialsDialogMessage => "Converting materials...";

        public override string MaterialExceptionDialogMessage => "An error occured when converting materials. Aborted.";

        public override string WarningForPerformance => "In many cases, the converted avatar's performance rank will be \"Very Poor\" for Quest. Quest users need to use \"Show Avatar\" due to Performance Options limitation.\\nn" +
            "And texture's transparency doesn't make any effects, so this will be an issue for facial expression. In this case, please take steps by yourself (for example, by editing animation clips).";

        public override string InfoForAppearance => "You should check avatar's appearance by using another Blueprint ID or Avatars 3.0 local testing.";

        public override string ResizeTexturesLabel => "Resize Textures";
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

        public override string GenerateQuestTexturesLabel => "Quest用のテクスチャを生成する";

        public override string SupportedShadersLabel => "対応シェーダー";

        public override string OutputSettingsLabel => "保存設定";

        public override string SaveToLabel => "保存先";

        public override string SelectButtonLabel => "選択";

        public override string ConvertButtonLabel => "変換";

        public override string ConvertingMaterialsDialogMessage => "マテリアルを変換中";

        public override string MaterialExceptionDialogMessage => "マテリアルの変換中にエラーが発生しました。変換を中止します。";

        public override string WarningForPerformance => "多くの場合、Questから見た場合のパフォーマンスランクはVery Poorになります。Performance Optionsによる制限があるためQuestから見るにはShow Avatarの操作をする必要があります。\n\n" +
            "またテクスチャの透過が反映されないため、頬染めなどの表現に問題がある場合があります。そのような場合はアニメーション編集などの方法で対策する必要があります。";

        public override string InfoForAppearance => "別のBlueprint IDでのアップロードやAvatars 3.0のローカルテストを使用してアバターの見た目を確認することをお勧めします。";

        public override string ResizeTexturesLabel => "テクスチャをリサイズする";
    }
}
