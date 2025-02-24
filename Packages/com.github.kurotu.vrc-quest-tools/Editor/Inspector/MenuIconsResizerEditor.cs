using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for MenuIconsResizer.
    /// </summary>
    [CustomEditor(typeof(MenuIconsResizer))]
    internal class MenuIconsResizerEditor : VRCQuestToolsEditorOnlyEditorBase<MenuIconsResizer>
    {
        /// <inheritdoc/>
        protected override string Description => VRCQuestToolsSettings.I18nResource.MenuIconsResizerEditorDescription;

        /// <inheritdoc/>
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var so = new SerializedObject(target);
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty("resizeModePC"), new GUIContent(i18n.MenuIconsResizerEditorResizeModePCLabel));
            EditorGUILayout.PropertyField(so.FindProperty("resizeModeAndroid"), new GUIContent(i18n.MenuIconsResizerEditorResizeModeAndroidLabel));
            EditorGUILayout.PropertyField(so.FindProperty("compressTextures"), new GUIContent(i18n.MenuIconsResizerEditorCompressTexturesLabel));

            so.ApplyModifiedProperties();
        }
    }
}
