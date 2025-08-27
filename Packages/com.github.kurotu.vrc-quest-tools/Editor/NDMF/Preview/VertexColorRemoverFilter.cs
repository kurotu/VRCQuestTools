using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf.preview;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// NDMF Preview filter for <see cref="IVertexColorRemoverComponent"/> such as <see cref="AvatarConverterSettings"/> and <see cref="VertexColorRemoverSettings"/>.
    /// </summary>
    internal class VertexColorRemoverFilter : IRenderFilter
    {
        private static readonly TogglablePreviewNode PreviewNode = TogglablePreviewNode.Create(() => "Vertex Color Remover", "vrc-quest-tools/VertexColorRemoverPreview", true);

        /// <inheritdoc/>
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return PreviewNode;
        }

        /// <inheritdoc/>
        public bool IsEnabled(ComputeContext context)
        {
            return context.Observe(PreviewNode.IsEnabled);
        }

        /// <inheritdoc/>
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var rootConversions = context.GetAvatarRoots()
                .Select(root => root.GetComponent<AvatarConverterSettings>())
                .Where(component => component != null)
                .ToArray();

            foreach (var conv in rootConversions)
            {
                context.Observe(conv, c => c.removeVertexColor);
            }

            return rootConversions
                .Where(root => root.removeVertexColor)
                .SelectMany(root => root.GetComponentsInChildren<Renderer>())
                .Where(renderer => renderer is SkinnedMeshRenderer || renderer is MeshRenderer)
                .Select(renderer => RenderGroup.For(renderer))
                .ToImmutableList();
        }

        /// <inheritdoc/>
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var proxy = proxyPairs.First().Item2;
            var mesh = RendererUtility.GetSharedMesh(proxy);
            Mesh newMesh = mesh;
            if (mesh != null && mesh.colors32 != null && mesh.colors32.Length > 0)
            {
                newMesh = Mesh.Instantiate(newMesh);
                newMesh.colors32 = null;
            }
            return Task.FromResult<IRenderFilterNode>(new VertexColorRemoverFilterNode(newMesh));
        }

        private class VertexColorRemoverFilterNode : IRenderFilterNode
        {
            private Mesh colorlessMesh;
            private bool disposedValue;

            public VertexColorRemoverFilterNode(Mesh mesh)
            {
                this.colorlessMesh = mesh;
            }

            public RenderAspects WhatChanged => RenderAspects.Mesh;

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (RendererUtility.GetSharedMesh(proxy) != null)
                {
                    RendererUtility.SetSharedMesh(proxy, colorlessMesh);
                }
            }

            public void Dispose()
            {
                Dispose(true);
                System.GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // Dispose managed resources if any
                    }
                    if (colorlessMesh != null)
                    {
                        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(colorlessMesh)))
                        {
                            Object.DestroyImmediate(colorlessMesh);
                        }
                        colorlessMesh = null;
                    }
                    disposedValue = true;
                }
            }
        }
    }
}
