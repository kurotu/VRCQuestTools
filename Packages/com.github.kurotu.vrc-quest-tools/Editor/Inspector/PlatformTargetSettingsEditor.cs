using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for PlatformTargetSettings.
    /// </summary>
    [CustomEditor(typeof(PlatformTargetSettings))]
    internal class PlatformTargetSettingsEditor : VRCQuestToolsEditorOnlyEditorBase<PlatformTargetSettings>
    {
        /// <inheritdoc/>
        protected override string Description => VRCQuestToolsSettings.I18nResource.PlatformTargetSettingsEditorDescription;

        /// <inheritdoc/>
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var so = new SerializedObject(target);
            so.Update();

            var buildTarget = so.FindProperty("buildTarget");
            EditorGUILayout.PropertyField(buildTarget, new GUIContent(i18n.BuildTargetLabel));

            so.ApplyModifiedProperties();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.ComponentLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                var platformComponents = TargetComponent.GetComponentsInChildren<IPlatformDependentComponent>(true)
                    .Select(c => (Component)c);
                Component[] convertSettings = TargetComponent.GetComponentsInChildren<AvatarConverterSettings>(true);

                var components = platformComponents.Concat(convertSettings).ToArray();
                foreach (var c in components)
                {
                    EditorGUILayout.ObjectField(c, c.GetType(), true);
                }
            }
        }
    }
}
