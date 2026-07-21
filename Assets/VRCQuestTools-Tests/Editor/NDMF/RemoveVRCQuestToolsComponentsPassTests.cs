// <copyright file="RemoveVRCQuestToolsComponentsPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using NUnit.Framework;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="RemoveVRCQuestToolsComponentsPass"/> driven through NDMF's <see cref="BuildContext"/>.
    /// </summary>
    public class RemoveVRCQuestToolsComponentsPassTests
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
        /// All VRCQuestToolsEditorOnly-derived settings components must be destroyed so none of them
        /// leak into the build output.
        /// </summary>
        [Test]
        public void Execute_DestroysAllVRCQuestToolsEditorOnlyComponents()
        {
            builder = new NdmfTestAvatarBuilder();
            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            var targetSettings = builder.Root.AddComponent<PlatformTargetSettings>();

            var context = new BuildContext(builder.Root, null);
            RemoveVRCQuestToolsComponentsPass.Instance.RunForTest(context);

            Assert.IsTrue(settings == null, "AvatarConverterSettings must be destroyed.");
            Assert.IsTrue(targetSettings == null, "PlatformTargetSettings must be destroyed.");
        }
    }
}
