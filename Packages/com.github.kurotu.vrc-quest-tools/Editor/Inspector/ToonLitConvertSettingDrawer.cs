using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for ToonLitConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(ToonLitConvertSetting))]
    internal class ToonLitConvertSettingDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0.0f;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUILayout.LabelField(label);

            EditorGUI.indentLevel++;

            var generateQuestTextures = property.FindPropertyRelative("generateQuestTextures");
            EditorGUILayout.PropertyField(generateQuestTextures, new GUIContent(i18n.GenerateQuestTexturesLabel));
            using (var disabled = new EditorGUI.DisabledScope(!generateQuestTextures.boolValue))
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("maxTextureSize"), new GUIContent(i18n.TexturesSizeLimitLabel));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("mainTextureBrightness"), new GUIContent(i18n.MainTextureBrightnessLabel, i18n.MainTextureBrightnessTooltip));
            }

            EditorGUI.indentLevel--;
        }
    }
}
