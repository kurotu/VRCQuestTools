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
            return 0.0f;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (label.text != string.Empty || label.image != null)
            {
                EditorGUILayout.LabelField(label);
            }

            EditorGUI.indentLevel++;
            DrawPorpertyFields(property);
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw property fields for ToonLitConvertSettings.
        /// </summary>
        /// <param name="property">ToonLitConvertSettings serialized property</param>
        internal static void DrawPorpertyFields(SerializedProperty property)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var generateQuestTextures = property.FindPropertyRelative("generateQuestTextures");
            EditorGUILayout.PropertyField(generateQuestTextures, new GUIContent(i18n.GenerateQuestTexturesLabel));
            using (var disabled = new EditorGUI.DisabledScope(!generateQuestTextures.boolValue))
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("maxTextureSize"), new GUIContent(i18n.IMaterialConvertSettingsTexturesSizeLimitLabel));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("mainTextureBrightness"), new GUIContent(i18n.IMaterialConvertSettingsMainTextureBrightnessLabel, i18n.IMaterialConvertSettingsMainTextureBrightnessTooltip));
            }
        }
    }
}
