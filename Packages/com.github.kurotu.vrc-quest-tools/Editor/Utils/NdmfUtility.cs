namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility class for NDMF-related operations.
    /// </summary>
    internal static class NdmfUtility
    {
        /// <summary>
        /// Notifies that an object has been updated.
        /// </summary>
        /// <param name="obj">Updated object.</param>
        public static void NotifyObjectUpdate(UnityEngine.Object obj)
        {
#if VQT_HAS_NDMF
            nadena.dev.ndmf.preview.ChangeNotifier.NotifyObjectUpdate(obj);
#endif
        }
    }
}
