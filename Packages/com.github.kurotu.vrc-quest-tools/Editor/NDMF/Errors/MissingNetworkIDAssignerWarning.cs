using nadena.dev.ndmf;
#if VQT_HAS_NDMF_ERROR_REPORT
using nadena.dev.ndmf.localization;
#else
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Warning for missing NetworkIDAssigner.
    /// </summary>
    internal class MissingNetworkIDAssignerWarning : SimpleError
    {
        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.MissingNetworkIDAssignerTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.MissingNetworkIDAssignerDescription;

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.NonFatal;
    }
}
