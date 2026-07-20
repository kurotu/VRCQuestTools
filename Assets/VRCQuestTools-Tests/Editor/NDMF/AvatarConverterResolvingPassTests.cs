// <copyright file="AvatarConverterResolvingPassTests.cs" company="kurotu">
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
    /// Guard-clause tests for <see cref="AvatarConverterResolvingPass"/> driven through NDMF's
    /// <see cref="BuildContext"/>.
    /// </summary>
    public class AvatarConverterResolvingPassTests
    {
        private GameObject root;
        private NdmfTestAvatarBuilder builder;

        /// <summary>
        /// Cleans up objects created during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (root != null)
            {
                Object.DestroyImmediate(root);
                root = null;
            }

            builder?.Destroy();
            builder = null;
        }

        /// <summary>
        /// Without a VRCAvatarDescriptor at all, the pass must not attempt any conversion preparation.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNoAvatarDescriptor()
        {
            root = new GameObject("NoDescriptor");

            var context = new BuildContext(root, null);
            Assert.DoesNotThrow(() => AvatarConverterResolvingPass.Instance.RunForTest(context));

            Assert.IsNull(root.GetComponent<ConvertedAvatar>(), "No ConvertedAvatar marker should be added without a VRCAvatarDescriptor.");
        }

        /// <summary>
        /// With a VRCAvatarDescriptor but no AvatarConverterSettings, the pass must not attempt any
        /// conversion preparation.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNoAvatarConverterSettings()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            var context = new BuildContext(builder.Root, null);
            Assert.DoesNotThrow(() => AvatarConverterResolvingPass.Instance.RunForTest(context));

            Assert.IsNull(builder.Root.GetComponent<ConvertedAvatar>(), "No ConvertedAvatar marker should be added without AvatarConverterSettings.");
        }
    }
}
