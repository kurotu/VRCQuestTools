using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for NetworkIDAssigner.
    /// </summary>
    [CustomEditor(typeof(NetworkIDAssigner))]
    internal class NetworkIDAssignerEditor : VRCQuestToolsEditorOnlyEditorBase<NetworkIDAssigner>
    {
        /// <inheritdoc />
        public override void OnInspectorGUIInternal()
        {
            var so = new SerializedObject(target);
            so.Update();

            var i18n = VRCQuestToolsSettings.I18nResource;
            EditorGUILayout.LabelField(i18n.NetworkIDAssignerEditorDescription, EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();
            var assignmentMethod = so.FindProperty(nameof(NetworkIDAssigner.assignmentMethod));
            EditorGUILayout.PropertyField(assignmentMethod, new GUIContent(i18n.NetworkIDAssignerEditorAssignmentMethodLabel));

            switch ((NetworkIDAssignmentMethod)assignmentMethod.enumValueIndex)
            {
                case NetworkIDAssignmentMethod.HierarchyHash:
                    EditorGUILayout.LabelField(i18n.NetworkIDAssignerEditorAssignmentMethodHierachyHashTooltip, EditorStyles.wordWrappedMiniLabel);
                    break;
                case NetworkIDAssignmentMethod.VRChatSDK:
                    EditorGUILayout.LabelField(i18n.NetworkIDAssignerEditorAssignmentMethodVRChatSDKTooltip, EditorStyles.wordWrappedMiniLabel);
                    break;
                default:
                    EditorGUILayout.HelpBox("Undefined message. Please report.", MessageType.Warning);
                    break;
            }

            so.ApplyModifiedProperties();
        }
    }
}
