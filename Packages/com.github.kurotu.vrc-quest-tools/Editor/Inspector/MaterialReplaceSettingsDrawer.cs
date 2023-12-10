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
            return 0.0f;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            if (label.text != string.Empty || label.image != null)
            {
                EditorGUILayout.LabelField(label);
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(property.FindPropertyRelative("material"), new GUIContent(i18n.MaterialReplaceSettingsMaterialLabel));

            EditorGUI.indentLevel--;
        }
    }
}
