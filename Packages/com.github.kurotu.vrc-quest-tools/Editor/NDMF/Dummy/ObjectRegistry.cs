#if !VQT_HAS_NDMF_ERROR_REPORT
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf.Dummy
{
    /// <summary>
    /// ObjectRegistry is a dummy class for NDMF.
    /// </summary>
    internal static class ObjectRegistry
    {
        /// <summary>
        /// GetReference is a dummy method for NDMF.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>ObjectReference.</returns>
        internal static ObjectReference GetReference(Object obj)
        {
            return new ObjectReference(obj);
        }
    }
}
#endif
