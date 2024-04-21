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
    internal class PlatformGameObjectRemoverEditor : VRCQuestToolsEditorOnlyEditorBase<PlatformGameObjectRemover>
    {
        /// <inheritdoc/>
        public override void OnInspectorGUIInternal()
        {
            Views.EditorGUIUtility.LanguageSelector();
            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUILayout.Space();

#if !VQT_HAS_NDMF
            EditorGUILayout.HelpBox(i18n.ComponentRequiresNdmf, MessageType.Warning);
#endif
            EditorGUILayout.HelpBox(i18n.PlatformGameObjectRemoverEditorDescription, MessageType.Info);

            EditorGUILayout.HelpBox(i18n.PlatformTargetSettingsIsRequiredToEnforcePlatform, MessageType.Info);

            EditorGUILayout.Space();

            var so = new SerializedObject(target);
            so.Update();

            var removeOnPC = so.FindProperty("removeOnPC");
            EditorGUILayout.PropertyField(removeOnPC, new GUIContent(i18n.PlatformGameObjectRemoverEditorRemoveOnPCLabel));

            var removeOnAndroid = so.FindProperty("removeOnAndroid");
            EditorGUILayout.PropertyField(removeOnAndroid, new GUIContent(i18n.PlatformGameObjectRemoverEditorRemoveOnAndroidLabel));

            so.ApplyModifiedProperties();
        }
    }
}
