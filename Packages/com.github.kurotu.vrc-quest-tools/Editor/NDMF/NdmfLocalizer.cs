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
                }
                return key;
            }),
        });
    }
}
