// <copyright file="CheckTextureFormatPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Guard-clause test for <see cref="CheckTextureFormatPass"/> driven through NDMF's
    /// <see cref="BuildContext"/>. The format-check logic itself branches on
    /// <c>EditorUserBuildSettings.activeBuildTarget</c>, which a test must not mutate (it's shared,
    /// session-wide Editor state), so only the avatar-descriptor guard is covered here.
    /// </summary>
    public class CheckTextureFormatPassTests
    {
        private GameObject root;

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
        }

        /// <summary>
        /// Without a VRCAvatarDescriptor at all, the pass must not attempt any texture format check.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNoAvatarDescriptor()
        {
            root = new GameObject("NoDescriptor");

            var context = new BuildContext(root, null);
            Assert.DoesNotThrow(() => CheckTextureFormatPass.Instance.RunForTest(context));
        }
    }
}
