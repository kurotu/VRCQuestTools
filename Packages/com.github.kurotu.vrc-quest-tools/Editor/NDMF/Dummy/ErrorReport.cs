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
            var item = new NdmfReportWindow.ReportItem();
            item.message = e.ToString();
            switch (e.Severity)
            {
                case ErrorSeverity.Information:
                    item.type = UnityEditor.MessageType.Info;
                    break;
                case ErrorSeverity.Warning:
                    item.type = UnityEditor.MessageType.Warning;
                    break;
                case ErrorSeverity.NonFatal:
                    item.type = UnityEditor.MessageType.Warning;
                    break;
                case ErrorSeverity.Error:
                    item.type = UnityEditor.MessageType.Error;
                    break;
            }
            NdmfReportWindow.AddReportItem(item);
            if (e.Severity == ErrorSeverity.Error)
            {
                throw new System.Exception(e.ToString());
            }
        }
    }
}
#endif
