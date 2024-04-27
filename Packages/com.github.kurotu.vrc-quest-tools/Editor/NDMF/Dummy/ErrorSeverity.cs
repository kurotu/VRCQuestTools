#if !VQT_HAS_NDMF_ERROR_REPORT
namespace KRT.VRCQuestTools.Ndmf.Dummy
{
    /// <summary>
    /// ErrorSeverity is a dummy enum for NDMF.
    /// </summary>
    internal enum ErrorSeverity
    {
        /// <summary>
        /// Information.
        /// </summary>
        Information,

        /// <summary>
        /// Warning.
        /// </summary>
        Warning,

        /// <summary>
        /// NonFatal.
        /// </summary>
        NonFatal,

        /// <summary>
        /// Error.
        /// </summary>
        Error,
    }
}
#endif
