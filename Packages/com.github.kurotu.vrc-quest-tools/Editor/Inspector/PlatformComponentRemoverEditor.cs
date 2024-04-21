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
        private const float AndroidCheckboxWidth = 50f;

        /// <inheritdoc />
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUILayout.HelpBox(i18n.PlatformComponentRemoverEditorDescription, MessageType.Info);

            EditorGUILayout.HelpBox(i18n.PlatformTargetSettingsIsRequiredToEnforcePlatform, MessageType.Info);

            EditorGUILayout.Space();

            ((PlatformComponentRemover)target).UpdateComponentSettings();

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
                        EditorGUILayout.LabelField("PC", GUILayout.Width(PCCheckboxWidth));
                        EditorGUILayout.LabelField("Android", GUILayout.Width(AndroidCheckboxWidth));
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
                            EditorGUILayout.PropertyField(removeOnPC, GUIContent.none, GUILayout.Width(PCCheckboxWidth));
                            var removeOnAndroid = element.FindPropertyRelative("removeOnAndroid");
                            EditorGUILayout.PropertyField(removeOnAndroid, GUIContent.none, GUILayout.Width(AndroidCheckboxWidth));
                        }
                    }
                }
            }

            so.ApplyModifiedProperties();
        }
    }
}
