using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDKBase;

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

#if VQT_HAS_NDMF
            // Temporary force preview toggle (non-serialized), shown regardless of Advanced foldout state
            using (var disabledPreview = new EditorGUI.DisabledGroupScope(IsMobileBuildTarget(TargetComponent.gameObject)))
            {
                var component = TargetComponent;
                var forceLabel = component.ForceMaterialPreview ? i18n.ForceMaterialPreviewDisableLabel : i18n.ForceMaterialPreviewEnableLabel;
                var oldBg = GUI.backgroundColor;
                if (component.ForceMaterialPreview)
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button(new GUIContent("[NDMF] " + forceLabel, i18n.ForceMaterialPreviewTooltip)))
                {
                    component.forceMaterialPreview = !component.forceMaterialPreview;
                }
                GUI.backgroundColor = oldBg;
            }
#endif

            serializedObject.ApplyModifiedProperties();
        }

#if VQT_HAS_NDMF
        // MaterialConversionSettings may be on a child object rather than the avatar root,
        // so the avatar descriptor is resolved via the parent hierarchy.
        private static bool IsMobileBuildTarget(GameObject componentGameObject)
        {
            var avatarDescriptor = componentGameObject.GetComponentInParent<VRC_AvatarDescriptor>(true);
            if (avatarDescriptor == null)
            {
                return false;
            }

            var targetSettings = avatarDescriptor.GetComponent<PlatformTargetSettings>();
            var buildTarget = targetSettings != null ? targetSettings.buildTarget : Models.BuildTarget.Auto;
            if (buildTarget == Models.BuildTarget.Auto)
            {
                buildTarget = EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS
                    ? Models.BuildTarget.Android
                    : Models.BuildTarget.PC;
            }

            return buildTarget == Models.BuildTarget.Android;
        }
#endif

        private class EditorState : ScriptableSingleton<EditorState>
        {
            public bool foldOutAdditionalMaterialSettings = true;
            public bool foldOutAdvancedSettings;
        }
    }
}
