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
        /// List of possible convert types.
        /// </summary>
        private static readonly List<Type> Types = new List<Type>
        {
            typeof(ToonLitConvertSettings),
            typeof(MatCapLitConvertSettings),
            typeof(ToonStandardConvertSettings),
            typeof(MaterialReplaceSettings),
        };

        /// <summary>
        /// Get convert types and localized labels.
        /// </summary>
        /// <param name="isForDefaultConvertSettings">True if the popup is for the default convert settings.</param>
        /// <returns>Lcalized labels.</returns>
        internal static List<PopupItem> GetDefaultConvertTypePopups(bool isForDefaultConvertSettings)
        {
            var types = new List<Type>(Types);
            if (isForDefaultConvertSettings)
            {
                types.Remove(typeof(MaterialReplaceSettings));
            }
            return types.Select(t => new PopupItem(t, GetConvertTypePopupLabel(t))).ToList();
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
            else if (type == typeof(ToonStandardConvertSettings))
            {
                return i18n.MaterialConvertTypePopupLabelToonStandard;
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

        /// <summary>
        /// Represents a popup entry for material convert settings.
        /// </summary>
        internal class PopupItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PopupItem"/> class.
            /// </summary>
            /// <param name="type">IMaterialConvertSettings type.</param>
            /// <param name="label">Popup label.</param>
            public PopupItem(Type type, string label)
            {
                Type = type;
                Label = label;
            }

            /// <summary>
            /// Gets type of the material convert settings.
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// Gets label for the popup entry.
            /// </summary>
            public string Label { get; }
        }
    }
}
