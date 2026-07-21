// <copyright file="RemoveUnsupportedComponentsPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Text.RegularExpressions;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="RemoveUnsupportedComponentsPass"/> driven through NDMF's <see cref="BuildContext"/>.
    /// </summary>
    public class RemoveUnsupportedComponentsPassTests
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
        /// Without a ConvertedAvatar marker, the avatar hasn't gone through conversion yet, so
        /// unsupported components must be left untouched.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNotConverted()
        {
            builder = new NdmfTestAvatarBuilder();
            var light = builder.Root.AddComponent<Light>();

            var context = new BuildContext(builder.Root, null);
            RemoveUnsupportedComponentsPass.Instance.RunForTest(context);

            Assert.IsFalse(light == null, "Unsupported component must not be removed without a ConvertedAvatar marker.");
        }

        /// <summary>
        /// With a ConvertedAvatar marker, unsupported components (e.g. Light) must be removed and a
        /// warning reported for each one.
        /// </summary>
        [Test]
        public void Execute_RemovesUnsupportedComponents_WhenConverted()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<ConvertedAvatar>();
            var light = builder.Root.AddComponent<Light>();

            var context = new BuildContext(builder.Root, null);
            LogAssert.Expect(LogType.Warning, new Regex(@"^\[NDMF\] Error Reported: "));
            RemoveUnsupportedComponentsPass.Instance.RunForTest(context);

            Assert.IsTrue(light == null, "Unsupported component must be removed once the avatar is marked as converted.");
        }
    }
}
