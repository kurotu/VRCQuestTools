using nadena.dev.ndmf;
using UnityEngine;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Proxy class for NDMF ObjectRestry.
    /// </summary>
    internal static class NdmfObjectRegistry
    {
        /// <summary>
        /// Get reference of the object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>ObjectReference.</returns>
        internal static ObjectReference GetReference(Object obj)
        {
            return ObjectRegistry.GetReference(obj);
        }

        /// <summary>
        /// Register replaced object.
        /// </summary>
        /// <param name="original">Original object.</param>
        /// <param name="replaced">Replaced object.</param>
        internal static void RegisterReplacedObject(Object original, Object replaced)
        {
            ObjectRegistry.RegisterReplacedObject(original, replaced);
        }
    }
}
