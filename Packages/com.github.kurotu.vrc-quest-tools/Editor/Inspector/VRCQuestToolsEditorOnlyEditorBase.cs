using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Base class for editor of VRCQuestToolsEditorOnly components.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    public class VRCQuestToolsEditorOnlyEditorBase<T> : Editor
        where T : VRCQuestToolsEditorOnly
    {
        /// <inheritdoc />
        public sealed override void OnInspectorGUI()
        {
            Views.EditorGUIUtility.LanguageSelector();
            Views.EditorGUIUtility.UpdateNotificationPanel();
#if !VQT_HAS_NDMF
            if (target is IVRCQuestToolsNdmfComponent)
            {
                EditorGUILayout.Space();
                var i18n = VRCQuestToolsSettings.I18nResource;
                EditorGUILayout.HelpBox(i18n.ComponentRequiresNdmf, MessageType.Warning);
            }
#endif

            Views.EditorGUIUtility.HorizontalDivider(2);

            OnInspectorGUIInternal();
        }

        /// <summary>
        /// Called after header GUI is drawed in OnInspectorGUI.
        /// </summary>
        public virtual void OnInspectorGUIInternal()
        {
            base.OnInspectorGUI();
        }
    }
}
