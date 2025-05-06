using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for MenuIconResizer.
    /// </summary>
    [CustomEditor(typeof(MenuIconResizer))]
    internal class MenuIconResizerEditor : VRCQuestToolsEditorOnlyEditorBase<MenuIconResizer>
    {
        /// <inheritdoc/>
        protected override string Description => VRCQuestToolsSettings.I18nResource.MenuIconResizerEditorDescription;

        /// <inheritdoc/>
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            var so = new SerializedObject(target);
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty("resizeModePC"), new GUIContent(i18n.MenuIconResizerEditorResizeModePCLabel));
            EditorGUILayout.PropertyField(so.FindProperty("resizeModeAndroid"), new GUIContent(i18n.MenuIconResizerEditorResizeModeAndroidLabel));
            EditorGUILayout.PropertyField(so.FindProperty("compressTextures"), new GUIContent(i18n.MenuIconResizerEditorCompressTexturesLabel));
            EditorGUILayout.PropertyField(so.FindProperty("mobileTextureFormat"), new GUIContent(i18n.MenuIconResizerEditorMobileTextureFormatLabel));

            so.ApplyModifiedProperties();
        }
    }
}
