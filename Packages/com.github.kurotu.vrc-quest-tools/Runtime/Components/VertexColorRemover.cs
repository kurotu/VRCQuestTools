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
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/vertex-color-remover?lang=auto")]
    public class VertexColorRemover : VRCQuestToolsEditorOnly, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Gets the value whether vertex color are removed from children's renderers.
        /// </summary>
        public bool includeChildren = false;

        private int serializedVersion = 2;

        [SerializeField]
        [System.Obsolete("Use enabled instead")]
        private bool active;

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

        /// <summary>
        /// Restore vertex color for gameObject's renderer.
        /// </summary>
        public void RestoreVertexColor()
        {
#if UNITY_EDITOR
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
#else
            Debug.LogError("RestoreVertexColor is not supported in runtime");
#endif
        }

        /// <summary>
        /// Called before serialization.
        /// </summary>
        public void OnBeforeSerialize()
        {
            // nothing to do
        }

        /// <summary>
        /// Called after deserialization to migrate parameters.
        /// </summary>
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

            if (type == typeof(MeshRenderer) && renderer.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                return meshFilter.sharedMesh;
            }

            Debug.LogErrorFormat("{0} is not either SkinnedMeshRenderer or MeshRenderer", renderer);
            return null;
        }

        private void HierarchyChanged()
        {
            RemoveVertexColor();
        }

        private void OnEnable()
        {
            RemoveVertexColor();
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged += HierarchyChanged;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= HierarchyChanged;
#endif
        }
    }
}
