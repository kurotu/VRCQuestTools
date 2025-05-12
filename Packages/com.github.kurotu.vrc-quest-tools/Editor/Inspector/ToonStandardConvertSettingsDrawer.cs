using System;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PropertyDrawer for ToonStandardConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(ToonStandardConvertSettings))]
    internal class ToonStandardConvertSettingsDrawer : MaterialConvertSettingsDrawerBase
    {
        /// <inheritdoc/>
        protected override Type MaterialConvertSettingsType => typeof(ToonStandardConvertSettings);

        /// <inheritdoc/>
        protected override Rect DrawPropertyFields(Rect fieldRect, SerializedProperty property)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUI.indentLevel++;

            var generateQuestTextures = property.FindPropertyRelative("generateQuestTextures");
            EditorGUI.PropertyField(fieldRect, generateQuestTextures, new GUIContent(i18n.GenerateAndroidTexturesLabel, i18n.GenerateAndroidTexturesTooltip));
            fieldRect.y += EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            using (var disabled = new EditorGUI.DisabledScope(!generateQuestTextures.boolValue))
            {
                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("maxTextureSize"), new GUIContent(i18n.IMaterialConvertSettingsTexturesSizeLimitLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("mobileTextureFormat"), new GUIContent(i18n.IMaterialConvertSettingsMobileTextureFormatLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
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
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mobileTextureFormat"));
            height += EditorGUIUtility.standardVerticalSpacing;
            return height;
        }
    }
}
