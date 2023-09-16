using System.Linq;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Mark to remove vertex color in the scene.
    /// </summary>
    public class VertexColorRemover : MonoBehaviour
#if VQT_VRCSDK_HAS_PUBLIC_API
        , VRC.SDKBase.IEditorOnly
#endif
    {
#if UNITY_EDITOR
        /// <summary>
        /// Gets the value whether the component is working.
        /// </summary>
        public bool active = true;

        /// <summary>
        /// Gets the value whether vertex color are removed from children's renderers.
        /// </summary>
        public bool includeChildren = false;

        /// <summary>
        /// Remove vertex color from gameObject's renderer.
        /// </summary>
        public void RemoveVertexColor()
        {
            if (!active)
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

        private void Reset()
        {
            RemoveVertexColor();
        }

        private void OnValidate()
        {
            RemoveVertexColor();
        }
#endif
    }
}
