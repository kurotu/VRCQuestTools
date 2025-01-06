using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Custom editor for MaterialSwap component.
    /// </summary>
    [CustomEditor(typeof(MaterialSwap))]
    internal class MaterialSwapEditor : VRCQuestToolsEditorOnlyEditorBase<MaterialSwap>
    {
        private ReorderableList materialMappingsList;

        protected override string Description => VRCQuestToolsSettings.I18nResource.MaterialSwapEditorDescription;

        private void OnEnable()
        {
            var materialMappings = serializedObject.FindProperty("materialMappings");
            materialMappingsList = new ReorderableList(serializedObject, materialMappings, true, true, true, true);

            materialMappingsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Material Mappings");
            };

            materialMappingsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = materialMappingsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                var halfWidth = rect.width / 2;
                var originalRect = new Rect(rect.x, rect.y, halfWidth - 5, rect.height);
                var replacementRect = new Rect(rect.x + halfWidth + 5, rect.y, halfWidth - 5, rect.height);

                var i18n = VRCQuestToolsSettings.I18nResource;
                EditorGUI.PropertyField(originalRect, element.FindPropertyRelative("originalMaterial"), 
                    new GUIContent(i18n.MaterialSwapEditorOriginalMaterialLabel));
                EditorGUI.PropertyField(replacementRect, element.FindPropertyRelative("replacementMaterial"), 
                    new GUIContent(i18n.MaterialSwapEditorReplacementMaterialLabel));
            };

            materialMappingsList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
        }

        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            serializedObject.Update();

            // Draw material mappings list
            EditorGUILayout.Space();
            materialMappingsList.DoLayoutList();
            EditorGUILayout.Space();

            // Platform toggle
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enabledOnAndroid"),
                new GUIContent(i18n.MaterialSwapEditorEnabledOnAndroidLabel));

            // Child inclusion toggle  
            EditorGUILayout.PropertyField(serializedObject.FindProperty("includeChildren"),
                new GUIContent(i18n.MaterialSwapEditorIncludeChildrenLabel));

            // Validation warnings
            var component = target as MaterialSwap;
            if (component.GetComponent<Renderer>() == null)
            {
                EditorGUILayout.HelpBox(i18n.MaterialSwapEditorNoRendererWarning, MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}