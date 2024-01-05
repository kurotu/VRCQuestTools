using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for ConvertedAvatar.
    /// </summary>
    [CustomEditor(typeof(ConvertedAvatar))]
    internal class ConvertedAvatarEditor : Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            EditorGUILayout.LabelField(i18n.ConvertedAvatarEditorMessage, EditorStyles.wordWrappedLabel);
#if VQT_HAS_NDMF
            EditorGUILayout.HelpBox(i18n.ConvertedAvatarEditorNDMFMessage, MessageType.Info);
#endif
        }
    }
}
