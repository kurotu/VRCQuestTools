// <copyright file="PlatformComponentRemoverPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="PlatformComponentRemoverPass"/> driven through NDMF's <see cref="BuildContext"/>.
    /// </summary>
    public class PlatformComponentRemoverPassTests
    {
        private NdmfTestAvatarBuilder builder;

        /// <summary>
        /// Cleans up objects created during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            builder?.Destroy();
            builder = null;
        }

        /// <summary>
        /// A component marked for removal on the resolved platform is destroyed.
        /// </summary>
        [Test]
        public void Execute_RemovesComponent_WhenFlaggedForTargetPlatform()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            var light = builder.Root.AddComponent<Light>();
            var pcr = builder.Root.AddComponent<PlatformComponentRemover>();
            pcr.componentSettings = new[]
            {
                new PlatformComponentRemoverItem { component = light, removeOnAndroid = true },
            };

            var context = new BuildContext(builder.Root, null);
            PlatformComponentRemoverPass.Instance.RunForTest(context);

            Assert.IsTrue(light == null, "Component flagged for removal on the resolved platform must be destroyed.");
        }

        /// <summary>
        /// Removing a PhysBone that another PhysBone references as a collider must clear that
        /// dangling reference, not just destroy the removed component.
        /// </summary>
        [Test]
        public void Execute_ClearsColliderReference_WhenReferencedColliderIsRemoved()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            var colliderGameObject = new GameObject("Collider");
            colliderGameObject.transform.SetParent(builder.Root.transform, false);
            var collider = colliderGameObject.AddComponent<VRCPhysBoneCollider>();

            var boneGameObject = new GameObject("Bone");
            boneGameObject.transform.SetParent(builder.Root.transform, false);
            var bone = boneGameObject.AddComponent<VRCPhysBone>();
            bone.colliders.Add(collider);

            var pcr = builder.Root.AddComponent<PlatformComponentRemover>();
            pcr.componentSettings = new[]
            {
                new PlatformComponentRemoverItem { component = collider, removeOnAndroid = true },
            };

            var context = new BuildContext(builder.Root, null);
            PlatformComponentRemoverPass.Instance.RunForTest(context);

            Assert.IsTrue(collider == null, "Flagged collider must be destroyed.");
            Assert.IsTrue(bone != null, "The referencing PhysBone itself must not be destroyed.");

            // Use ReferenceEquals, not Unity's overloaded ==: a destroyed-but-still-referenced
            // UnityEngine.Object also compares == null (Unity's "fake null"), so == can't tell
            // "list entry was explicitly cleared" apart from "list still holds a dangling
            // reference to the now-destroyed collider" -- exactly the bug this test guards against.
            Assert.IsTrue(ReferenceEquals(bone.colliders[0], null), "The dangling collider reference on the remaining PhysBone must be explicitly cleared, not just left pointing at a destroyed object.");
        }
    }
}
