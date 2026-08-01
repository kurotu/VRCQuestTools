// <copyright file="MeshFlipperFilter.cs" company="kurotu">
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

#pragma warning disable SA1414 // tuple element names
#pragma warning disable SA1648 // inherit documentation

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// NDMF Preview filter for <see cref="MeshFlipper"/>.
    /// </summary>
    internal class MeshFlipperFilter : IRenderFilter
    {
        private static readonly TogglablePreviewNode PreviewNode = TogglablePreviewNode.Create(() => "Mesh Flipper", "vrc-quest-tools/MeshFlipperPreview", true);

        private readonly MeshFlipperProcessingPhase phase;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshFlipperFilter"/> class.
        /// </summary>
        /// <param name="phase">Processing phase for mesh flippers.</param>
        public MeshFlipperFilter(MeshFlipperProcessingPhase phase)
        {
            this.phase = phase;
        }

        /// <inheritdoc/>
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            // Show the single preview node for all phases.
            if (phase == MeshFlipperProcessingPhase.BeforePolygonReduction)
            {
                yield return PreviewNode;
            }
        }

        /// <inheritdoc/>
        public bool IsEnabled(ComputeContext context)
        {
            return context.Observe(PreviewNode.IsEnabled);
        }

        /// <inheritdoc/>
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var components = context.GetComponentsByType<MeshFlipper>()
                .Where(mf => mf.processingPhase == phase);

            foreach (var mf in components)
            {
                context.Observe(mf, mf => mf.processingPhase);
            }

            return components
                .Select(mf => context.GetComponent<Renderer>(mf.gameObject))
                .Where(r => r is SkinnedMeshRenderer || r is MeshRenderer)
                .Select(r => RenderGroup.For(r))
                .ToImmutableList();
        }

        /// <inheritdoc/>
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            // Observed lookup: registers a component-list monitor so removing the component invalidates this node,
            // and returns null instead of racing with component removal.
            var meshFlipper = context.GetComponent<MeshFlipper>(group.Renderers[0].gameObject);
            var targetRenderer = proxyPairs.First().Item2;

            var mesh = targetRenderer != null ? RendererUtility.GetSharedMesh(targetRenderer) : null;
            if (meshFlipper == null || mesh == null)
            {
                // NDMF's NodeController does not null-check Instantiate's result; a null node would throw and
                // take down the whole preview pipeline build. Return a no-op node instead.
                return Task.FromResult<IRenderFilterNode>(new MeshFlipperFilterNode(null, false, null, null, null));
            }

            context.Observe(meshFlipper);
            context.Observe(mesh);

            var avatarRoot = context.GetAvatarRoot(meshFlipper.gameObject);

            // NdmfHelper.ResolveBuildTarget reads PlatformTargetSettings without ComputeContext, so observe it
            // here to rebuild the preview when the target platform changes. context.GetComponent registers a
            // component-list monitor, so a PlatformTargetSettings added later is caught as well.
            var platformSettings = avatarRoot != null ? context.GetComponent<PlatformTargetSettings>(avatarRoot) : null;
            if (platformSettings != null)
            {
                context.Observe(platformSettings);
            }

            var shouldProcess = meshFlipper.processingPhase == phase;
            var isMobileTarget = NdmfHelper.ResolveBuildTarget(avatarRoot) == Models.BuildTarget.Android;
            if (isMobileTarget)
            {
                shouldProcess &= meshFlipper.enabledOnAndroid;
            }
            else
            {
                shouldProcess &= meshFlipper.enabledOnPC;
            }

            Mesh result = mesh;
            var ownsMesh = false;
            if (shouldProcess)
            {
                try
                {
                    result = MeshFlipper.CreateFlippedMesh(meshFlipper, mesh);
                    ownsMesh = true;

                    // Register the replaced mesh so the ObjectRegistry can trace it back to the original,
                    // matching build-pass behavior. Runs inside Instantiate where an ObjectRegistryScope is active.
                    NdmfObjectRegistry.TryRegisterReplacedObjectToActiveRegistry(mesh, result);
                }
                catch (MeshFlipperMaskMissingException)
                {
                    // do not report missing mask.
                }
            }
            return Task.FromResult<IRenderFilterNode>(new MeshFlipperFilterNode(result, ownsMesh, meshFlipper, mesh, avatarRoot));
        }

        private class MeshFlipperFilterNode : IRenderFilterNode
        {
            /// <summary>
            /// Upstream changes this node's flipped mesh survives. The mesh was flipped from the proxy's mesh,
            /// so only an upstream Mesh change (or an invalidation of this node's own observations, which
            /// arrives as updatedAspects == 0) requires re-flipping; blendshape, bone, material and texture
            /// changes cannot affect the flipped geometry.
            /// </summary>
            private const RenderAspects ReusableAspects = RenderAspects.Shapes | RenderAspects.Material | RenderAspects.Texture;

            private readonly bool ownsMesh;
            private readonly MeshFlipper meshFlipper;
            private readonly Mesh sourceMesh;
            private readonly GameObject avatarRoot;
            private Mesh flippedMesh;
            private bool disposedValue;

            public MeshFlipperFilterNode(Mesh flippedMesh, bool ownsMesh, MeshFlipper meshFlipper, Mesh sourceMesh, GameObject avatarRoot)
            {
                this.flippedMesh = flippedMesh;
                this.ownsMesh = ownsMesh;
                this.meshFlipper = meshFlipper;
                this.sourceMesh = sourceMesh;
                this.avatarRoot = avatarRoot;
            }

            public RenderAspects WhatChanged => RenderAspects.Mesh;

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context, RenderAspects updatedAspects)
            {
                // No-op nodes (flippedMesh == null) and destroyed components always rebuild; rebuilding them is
                // cheap and re-evaluates the condition that made them no-ops in the first place.
                if (disposedValue || flippedMesh == null || meshFlipper == null
                    || updatedAspects == 0 || (updatedAspects & ~ReusableAspects) != 0)
                {
                    return Task.FromResult<IRenderFilterNode>(null);
                }

                // Re-register every observation Instantiate made, onto the new context. The NodeController built
                // around a reused node takes the new ComputeContext passed in here; missing one observation would
                // mean this node is never invalidated again.
                context.Observe(meshFlipper);
                if (sourceMesh != null)
                {
                    context.Observe(sourceMesh);
                }
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
                if (flippedMesh == null)
                {
                    return;
                }

                switch (proxy)
                {
                    case SkinnedMeshRenderer smr:
                        smr.sharedMesh = flippedMesh;
                        return;
                    case MeshRenderer mr:
                        {
                            var mf = mr.GetComponent<MeshFilter>();
                            if (mf == null)
                            {
                                return;
                            }
                            mf.sharedMesh = flippedMesh;
                            return;
                        }
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                System.GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (flippedMesh != null)
                    {
                        // Only destroy meshes this node created. When shouldProcess was false, flippedMesh is the
                        // source mesh itself, which may be an in-memory mesh owned by an upstream preview node.
                        if (ownsMesh && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(flippedMesh)))
                        {
                            UnityEngine.Object.DestroyImmediate(flippedMesh);
                        }
                        flippedMesh = null;
                    }
                    disposedValue = true;
                }
            }
        }
    }
}
