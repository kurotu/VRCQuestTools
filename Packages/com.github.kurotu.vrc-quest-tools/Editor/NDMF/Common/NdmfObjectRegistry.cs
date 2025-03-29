using System.Collections.Generic;
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
    internal class NdmfObjectRegistry
    {
        private List<Object> registeredObjects = new List<Object>();

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
        internal void RegisterReplacedObject(Object original, Object replaced)
        {
            if (registeredObjects.Contains(replaced))
            {
                Debug.LogWarning($"{VRCQuestTools.Name} Replaced object {replaced} is already registered.");
                return;
            }
            ObjectRegistry.RegisterReplacedObject(original, replaced);
            registeredObjects.Add(replaced);
        }
    }
}
