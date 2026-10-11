// <copyright file="MaterialConversionFilterReplacementTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Exercises replacement settings through the actual NDMF preview filter.
    /// </summary>
    internal class MaterialConversionFilterReplacementTests
    {
        /// <summary>
        /// Preview initialization must observe the aggregate key and apply the replacement to the proxy.
        /// </summary>
        /// <param name="avatarConverter">Whether to use avatar conversion or material conversion settings.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void Instantiate_AdditionalReplacement_UpdatesProxy(bool avatarConverter)
        {
            var sourceShader = Shader.Find("Standard");
            var replacementShader = Shader.Find("VRChat/Mobile/Toon Lit");
            Assert.IsNotNull(sourceShader);
            Assert.IsNotNull(replacementShader);

            var builder = new NdmfTestAvatarBuilder();
            var mesh = new Mesh
            {
                vertices = new[] { Vector3.zero, Vector3.right, Vector3.up },
                triangles = new[] { 0, 1, 2 },
            };
            var source = new Material(sourceShader);
            var replacement = new Material(replacementShader);
            var proxyObject = new GameObject("ReplacementPreviewProxy");
            var context = new ComputeContext("material replacement preview regression");
            IRenderFilterNode node = null;

            try
            {
                IMaterialConversionComponent settings;
                if (avatarConverter)
                {
                    var component = builder.Root.AddComponent<AvatarConverterSettings>();
                    component.ndmfPhase = AvatarConverterNdmfPhase.Optimizing;
                    settings = component;
                }
                else
                {
                    var component = builder.Root.AddComponent<MaterialConversionSettings>();
                    component.ndmfPhase = AvatarConverterNdmfPhase.Optimizing;
                    settings = component;
                }
                settings.AdditionalMaterialConvertSettings = new[]
                {
                    new AdditionalMaterialConvertSettings
                    {
                        targetMaterial = source,
                        materialConvertSettings = new MaterialReplaceSettings { material = replacement },
                    },
                };
                builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

                var original = builder.AddSkinnedMeshRenderer("Body", mesh, source);
                var proxy = proxyObject.AddComponent<SkinnedMeshRenderer>();
                proxy.sharedMesh = mesh;
                proxy.sharedMaterial = source;
                var group = RenderGroup.For(original).WithData<Component>((Component)settings);
                var pairs = new[] { ((Renderer)original, (Renderer)proxy) };
                var filter = new MaterialConversionFilter(AvatarConverterNdmfPhase.Optimizing);

                using (new ObjectRegistryScope(new ObjectRegistry(builder.Root.transform)))
                {
                    var task = filter.Instantiate(group, pairs, context);
                    Assert.IsTrue(task.IsCompleted, "Replacement-only preview initialization must complete synchronously.");
                    node = task.Result;
                }

                Assert.IsNotNull(node);
                node.OnFrame(original, proxy);
                Assert.AreSame(replacement, proxy.sharedMaterial, "The filter must apply the replacement, rather than return a no-op node.");
                Assert.AreSame(source, original.sharedMaterial, "Preview must preserve the original renderer.");

                var refreshedContext = new ComputeContext("refreshed material replacement preview regression");
                try
                {
                    var refresh = node.Refresh(pairs, refreshedContext, RenderAspects.Shapes);
                    Assert.AreSame(node, refresh.Result, "A reused node must also observe replacement settings without throwing.");
                }
                finally
                {
                    refreshedContext.Invalidate();
                    ComputeContext.FlushInvalidates();
                }
            }
            finally
            {
                node?.Dispose();
                context.Invalidate();
                ComputeContext.FlushInvalidates();
                builder.Destroy();
                Object.DestroyImmediate(proxyObject);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(source);
                if (replacement != null)
                {
                    Object.DestroyImmediate(replacement);
                }
            }
        }
    }
}
