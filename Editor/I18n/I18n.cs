// <copyright file="I18n.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

namespace KRT.VRCQuestTools
{
    static class I18n
    {
        internal static I18nBase GetI18n()
        {
            switch (VRCQuestToolsSettings.DisplayLanguage)
            {
                case DisplayLanguage.Auto:
                    return ResolveAutoLanguage();
                case DisplayLanguage.Japanese:
                    return new I18nJapanese();
                case DisplayLanguage.English:
                    return new I18nEnglish();
                default:
                    throw new System.NotImplementedException($"I18n for {VRCQuestToolsSettings.DisplayLanguage} is not implemented");
            }
        }

        private static I18nBase ResolveAutoLanguage()
        {
            var systemCultureName = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (systemCultureName == "ja-JP")
            {
                return new I18nJapanese();
            }
            return new I18nEnglish();
        }

    }
}
