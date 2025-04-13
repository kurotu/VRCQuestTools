using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for ToonLitConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(MaterialSwap.MaterialMapping))]
    internal class MaterialMappingDrawer : PropertyDrawer
    {
        private readonly float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2.0f;

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0.0f;

            if (label.text != string.Empty || label.image != null)
            {
                height += EditorGUIUtility.singleLineHeight;
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("originalMaterial"));
            height += EditorGUIUtility.standardVerticalSpacing;

            var replacementMaterial = property.FindPropertyRelative("replacementMaterial");
            height += EditorGUI.GetPropertyHeight(replacementMaterial);
            height += EditorGUIUtility.standardVerticalSpacing;

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

                if (label.text != string.Empty || label.image != null)
                {
                    EditorGUI.LabelField(fieldRect, label);
                    fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.indentLevel++;

                var leftRect = fieldRect;
                leftRect.width = fieldRect.width * 0.75f;
                var rightRect = fieldRect;
                rightRect.width = fieldRect.width * 0.25f;
                rightRect.x = leftRect.x + leftRect.width;
                var originalMaterial = property.FindPropertyRelative("originalMaterial");
                EditorGUI.PropertyField(leftRect, originalMaterial, new GUIContent(i18n.MaterialSwapEditorOriginalMaterialLabel));
                if (GUI.Button(rightRect, i18n.MaterialSwapEditorSelectMaterialLabel))
                {
                    OnClickMaterialSelectButton(originalMaterial);
                }
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var replacementMaterial = property.FindPropertyRelative("replacementMaterial");
                EditorGUI.PropertyField(fieldRect, replacementMaterial, new GUIContent(i18n.MaterialSwapEditorReplacementMaterialLabel));

                if (ShouldShowMaterialWarning(replacementMaterial))
                {
                    fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    fieldRect.height = EditorGUIUtility.singleLineHeight * 2;
                    EditorGUI.HelpBox(fieldRect, i18n.MaterialSwapEditorReplacementMaterialError, MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void OnClickMaterialSelectButton(SerializedProperty materialProperty)
        {
            var materialSwap = (MaterialSwap)materialProperty.serializedObject.targetObject;
            var materials = GetChildrenMaterials(materialSwap).ToList();
            materials.Sort((a, b) => a.name.CompareTo(b.name));

            Views.AvatarMaterialSelectorWindow.Open((Material)materialProperty.objectReferenceValue, materials.ToArray(), m =>
            {
                materialProperty.objectReferenceValue = m;
                materialProperty.serializedObject.ApplyModifiedProperties();
            });
        }

        private bool ShouldShowMaterialWarning(SerializedProperty replacementMaterial)
        {
            Material replacement = (Material)replacementMaterial.objectReferenceValue;
            return replacement != null && !VRCSDKUtility.IsMaterialAllowedForQuestAvatar(replacement);
        }

        private Material[] GetChildrenMaterials(MaterialSwap materialSwap)
        {
            return VRChatAvatar.GetRelatedMaterials(materialSwap.gameObject);
        }
    }
}
