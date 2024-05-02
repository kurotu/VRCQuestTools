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
        /// <inheritdoc/>
        protected override string Description => VRCQuestToolsSettings.I18nResource.NetworkIDAssignerEditorDescription;

        /// <inheritdoc />
        public override void OnInspectorGUIInternal()
        {
            // Do nothing
        }
    }
}
