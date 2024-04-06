#if !VQT_HAS_NDMF_ERROR_REPORT
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf.Dummy
{
    /// <summary>
    /// ObjectReference is a dummy class for NDMF.
    /// </summary>
    internal class ObjectReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectReference"/> class.
        /// </summary>
        /// <param name="obj">Object.</param>
        internal ObjectReference(Object obj)
        {
            Object = obj;
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        internal Object Object { get; }
    }
}
#endif
