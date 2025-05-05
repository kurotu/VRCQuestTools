using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for ToonLitConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(MatCapLitConvertSettings))]
    internal class MatCapLitConvertSettingsDrawer : ToonLitConvertSettingsDrawer
    {
        /// <inheritdoc />
        protected override System.Type MaterialConvertSettingsType => typeof(MatCapLitConvertSettings);

        /// <inheritdoc />
        protected override float GetPropertyFieldsHeight(SerializedProperty property)
        {
            var height = base.GetPropertyFieldsHeight(property);

            height += EditorGUIUtility.standardVerticalSpacing;

            var matCapTexture = property.FindPropertyRelative("matCapTexture");
            height += EditorGUI.GetPropertyHeight(matCapTexture);
            if (matCapTexture.objectReferenceValue == null)
            {
                height += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        /// <inheritdoc />
        protected override Rect DrawPropertyFields(Rect position, SerializedProperty property)
        {
            var fieldRect = base.DrawPropertyFields(position, property);

            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUI.indentLevel++;

            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            var matCapTexture = property.FindPropertyRelative("matCapTexture");
            EditorGUI.PropertyField(fieldRect, matCapTexture, new GUIContent(i18n.MatCapLitConvertSettingsMatCapTextureLabel));
            if (matCapTexture.objectReferenceValue == null)
            {
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                fieldRect.height = EditorGUIUtility.singleLineHeight * 2;
                const float indentWidth = 12;
                fieldRect.x += indentWidth;
                EditorGUI.HelpBox(fieldRect, i18n.MatCapLitConvertSettingsMatCapTextureWarning, MessageType.Warning);
                fieldRect.x -= indentWidth;
                fieldRect.height = EditorGUIUtility.singleLineHeight;
            }

            EditorGUI.indentLevel--;
            return fieldRect;
        }
    }
}
