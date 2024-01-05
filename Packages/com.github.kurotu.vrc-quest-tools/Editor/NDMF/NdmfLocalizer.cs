#if VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.I18n;
using nadena.dev.ndmf.localization;
using System.Collections.Generic;

namespace KRT.VRCQuestTools.Ndmf
{
    internal static class NdmfLocalizer
    {
        internal static I18nBase en = new I18nEnglish();
        internal static I18nBase ja = new I18nJapanese();

        internal const string RemovedUnsupportedComponentTitle = "Warning:Removed Unsupported Component";
        internal const string RemovedUnsupportedComponentDescription = "Removed {0} from \"{1}\".";

        internal static Localizer Instance = new Localizer("en-US", () => new List<(string, System.Func<string, string>)>
        {
            ("en-US", (key) => {
                switch (key)
                {
                    case RemovedUnsupportedComponentTitle:
                        return "Removed Unsupported Component";
                    case RemovedUnsupportedComponentDescription:
                        return "Unsupported component {0} is removed from \"{1}\" during build. Please test the avatar.";
                }
                return key;
            }),
            ("ja-JP", (key) => {
                switch (key)
                {
                    case RemovedUnsupportedComponentTitle:
                        return "非対応コンポーネントの削除";
                    case RemovedUnsupportedComponentDescription:
                        return "ビルド中に非対応コンポーネント {0} を \"{1}\" から削除しました。動作に支障がないか確認してください。";
                }
                return key;
            }),
        });
    }
}
#endif
