using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for ToonLitConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(ToonLitConvertSettings))]
    internal class ToonLitConvertSettingsDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0.0f;
            if (label.text != string.Empty || label.image != null)
            {
                height += EditorGUIUtility.singleLineHeight;
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            height += GetPropertyFieldsHeight(property);
            return height;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldRect = position;
            if (label.text != string.Empty || label.image != null)
            {
                EditorGUI.LabelField(fieldRect, label);
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel++;
            DrawPorpertyFields(fieldRect, property);
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw property fields for ToonLitConvertSettings.
        /// </summary>
        /// <param name="position">Position of property fields.</param>
        /// <param name="property">Serialized property of ToonLitConvertSettings.</param>
        internal static void DrawPorpertyFields(Rect position, SerializedProperty property)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            var generateQuestTextures = property.FindPropertyRelative("generateQuestTextures");
            EditorGUI.PropertyField(fieldRect, generateQuestTextures, new GUIContent(i18n.GenerateQuestTexturesLabel));
            fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            using (var disabled = new EditorGUI.DisabledScope(!generateQuestTextures.boolValue))
            {
                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("maxTextureSize"), new GUIContent(i18n.IMaterialConvertSettingsTexturesSizeLimitLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("mainTextureBrightness"), new GUIContent(i18n.IMaterialConvertSettingsMainTextureBrightnessLabel, i18n.IMaterialConvertSettingsMainTextureBrightnessTooltip));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("generateShadowFromNormalMap"), new GUIContent(i18n.ToonLitConvertSettingsGenerateShadowFromNormalMapLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
            }
        }

        /// <summary>
        /// Get height of property fields for ToonLitConvertSettings.
        /// </summary>
        /// <param name="property">Serialized property of ToonLitConvertSettings.</param>
        /// <returns>Height.</returns>
        internal static float GetPropertyFieldsHeight(SerializedProperty property)
        {
            var height = 0.0f;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateQuestTextures"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("maxTextureSize"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mainTextureBrightness"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateShadowFromNormalMap"));
            return height;
        }
    }
}
