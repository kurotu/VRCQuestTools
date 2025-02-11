using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Custom editor for MaterialSwap component.
    /// </summary>
    [CustomEditor(typeof(MaterialSwap))]
    internal class MaterialSwapEditor : VRCQuestToolsEditorOnlyEditorBase<MaterialSwap>
    {
        /// <inheritdoc />
        protected override string Description => VRCQuestToolsSettings.I18nResource.MaterialSwapEditorDescription;

        /// <inheritdoc />
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            serializedObject.Update();

            // Draw material mappings list
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("materialMappings"), new GUIContent(i18n.MaterialSwapEditorMaterialMappingsLabel));
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
