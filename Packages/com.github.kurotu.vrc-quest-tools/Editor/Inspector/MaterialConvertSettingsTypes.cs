using System;
using System.Collections.Generic;
using System.Linq;

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
            typeof(MatCapLitConvertSettings),
            typeof(StandardLiteConvertSettings),
        };

        /// <summary>
        /// List of possible convert types.
        /// </summary>
        internal static readonly List<Type> Types = new List<Type>
        {
            typeof(ToonLitConvertSettings),
            typeof(MatCapLitConvertSettings),
            typeof(StandardLiteConvertSettings),
            typeof(MaterialReplaceSettings),
        };

        /// <summary>
        /// Get localized labels for default convert types.
        /// </summary>
        /// <returns>Lcalized labels.</returns>
        internal static string[] GetDefaultConvertTypePopupLabels()
        {
            return DefaultTypes.Select(GetConvertTypePopupLabel).ToArray();
        }

        /// <summary>
        /// Get localized labels for possible convert types.
        /// </summary>
        /// <returns>Localized labels.</returns>
        internal static string[] GetConvertTypePopupLabels()
        {
            return Types.Select(GetConvertTypePopupLabel).ToArray();
        }

        private static string GetConvertTypePopupLabel(Type type)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            if (type == typeof(ToonLitConvertSettings))
            {
                return i18n.MaterialConvertTypePopupLabelToonLit;
            }
            else if (type == typeof(MatCapLitConvertSettings))
            {
                return i18n.MaterialConvertTypePopupLabelMatCapLit;
            }
            else if (type == typeof(MaterialReplaceSettings))
            {
                return i18n.MaterialConvertTypePopupLabelMaterialReplace;
            }
            else
            {
                return type.Name;
            }
        }
    }
}
