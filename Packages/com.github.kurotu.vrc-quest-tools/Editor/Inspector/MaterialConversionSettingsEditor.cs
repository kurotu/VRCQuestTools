using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for <see cref="MaterialConversionSettings"/>.
    /// </summary>
    [CustomEditor(typeof(MaterialConversionSettings))]
    internal class MaterialConversionSettingsEditor : VRCQuestToolsEditorOnlyEditorBase<MaterialConversionSettings>
    {
        private I18nBase i18n = VRCQuestToolsSettings.I18nResource;
        private ReorderableList materialConvertSettingsReorderableList;

        /// <inheritdoc/>
        protected override string Description
        {
            get
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                return i18n.MaterialConversionSettingsEditorDescription;
            }
        }

        /// <inheritdoc />
        public override void OnInspectorGUIInternal()
        {
            i18n = VRCQuestToolsSettings.I18nResource;
            var editorState = EditorState.instance;

            serializedObject.Update();
            var materialConvertSettings = serializedObject.FindProperty("materialConvertSettings");

            var headerRect = new Rect(EditorGUILayout.GetControlRect());
            using (var property = new EditorGUI.PropertyScope(headerRect, new GUIContent(i18n.MaterialConversionSettingsEditorConversionSettingsLabel), materialConvertSettings))
            {
                editorState.foldOutmaterialSettings = EditorGUI.Foldout(headerRect, editorState.foldOutmaterialSettings, property.content, true);
                if (editorState.foldOutmaterialSettings)
                {
                    if (materialConvertSettingsReorderableList == null)
                    {
                        materialConvertSettingsReorderableList = new ReorderableList(serializedObject, materialConvertSettings, true, false, true, true);
                        materialConvertSettingsReorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                        {
                            EditorGUI.PropertyField(rect, materialConvertSettings.GetArrayElementAtIndex(index));
                            serializedObject.ApplyModifiedProperties();
                        };
                        materialConvertSettingsReorderableList.elementHeightCallback = (index) =>
                        {
                            var element = materialConvertSettings.GetArrayElementAtIndex(index);
                            return EditorGUI.GetPropertyHeight(element);
                        };
                        materialConvertSettingsReorderableList.onAddCallback = (list) =>
                        {
                            var index = list.serializedProperty.arraySize;
                            list.serializedProperty.arraySize++;
                            list.index = index;
                            var element = list.serializedProperty.GetArrayElementAtIndex(index);
                            element.managedReferenceValue = new AdditionalMaterialConvertSettings();
                            serializedObject.ApplyModifiedProperties();
                        };
                        materialConvertSettingsReorderableList.onRemoveCallback = (list) =>
                        {
                            if (list.index < 0 || list.index >= list.serializedProperty.arraySize)
                            {
                                return;
                            }
                            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                            serializedObject.ApplyModifiedProperties();
                        };
                    }
                    materialConvertSettingsReorderableList.DoLayoutList();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private class EditorState : ScriptableSingleton<EditorState>
        {
            public bool foldOutmaterialSettings;
        }
    }
}
