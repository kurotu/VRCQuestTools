#if VQT_HAS_NDMF_PREVIEW

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf.preview;
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
        private static readonly TogglablePreviewNode PreviewNode = TogglablePreviewNode.Create(() => "Mesh Flipper", "vrc-quest-tools/MeshFlipperPreview", false);

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
            return context.GetComponentsByType<MeshFlipper>()
                .Select(mf => context.GetComponent<Renderer>(mf.gameObject))
                .Where(r => r is SkinnedMeshRenderer || r is MeshRenderer)
                .Select(r => RenderGroup.For(r))
                .ToImmutableList();
        }

        /// <inheritdoc/>
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var meshFlipper = group.Renderers.First().GetComponent<MeshFlipper>();
            var targetRenderer = proxyPairs.First().Item2;

            var mesh = RendererUtility.GetSharedMesh(targetRenderer);
            if (mesh == null)
            {
                return null;
            }

            context.Observe(meshFlipper);
            context.Observe(mesh);

            var shouldProcess = meshFlipper.processingPhase == phase;
            var isMobileTarget = NdmfHelper.ResolveBuildTarget(context.GetAvatarRoot(meshFlipper.gameObject)) == Models.BuildTarget.Android;
            if (isMobileTarget)
            {
                shouldProcess &= meshFlipper.enabledOnAndroid;
            }
            else
            {
                shouldProcess &= meshFlipper.enabledOnPC;
            }

            Mesh result = mesh;
            if (shouldProcess)
            {
                try
                {
                    result = MeshFlipper.CreateFlippedMesh(meshFlipper, mesh);
                }
                catch (MeshFlipperMaskMissingException)
                {
                    // do not report missing mask.
                }
            }
            return Task.FromResult<IRenderFilterNode>(new MeshFlipperFilterNode(result, targetRenderer));
        }

        private class MeshFlipperFilterNode : IRenderFilterNode
        {
            private Mesh flippedMesh;

            public MeshFlipperFilterNode(Mesh flippedMesh, Renderer targetRenderer)
            {
                this.flippedMesh = flippedMesh;
            }

            public RenderAspects WhatChanged => RenderAspects.Mesh;

            public void OnFrame(Renderer original, Renderer proxy)
            {
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
        }
    }
}

#endif
