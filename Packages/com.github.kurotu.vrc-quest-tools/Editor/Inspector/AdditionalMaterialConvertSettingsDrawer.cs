using System;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
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
            return 0.0f;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                using (new EditorGUILayout.HorizontalScope())
                {
                    var targetMaterial = property.FindPropertyRelative("targetMaterial");
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.PropertyField(targetMaterial, new GUIContent(i18n.AdditionalMaterialConvertSettingsTargetMaterialLabel));
                    }
                    if (GUILayout.Button(i18n.AdditionalMaterialConvertSettingsSelectMaterialLabel))
                    {
                        OnClickMaterialSelectButton(targetMaterial);
                    }
                }

                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    var name = property.FindPropertyRelative("materialConvertSettings").managedReferenceFullTypename.Split(' ').Last();
                    var type = SystemUtility.GetTypeByName(name);
                    var selectedIndex = MaterialConvertSettingsTypes.Types.IndexOf(type);
                    selectedIndex = EditorGUILayout.Popup(i18n.AvatarConverterMaterialConvertSettingLabel, selectedIndex, MaterialConvertSettingsTypes.GetConvertTypePopupLabels());
                    if (ccs.changed)
                    {
                        var newType = MaterialConvertSettingsTypes.Types[selectedIndex];
                        property.FindPropertyRelative("materialConvertSettings").managedReferenceValue = Activator.CreateInstance(newType);
                    }
                }

                var settings = property.FindPropertyRelative("materialConvertSettings");
                EditorGUILayout.PropertyField(settings, new GUIContent());

                EditorGUI.indentLevel--;
            }
        }

        private void OnClickMaterialSelectButton(SerializedProperty materialProperty)
        {
            var settings = (AvatarConverterSettings)materialProperty.serializedObject.targetObject;
            var avatar = new VRChatAvatar(settings.AvatarDescriptor);
            var materials = avatar.Materials.ToList();
            materials.Sort((a, b) => a.name.CompareTo(b.name));

            AvatarMaterialSelectorWindow.Open((Material)materialProperty.objectReferenceValue, materials.ToArray(), m =>
            {
                materialProperty.objectReferenceValue = m;
                materialProperty.serializedObject.ApplyModifiedProperties();
            });
        }
    }
}
