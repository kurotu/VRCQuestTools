using System.Linq;
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

                var isForDefault = !IsArrayElement(property);
                var popups = MaterialConvertSettingsTypes.GetDefaultConvertTypePopups(isForDefault);
                var selectedIndex = popups.FindIndex(t => t.Type == MaterialConvertSettingsType);
                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    selectedIndex = EditorGUI.Popup(fieldRect, label.text, selectedIndex, popups.Select(p => p.Label).ToArray());
                    if (ccs.changed)
                    {
                        var type = popups[selectedIndex].Type;
                        var newValue = System.Activator.CreateInstance(type);
                        if (newValue is IMaterialConvertSettings settings)
                        {
                            settings.LoadDefaultAssets();
                        }
                        property.managedReferenceValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
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

        private bool IsArrayElement(SerializedProperty property)
        {
            var path = property.propertyPath;
            var index = path.LastIndexOf(".Array.data[");
            if (index < 0)
            {
                return false;
            }
            var parentPath = path.Substring(0, index);
            var parentProperty = property.serializedObject.FindProperty(parentPath);
            if (parentProperty == null)
            {
                return false;
            }
            return parentProperty.isArray && parentProperty.propertyType != SerializedPropertyType.String;
        }
    }
}
