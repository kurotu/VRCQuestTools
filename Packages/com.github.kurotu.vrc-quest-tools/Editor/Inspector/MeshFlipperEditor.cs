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

            var useMask = so.FindProperty("useMask");
            EditorGUILayout.PropertyField(useMask, new GUIContent(i18n.MeshFlipperEditorUseMaskLabel));
            if (useMask.boolValue)
            {
                using (var indentLevelScope = new EditorGUI.IndentLevelScope(1))
                {
                    var maskTexture = so.FindProperty("maskTexture");
                    EditorGUILayout.PropertyField(maskTexture, new GUIContent(i18n.MeshFlipperEditorMaskTextureLabel));
                    if (maskTexture.objectReferenceValue != null)
                    {
                        var mask = maskTexture.objectReferenceValue as Texture2D;
                        if (!mask.isReadable)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.HelpBox(i18n.MeshFlipperEditorMaskTextureNotReadableError, MessageType.Error);
                                if (GUILayout.Button(i18n.FixLabel, GUILayout.Height(38), GUILayout.Width(60)))
                                {
                                    OnClickFixReadable(mask);
                                }
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(i18n.MeshFlipperEditorMaskTextureMissingError, MessageType.Error);
                    }
                    var maskMode = so.FindProperty("maskMode");
                    EditorGUILayout.PropertyField(maskMode, new GUIContent(i18n.MeshFlipperEditorMaskModeLabel));
                }
            }

            var processingPhase = so.FindProperty("processingPhase");
            EditorGUILayout.PropertyField(processingPhase, new GUIContent(i18n.MeshFlipperEditorProcessingPhaseLabel));

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

        private void OnClickFixReadable(Texture2D mask)
        {
            var path = AssetDatabase.GetAssetPath(mask);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }
}
