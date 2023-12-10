using System;
using System.Collections.Generic;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Represents material convert settings types.
    /// </summary>
    internal static class MaterialConvertSettingsTypes
    {
        /// <summary>
        /// List of default convert types.
        /// </summary>
        internal static readonly List<Type> DefaultTypes = new List<Type>
        {
            typeof(ToonLitConvertSettings),
        };

        /// <summary>
        /// List of possible convert types.
        /// </summary>
        internal static readonly List<Type> Types = new List<Type>
        {
            typeof(ToonLitConvertSettings),
            typeof(MaterialReplaceSettings),
        };
    }
}
