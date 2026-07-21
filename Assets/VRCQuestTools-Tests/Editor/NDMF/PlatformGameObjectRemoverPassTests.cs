// <copyright file="PlatformGameObjectRemoverPassTests.cs" company="kurotu">
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
    /// Tests for <see cref="PlatformGameObjectRemoverPass"/> driven through NDMF's <see cref="BuildContext"/>.
    /// </summary>
    public class PlatformGameObjectRemoverPassTests
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
        /// A GameObject with removeOnAndroid enabled is destroyed when the build target is Android.
        /// </summary>
        [Test]
        public void Execute_RemovesGameObject_WhenRemoveOnAndroidAndTargetIsAndroid()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            var child = new GameObject("Child");
            child.transform.SetParent(builder.Root.transform, false);
            child.AddComponent<PlatformGameObjectRemover>().removeOnAndroid = true;

            var context = new BuildContext(builder.Root, null);
            PlatformGameObjectRemoverPass.Instance.RunForTest(context);

            Assert.IsTrue(child == null, "GameObject with removeOnAndroid must be destroyed for the Android target.");
        }

        /// <summary>
        /// A GameObject without removeOnAndroid enabled is left untouched when the build target is Android.
        /// </summary>
        [Test]
        public void Execute_KeepsGameObject_WhenRemoveOnAndroidIsDisabled()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.Android;

            var child = new GameObject("Child");
            child.transform.SetParent(builder.Root.transform, false);
            child.AddComponent<PlatformGameObjectRemover>().removeOnAndroid = false;

            var context = new BuildContext(builder.Root, null);
            PlatformGameObjectRemoverPass.Instance.RunForTest(context);

            Assert.IsFalse(child == null, "GameObject must not be destroyed when removeOnAndroid is disabled.");
        }

        /// <summary>
        /// A GameObject with removeOnPC enabled is destroyed when the build target is PC.
        /// </summary>
        [Test]
        public void Execute_RemovesGameObject_WhenRemoveOnPCAndTargetIsPC()
        {
            builder = new NdmfTestAvatarBuilder();
            builder.Root.AddComponent<PlatformTargetSettings>().buildTarget = Models.BuildTarget.PC;

            var child = new GameObject("Child");
            child.transform.SetParent(builder.Root.transform, false);
            child.AddComponent<PlatformGameObjectRemover>().removeOnPC = true;

            var context = new BuildContext(builder.Root, null);
            PlatformGameObjectRemoverPass.Instance.RunForTest(context);

            Assert.IsTrue(child == null, "GameObject with removeOnPC must be destroyed for the PC target.");
        }
    }
}
