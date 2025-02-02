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
        protected override string Description => VRCQuestToolsSettings.I18nResource.MaterialSwapEditorDescription;

        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            serializedObject.Update();

            // Draw material mappings list
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("materialMappings"), new GUIContent(i18n.MaterialSwapEditorMaterialMappingsLabel));
            EditorGUILayout.Space();

            // Validation warnings
            var component = target as MaterialSwap;
            if (component.GetComponentsInChildren<Renderer>(true).Length == 0)
            {
                EditorGUILayout.HelpBox(i18n.MaterialSwapEditorNoRendererWarning, MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
