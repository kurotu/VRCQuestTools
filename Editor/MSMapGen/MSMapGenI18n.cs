// <copyright file="MSMapGenI18n.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

namespace KRT.VRCQuestTools
{
    static class MSMapGenI18n
    {
        public static MSMapGenI18nBase Create()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "ja-JP")
            {
                return new MSMapGenI18nJapanese();
            }
            else
            {
                return new MSMapGenI18nEnglish();
            }
        }
    }

    abstract class MSMapGenI18nBase
    {
        public abstract string OverwriteWarningDialogTitle { get; }
        public abstract string OverwriteWarningDialogMessage(string artifactPath);
        public abstract string ButtonOK { get; }
        public abstract string ButtonCancel { get; }
        public abstract string InvertSmoothness { get; }
        public abstract string AllowOverwriting { get; }
        public abstract string Select { get; }
        public abstract string SaveFileDialogTitle(string thing);
        public abstract string SaveFileDialogMessage { get; }
        public abstract string SaveAs { get; }
        public abstract string Generate { get; }
    }

    class MSMapGenI18nEnglish : MSMapGenI18nBase
    {
        public override string ButtonCancel => "Cancel";

        public override string ButtonOK => "OK";

        public override string OverwriteWarningDialogMessage(string artifactPath)
        {
            return $"\"{artifactPath}\" already exists. Do you want to overwrite?";
        }

        public override string OverwriteWarningDialogTitle => "Warning - VRCStandardLiteUtils";

        public override string InvertSmoothness => "Invert Smoothness";

        public override string AllowOverwriting => "Allow Overwriting";

        public override string Select => "Select";

        public override string SaveFileDialogTitle(string thing)
        {
            return $"Save {thing}";
        }

        public override string SaveFileDialogMessage => "Please enter a file name to save the texture to";

        public override string SaveAs => "Save as";

        public override string Generate => "Generate";
    }

    class MSMapGenI18nJapanese : MSMapGenI18nBase
    {
        public override string ButtonCancel => "キャンセル";

        public override string ButtonOK => "OK";

        public override string OverwriteWarningDialogMessage(string artifactPath)
        {
            return $"\"{artifactPath}\" が既に存在します。上書きしますか？";
        }

        public override string OverwriteWarningDialogTitle => "警告 - VRCStandardLiteUtils";

        public override string InvertSmoothness => "Smoothness を反転";

        public override string AllowOverwriting => "上書きを許可";

        public override string Select => "選択";

        public override string SaveFileDialogTitle(string thing)
        {
            return $"{thing} を保存";
        }

        public override string SaveFileDialogMessage => "テクスチャの保存先を選択してください";

        public override string SaveAs => "保存先";

        public override string Generate => "生成";
    }
}
