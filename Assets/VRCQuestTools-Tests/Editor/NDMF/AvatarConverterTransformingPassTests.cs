// <copyright file="AvatarConverterTransformingPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Guard-clause tests for <see cref="AvatarConverterTransformingPass"/> driven through NDMF's
    /// <see cref="BuildContext"/>. The full material conversion behind
    /// <see cref="AvatarConverterPassUtility.ConvertAvatarInPass(BuildContext)"/> needs real shader/texture
    /// assets and is intentionally out of scope here (see NDMF plugin skill's testing guidance);
    /// only the early-return branches that gate whether conversion runs at all are covered.
    /// </summary>
    public class AvatarConverterTransformingPassTests
    {
        private NdmfTestAvatarBuilder builder;
        private Material material;
        private Mesh mesh;

        /// <summary>
        /// Cleans up objects created during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            builder?.Destroy();
            builder = null;

            if (material != null)
            {
                Object.DestroyImmediate(material);
                material = null;
            }

            if (mesh != null)
            {
                Object.DestroyImmediate(mesh);
                mesh = null;
            }
        }

        /// <summary>
        /// Without any IMaterialOperatorComponent (e.g. AvatarConverterSettings, MaterialSwap) in the
        /// hierarchy, the pass must do nothing.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNoMaterialOperatorComponent()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            material = new Material(Shader.Find("Standard"));
            mesh = new Mesh();
            var smr = builder.AddSkinnedMeshRenderer("Body", mesh, material);

            var context = new BuildContext(builder.Root, null);
            Assert.DoesNotThrow(() => AvatarConverterTransformingPass.Instance.RunForTest(context));

            Assert.AreSame(material, smr.sharedMaterial, "Material must be untouched when there is no material operator component.");
        }

        /// <summary>
        /// When AvatarConverterSettings.ndmfPhase resolves to Optimizing, the Transforming-phase pass
        /// must not run the conversion (it's the Optimizing-phase pass's job instead).
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNdmfPhaseResolvesToOptimizing()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            material = new Material(Shader.Find("Standard"));
            mesh = new Mesh();
            var smr = builder.AddSkinnedMeshRenderer("Body", mesh, material);

            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            settings.ndmfPhase = Models.AvatarConverterNdmfPhase.Optimizing;

            var context = new BuildContext(builder.Root, null);
            using (new ObjectRegistryScope(context.ObjectRegistry))
            {
                Assert.DoesNotThrow(() => AvatarConverterTransformingPass.Instance.RunForTest(context));
            }

            Assert.AreSame(material, smr.sharedMaterial, "Material must be untouched when the resolved NDMF phase is Optimizing, not Transforming.");
        }
    }
}
