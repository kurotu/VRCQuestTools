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
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            EditorGUILayout.HelpBox(i18n.PlatformTargetSettingsEditorDescription, MessageType.Info);

            var targetComponent = (Component)target;
            if (!VRCSDKUtility.IsAvatarRoot(targetComponent.gameObject))
            {
                EditorGUILayout.HelpBox(i18n.PlatformTargetSettingsShouldBeAttachedToAvatarRoot, MessageType.Error);
            }

            EditorGUILayout.Space();

            var so = new SerializedObject(target);
            so.Update();

            var buildTarget = so.FindProperty("buildTarget");
            EditorGUILayout.PropertyField(buildTarget, new GUIContent(i18n.BuildTargetLabel));

            so.ApplyModifiedProperties();

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(true))
            {
                var component = (PlatformTargetSettings)target;
                Component[] componentRemovers = component.GetComponentsInChildren<PlatformComponentRemover>(true);
                Component[] gameObjectRemovers = component.GetComponentsInChildren<PlatformGameObjectRemover>(true);
                var components = componentRemovers.Concat(gameObjectRemovers).ToArray();
                foreach (var c in components)
                {
                    EditorGUILayout.ObjectField(c, c.GetType(), true);
                }
            }
        }
    }
}
