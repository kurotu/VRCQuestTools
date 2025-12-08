using System.Linq;
using KRT.VRCQuestTools.Components;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility class for working with VRCQuestTools components.
    /// </summary>
    internal static class ComponentUtility
    {
        /// <summary>
        /// Gets the primary material conversion component from the specified GameObject.
        /// </summary>
        /// <param name="rootObject">The root GameObject to search for the material conversion component.</param>
        /// <returns>The primary material conversion component if found; otherwise, <c>null</c>.</returns>
        public static IMaterialConversionComponent GetPrimaryMaterialConversionComponent(GameObject rootObject)
        {
            var components = rootObject.GetComponents<IMaterialConversionComponent>();
            return components.FirstOrDefault(c => c.IsPrimaryRoot);
        }
    }
}
