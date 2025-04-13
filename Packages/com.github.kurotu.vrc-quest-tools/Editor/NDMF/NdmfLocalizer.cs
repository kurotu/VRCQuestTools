using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.I18n;

#if VQT_HAS_NDMF_ERROR_REPORT
using nadena.dev.ndmf.localization;

#else
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

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
        internal const string MaterialSwapNullErrorTitle = "NDMF:MaterialSwapNullErrorTitle";
        internal const string MaterialSwapNullErrorDescription = "NDMF:MaterialSwapNullErrorDescription";
        internal const string ObjectConversionErrorTitle = "NDMF:ObjectConversionErrorTitle";
        internal const string ObjectConversionErrorDescription = "NDMF:ObjectConversionErrorDescription";
        internal const string TargetMaterialNullErrorTitle = "NDMF:TargetMaterialNullErrorTitle";
        internal const string UnsupportedTextureFormatTitle = "NDMF:UnsupportedTextureFormatTitle";
        internal const string UnsupportedTextureFormatDescription = "NDMF:UnsupportedTextureFormatDescription";
        internal const string UnknownTextureFormatTitle = "NDMF:UnknownTextureFormatTitle";
        internal const string UnknownTextureFormatDescription = "NDMF:UnknownTextureFormatDescription";
        internal const string MissingNetworkIDAssignerTitle = "NDMF:MissingNetworkIDAssignerTitle";
        internal const string MissingNetworkIDAssignerDescription = "NDMF:MissingNetworkIDAssignerDescription";
        internal const string MeshFlipperMaskMissingErrorTitle = "NDMF:MeshFlipperMaskMissingErrorTitle";
        internal const string MeshFlipperMaskMissingErrorDescription = "NDMF:MeshFlipperMaskMissingErrorDescription";
        internal const string MeshFlipperMaskNotReadableErrorTitle = "NDMF:MeshFlipperMaskNotReadableErrorTitle";
        internal const string MeshFlipperMaskNotReadableErrorDescription = "NDMF:MeshFlipperMaskNotReadableErrorDescription";
        internal const string ReplacementMaterialErrorTitle = "NDMF:ReplacementMaterialErrorTitle";
        internal const string ReplacementMaterialErrorDescription = "NDMF:ReplacementMaterialErrorDescription";
#pragma warning restore SA1600

        /// <summary>
        /// Instance of Localizer.
        /// </summary>
        internal static Localizer Instance = new Localizer("en-US", () => new List<(string, System.Func<string, string>)>
        {
            ("en-US", (key) =>
            {
                return EnUS.Value.GetText(key);
            }),
            ("ja-JP", (key) =>
            {
                return JaJP.Value.GetText(key);
            }),
            ("ru-RU", (key) =>
            {
                return RuRU.Value.GetText(key);
            }),
        });

        private static readonly Lazy<I18nEnglish> EnUS = new Lazy<I18nEnglish>(() => new I18nEnglish());
        private static readonly Lazy<I18nJapanese> JaJP = new Lazy<I18nJapanese>(() => new I18nJapanese());
        private static readonly Lazy<I18nRussian> RuRU = new Lazy<I18nRussian>(() => new I18nRussian());
    }
}
