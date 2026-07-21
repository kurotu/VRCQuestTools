// <copyright file="MenuIconResizerPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Guard-clause tests for <see cref="MenuIconResizerPass"/> driven through NDMF's
    /// <see cref="BuildContext"/>. Actual icon resizing/compression needs real texture assets and is
    /// intentionally out of scope here; only the early-return branches are covered.
    /// </summary>
    public class MenuIconResizerPassTests
    {
        private GameObject root;
        private NdmfTestAvatarBuilder builder;
        private VRCExpressionsMenu menu;

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

            if (menu != null)
            {
                Object.DestroyImmediate(menu);
                menu = null;
            }
        }

        /// <summary>
        /// Without a VRCAvatarDescriptor at all, the pass must not touch anything.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNoAvatarDescriptor()
        {
            root = new GameObject("NoDescriptor");

            var context = new BuildContext(root, null);
            Assert.DoesNotThrow(() => MenuIconResizerPass.Instance.RunForTest(context));
        }

        /// <summary>
        /// With a VRCAvatarDescriptor but no expressions menu assigned, the pass must do nothing.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNoExpressionsMenu()
        {
            builder = new NdmfTestAvatarBuilder();

            var context = new BuildContext(builder.Root, null);
            Assert.DoesNotThrow(() => MenuIconResizerPass.Instance.RunForTest(context));
        }

        /// <summary>
        /// With an expressions menu that has no controls (and therefore no icon textures), the pass
        /// must do nothing, even without a MenuIconResizer component present.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenMenuHasNoIcons()
        {
            builder = new NdmfTestAvatarBuilder();
            menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            builder.AvatarDescriptor.expressionsMenu = menu;

            var context = new BuildContext(builder.Root, null);
            Assert.DoesNotThrow(() => MenuIconResizerPass.Instance.RunForTest(context));

            Assert.AreSame(menu, builder.AvatarDescriptor.expressionsMenu, "The menu must not be duplicated/replaced when there is nothing to resize.");
        }
    }
}
