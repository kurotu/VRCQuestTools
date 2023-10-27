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
    public class VertexColorRemoverEditor : Editor
    {
        /// <summary>
        /// GUI callback.
        /// </summary>
        public override void OnInspectorGUI()
        {
            VertexColorRemover remover = target as VertexColorRemover;
            var i18n = VRCQuestToolsSettings.I18nResource;

            using (new EditorGUI.DisabledGroupScope(remover.active))
            {
                if (GUILayout.Button(i18n.VertexColorRemoverEditorRemove))
                {
                    OnClickRemove();
                }
            }

            using (new EditorGUI.DisabledGroupScope(!remover.active))
            {
                if (GUILayout.Button(i18n.VertexColorRemoverEditorRestore))
                {
                    OnClickRestore();
                }
            }

            var so = new SerializedObject(remover);
            so.Update();

            var active = so.FindProperty("active");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(active);
            if (EditorGUI.EndChangeCheck())
            {
                if (active.boolValue)
                {
                    OnClickRemove();
                }
                else
                {
                    OnClickRestore();
                }
            }

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
            remover.active = true;
            remover.RemoveVertexColor();
            PrefabUtility.RecordPrefabInstancePropertyModifications(remover);
        }

        private void OnClickRestore()
        {
            VertexColorRemover remover = target as VertexColorRemover;
            remover.active = false;
            RestoreVertexColor();
            PrefabUtility.RecordPrefabInstancePropertyModifications(remover);
        }

        private void OnChangeIncludeChildren(bool includeChildren)
        {
            VertexColorRemover remover = target as VertexColorRemover;
            if (!remover.active)
            {
                return;
            }
            if (includeChildren)
            {
                remover.RemoveVertexColor();
            }
            else
            {
                RestoreVertexColor();
                remover.RemoveVertexColor();
            }
        }

        private void RestoreVertexColor()
        {
            VertexColorRemover remover = target as VertexColorRemover;

            Renderer[] skinnedMeshRenderers = remover.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Renderer[] meshRenderers = remover.GetComponentsInChildren<MeshRenderer>(true);
            var renderers = skinnedMeshRenderers.Concat(meshRenderers);
            var paths = renderers
                .Select(r => RendererUtility.GetSharedMesh(r))
               .Where(m => m != null)
               .Select(m => AssetDatabase.GetAssetPath(m))
               .Distinct();
            foreach (var p in paths)
            {
                AssetDatabase.ImportAsset(p);
            }
        }
    }
}
