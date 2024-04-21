#if VQT_HAS_NDMF_ERROR_REPORT
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
        internal const string RemovedUnsupportedComponentTitle = "Warning:Removed Unsupported Component";
        internal const string RemovedUnsupportedComponentDescription = "Removed {0} from \"{1}\".";
        internal const string MaterialConversionErrorTitle = "Error:Material Conversion Error";
        internal const string MaterialConversionErrorDescription = "Failed to convert material {0} ({1}).\n\n{2}: {3}\n{4}";
        internal const string ObjectConversionErrorTitle = "Error:Object Conversion Error";
        internal const string ObjectConversionErrorDescription = "Failed to convert {0}: {1}.\n\n{2}: {3}\n{4}";
#pragma warning restore SA1600

        /// <summary>
        /// Instance of Localizer.
        /// </summary>
        internal static Localizer Instance = new Localizer("en-US", () => new List<(string, System.Func<string, string>)>
        {
            ("en-US", (key) =>
            {
                switch (key)
                {
                    case RemovedUnsupportedComponentTitle:
                        return "Removed Unsupported Component";
                    case RemovedUnsupportedComponentDescription:
                        return "Unsupported component {0} is removed from \"{1}\" during build. Please test the avatar.";
                    case MaterialConversionErrorTitle:
                        return "Material Conversion Error";
                    case MaterialConversionErrorDescription:
                        return "An error occured when conversion a material.\n" +
                        "\n" +
                        "Material: {0}\n" +
                        "Shader: {1}\n" +
                        "\n" +
                        "{2}: {3}\n" +
                        "{4}";
                    case ObjectConversionErrorTitle:
                        return "{0} Conversion Error";
                    case ObjectConversionErrorDescription:
                        return "An error occured when converting {0}.\n" +
                        "\n" +
                       "{0}: {1}\n" +
                        "\n" +
                        "{2}: {3}\n" +
                        "{4}";
                }
                return key;
            }),
            ("ja-JP", (key) =>
            {
                switch (key)
                {
                    case RemovedUnsupportedComponentTitle:
                        return "非対応コンポーネントの削除";
                    case RemovedUnsupportedComponentDescription:
                        return "ビルド中に非対応コンポーネント {0} を \"{1}\" から削除しました。動作に支障がないか確認してください。";
                    case MaterialConversionErrorTitle:
                        return "マテリアル変換エラー";
                    case MaterialConversionErrorDescription:
                        return "マテリアルの変換中にエラーが発生しました。\n" +
                        "\n" +
                        "Material: {0}\n" +
                        "Shader: {1}\n" +
                        "\n" +
                        "{2}: {3}\n" +
                        "{4}";
                    case ObjectConversionErrorTitle:
                        return "オブジェクト変換エラー";
                    case ObjectConversionErrorDescription:
                        return "{0} の変換中にエラーが発生しました。\n" +
                        "\n" +
                       "{0}: {1}\n" +
                        "\n" +
                        "{2}: {3}\n" +
                        "{4}";
                }
                return key;
            }),
        });
    }
}
