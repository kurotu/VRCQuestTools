using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for MeshFlipper.
    /// </summary>
    [CustomEditor(typeof(MeshFlipper))]
    internal class MeshFlipperEditor : VRCQuestToolsEditorOnlyEditorBase<MeshFlipper>
    {
        /// <inheritdoc />
        protected override string Description => VRCQuestToolsSettings.I18nResource.MeshFlipperEditorDescription;

        /// <inheritdoc />
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var so = new SerializedObject(target);
            so.Update();

            var direction = so.FindProperty("direction");
            EditorGUILayout.PropertyField(direction, new GUIContent(i18n.MeshFlipperEditorDirectionLabel));

            if (direction.enumValueIndex == (int)MeshFlipperMeshDirection.BothSides)
            {
                EditorGUILayout.HelpBox(i18n.MeshFlipperEditorEnabledOnAndroidWarning, MessageType.Warning);
            }

            var enabledOnPC = so.FindProperty("enabledOnPC");
            EditorGUILayout.PropertyField(enabledOnPC, new GUIContent(i18n.MeshFlipperEditorEnabledOnPCLabel));

            if (enabledOnPC.boolValue)
            {
                EditorGUILayout.HelpBox(i18n.MeshFlipperEditorEnabledOnPCWarning, MessageType.Info);
            }

            var enabledOnAndroid = so.FindProperty("enabledOnAndroid");
            EditorGUILayout.PropertyField(enabledOnAndroid, new GUIContent(i18n.MeshFlipperEditorEnabledOnAndroidLabel));

            so.ApplyModifiedProperties();
        }
    }
}
