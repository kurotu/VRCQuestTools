using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Handles the GUI for material conversion settings in the inspector.
    /// </summary>
    internal static class MaterialConversionGUI
    {
        /// <summary>
        /// Draws the material conversion settings in the inspector.
        /// </summary>
        /// <param name="so">SerializedObject of the target component.</param>
        /// <param name="foldOutAdditionalMaterialSettings">Whether to show the foldout for additional material settings.</param>
        /// <param name="additionalMaterialConvertSettingsReorderableList">ReorderableList for additional material convert settings.</param>
        /// <returns>Updated foldOutAdditionalMaterialSettings.</returns>
        internal static bool Draw(SerializedObject so, bool foldOutAdditionalMaterialSettings, ReorderableList additionalMaterialConvertSettingsReorderableList)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var targetComponent = (Component)so.targetObject;

            var useDefaultConversion = ((IMaterialConversionComponent)targetComponent).IsPrimaryRoot;
            using (var disabled = new EditorGUI.DisabledScope(!useDefaultConversion))
            {
                var defaultMaterialConvertSettings = so.FindProperty("defaultMaterialConvertSettings");
                EditorGUILayout.PropertyField(defaultMaterialConvertSettings, new GUIContent(i18n.AvatarConverterDefaultMaterialConvertSettingLabel));
            }

            var additionalMaterialConvertSettings = so.FindProperty("additionalMaterialConvertSettings");

            var headerRect = new Rect(EditorGUILayout.GetControlRect());
            using (var property = new EditorGUI.PropertyScope(headerRect, new GUIContent(i18n.AvatarConverterAdditionalMaterialConvertSettingsLabel), additionalMaterialConvertSettings))
            {
                foldOutAdditionalMaterialSettings = EditorGUI.Foldout(headerRect, foldOutAdditionalMaterialSettings, property.content, true);
                if (foldOutAdditionalMaterialSettings)
                {
                    if (additionalMaterialConvertSettingsReorderableList == null)
                    {
                        additionalMaterialConvertSettingsReorderableList = new ReorderableList(so, additionalMaterialConvertSettings, true, false, true, true);
                        additionalMaterialConvertSettingsReorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                        {
                            EditorGUI.PropertyField(rect, additionalMaterialConvertSettings.GetArrayElementAtIndex(index));
                            so.ApplyModifiedProperties();
                        };
                        additionalMaterialConvertSettingsReorderableList.elementHeightCallback = (index) =>
                        {
                            var element = additionalMaterialConvertSettings.GetArrayElementAtIndex(index);
                            return EditorGUI.GetPropertyHeight(element);
                        };
                        additionalMaterialConvertSettingsReorderableList.onAddCallback = (list) =>
                        {
                            var index = list.serializedProperty.arraySize;
                            list.serializedProperty.arraySize++;
                            list.index = index;
                            var element = list.serializedProperty.GetArrayElementAtIndex(index);
                            var newValue = new AdditionalMaterialConvertSettings();
                            newValue.LoadDefaultAssets();
                            element.managedReferenceValue = newValue;
                            so.ApplyModifiedProperties();
                        };
                        additionalMaterialConvertSettingsReorderableList.onRemoveCallback = (list) =>
                        {
                            if (list.index < 0 || list.index >= list.serializedProperty.arraySize)
                            {
                                return;
                            }
                            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                            so.ApplyModifiedProperties();
                        };
                    }
                    additionalMaterialConvertSettingsReorderableList.DoLayoutList();
                }
            }

            var targetGameObject = targetComponent.gameObject;
            var unverifiedMaterials = VRChatAvatar.GetRelatedMaterials(targetGameObject)
                .Where(m => VRCQuestTools.AvatarConverter.MaterialWrapperBuilder.DetectShaderCategory(m) == MaterialWrapperBuilder.ShaderCategory.Unverified)
                .ToArray();

            if (unverifiedMaterials.Length > 0)
            {
                Views.EditorGUIUtility.HelpBoxGUI(MessageType.Warning, () =>
                {
                    EditorGUILayout.LabelField(i18n.WarningForUnsupportedShaders, EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.Space(1);
                    EditorGUILayout.LabelField($"{i18n.SupportedShadersLabel}: Standard, UTS2, arktoon, AXCS, Sunao, lilToon, Poiyomi", EditorStyles.wordWrappedMiniLabel);
                    EditorGUI.BeginDisabledGroup(true);
                    foreach (var m in unverifiedMaterials)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(m, typeof(Material), false);
                        EditorGUILayout.ObjectField(m.shader, typeof(Shader), false);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.EndDisabledGroup();
                });
            }

            return foldOutAdditionalMaterialSettings;
        }
    }
}
