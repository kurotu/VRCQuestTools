// <copyright file="VertexColorRemover.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    /// <summary>
    /// 頂点カラーを消去
    /// </summary>
    public static class VertexColorRemover
    {
        private const string Tag = "VertexColorRemover";

        [MenuItem(MenuPaths.GameObjectRemoveAllVertexColors)]
        private static void RemoveAllVertexColors()
        {
            var obj = Selection.activeGameObject;
            RemoveAllVertexColors(obj);
            Debug.LogFormat("[{0}] All vertex colors are removed from {1}", Tag, obj);
        }

        public static void RemoveAllVertexColors(GameObject gameObject)
        {
            var skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var s in skinnedMeshRenderers)
            {
                var mesh = GetSharedMesh(s);
                if (mesh == null)
                {
                    continue;
                }
                if (mesh.colors32.Length > 0)
                {
                    mesh.colors32 = null;
                }
            }
            var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var m in meshRenderers)
            {
                var mesh = GetSharedMesh(m);
                if (mesh == null)
                {
                    continue;
                }
                if (mesh.colors32.Length > 0)
                {
                    mesh.colors32 = null;
                }
            }
        }

        [MenuItem(MenuPaths.GameObjectRemoveAllVertexColors, true)]
        public static bool ValidateMenu()
        {
            return Selection.activeGameObject != null;
        }

        private static Mesh GetSharedMesh(Renderer r)
        {
            var type = r.GetType();
            if (type == typeof(SkinnedMeshRenderer))
            {
                return ((SkinnedMeshRenderer)r).sharedMesh;
            }

            if (type == typeof(MeshRenderer))
            {
                return r.GetComponent<MeshFilter>().sharedMesh;
            }

            Debug.LogErrorFormat("[{0}] {1} is not either SkinnedMeshRenderer or MeshRenderer", Tag, r);
            return null;
        }
    }
}
