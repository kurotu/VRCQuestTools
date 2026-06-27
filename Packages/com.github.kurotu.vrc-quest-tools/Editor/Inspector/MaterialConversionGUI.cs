using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

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
                    additionalMaterialConvertSettingsReorderableList.DoLayoutList();
                }
            }

            var targetGameObject = targetComponent.gameObject;
            // Get the avatar root object using VRCSDKUtility
            var searchRoot = VRCSDKUtility.GetAvatarRoot(targetGameObject);

            // If no avatar root found, find the topmost IMaterialConversionComponent in the hierarchy
            if (searchRoot == null)
            {
                var current = targetComponent.transform;
                GameObject topmostWithMaterialConversion = null;
                while (current != null)
                {
                    if (current.GetComponent<IMaterialConversionComponent>() != null)
                    {
                        topmostWithMaterialConversion = current.gameObject;
                    }
                    current = current.parent;
                }
                searchRoot = topmostWithMaterialConversion;
            }
            if (searchRoot == null)
            {
                searchRoot = targetComponent.gameObject;
            }

            var materialConversionComponents = searchRoot.GetComponentsInChildren<IMaterialConversionComponent>(true);
            var materialSwapComponents = searchRoot.GetComponentsInChildren<MaterialSwap>(true);
            var unverifiedMaterials = VRChatAvatar.GetRelatedMaterials(targetGameObject)
                .Where(m => VRCQuestTools.AvatarConverter.MaterialWrapperBuilder.DetectShaderCategory(m) == MaterialWrapperBuilder.ShaderCategory.Unverified
                       && !IsHandledMaterial(m, materialConversionComponents, materialSwapComponents))
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

        /// <summary>
        /// Creates a reorderable list for additional material conversion settings.
        /// </summary>
        /// <param name="so">Target serialized object.</param>
        /// <param name="additionalMaterialConvertSettings">Property representing additional material conversion settings.</param>
        /// <returns>A reorderable list.</returns>
        internal static ReorderableList CreateAdditionalMaterialConvertSettingsList(SerializedObject so, SerializedProperty additionalMaterialConvertSettings)
        {
            ReorderableList additionalMaterialConvertSettingsReorderableList = new ReorderableList(so, additionalMaterialConvertSettings, true, false, true, true);
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
            return additionalMaterialConvertSettingsReorderableList;
        }

        /// <summary>
        /// Checks if a material is handled by any material conversion or replacement settings.
        /// </summary>
        /// <param name="material">The material to check.</param>
        /// <param name="materialConversionComponents">All components implementing IMaterialConversionComponent in the avatar hierarchy.</param>
        /// <param name="materialSwapComponents">All MaterialSwap components in the avatar hierarchy.</param>
        /// <returns>True if the material is handled by conversion or replacement settings.</returns>
        private static bool IsHandledMaterial(Material material, IMaterialConversionComponent[] materialConversionComponents, MaterialSwap[] materialSwapComponents)
        {
            if (material == null)
            {
                return false;
            }

            // Check if material is handled by additional material conversion settings
            // Check all components that implement IMaterialConversionComponent in the avatar hierarchy
            foreach (var component in materialConversionComponents)
            {
                if (CheckMaterialInConversionSettings(material, component))
                {
                    return true;
                }
            }

            // Check if material is handled by MaterialSwap components in the avatar hierarchy
            foreach (var materialSwap in materialSwapComponents)
            {
                if (materialSwap.materialMappings != null)
                {
                    foreach (var mapping in materialSwap.materialMappings)
                    {
                        if (mapping.originalMaterial == material && mapping.replacementMaterial != null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a material is handled by the conversion settings of a specific component.
        /// </summary>
        /// <param name="material">The material to check.</param>
        /// <param name="materialConversionComponent">The component to check settings for.</param>
        /// <returns>True if the material is handled by the component's settings.</returns>
        private static bool CheckMaterialInConversionSettings(Material material, IMaterialConversionComponent materialConversionComponent)
        {
            var additionalSettings = materialConversionComponent.AdditionalMaterialConvertSettings;
            if (additionalSettings != null)
            {
                foreach (var setting in additionalSettings)
                {
                    if (setting.targetMaterial == material)
                    {
                        // Check if it's a MaterialReplaceSettings with a valid replacement
                        if (setting.materialConvertSettings is MaterialReplaceSettings replaceSettings)
                        {
                            return replaceSettings.material != null;
                        }

                        // For other conversion settings, consider them as handled
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
