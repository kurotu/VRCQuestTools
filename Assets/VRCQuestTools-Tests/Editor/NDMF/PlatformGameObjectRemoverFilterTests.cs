// <copyright file="PlatformGameObjectRemoverFilterTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if VQT_NDMF_HAS_PROP_CACHE_DEBUG
using System.Linq;
#endif
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf.preview;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="PlatformGameObjectRemoverFilter"/>, the NDMF Preview counterpart of
    /// <see cref="PlatformGameObjectRemoverPass"/>.
    /// </summary>
    public class PlatformGameObjectRemoverFilterTests
    {
        private NdmfTestAvatarBuilder builder;
        private GameObject proxyObject;

        /// <summary>
        /// Cleans up objects created during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            builder?.Destroy();
            builder = null;
            if (proxyObject != null)
            {
                Object.DestroyImmediate(proxyObject);
                proxyObject = null;
            }
        }

#if VQT_NDMF_HAS_PROP_CACHE_DEBUG
        /// <summary>
        /// Renderers under a GameObject removed for the Android target are targeted by the filter.
        /// </summary>
        [Test]
        public void GetTargetGroups_ReturnsRenderer_WhenRemoveOnAndroidAndTargetIsAndroid()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;
            var renderer = AddRemovedChildRenderer(removeOnPC: false, removeOnAndroid: true);

            Assert.IsTrue(GetTargetRenderers().Contains(renderer), "Renderer under a removed GameObject must be targeted.");
        }

        /// <summary>
        /// Renderers are left alone when the remover is not configured for the resolved target.
        /// </summary>
        [Test]
        public void GetTargetGroups_ReturnsNothing_WhenRemoveOnAndroidIsDisabled()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;
            var renderer = AddRemovedChildRenderer(removeOnPC: true, removeOnAndroid: false);

            Assert.IsFalse(GetTargetRenderers().Contains(renderer), "Renderer must not be targeted when the GameObject is kept for the target.");
        }

        /// <summary>
        /// Renderers under a GameObject removed for the PC target are targeted by the filter.
        /// </summary>
        [Test]
        public void GetTargetGroups_ReturnsRenderer_WhenRemoveOnPCAndTargetIsPC()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.PC;
            var renderer = AddRemovedChildRenderer(removeOnPC: true, removeOnAndroid: false);

            Assert.IsTrue(GetTargetRenderers().Contains(renderer), "Renderer under a removed GameObject must be targeted.");
        }

        /// <summary>
        /// Renderer types which NDMF's preview proxy doesn't support must be excluded, otherwise NDMF
        /// discards all groups of this filter and the preview silently stops working.
        /// </summary>
        [Test]
        public void GetTargetGroups_ExcludesUnsupportedRendererTypes()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;
            var renderer = AddRemovedChildRenderer(removeOnPC: false, removeOnAndroid: true);
            var particle = renderer.gameObject.AddComponent<ParticleSystem>();
            var particleRenderer = particle.GetComponent<ParticleSystemRenderer>();

            var targets = GetTargetRenderers();
            Assert.IsTrue(targets.Contains(renderer), "Supported renderer must be targeted.");
            Assert.IsFalse(targets.Contains(particleRenderer), "ParticleSystemRenderer must be excluded from render groups.");
        }
#endif

        /// <summary>
        /// The filter node disables the proxy renderer on each frame without touching the original.
        /// </summary>
        [Test]
        public void OnFrame_DisablesProxyRendererOnly()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;
            var original = AddRemovedChildRenderer(removeOnPC: false, removeOnAndroid: true);

            proxyObject = new GameObject("Proxy");
            var proxy = proxyObject.AddComponent<SkinnedMeshRenderer>();

            var group = RenderGroup.For(original);
            var pairs = new[] { ((Renderer)original, (Renderer)proxy) };
            var node = new PlatformGameObjectRemoverFilter().Instantiate(group, pairs, new ComputeContext("test")).Result;

            node.OnFrame(original, proxy);

            Assert.IsFalse(proxy.enabled, "Proxy renderer must be disabled to hide it in the preview.");
            Assert.IsTrue(original.enabled, "Original renderer must not be modified.");
        }

        private SkinnedMeshRenderer AddRemovedChildRenderer(bool removeOnPC, bool removeOnAndroid)
        {
            var child = new GameObject("Child");
            child.transform.SetParent(builder.Root.transform, false);
            var remover = child.AddComponent<PlatformGameObjectRemover>();
            remover.removeOnPC = removeOnPC;
            remover.removeOnAndroid = removeOnAndroid;
            return child.AddComponent<SkinnedMeshRenderer>();
        }

#if VQT_NDMF_HAS_PROP_CACHE_DEBUG
        private Renderer[] GetTargetRenderers()
        {
            // Global queries are cached and don't observe objects created inside a synchronous test body.
            // PropCacheDebug.InvalidateAllCaches() itself requires NDMF 1.10.0+ (see VQT_NDMF_HAS_PROP_CACHE_DEBUG),
            // so GetTargetGroups() is only exercised here; PlatformGameObjectRemoverFilter still supports the
            // project's NDMF 1.5.0 floor, this test just can't verify it against that old a version.
            PropCacheDebug.InvalidateAllCaches();
            var groups = new PlatformGameObjectRemoverFilter().GetTargetGroups(new ComputeContext("test"));
            return groups.SelectMany(g => g.Renderers).ToArray();
        }
#endif
    }
}
