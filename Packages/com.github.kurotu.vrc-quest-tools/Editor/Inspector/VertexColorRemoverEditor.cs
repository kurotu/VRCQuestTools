using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Inspector for VertexColorRemover to provide "restore" function.
    /// </summary>
    [CustomEditor(typeof(VertexColorRemover))]
    public class VertexColorRemoverEditor : VRCQuestToolsEditorOnlyEditorBase<VertexColorRemover>
    {
        /// <inheritdoc/>
        protected override string Description => VRCQuestToolsSettings.I18nResource.VertexColorRemoverEditorDescription;

        /// <summary>
        /// GUI callback.
        /// </summary>
        public override void OnInspectorGUIInternal()
        {
            VertexColorRemover remover = target as VertexColorRemover;
            var i18n = VRCQuestToolsSettings.I18nResource;

            using (new EditorGUI.DisabledGroupScope(remover.enabled))
            {
                if (GUILayout.Button(i18n.VertexColorRemoverEditorRemove))
                {
                    OnClickRemove();
                }
            }

            using (new EditorGUI.DisabledGroupScope(!remover.enabled))
            {
                if (GUILayout.Button(i18n.VertexColorRemoverEditorRestore))
                {
                    OnClickRestore();
                }
            }

            var so = new SerializedObject(remover);
            so.Update();

            EditorGUI.BeginChangeCheck();
            var includeChildren = so.FindProperty("includeChildren");
            EditorGUILayout.PropertyField(includeChildren);
            if (EditorGUI.EndChangeCheck())
            {
                OnChangeIncludeChildren(includeChildren.boolValue);
            }

            so.ApplyModifiedProperties();
        }

        private void OnClickRemove()
        {
            VertexColorRemover remover = target as VertexColorRemover;
            remover.enabled = true;
            PrefabUtility.RecordPrefabInstancePropertyModifications(remover);
        }

        private void OnClickRestore()
        {
            VertexColorRemover remover = target as VertexColorRemover;
            remover.enabled = false;
            remover.RestoreVertexColor();
            PrefabUtility.RecordPrefabInstancePropertyModifications(remover);
        }

        private void OnChangeIncludeChildren(bool includeChildren)
        {
            VertexColorRemover remover = target as VertexColorRemover;
            remover.includeChildren = includeChildren;
            if (!remover.enabled)
            {
                return;
            }
            if (includeChildren)
            {
                remover.RemoveVertexColor();
            }
            else
            {
                remover.RestoreVertexColor();
                remover.RemoveVertexColor();
            }
        }
    }
}
