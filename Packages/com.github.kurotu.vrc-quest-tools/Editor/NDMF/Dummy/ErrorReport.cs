#if !VQT_HAS_NDMF_ERROR_REPORT
namespace KRT.VRCQuestTools.Ndmf.Dummy
{
    /// <summary>
    /// ErrorReport is a dummy class for NDMF.
    /// </summary>
    internal class ErrorReport
    {
        /// <summary>
        /// ReportError is a dummy method for NDMF.
        /// </summary>
        /// <param name="e">Error.</param>
        internal static void ReportError(SimpleError e)
        {
        }
    }
}
#endif
