using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Proxy class for NDMF ErrorReport.
    /// </summary>
    internal static class NdmfErrorReport
    {
        /// <summary>
        /// Report error.
        /// </summary>
        /// <param name="error">Error.</param>
        internal static void ReportError(SimpleError error)
        {
            ErrorReport.ReportError(error);
        }
    }
}
