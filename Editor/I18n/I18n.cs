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
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "ja-JP")
            {
                return new I18nJapanese();
            }
            return new I18nEnglish();
        }
    }
}
