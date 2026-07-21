// <copyright file="RemoveVertexColorPassTests.cs" company="kurotu">
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
    /// Tests for <see cref="RemoveVertexColorPass"/> driven through NDMF's <see cref="BuildContext"/>
    /// against a minimal in-code avatar, instead of a Fixtures scene or reflection.
    /// </summary>
    public class RemoveVertexColorPassTests
    {
        private NdmfTestAvatarBuilder builder;
        private Mesh originalMesh;

        /// <summary>
        /// Cleans up objects created during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            builder?.Destroy();
            builder = null;

            if (originalMesh != null)
            {
                Object.DestroyImmediate(originalMesh);
                originalMesh = null;
            }
        }

        /// <summary>
        /// The pass should duplicate a mesh with non-white vertex colors, strip the colors on the
        /// duplicate, leave the original mesh untouched, and register the replacement in ObjectRegistry.
        /// </summary>
        [Test]
        public void Execute_RemovesVertexColorAndTracksObjectRegistry()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;
            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            settings.removeVertexColor = true;

            originalMesh = new Mesh();
            originalMesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            originalMesh.triangles = new[] { 0, 1, 2 };
            originalMesh.colors32 = new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255),
            };

            var smr = builder.AddSkinnedMeshRenderer("Body", originalMesh);

            var context = new BuildContext(builder.Root, null);
            Mesh replacedMesh = null;
            try
            {
                using (new ObjectRegistryScope(context.ObjectRegistry))
                {
                    RemoveVertexColorPass.Instance.RunForTest(context);

                    // Capture the duplicate before any assertion can throw, so the finally block
                    // below can always clean it up even if an assertion fails partway through.
                    replacedMesh = smr.sharedMesh;

                    // ObjectRegistry.ActiveRegistry is only set inside this scope, so the lookup
                    // must happen before it (and the registration it reads) goes out of scope.
                    var reference = NdmfObjectRegistry.GetReference(smr.sharedMesh);
                    Assert.AreSame(originalMesh, reference.Object, "ObjectRegistry should trace the replacement mesh back to the original.");
                }

                Assert.AreNotSame(originalMesh, smr.sharedMesh, "Pass should replace the mesh with a duplicate, not mutate it in place.");
                Assert.IsTrue(originalMesh.colors32 != null && originalMesh.colors32.Length > 0, "Original mesh asset must remain untouched.");
                Assert.IsTrue(smr.sharedMesh.colors32 == null || smr.sharedMesh.colors32.Length == 0, "Replacement mesh must have vertex colors removed.");
            }
            finally
            {
                if (replacedMesh != null)
                {
                    Object.DestroyImmediate(replacedMesh);
                }
            }
        }

        /// <summary>
        /// The pass must not touch avatars that don't opt in via AvatarConverterSettings.removeVertexColor.
        /// </summary>
        [Test]
        public void Execute_SkipsWhenRemoveVertexColorIsDisabled()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;
            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            settings.removeVertexColor = false;

            originalMesh = new Mesh();
            originalMesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            originalMesh.triangles = new[] { 0, 1, 2 };
            originalMesh.colors32 = new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255),
                new Color32(255, 0, 0, 255),
            };

            var smr = builder.AddSkinnedMeshRenderer("Body", originalMesh);

            var context = new BuildContext(builder.Root, null);
            using (new ObjectRegistryScope(context.ObjectRegistry))
            {
                RemoveVertexColorPass.Instance.RunForTest(context);
            }

            Assert.AreSame(originalMesh, smr.sharedMesh, "Pass must not replace the mesh when removeVertexColor is disabled.");
        }
    }
}
