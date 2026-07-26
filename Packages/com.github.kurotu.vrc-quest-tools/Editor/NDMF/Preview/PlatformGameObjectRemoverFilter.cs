// <copyright file="PlatformGameObjectRemoverFilter.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf.preview;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// NDMF Preview filter for <see cref="PlatformGameObjectRemover"/>.
    /// Hides renderers under GameObjects which <see cref="PlatformGameObjectRemoverPass"/> removes for the resolved build target.
    /// </summary>
    internal class PlatformGameObjectRemoverFilter : IRenderFilter
    {
        private static readonly TogglablePreviewNode PreviewNode = TogglablePreviewNode.Create(() => "Platform GameObject Remover", "vrc-quest-tools/PlatformGameObjectRemoverPreview", true);

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
            var renderers = new HashSet<Renderer>();
            foreach (var root in context.GetAvatarRoots())
            {
                if (root == null)
                {
                    continue;
                }

                // NdmfHelper.ResolveBuildTarget reads PlatformTargetSettings without ComputeContext,
                // so observe it here to rebuild the preview when the target platform changes.
                if (root.TryGetComponent<PlatformTargetSettings>(out var platformSettings))
                {
                    context.Observe(platformSettings);
                }

                var buildTarget = NdmfHelper.ResolveBuildTarget(root);
                foreach (var remover in context.GetComponentsInChildren<PlatformGameObjectRemover>(root, true))
                {
                    if (remover == null)
                    {
                        continue;
                    }

                    var shouldRemove = buildTarget == Models.BuildTarget.PC
                        ? context.Observe(remover, c => c.removeOnPC)
                        : context.Observe(remover, c => c.removeOnAndroid);
                    if (!shouldRemove)
                    {
                        continue;
                    }

                    foreach (var renderer in context.GetComponentsInChildren<Renderer>(remover.gameObject, true))
                    {
                        // NDMF discards all groups of this filter when a group contains a renderer type
                        // which its preview proxy doesn't support, so exclude such renderers here.
                        if (renderer is SkinnedMeshRenderer || renderer is MeshRenderer)
                        {
                            renderers.Add(renderer);
                        }
                    }
                }
            }

            return renderers.Select(renderer => RenderGroup.For(renderer)).ToImmutableList();
        }

        /// <inheritdoc/>
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            return Task.FromResult<IRenderFilterNode>(new PlatformGameObjectRemoverFilterNode());
        }

        private class PlatformGameObjectRemoverFilterNode : IRenderFilterNode
        {
            public RenderAspects WhatChanged => 0;

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (proxy == null)
                {
                    return;
                }

                // The preview pipeline resets the proxy state every frame, so disable it here instead of Instantiate.
                proxy.enabled = false;
            }

            public void Dispose()
            {
            }
        }
    }
}
