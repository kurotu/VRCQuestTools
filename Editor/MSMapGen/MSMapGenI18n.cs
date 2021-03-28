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
        public abstract string TextureLabel { get; }
        public abstract string InvertLabel { get; }
        public abstract string SaveFileDialogTitle(string thing);
        public abstract string SaveFileDialogMessage { get; }
        public abstract string GenerateButtonLabel { get; }
    }

    class MSMapGenI18nEnglish : MSMapGenI18nBase
    {
        public override string TextureLabel => "Texture";

        public override string InvertLabel => "Invert";

        public override string SaveFileDialogTitle(string thing)
        {
            return $"Save {thing}";
        }

        public override string SaveFileDialogMessage => "Please enter a file name to save the texture to";

        public override string GenerateButtonLabel => "Generate Metallic Smoothness";
    }

    class MSMapGenI18nJapanese : MSMapGenI18nBase
    {
        public override string TextureLabel => "テクスチャ";

        public override string InvertLabel => "反転";

        public override string SaveFileDialogTitle(string thing)
        {
            return $"{thing} を保存";
        }

        public override string SaveFileDialogMessage => "テクスチャの保存先を選択してください";

        public override string GenerateButtonLabel => "Metallic Smoothness を生成";
    }
}
