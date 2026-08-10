using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for PlatformComponentRemover.
    /// </summary>
    [CustomEditor(typeof(PlatformComponentRemover))]
    internal class PlatformComponentRemoverEditor : VRCQuestToolsEditorOnlyEditorBase<PlatformComponentRemover>
    {
        private const float PCCheckboxWidth = 20f;
        private const float MobileCheckboxWidth = 50f;

        /// <inheritdoc/>
        protected override string Description => VRCQuestToolsSettings.I18nResource.PlatformComponentRemoverEditorDescription;

        /// <summary>
        /// Gets whether the avatar still stores its Avatar Dynamics settings in AvatarConverterSettings.
        /// In that case the manual conversion applies those settings instead of this component's ones.
        /// </summary>
        /// <param name="remover">Component to inspect.</param>
        /// <returns>true when the avatar has unmigrated Avatar Dynamics settings.</returns>
        internal static bool HasUnmigratedAvatarDynamicsSettings(PlatformComponentRemover remover)
        {
            if (remover == null)
            {
                return false;
            }

            var converterSettings = remover.GetComponentInParent<AvatarConverterSettings>();
            return converterSettings != null && converterSettings.HasLegacyAvatarDynamicsSettings;
        }

        /// <inheritdoc />
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

#if !VQT_HAS_NDMF
            if (HasUnmigratedAvatarDynamicsSettings(TargetComponent))
            {
                EditorGUILayout.HelpBox(i18n.PlatformComponentRemoverManualConversionUnavailable, MessageType.Warning);
            }
#endif

            TargetComponent.UpdateComponentSettings();

            var so = new SerializedObject(target);
            so.Update();

            var componentSettings = so.FindProperty("componentSettings");
            var componentSettingsLabelRect = EditorGUILayout.GetControlRect();
            var componentSettingsLabel = new GUIContent(i18n.PlatformComponentRemoverEditorComponentSettingsLabel, i18n.PlatformComponentRemoverEditorComponentSettingsTooltip);
            using (var property = new EditorGUI.PropertyScope(componentSettingsLabelRect, componentSettingsLabel, componentSettings))
            {
                EditorGUI.LabelField(componentSettingsLabelRect, property.content);
            }

            using (new EditorGUI.IndentLevelScope())
            {
                using (var horizontal = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(i18n.ComponentLabel, GUILayout.MinWidth(30));
                    using (new EditorGUI.IndentLevelScope(-1))
                    {
                        EditorGUILayout.LabelField(new GUIContent("PC", i18n.PlatformComponentRemoverEditorCheckboxPCTooltip), GUILayout.Width(PCCheckboxWidth));
                        EditorGUILayout.LabelField(new GUIContent("Mobile", i18n.PlatformComponentRemoverEditorCheckboxMobileTooltip), GUILayout.Width(MobileCheckboxWidth));
                    }
                }
                var count = componentSettings.arraySize;
                for (var i = 0; i < count; i++)
                {
                    var element = componentSettings.GetArrayElementAtIndex(i);
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        var component = element.FindPropertyRelative("component");
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            EditorGUILayout.PropertyField(component, GUIContent.none);
                        }

                        using (new EditorGUI.IndentLevelScope(-1))
                        {
                            var removeOnPC = element.FindPropertyRelative("removeOnPC");
                            Views.EditorGUIUtility.InvertedBoolPropertyField(removeOnPC, GUIContent.none, GUILayout.Width(PCCheckboxWidth));
                            var removeOnAndroid = element.FindPropertyRelative("removeOnAndroid");
                            Views.EditorGUIUtility.InvertedBoolPropertyField(removeOnAndroid, GUIContent.none, GUILayout.Width(MobileCheckboxWidth));
                        }
                    }
                }
            }

            so.ApplyModifiedProperties();
        }
    }
}
