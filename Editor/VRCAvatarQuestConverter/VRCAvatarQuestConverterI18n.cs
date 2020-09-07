// <copyright file="VRCAvatarQuestConverterI18n.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

namespace Kurotu
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
        public abstract string OverwriteWarningDialogTitle();
        public abstract string OverwriteWarningDialogMessage(string artifactsDir);
        public abstract string OverwriteWarningDialogButtonOK();
        public abstract string OverwriteWarningDialogButtonCancel();
        public abstract string OverwriteWarningDialogButtonUseAltDir(string altDir);
    }

    class VRCAvatarQuestConverterI18nEnglish : VRCAvatarQuestConverterI18nBase
    {
        public override string OverwriteWarningDialogButtonCancel()
        {
            return "Cancel";
        }

        public override string OverwriteWarningDialogButtonOK()
        {
            return "OK";
        }

        public override string OverwriteWarningDialogButtonUseAltDir(string altDir)
        {
            return $"Use \"{altDir}\"";
        }

        public override string OverwriteWarningDialogMessage(string artifactsDir)
        {
            return $"\"{artifactsDir}\" already exists. Do you want to overwrite?";
        }

        public override string OverwriteWarningDialogTitle()
        {
            return "VRCAvatarQuestConverter Warning";
        }
    }

    class VRCAvatarQuestConverterI18nJapanese : VRCAvatarQuestConverterI18nBase
    {
        public override string OverwriteWarningDialogButtonCancel()
        {
            return "キャンセル";
        }

        public override string OverwriteWarningDialogButtonOK()
        {
            return "OK";
        }

        public override string OverwriteWarningDialogButtonUseAltDir(string altDir)
        {
            return $"\"{altDir}\" を使用する";
        }

        public override string OverwriteWarningDialogMessage(string artifactsDir)
        {
            return $"\"{artifactsDir}\" が既に存在します。上書きしますか？";
        }

        public override string OverwriteWarningDialogTitle()
        {
            return "VRCAvatarQuestConverter 警告";
        }
    }
}
