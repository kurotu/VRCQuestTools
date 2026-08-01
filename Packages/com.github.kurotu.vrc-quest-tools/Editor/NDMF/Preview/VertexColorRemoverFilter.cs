// <copyright file="VertexColorRemoverFilter.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
    /// NDMF Preview filter which previews vertex color removal of <see cref="RemoveVertexColorPass"/> for avatars with <see cref="AvatarConverterSettings"/>.
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
            // Observed lookup: GetAvatarRoots() is backed by a PropCache whose SequenceEqual comparer stops
            // propagation when the root list is unchanged, so a plain GetComponent here would never see an
            // AvatarConverterSettings added to an existing root. context.GetComponent registers a
            // component-list monitor on each root instead.
            var rootConversions = context.GetAvatarRoots()
                .Select(root => context.GetComponent<AvatarConverterSettings>(root))
                .Where(component => component != null)
                .ToArray();

            return rootConversions
                .SelectMany(root => context.GetComponentsInChildren<Renderer>(root.gameObject, true))
                .Distinct()
                .Where(renderer => renderer is SkinnedMeshRenderer || renderer is MeshRenderer)
                .Select(renderer => RenderGroup.For(renderer))
                .ToImmutableList();
        }

        /// <inheritdoc/>
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var root = context.GetAvatarRoot(group.Renderers[0].gameObject);

            // Observed lookup so removing the component invalidates this node. NDMF's NodeController does not
            // null-check Instantiate's result, so return a no-op node instead of racing with component removal.
            var settings = root != null ? context.GetComponent<AvatarConverterSettings>(root) : null;
            if (settings == null)
            {
                return Task.FromResult<IRenderFilterNode>(new VertexColorRemoverFilterNode(null, false, null, null));
            }

            // NdmfHelper.ResolveBuildTarget reads PlatformTargetSettings without ComputeContext, so observe it
            // here. context.GetComponent also catches a PlatformTargetSettings added later.
            var platformSettings = context.GetComponent<PlatformTargetSettings>(root);
            if (platformSettings != null)
            {
                context.Observe(platformSettings);
            }

            var removeVertexColor = context.Observe(settings, s => s.removeVertexColor);
            var forcePreview = context.Observe(settings, s => s.ForceMaterialPreview);
            var isTargetMobile = NdmfHelper.ResolveBuildTarget(root) == Models.BuildTarget.Android;

            var proxy = proxyPairs.First().Item2;
            var mesh = proxy != null ? RendererUtility.GetSharedMesh(proxy) : null;
            Mesh newMesh = mesh;

            var ownsMesh = false;
            var shouldRemove = (removeVertexColor || forcePreview) && (isTargetMobile || forcePreview) && mesh != null && mesh.colors32 != null && mesh.colors32.Length > 0;
            if (shouldRemove)
            {
                newMesh = Mesh.Instantiate(newMesh);
                newMesh.colors32 = null;
                ownsMesh = true;

                // Register the replaced mesh so the ObjectRegistry can trace it back to the original,
                // matching build-pass behavior. Runs inside Instantiate where an ObjectRegistryScope is active.
                NdmfObjectRegistry.TryRegisterReplacedObjectToActiveRegistry(mesh, newMesh);
            }
            return Task.FromResult<IRenderFilterNode>(new VertexColorRemoverFilterNode(newMesh, ownsMesh, settings, root));
        }

        private class VertexColorRemoverFilterNode : IRenderFilterNode
        {
            /// <summary>
            /// Upstream changes this node's colorless mesh survives. The mesh was cloned from the proxy's mesh,
            /// so only an upstream Mesh change (or an invalidation of this node's own observations, which
            /// arrives as updatedAspects == 0) requires re-cloning; blendshape, bone, material and texture
            /// changes cannot affect the vertex colors.
            /// </summary>
            private const RenderAspects ReusableAspects = RenderAspects.Shapes | RenderAspects.Material | RenderAspects.Texture;

            private readonly bool ownsMesh;
            private readonly AvatarConverterSettings settings;
            private readonly GameObject avatarRoot;
            private Mesh colorlessMesh;
            private bool disposedValue;

            public VertexColorRemoverFilterNode(Mesh mesh, bool ownsMesh, AvatarConverterSettings settings, GameObject avatarRoot)
            {
                this.colorlessMesh = mesh;
                this.ownsMesh = ownsMesh;
                this.settings = settings;
                this.avatarRoot = avatarRoot;
            }

            public RenderAspects WhatChanged => RenderAspects.Mesh;

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context, RenderAspects updatedAspects)
            {
                // No-op nodes (colorlessMesh == null) and destroyed components always rebuild; rebuilding them
                // is cheap and re-evaluates the condition that made them no-ops in the first place.
                if (disposedValue || colorlessMesh == null || settings == null
                    || updatedAspects == 0 || (updatedAspects & ~ReusableAspects) != 0)
                {
                    return Task.FromResult<IRenderFilterNode>(null);
                }

                // Re-register every observation Instantiate made, onto the new context. The NodeController built
                // around a reused node takes the new ComputeContext passed in here; missing one observation would
                // mean this node is never invalidated again.
                context.Observe(settings, s => s.removeVertexColor);
                context.Observe(settings, s => s.ForceMaterialPreview);
                if (avatarRoot != null)
                {
                    var platformSettings = context.GetComponent<PlatformTargetSettings>(avatarRoot);
                    if (platformSettings != null)
                    {
                        context.Observe(platformSettings);
                    }
                }

                return Task.FromResult<IRenderFilterNode>(this);
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (colorlessMesh == null)
                {
                    return;
                }

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
                        // Only destroy meshes this node created. When shouldRemove was false, colorlessMesh is
                        // the source mesh itself, which may be an in-memory mesh owned by an upstream preview node.
                        if (ownsMesh && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(colorlessMesh)))
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
