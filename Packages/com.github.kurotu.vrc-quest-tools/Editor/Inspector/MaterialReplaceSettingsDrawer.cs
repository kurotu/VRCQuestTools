using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for ToonLitConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(MaterialReplaceSettings))]
    internal class MaterialReplaceSettingsDrawer : PropertyDrawer
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
                EditorGUI.LabelField(fieldRect, label);
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel++;

            var materialLabel = new GUIContent(i18n.MaterialReplaceSettingsMaterialLabel, i18n.MaterialReplaceSettingsMaterialTooltip);
            EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("material"), materialLabel);

            EditorGUI.indentLevel--;
        }
    }
}
