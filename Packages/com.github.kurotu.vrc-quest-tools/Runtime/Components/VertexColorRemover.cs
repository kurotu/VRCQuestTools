using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Mark to remove vertex color in the scene.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Vertex Color Remover")]
    [ExecuteInEditMode]
    public class VertexColorRemover : VRCQuestToolsEditorOnly, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        /// <summary>
        /// Gets the value whether vertex color are removed from children's renderers.
        /// </summary>
        public bool includeChildren = false;

        private int serializedVersion = 2;

        [SerializeField]
        [System.Obsolete("Use enabled instead")]
        private bool active;

        public void OnEnable()
        {
            RemoveVertexColor();
            EditorApplication.hierarchyChanged += HierarchyChanged;
        }

        public void OnDisable()
        {
            EditorApplication.hierarchyChanged -= HierarchyChanged;
        }

        /// <summary>
        /// Remove vertex color from gameObject's renderer.
        /// </summary>
        public void RemoveVertexColor()
        {
            if (!enabled)
            {
                return;
            }
            Renderer[] skinnedMeshRenderers;
            Renderer[] meshRenderers;
            if (includeChildren)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
                meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            }
            else
            {
                skinnedMeshRenderers = GetComponents<SkinnedMeshRenderer>();
                meshRenderers = GetComponents<MeshRenderer>();
            }
            var renderers = skinnedMeshRenderers.Concat(meshRenderers).ToArray();

            foreach (var renderer in skinnedMeshRenderers.Concat(meshRenderers))
            {
                RemoveVertexColor(renderer);
            }
        }

        public void RestoreVertexColor()
        {
            Renderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Renderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            var renderers = skinnedMeshRenderers.Concat(meshRenderers);
            var paths = renderers
                .Select(r => GetSharedMesh(r))
               .Where(m => m != null)
               .Select(m => AssetDatabase.GetAssetPath(m))
               .Distinct();
            foreach (var p in paths)
            {
                AssetDatabase.ImportAsset(p);
            }
        }

        private static void RemoveVertexColor(Renderer renderer)
        {
            var mesh = GetSharedMesh(renderer);
            if (mesh == null)
            {
                return;
            }
            if (mesh.colors32.Length > 0)
            {
                mesh.colors32 = null;
                Debug.Log($"[VRCQuestTools] Removed vertex color from {renderer.name}", renderer);
            }
        }

        private static Mesh GetSharedMesh(Renderer renderer)
        {
            var type = renderer.GetType();
            if (type == typeof(SkinnedMeshRenderer))
            {
                return ((SkinnedMeshRenderer)renderer).sharedMesh;
            }

            if (type == typeof(MeshRenderer))
            {
                return renderer.GetComponent<MeshFilter>().sharedMesh;
            }

            Debug.LogErrorFormat("{0} is not either SkinnedMeshRenderer or MeshRenderer", renderer);
            return null;
        }

        private void HierarchyChanged()
        {
            RemoveVertexColor();
        }

        public void OnBeforeSerialize()
        {
            // nothing to do
        }

        public void OnAfterDeserialize()
        {
            if (serializedVersion < 2)
            {
#pragma warning disable CS0618
                enabled = this.active;
#pragma warning restore CS0618
                serializedVersion = 2;
            }
        }
#endif
    }
}
