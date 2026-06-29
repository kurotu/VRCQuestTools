using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

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
        /// Register a replaced object into the ambient ObjectRegistry. Intended for NDMF preview filters,
        /// whose <c>Instantiate</c>/<c>Refresh</c> calls run inside an active <c>ObjectRegistryScope</c>.
        /// Safe to call when no active registry exists or when the object is already registered.
        /// </summary>
        /// <param name="original">Original object.</param>
        /// <param name="replaced">Replaced object.</param>
        internal static void TryRegisterReplacedObjectToActiveRegistry(Object original, Object replaced)
        {
            if (original == null || replaced == null || original == replaced)
            {
                return;
            }
            var objRef = ObjectRegistry.GetReference(original);
#if VQT_NDMF_HAS_TRY_REGISTER_REPLACED_OBJECT
            // NDMF >= 1.6.8: dedicated no-throw API.
            ObjectRegistry.TryRegisterReplacedObject(objRef, replaced);
#else
            // NDMF < 1.6.8: RegisterReplacedObject throws ArgumentException when already registered.
            try
            {
                ObjectRegistry.RegisterReplacedObject(objRef, replaced);
            }
            catch (System.ArgumentException)
            {
            }
#endif
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
                Logger.LogWarning($"Replaced object {replaced} is already registered.");
                return;
            }
            ObjectRegistry.RegisterReplacedObject(original, replaced);
            registeredObjects.Add(replaced);
        }
    }
}
