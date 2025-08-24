using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf.preview;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

#pragma warning disable SA1414 // tuple element names
#pragma warning disable SA1648 // inherit documentation

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// NDMF Preview filter for <see cref="IMaterialConversionComponent"/> such as <see cref="AvatarConverterSettings"/> and <see cref="MaterialConversionSettings"/>.
    /// </summary>
    internal class MaterialConversionFilter : IRenderFilter
    {
        private static readonly TogglablePreviewNode PreviewNode = TogglablePreviewNode.Create(() => "Material Conversion", "vrc-quest-tools/MaterialConversionPreview", true);

        private readonly AvatarConverterNdmfPhase phase;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialConversionFilter"/> class.
        /// </summary>
        /// <param name="phase">Processing phase for material conversion.</param>
        public MaterialConversionFilter(AvatarConverterNdmfPhase phase)
        {
            this.phase = phase;
        }

        /// <inheritdoc/>
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            // Show the single preview node for all phases.
            if (phase == AvatarConverterNdmfPhase.Optimizing)
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
            var rootConversions = context.GetAvatarRoots()
                .Select(root => ComponentUtility.GetPrimaryMaterialConversionComponent(root))
                .Cast<Component>()
                .Where(component =>
                {
                    if (component == null)
                    {
                        return false;
                    }
                    return AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(component.gameObject) == phase;
                })
                .ToArray();

            foreach (var rootConversion in rootConversions)
            {
                context.Observe(rootConversion, c => AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(c.gameObject));
                Debug.Log($"[{VRCQuestTools.Name}] Found material conversion component {rootConversion} for {phase}", rootConversion);
            }

            return rootConversions.Select(root => root.GetComponentsInChildren<Renderer>().Where(r => r is SkinnedMeshRenderer || r is MeshRenderer))
                .Where(r => r.Any())
                .Select(r => RenderGroup.For(r))
                .ToImmutableList();
        }

        /// <inheritdoc/>
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var avatarRoot = context.GetAvatarRoot(group.Renderers[0].gameObject);
            Debug.Log($"[{VRCQuestTools.Name}] Reload preview for {avatarRoot} in {phase}", avatarRoot);

            IMaterialConversionComponent settings = avatarRoot.GetComponent<AvatarConverterSettings>();
            if (settings == null)
            {
                settings = avatarRoot.GetComponent<MaterialConversionSettings>();
            }
            context.Observe(settings as Object);
            if (settings is Component c && c.TryGetComponent<PlatformTargetSettings>(out var targetSettings))
            {
                context.Observe(targetSettings);
            }

            if (AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(avatarRoot) != phase)
            {
                // If the phase does not match, we do not process this filter.
                return Task.FromResult<IRenderFilterNode>(new MaterialConversionFilterNode(new Dictionary<Material, Material>(), false));
            }

            var isTargetMobile = NdmfHelper.ResolveBuildTarget(avatarRoot) == Models.BuildTarget.Android;
            if (!isTargetMobile)
            {
                // If the target is not mobile, we do not process this filter.
                return Task.FromResult<IRenderFilterNode>(new MaterialConversionFilterNode(new Dictionary<Material, Material>(), false));
            }

            var removeExtraMaterialSlots = settings.RemoveExtraMaterialSlots;
            List<Material> avatarMaterials = new List<Material>();
            foreach (var (original, _) in proxyPairs)
            {
                context.Observe(original);
                var slots = removeExtraMaterialSlots
                    ? RendererUtility.GetSharedMesh(original).subMeshCount
                    : original.sharedMaterials.Length;
                foreach (var m in original.sharedMaterials.Take(slots))
                {
                    avatarMaterials.Add(m);
                    context.Observe(m);
                }
            }

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var settingsMap = converter.CreateMaterialConvertSettingsMap(avatarRoot, avatarMaterials.ToArray());
            var materialMap = converter.ConvertMaterialsForAndroid(settingsMap, false, string.Empty, null);

            return Task.FromResult<IRenderFilterNode>(new MaterialConversionFilterNode(materialMap, removeExtraMaterialSlots));
        }

        private class MaterialConversionFilterNode : IRenderFilterNode
        {
            private readonly Dictionary<Material, Material> materialMap;
            private readonly bool removeExtraMaterialSlots;
            private bool disposedValue;

            public MaterialConversionFilterNode(Dictionary<Material, Material> materialMap, bool removeExtraMaterialSlots)
            {
                this.materialMap = materialMap;
                this.removeExtraMaterialSlots = removeExtraMaterialSlots;
            }

            public RenderAspects WhatChanged => RenderAspects.Material;

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (materialMap.Count == 0)
                {
                    return;
                }
                var slots = removeExtraMaterialSlots ? RendererUtility.GetSharedMesh(original).subMeshCount : original.sharedMaterials.Length;
                proxy.sharedMaterials = original.sharedMaterials.Take(slots).Select(m => materialMap.TryGetValue(m, out var converted) ? converted : m).ToArray();
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
                    foreach (var material in materialMap.Values)
                    {
                        if (material != null)
                        {
                            // destroy all on-memory textures here.
                            foreach (var prop in material.GetTexturePropertyNames())
                            {
                                var texture = material.GetTexture(prop);
                                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture)))
                                {
                                    Object.DestroyImmediate(texture);
                                }
                            }
                            Object.DestroyImmediate(material);
                        }
                    }

                    disposedValue = true;
                }
            }
        }
    }
}
