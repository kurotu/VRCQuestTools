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
    internal class MaterialReplaceSettingsDrawer : MaterialConvertSettingsDrawerBase
    {
        private readonly float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2.0f;

        /// <inheritdoc />
        protected override System.Type MaterialConvertSettingsType => typeof(MaterialReplaceSettings);

        /// <inheritdoc />
        protected override float GetPropertyFieldsHeight(SerializedProperty property)
        {
            var replacementMaterial = property.FindPropertyRelative("material");
            var height = EditorGUI.GetPropertyHeight(replacementMaterial);

            if (ShouldShowMaterialWarning(replacementMaterial))
            {
                height += helpBoxHeight;
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        /// <inheritdoc />
        protected override Rect DrawPropertyFields(Rect position, SerializedProperty property)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            var materialLabel = new GUIContent(i18n.MaterialReplaceSettingsMaterialLabel, i18n.MaterialReplaceSettingsMaterialTooltip);
            var replacementMaterial = property.FindPropertyRelative("material");
            EditorGUI.PropertyField(fieldRect, replacementMaterial, materialLabel);

            if (ShouldShowMaterialWarning(replacementMaterial))
            {
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                fieldRect.height = helpBoxHeight;
                EditorGUI.HelpBox(fieldRect, i18n.MaterialReplaceSettingsMaterialWarning, MessageType.Error);
            }

            return fieldRect;
        }

        private bool ShouldShowMaterialWarning(SerializedProperty replacementMaterial)
        {
            Material replacement = (Material)replacementMaterial.objectReferenceValue;
            return replacement != null && !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(replacement);
        }
    }
}
