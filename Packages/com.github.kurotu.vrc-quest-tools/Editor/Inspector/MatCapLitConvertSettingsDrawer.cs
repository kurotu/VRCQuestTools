using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for ToonLitConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(MatCapLitConvertSettings))]
    internal class MatCapLitConvertSettingsDrawer : PropertyDrawer
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
            height += ToonLitConvertSettingsDrawer.GetPropertyFieldsHeight(property) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;
            return height;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            if (label.text != string.Empty || label.image != null)
            {
                EditorGUILayout.LabelField(label);
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel++;
            ToonLitConvertSettingsDrawer.DrawPorpertyFields(position, property);
            fieldRect.y += ToonLitConvertSettingsDrawer.GetPropertyFieldsHeight(property) + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("matCapTexture"), new GUIContent(i18n.MatCapLitConvertSettingsMatCapTextureLabel));

            EditorGUI.indentLevel--;
        }
    }
}
