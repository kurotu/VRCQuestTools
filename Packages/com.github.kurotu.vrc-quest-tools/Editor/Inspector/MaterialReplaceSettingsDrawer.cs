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
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;
            return height;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                var fieldRect = position;
                fieldRect.height = EditorGUIUtility.singleLineHeight;

                var selectedIndex = MaterialConvertSettingsTypes.Types.IndexOf(typeof(MaterialReplaceSettings));
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

                EditorGUI.indentLevel++;

                var materialLabel = new GUIContent(i18n.MaterialReplaceSettingsMaterialLabel, i18n.MaterialReplaceSettingsMaterialTooltip);
                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("material"), materialLabel);

                EditorGUI.indentLevel--;
            }
        }
    }
}
