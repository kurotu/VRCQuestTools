using KRT.VRCQuestTools.Components;
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
            var editorState = EditorState.instance;

            serializedObject.Update();

            var useDefaultConversion = TargetComponent.IsPrimaryRoot;
            if (!useDefaultConversion)
            {
                EditorGUILayout.HelpBox(i18n.MaterialConversionSettingsEditorDefaultConversionWarning, MessageType.Info);
            }

            additionalMaterialConvertSettingsReorderableList ??= MaterialConversionGUI.CreateAdditionalMaterialConvertSettingsList(serializedObject, serializedObject.FindProperty(nameof(MaterialConversionSettings.additionalMaterialConvertSettings)));
            editorState.foldOutAdditionalMaterialSettings = MaterialConversionGUI.Draw(serializedObject, editorState.foldOutAdditionalMaterialSettings, additionalMaterialConvertSettingsReorderableList);

            editorState.foldOutAdvancedSettings = Views.EditorGUIUtility.Foldout(i18n.AdvancedConverterSettingsLabel, editorState.foldOutAdvancedSettings);
            if (editorState.foldOutAdvancedSettings)
            {
                using (var disabled = new EditorGUI.DisabledScope(!useDefaultConversion))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("removeExtraMaterialSlots"), new GUIContent(i18n.RemoveExtraMaterialSlotsLabel, i18n.RemoveExtraMaterialSlotsTooltip));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ndmfPhase"), new GUIContent(i18n.NdmfPhaseLabel, i18n.NdmfPhaseTooltip));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("enableMaterialPreview"), new GUIContent(i18n.EnableMaterialPreviewLabel, i18n.EnableMaterialPreviewTooltip));
                }
            }

            // Temporary force preview toggle (non-serialized), shown regardless of Advanced foldout state
            var comp2 = TargetComponent;
            var forceLabel2 = comp2.ForceMaterialPreview ? i18n.ForceMaterialPreviewDisableLabel : i18n.ForceMaterialPreviewEnableLabel;
            var oldBg2 = GUI.backgroundColor;
            if (comp2.ForceMaterialPreview)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUILayout.Button(new GUIContent(forceLabel2, i18n.ForceMaterialPreviewTooltip)))
            {
                comp2.forceMaterialPreview = !comp2.forceMaterialPreview;
            }
            GUI.backgroundColor = oldBg2;

            serializedObject.ApplyModifiedProperties();
        }

        private class EditorState : ScriptableSingleton<EditorState>
        {
            public bool foldOutAdditionalMaterialSettings = true;
            public bool foldOutAdvancedSettings;
        }
    }
}
