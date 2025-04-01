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

                var selectedIndex = MaterialConvertSettingsTypes.Types.IndexOf(typeof(ToonLitConvertSettings));
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
        /// Draw property fields for ToonLitConvertSettings.
        /// </summary>
        /// <param name="position">Position of property fields.</param>
        /// <param name="property">Serialized property of ToonLitConvertSettings.</param>
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

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("mainTextureBrightnessMode"), new GUIContent(i18n.IMaterialConvertSettingsMainTextureBrightnessModeLabel, i18n.IMaterialConvertSettingsMainTextureBrightnessModeTooltip));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("mainTextureBrightness"), new GUIContent(i18n.IMaterialConvertSettingsMainTextureBrightnessLabel, i18n.IMaterialConvertSettingsMainTextureBrightnessTooltip));
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("generateShadowFromNormalMap"), new GUIContent(i18n.ToonLitConvertSettingsGenerateShadowFromNormalMapLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
            }

            EditorGUI.indentLevel--;
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
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mainTextureBrightnessMode"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mainTextureBrightness"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateShadowFromNormalMap"));
            return height;
        }
    }
}
