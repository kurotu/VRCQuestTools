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
            Logger.LogDebug("Getting target groups");
            var rootConversions = context.GetAvatarRoots()
                .Select(root => ComponentUtility.GetPrimaryMaterialConversionComponent(root))
                .Cast<Component>()
                .Where(component =>
                {
                    if (component == null)
                    {
                        return false;
                    }
                    if (AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(component.gameObject) != phase)
                    {
                        return false;
                    }

                    // Filter out avatars with preview disabled
                    var materialComponent = component as IMaterialConversionComponent;
                    if (materialComponent != null)
                    {
                        var previewEnabled = context.Observe(component, c => (c as IMaterialConversionComponent).EnableMaterialPreview || (c as IMaterialConversionComponent).ForceMaterialPreview);
                        return previewEnabled;
                    }
                    return true;
                })
                .ToArray();

            foreach (var rootConversion in rootConversions)
            {
                context.Observe(rootConversion, c => AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(c.gameObject));
            }

            return rootConversions.Select(root => context.GetComponentsInChildren<Renderer>(root.gameObject, true).Where(r => r is SkinnedMeshRenderer || r is MeshRenderer))
                .Where(r => r.Any())
                .Select(r => RenderGroup.For(r))
                .ToImmutableList();
        }

        /// <inheritdoc/>
        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var avatarRoot = context.GetAvatarRoot(group.Renderers[0].gameObject);
            Logger.LogDebug($"Instantiating material conversion filter for {avatarRoot}", avatarRoot);

            IMaterialConversionComponent settings = avatarRoot.GetComponent<AvatarConverterSettings>();
            if (settings == null)
            {
                settings = avatarRoot.GetComponent<MaterialConversionSettings>();
            }
            context.Observe(settings as Object, s => (s as IMaterialConversionComponent).GetCacheKey());

            // Hoisted out of the `if` below (rather than declared by its `out var`) so it stays in scope, and
            // definitely assigned, for the MaterialConversionFilterNode built at the end of this method: the node
            // has to re-observe it in Refresh. TryGetComponent leaves it null when there is no such component.
            PlatformTargetSettings targetSettings = null;
            if (settings is Component c && c.TryGetComponent<PlatformTargetSettings>(out targetSettings))
            {
                context.Observe(targetSettings);
            }

            if (AvatarConverterPassUtility.ResolveAvatarConverterNdmfPhase(avatarRoot) != phase)
            {
                // If the phase does not match, we do not process this filter.
                return Task.FromResult<IRenderFilterNode>(new MaterialConversionFilterNode(new Dictionary<Material, Material>(), false, null));
            }

            var isTargetMobile = NdmfHelper.ResolveBuildTarget(avatarRoot) == Models.BuildTarget.Android;
            var forcePreview = settings != null && settings.ForceMaterialPreview;
            if (!isTargetMobile && !forcePreview)
            {
                // If the target is not mobile and preview is not forced, we do not process this filter.
                return Task.FromResult<IRenderFilterNode>(new MaterialConversionFilterNode(new Dictionary<Material, Material>(), false, null));
            }

            var removeExtraMaterialSlots = settings.RemoveExtraMaterialSlots;
            HashSet<Material> avatarMaterials = new();
            foreach (var (original, proxy) in proxyPairs)
            {
                context.Observe(original);
                context.Observe(proxy);
                var slots = removeExtraMaterialSlots
                    ? RendererUtility.GetSharedMeshSubMeshCount(original)
                    : original.sharedMaterials.Length;
                foreach (var m in original.sharedMaterials.Take(slots))
                {
                    avatarMaterials.Add(m);
                    context.Observe(m);
                }
                foreach (var m in proxy.sharedMaterials.Take(slots))
                {
                    avatarMaterials.Add(m);
                    context.Observe(m);
                }
            }

            try
            {
                var converter = new AvatarConverter(new MaterialWrapperBuilder());
                var settingsMap = converter.CreateMaterialConvertSettingsMap(avatarRoot, avatarMaterials.ToArray());

                // Track reference changes: when an upstream filter (e.g. another plugin) already replaced a material and
                // registered it in the ObjectRegistry, the convert settings are keyed by the original material. Resolve
                // the replaced material back to its original to apply the same settings (mirrors the build-time
                // AvatarConverterPassUtility.TrackObjectRegistryForMaterialConversion/Swaps). Components are not mutated.
                foreach (var m in avatarMaterials)
                {
                    if (m == null || settingsMap.ContainsKey(m))
                    {
                        continue;
                    }
                    var original = NdmfObjectRegistry.GetReference(m)?.Object as Material;
                    if (original != null && original != m && settingsMap.TryGetValue(original, out var s))
                    {
                        settingsMap[m] = s;
                    }
                }

                // forEditorPreview: true re-uploads freshly generated normal maps so they render in the editor preview.
                var materialLease = SharedPreviewMaterialCache.Acquire(settingsMap, m => converter.ConvertMaterialsForMobile(m, false, string.Empty, null, forEditorPreview: true, avatarRoot: avatarRoot));

                // Register replaced materials so the ObjectRegistry can trace converted materials back to originals,
                // matching build-pass behavior. Runs inside Instantiate where an ObjectRegistryScope is active.
                // SharedPreviewMaterialCache can map multiple source materials to the same converted instance, so we
                // group by converted material and register a single deterministic representative to keep tracing stable.
                foreach (var materialGroup in materialLease.MaterialMap.GroupBy(kv => kv.Value))
                {
                    var converted = materialGroup.Key;

                    // Skip asset-backed converted materials such as MaterialReplaceSettings replacements.
                    // Build registers only in-memory converted materials. See AvatarConverterPassUtility.
                    if (converted == null || !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(converted)))
                    {
                        continue;
                    }

                    var original = materialGroup.Select(kv => kv.Key)
                        .Where(m => m != null)
                        .OrderBy(m => m.GetInstanceID())
                        .FirstOrDefault();
                    NdmfObjectRegistry.TryRegisterReplacedObjectToActiveRegistry(original, converted);
                }

                return Task.FromResult<IRenderFilterNode>(new MaterialConversionFilterNode(
                    materialLease.MaterialMap,
                    removeExtraMaterialSlots,
                    materialLease,
                    settings as Object,
                    targetSettings,
                    avatarMaterials.ToArray()));
            }
            catch (System.Exception e)
            {
                // Log the exception and skip preview for this group instead of letting it propagate to NDMF,
                // which would otherwise retry conversion indefinitely and destabilize the whole preview.
                Logger.LogException(e, avatarRoot);
                return Task.FromResult<IRenderFilterNode>(new MaterialConversionFilterNode(new Dictionary<Material, Material>(), false, null));
            }
        }

        private class MaterialConversionFilterNode : IRenderFilterNode
        {
            /// <summary>
            /// The only upstream change this node's output survives. Blendshape and bone updates cannot affect
            /// which material a slot uses, so the conversion result stays valid. Everything else can:
            /// Material and Texture change the very things the material map is keyed by and built from, and Mesh
            /// changes the submesh count, which decides how many slots <see cref="OnFrame"/> takes -- and
            /// therefore whether a material that was never converted (because it sat in an extra slot) starts
            /// being rendered.
            /// </summary>
            private const RenderAspects ReusableAspects = RenderAspects.Shapes;

            private readonly Dictionary<Material, Material> materialMap;
            private readonly bool removeExtraMaterialSlots;
            private readonly SharedMaterialMapLease materialLease;
            private readonly Object settingsComponent;
            private readonly PlatformTargetSettings platformTargetSettings;
            private readonly Material[] observedMaterials;
            private bool disposedValue;

            public MaterialConversionFilterNode(
                Dictionary<Material, Material> materialMap,
                bool removeExtraMaterialSlots,
                SharedMaterialMapLease materialLease,
                Object settingsComponent = null,
                PlatformTargetSettings platformTargetSettings = null,
                Material[] observedMaterials = null)
            {
                this.materialMap = materialMap;
                this.removeExtraMaterialSlots = removeExtraMaterialSlots;
                this.materialLease = materialLease;
                this.settingsComponent = settingsComponent;
                this.platformTargetSettings = platformTargetSettings;
                this.observedMaterials = observedMaterials;
            }

            public RenderAspects WhatChanged => RenderAspects.Material;

            /// <summary>
            /// Lets the preview pipeline carry this node over to a new generation instead of running
            /// <see cref="Instantiate"/> -- and therefore the whole material conversion -- again. Without this,
            /// NDMF's default implementation returns null and every upstream change, however unrelated, forces a
            /// full re-conversion of the avatar.
            /// </summary>
            /// <remarks>
            /// Returning <c>this</c> is safe for the shared converted materials: NDMF increments its own
            /// reference count on the reused node (NodeController.Refresh) and only disposes it once both the old
            /// and the new generation are gone, which is exactly the lifetime <see cref="SharedMaterialMapLease"/>
            /// needs. The observations, however, are not carried over: the NodeController built around a reused
            /// node takes the *new* ComputeContext passed in here, so everything <see cref="Instantiate"/>
            /// observed has to be observed again on it. Miss one and this node is never invalidated again --
            /// editing the avatar's convert settings would silently stop updating the preview.
            /// </remarks>
            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context, RenderAspects updatedAspects)
            {
                // updatedAspects == 0 means this node's own ComputeContext was invalidated -- the convert
                // settings or one of the source materials changed -- so the conversion has to be redone.
                // A node without a lease is one of the no-op nodes Instantiate returns early (wrong phase,
                // non-mobile target, or a conversion that threw); rebuilding those is cheap and re-evaluates
                // the condition that made them no-ops in the first place.
                if (disposedValue || materialLease == null || updatedAspects == 0 || (updatedAspects & ~ReusableAspects) != 0)
                {
                    return Task.FromResult<IRenderFilterNode>(null);
                }

                // Re-register every observation Instantiate made, onto the new context. See the remarks above.
                if (settingsComponent != null)
                {
                    context.Observe(settingsComponent, s => (s as IMaterialConversionComponent).GetCacheKey());
                }
                if (platformTargetSettings != null)
                {
                    context.Observe(platformTargetSettings);
                }
                foreach (var (original, proxy) in proxyPairs)
                {
                    context.Observe(original);
                    context.Observe(proxy);
                }
                if (observedMaterials != null)
                {
                    foreach (var m in observedMaterials)
                    {
                        if (m != null)
                        {
                            context.Observe(m);
                        }
                    }
                }

                return Task.FromResult<IRenderFilterNode>(this);
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (materialMap.Count == 0)
                {
                    return;
                }
                var slots = removeExtraMaterialSlots ? RendererUtility.GetSharedMeshSubMeshCount(proxy) : proxy.sharedMaterials.Length;
                proxy.sharedMaterials = proxy.sharedMaterials.Take(slots).Select(m => materialMap.TryGetValue(m, out var converted) ? converted : m).ToArray();
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
                    if (materialLease != null)
                    {
                        materialLease.Release();
                    }
                    else
                    {
                        foreach (var material in materialMap.Values)
                        {
                            if (material != null)
                            {
                                // destroy all on-memory objects here.
                                foreach (var prop in material.GetTexturePropertyNames())
                                {
                                    var texture = material.GetTexture(prop);
                                    if (texture != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture)))
                                    {
                                        Object.DestroyImmediate(texture);
                                    }
                                }
                                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(material)))
                                {
                                    Object.DestroyImmediate(material);
                                }
                            }
                        }
                        materialMap.Clear();
                    }

                    disposedValue = true;
                }
            }
        }
    }
}
