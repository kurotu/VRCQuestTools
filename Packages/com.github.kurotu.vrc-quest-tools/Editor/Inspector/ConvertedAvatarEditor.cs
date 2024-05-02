using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for ConvertedAvatar.
    /// </summary>
    [CustomEditor(typeof(ConvertedAvatar))]
    internal class ConvertedAvatarEditor : VRCQuestToolsEditorOnlyEditorBase<ConvertedAvatar>
    {
        /// <inheritdoc />
        protected override string Description => VRCQuestToolsSettings.I18nResource.ConvertedAvatarEditorMessage;

        /// <inheritdoc />
        public override void OnInspectorGUIInternal()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            EditorGUILayout.LabelField(i18n.ConvertedAvatarEditorNDMFMessage, EditorStyles.wordWrappedLabel);
        }
    }
}
