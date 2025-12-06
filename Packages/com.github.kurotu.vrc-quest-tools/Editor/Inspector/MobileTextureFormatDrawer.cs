using System;
using System.Linq;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PropertyDrawer for MobileTextureFormat.
    /// </summary>
    [CustomPropertyDrawer(typeof(MobileTextureFormat))]
    internal class MobileTextureFormatDrawer : PropertyDrawer
    {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    var labels = GetPopupLabels();
                    var values = GetPopupValues();
                    
                    // Find the current value's display index
                    var currentValue = (MobileTextureFormat)property.enumValueIndex;
                    var currentDisplayIndex = Array.IndexOf(values, currentValue);
                    if (currentDisplayIndex < 0)
                    {
                        currentDisplayIndex = 0;
                    }
                    
                    var newDisplayIndex = EditorGUI.Popup(position, label, currentDisplayIndex, labels);
                    if (ccs.changed)
                    {
                        // Convert display index back to enum value index
                        var newValue = values[newDisplayIndex];
                        property.enumValueIndex = (int)newValue;
                    }
                }
            }
        }

        private GUIContent[] GetPopupLabels()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var allValues = (MobileTextureFormat[])Enum.GetValues(typeof(MobileTextureFormat));
            
            // Reorder: NoOverride first, then ASTC formats
            var orderedValues = allValues.OrderBy(v => v == MobileTextureFormat.NoOverride ? 0 : 1).ToArray();
            
            return orderedValues.Select(enumValue =>
                {
                    if (enumValue == MobileTextureFormat.NoOverride)
                    {
                        return new GUIContent("No Override");
                    }
                    
                    var label = enumValue.ToString().Replace('_', ' ');
                    
                    if (enumValue == MobileTextureFormat.ASTC_4x4)
                    {
                        return new GUIContent($"{label} ({i18n.TextureFormatHighQuality})");
                    }
                    
                    if (enumValue == MobileTextureFormat.ASTC_12x12)
                    {
                        return new GUIContent($"{label} ({i18n.TextureFormatHighCompression})");
                    }
                    
                    return new GUIContent(label);
                })
                .ToArray();
        }

        private MobileTextureFormat[] GetPopupValues()
        {
            var allValues = (MobileTextureFormat[])Enum.GetValues(typeof(MobileTextureFormat));
            
            // Reorder: NoOverride first, then ASTC formats
            return allValues.OrderBy(v => v == MobileTextureFormat.NoOverride ? 0 : 1).ToArray();
        }
    }
}
