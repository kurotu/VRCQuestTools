using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Base class of PropertyDrawer for IMaterialConvertSettings.
    /// </summary>
    internal abstract class MaterialConvertSettingsDrawerBase : PropertyDrawer
    {
        /// <summary>
        /// Gets the type of IMaterialConvertSettings.
        /// </summary>
        protected abstract System.Type MaterialConvertSettingsType { get; }

        /// <inheritdoc />
        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0.0f;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            height += GetPropertyFieldsHeight(property);
            return height;
        }

        /// <inheritdoc />
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                var fieldRect = position;
                fieldRect.height = EditorGUIUtility.singleLineHeight;

                var selectedIndex = MaterialConvertSettingsTypes.Types.IndexOf(MaterialConvertSettingsType);
                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    selectedIndex = EditorGUI.Popup(fieldRect, label.text, selectedIndex, MaterialConvertSettingsTypes.GetConvertTypePopupLabels());
                    if (ccs.changed)
                    {
                        var type = MaterialConvertSettingsTypes.Types[selectedIndex];
                        property.managedReferenceValue = System.Activator.CreateInstance(type);
                        return;
                    }
                }
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                fieldRect = DrawPropertyFields(fieldRect, property);
            }
        }

        /// <summary>
        /// Draw property fields for IMaterialConvertSettings.
        /// </summary>
        /// <param name="property">Serialized property of IMaterialConvertSettings.</param>
        /// <returns>Height.</returns>
        protected abstract float GetPropertyFieldsHeight(SerializedProperty property);

        /// <summary>
        /// Get height of property fields for IMaterialConvertSettings.
        /// </summary>
        /// <param name="fieldRect">Position of property fields.</param>
        /// <param name="property">Serialized property of IMaterialConvertSettings.</param>
        /// <returns>Updated fieldRect.</returns>
        protected abstract Rect DrawPropertyFields(Rect fieldRect, SerializedProperty property);
    }
}
