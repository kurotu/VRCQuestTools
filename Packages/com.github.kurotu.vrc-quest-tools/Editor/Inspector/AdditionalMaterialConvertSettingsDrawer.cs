using System.Linq;
using KRT.VRCQuestTools.Models;
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

                EditorGUILayout.PropertyField(property.FindPropertyRelative("targetMaterial"), new GUIContent(i18n.AvatarConverterMaterialLabel));
                var settings = property.FindPropertyRelative("materialConvertSettings");
                EditorGUILayout.PropertyField(settings, new GUIContent($"{i18n.AvatarConverterMaterialConvertSettingLabel} ({settings.managedReferenceFullTypename.Split('.').Last()})"));

                EditorGUI.indentLevel--;
            }
        }
    }
}
