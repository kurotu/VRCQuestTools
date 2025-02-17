using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
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
        private readonly float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2.0f;

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0.0f;
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;

            var replacementMaterial = property.FindPropertyRelative("material");
            if (ShouldShowMaterialWarning(replacementMaterial))
            {
                height += helpBoxHeight;
                height += EditorGUIUtility.standardVerticalSpacing;
            }
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
                var replacementMaterial = property.FindPropertyRelative("material");
                EditorGUI.PropertyField(fieldRect, replacementMaterial, materialLabel);

                if (ShouldShowMaterialWarning(replacementMaterial))
                {
                    fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    fieldRect.height = helpBoxHeight;
                    EditorGUI.HelpBox(fieldRect, i18n.MaterialReplaceSettingsMaterialWarning, MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }
        }

        private bool ShouldShowMaterialWarning(SerializedProperty replacementMaterial)
        {
            Material replacement = (Material)replacementMaterial.objectReferenceValue;
            return replacement != null && !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(replacement);
        }
    }
}
