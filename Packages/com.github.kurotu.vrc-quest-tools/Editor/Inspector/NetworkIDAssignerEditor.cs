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
            var i18n = VRCQuestToolsSettings.I18nResource;
            EditorGUILayout.LabelField(i18n.NetworkIDAssignerEditorDescription, EditorStyles.wordWrappedLabel);
        }
    }
}
