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
        private readonly string[] mobileTextureFormatNames = Enum.GetNames(typeof(MobileTextureFormat))
            .Select(n => n.Replace('_', ' '))
            .ToArray();

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mobileTextureFormat = (MobileTextureFormat)property.enumValueIndex;

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    var labels = GetPopupLabels();
                    var newIndex = EditorGUI.Popup(position, label, property.enumValueIndex, labels);
                    if (ccs.changed)
                    {
                        property.enumValueIndex = newIndex;
                    }
                }
            }
        }

        private GUIContent[] GetPopupLabels()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            return mobileTextureFormatNames.Select((label, i) =>
                {
                    var enumValue = (MobileTextureFormat)Enum.Parse(typeof(MobileTextureFormat), Enum.GetNames(typeof(MobileTextureFormat))[i]);
                    
                    // DontOverride special handling
                    if (enumValue == MobileTextureFormat.DontOverride)
                    {
                        return $"{label.Replace("Dont", "Don't")}";
                    }
                    
                    // ASTC_4x4 is highest quality
                    if (enumValue == MobileTextureFormat.ASTC_4x4)
                    {
                        return $"{label} ({i18n.TextureFormatHighQuality})";
                    }
                    
                    // ASTC_12x12 is highest compression
                    if (enumValue == MobileTextureFormat.ASTC_12x12)
                    {
                        return $"{label} ({i18n.TextureFormatHighCompression})";
                    }
                    
                    return label;
                })
                .Select(label => new GUIContent(label))
                .ToArray();
        }
    }
}
