using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for ToonLitConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(ToonLitConvertSettings))]
    internal class ToonLitConvertSettingsDrawer : MaterialConvertSettingsDrawerBase
    {
        /// <inheritdoc/>
        protected override System.Type MaterialConvertSettingsType => typeof(ToonLitConvertSettings);

        /// <inheritdoc/>
        protected override Rect DrawPropertyFields(Rect position, SerializedProperty property)
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

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("mobileTextureFormat"), new GUIContent(i18n.IMaterialConvertSettingsMobileTextureFormatLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("mainTextureBrightness"), new GUIContent(i18n.IMaterialConvertSettingsMainTextureBrightnessLabel, i18n.IMaterialConvertSettingsMainTextureBrightnessTooltip));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("generateShadowFromNormalMap"), new GUIContent(i18n.ToonLitConvertSettingsGenerateShadowFromNormalMapLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
            }

            EditorGUI.indentLevel--;

            return fieldRect;
        }

        /// <inheritdoc/>
        protected override float GetPropertyFieldsHeight(SerializedProperty property)
        {
            var height = 0.0f;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateQuestTextures"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("maxTextureSize"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mainTextureBrightness"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mobileTextureFormat"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateShadowFromNormalMap"));
            return height;
        }
    }
}
