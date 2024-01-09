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
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            height += ToonLitConvertSettingsDrawer.GetPropertyFieldsHeight(property) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;

            var matCapTexture = property.FindPropertyRelative("matCapTexture");
            if (matCapTexture.objectReferenceValue == null)
            {
                height += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
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

                var selectedIndex = MaterialConvertSettingsTypes.Types.IndexOf(typeof(MatCapLitConvertSettings));
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

                ToonLitConvertSettingsDrawer.DrawPorpertyFields(fieldRect, property);
                EditorGUI.indentLevel++;
                fieldRect.y += ToonLitConvertSettingsDrawer.GetPropertyFieldsHeight(property) + EditorGUIUtility.standardVerticalSpacing;

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
            }
        }
    }
}
