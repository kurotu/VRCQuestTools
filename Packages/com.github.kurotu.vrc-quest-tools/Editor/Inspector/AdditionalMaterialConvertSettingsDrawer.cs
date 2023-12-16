using System;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for ToonLitConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(AdditionalMaterialConvertSettings))]
    internal class AdditionalMaterialConvertSettingsDrawer : PropertyDrawer
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

            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("targetMaterial"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("materialConvertSettings"));

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

            var leftRect = fieldRect;
            leftRect.width = fieldRect.width * 0.75f;
            var rightRect = fieldRect;
            rightRect.width = fieldRect.width * 0.25f;
            rightRect.x = leftRect.x + leftRect.width;
            var targetMaterial = property.FindPropertyRelative("targetMaterial");
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUI.PropertyField(leftRect, targetMaterial, new GUIContent(i18n.AdditionalMaterialConvertSettingsTargetMaterialLabel));
            }
            if (GUI.Button(rightRect, i18n.AdditionalMaterialConvertSettingsSelectMaterialLabel))
            {
                OnClickMaterialSelectButton(targetMaterial);
            }
            fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            using (var ccs = new EditorGUI.ChangeCheckScope())
            {
                var name = property.FindPropertyRelative("materialConvertSettings").managedReferenceFullTypename.Split(' ').Last();
                var type = SystemUtility.GetTypeByName(name);
                var selectedIndex = MaterialConvertSettingsTypes.Types.IndexOf(type);
                selectedIndex = EditorGUI.Popup(fieldRect, i18n.AvatarConverterMaterialConvertSettingLabel, selectedIndex, MaterialConvertSettingsTypes.GetConvertTypePopupLabels());
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (ccs.changed)
                {
                    var newType = MaterialConvertSettingsTypes.Types[selectedIndex];
                    property.FindPropertyRelative("materialConvertSettings").managedReferenceValue = Activator.CreateInstance(newType);
                }
            }

            var settings = property.FindPropertyRelative("materialConvertSettings");
            EditorGUI.PropertyField(fieldRect, settings, new GUIContent());

            EditorGUI.indentLevel--;
        }

        private void OnClickMaterialSelectButton(SerializedProperty materialProperty)
        {
            var settings = (AvatarConverterSettings)materialProperty.serializedObject.targetObject;
            var avatar = new VRChatAvatar(settings.AvatarDescriptor);
            var materials = avatar.Materials.ToList();
            materials.Sort((a, b) => a.name.CompareTo(b.name));

            Views.AvatarMaterialSelectorWindow.Open((Material)materialProperty.objectReferenceValue, materials.ToArray(), m =>
            {
                materialProperty.objectReferenceValue = m;
                materialProperty.serializedObject.ApplyModifiedProperties();
            });
        }
    }
}
