using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for PlatformGameObjectRemover.
    /// </summary>
    [CustomEditor(typeof(PlatformGameObjectRemover))]
    internal class PlatformGameObjectRemoverEditor : Editor
    {
        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            Views.EditorGUIUtility.LanguageSelector();
            var i18n = VRCQuestToolsSettings.I18nResource;
            #if !VQT_HAS_NDMF
            EditorGUILayout.HelpBox(i18n.ComponentRequiresNdmf, MessageType.Warning);
            #endif

            EditorGUILayout.Space();

            var so = new SerializedObject(target);
            so.Update();

            var buildTarget = so.FindProperty("buildTarget");
            EditorGUILayout.PropertyField(buildTarget, new GUIContent(i18n.BuildTargetLabel, i18n.BuildTargetTooltip));

            var removeOnPC = so.FindProperty("removeOnPC");
            EditorGUILayout.PropertyField(removeOnPC, new GUIContent(i18n.PlatformGameObjectRemoverEditorRemoveOnPCLabel));

            var removeOnAndroid = so.FindProperty("removeOnAndroid");
            EditorGUILayout.PropertyField(removeOnAndroid, new GUIContent(i18n.PlatformGameObjectRemoverEditorRemoveOnAndroidLabel));

            so.ApplyModifiedProperties();
        }
    }
}
