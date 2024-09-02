// <copyright file="I18n.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.I18n
{
    /// <summary>
    /// Manipulates i18n objects.
    /// </summary>
    internal static class I18n
    {
        /// <summary>
        /// Gets appropreate i18n object by VRCQuestToolsSettings.DisplayLanguage.
        /// </summary>
        /// <returns>Reolved i18n object.</returns>
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
                case DisplayLanguage.Russian:
                    return new I18nRussian();
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
            else if (systemCultureName == "ru-RU")
            {
                return new I18nRussian();
            }
            return new I18nEnglish();
        }
    }
}
