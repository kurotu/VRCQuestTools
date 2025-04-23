using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for StandardLiteConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(StandardLiteConvertSettings))]
    internal class StandardLiteConvertSettingsDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0.0f;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            height += GetPropertyFieldsHeight(property);
            return height;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var fieldRect = position;
                fieldRect.height = EditorGUIUtility.singleLineHeight;

                var selectedIndex = MaterialConvertSettingsTypes.Types.IndexOf(typeof(StandardLiteConvertSettings));
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

                DrawPorpertyFields(fieldRect, property);
            }
        }

        /// <summary>
        /// Draw property fields for StandardLiteConvertSettings.
        /// </summary>
        /// <param name="position">Position of property fields.</param>
        /// <param name="property">Serialized property of StandardLiteConvertSettings.</param>
        internal static void DrawPorpertyFields(Rect position, SerializedProperty property)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.indentLevel++;

            var generateQuestTextures = property.FindPropertyRelative("generateQuestTextures");
            EditorGUI.PropertyField(fieldRect, generateQuestTextures, new GUIContent(i18n.GenerateAndroidTexturesLabel, i18n.GenerateAndroidTexturesTooltip));
            fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            using (var disabled = new EditorGUI.DisabledScope(!generateQuestTextures.boolValue))
            {
                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("maxTextureSize"), new GUIContent(i18n.IMaterialConvertSettingsTexturesSizeLimitLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var autoMinimumBrightness = property.FindPropertyRelative("autoMinimumBrightness");
                EditorGUI.PropertyField(fieldRect, autoMinimumBrightness, new GUIContent(i18n.StandardLiteConvertSettingsAutoMinimumBrightnessLabel, i18n.StandardLiteConvertSettingsAutoMinimumBrightnessTooltip));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                if (!autoMinimumBrightness.boolValue)
                {
                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("minimumBrightness"), new GUIContent(i18n.StandardLiteConvertSettingsMinimumBrightnessLabel, i18n.StandardLiteConvertSettingsMinimumBrightnessTooltip));
                    fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Get height of property fields for StandardLiteConvertSettings.
        /// </summary>
        /// <param name="property">Serialized property of StandardLiteConvertSettings.</param>
        /// <returns>Height.</returns>
        internal static float GetPropertyFieldsHeight(SerializedProperty property)
        {
            var height = 0.0f;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateQuestTextures"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("maxTextureSize"));
            height += EditorGUIUtility.standardVerticalSpacing;

            var autoMinimumBrightness = property.FindPropertyRelative("autoMinimumBrightness");
            height += EditorGUI.GetPropertyHeight(autoMinimumBrightness);
            height += EditorGUIUtility.standardVerticalSpacing;
            if (!autoMinimumBrightness.boolValue)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("minimumBrightness"));
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }
}
