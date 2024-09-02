#if VQT_HAS_NDMF_ERROR_REPORT
using System;
using KRT.VRCQuestTools.I18n;
using nadena.dev.ndmf.localization;

#else
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif
using System.Collections.Generic;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Localizer for NDMF.
    /// </summary>
    internal static class NdmfLocalizer
    {
#pragma warning disable SA1600
        internal const string RemovedUnsupportedComponentTitle = "NDMF:RemovedUnsupportedComponentTitle";
        internal const string RemovedUnsupportedComponentDescription = "NDMF:RemovedUnsupportedComponentDescription";
        internal const string MaterialConversionErrorTitle = "NDMF:MaterialConversionErrorTitle";
        internal const string MaterialConversionErrorDescription = "NDMF:MaterialConversionErrorDescription";
        internal const string ObjectConversionErrorTitle = "NDMF:ObjectConversionErrorTitle";
        internal const string ObjectConversionErrorDescription = "NDMF:ObjectConversionErrorDescription";
        internal const string UnsupportedTextureFormatTitle = "NDMF:UnsupportedTextureFormatTitle";
        internal const string UnsupportedTextureFormatDescription = "NDMF:UnsupportedTextureFormatDescription";
        internal const string UnknownTextureFormatTitle = "NDMF:UnknownTextureFormatTitle";
        internal const string UnknownTextureFormatDescription = "NDMF:UnknownTextureFormatDescription";
#pragma warning restore SA1600

        internal static readonly Lazy<I18nEnglish> enUS = new Lazy<I18nEnglish>(() => new I18nEnglish());
        internal static readonly Lazy<I18nJapanese> jaJP = new Lazy<I18nJapanese>(() => new I18nJapanese());
        internal static readonly Lazy<I18nRussian> ruRU = new Lazy<I18nRussian>(() => new I18nRussian());

        /// <summary>
        /// Instance of Localizer.
        /// </summary>
        internal static Localizer Instance = new Localizer("en-US", () => new List<(string, System.Func<string, string>)>
        {
            ("en-US", (key) =>
            {
                return enUS.Value.GetText(key);
            }),
            ("ja-JP", (key) =>
            {
                return jaJP.Value.GetText(key);
            }),
            ("ru-RU", (key) =>
            {
                return ruRU.Value.GetText(key);
            }),
        });
    }
}
