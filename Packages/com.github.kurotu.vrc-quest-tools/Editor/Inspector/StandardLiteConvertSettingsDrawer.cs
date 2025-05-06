using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for StandardLiteConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(StandardLiteConvertSettings))]
    internal class StandardLiteConvertSettingsDrawer : MaterialConvertSettingsDrawerBase
    {
        /// <inheritdoc />
        protected override System.Type MaterialConvertSettingsType => typeof(StandardLiteConvertSettings);

        /// <inheritdoc />
        protected override Rect DrawPropertyFields(Rect position, SerializedProperty property)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var fieldRect = position;

            EditorGUI.indentLevel++;

            var generateQuestTextures = property.FindPropertyRelative("generateQuestTextures");
            EditorGUI.PropertyField(fieldRect, generateQuestTextures, new GUIContent(i18n.GenerateAndroidTexturesLabel, i18n.GenerateAndroidTexturesTooltip));
            fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            using (var disabled = new EditorGUI.DisabledScope(!generateQuestTextures.boolValue))
            {
                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("maxTextureSize"), new GUIContent(i18n.IMaterialConvertSettingsTexturesSizeLimitLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("mobileTextureFormat"), new GUIContent(i18n.IMaterialConvertSettingsMobileTextureFormatLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var useMinimumBrightness = property.FindPropertyRelative("useMinimumBrightness");
                EditorGUI.PropertyField(fieldRect, useMinimumBrightness, new GUIContent(i18n.StandardLiteConvertSettingsUseMinimumBrightnessLabel, i18n.StandardLiteConvertSettingsUseMinimumBrightnessTooltip));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                if (useMinimumBrightness.boolValue)
                {
                    EditorGUI.indentLevel++;
                    var autoMinimumBrightness = property.FindPropertyRelative("autoMinimumBrightness");
                    EditorGUI.PropertyField(fieldRect, autoMinimumBrightness, new GUIContent(i18n.StandardLiteConvertSettingsAutoMinimumBrightnessLabel, i18n.StandardLiteConvertSettingsAutoMinimumBrightnessTooltip));
                    fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    if (!autoMinimumBrightness.boolValue)
                    {
                        EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("minimumBrightness"), new GUIContent(i18n.StandardLiteConvertSettingsMinimumBrightnessLabel, i18n.StandardLiteConvertSettingsMinimumBrightnessTooltip));
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;

            return fieldRect;
        }

        /// <inheritdoc />
        protected override float GetPropertyFieldsHeight(SerializedProperty property)
        {
            var height = 0.0f;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateQuestTextures"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("maxTextureSize"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mobileTextureFormat"));
            height += EditorGUIUtility.standardVerticalSpacing;

            var useMinimumBrightness = property.FindPropertyRelative("useMinimumBrightness");
            height += EditorGUI.GetPropertyHeight(useMinimumBrightness);
            height += EditorGUIUtility.standardVerticalSpacing;

            if (useMinimumBrightness.boolValue)
            {
                var autoMinimumBrightness = property.FindPropertyRelative("autoMinimumBrightness");
                height += EditorGUI.GetPropertyHeight(autoMinimumBrightness);
                height += EditorGUIUtility.standardVerticalSpacing;
                if (!autoMinimumBrightness.boolValue)
                {
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("minimumBrightness"));
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
            }
            return height;
        }
    }
}
