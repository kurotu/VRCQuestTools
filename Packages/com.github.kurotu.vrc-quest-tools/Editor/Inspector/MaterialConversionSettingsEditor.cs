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
        private ReorderableList additionalMaterialConvertSettingsReorderableList;

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
            var i18n = VRCQuestToolsSettings.I18nResource;
            i18n = VRCQuestToolsSettings.I18nResource;
            var editorState = EditorState.instance;

            serializedObject.Update();

            var useDefaultConversion = TargetComponent.IsPrimaryRoot;
            if (!useDefaultConversion)
            {
                EditorGUILayout.HelpBox(i18n.MaterialConversionSettingsEditorDefaultConversionWarning, MessageType.Info);
            }

            editorState.foldOutAdditionalMaterialSettings = MaterialConversionGUI.Draw(serializedObject, editorState.foldOutAdditionalMaterialSettings, additionalMaterialConvertSettingsReorderableList);

            editorState.foldOutAdvancedSettings = Views.EditorGUIUtility.Foldout(i18n.AdvancedConverterSettingsLabel, editorState.foldOutAdvancedSettings);
            if (editorState.foldOutAdvancedSettings)
            {
                using (var disabled = new EditorGUI.DisabledScope(!useDefaultConversion))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("removeExtraMaterialSlots"), new GUIContent(i18n.RemoveExtraMaterialSlotsLabel, i18n.RemoveExtraMaterialSlotsTooltip));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ndmfPhase"), new GUIContent(i18n.NdmfPhaseLabel, i18n.NdmfPhaseTooltip));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private class EditorState : ScriptableSingleton<EditorState>
        {
            public bool foldOutAdditionalMaterialSettings = true;
            public bool foldOutAdvancedSettings;
        }
    }
}
