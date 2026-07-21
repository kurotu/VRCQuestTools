// <copyright file="BuildTargetConfigurationPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using NUnit.Framework;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="BuildTargetConfigurationPass"/> driven through NDMF's <see cref="BuildContext"/>.
    /// </summary>
    public class BuildTargetConfigurationPassTests
    {
        private NdmfTestAvatarBuilder builder;

        /// <summary>
        /// Cleans up objects and static session state modified during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            builder?.Destroy();
            builder = null;

            // NdmfSessionState.BuildTarget is process/session-wide state; always restore it so a
            // failed assertion above doesn't leak a non-Auto target into unrelated tests.
            NdmfSessionState.BuildTarget = Models.BuildTarget.Auto;
        }

        /// <summary>
        /// A pending session-wide build target request must be written onto a PlatformTargetSettings
        /// component on the avatar root, and the session-wide request must then be cleared back to Auto
        /// so it isn't accidentally reapplied to the next avatar built in the same Editor session.
        /// </summary>
        [Test]
        public void Execute_AppliesSessionBuildTargetAndResetsSessionState()
        {
            builder = new NdmfTestAvatarBuilder();
            NdmfSessionState.BuildTarget = Models.BuildTarget.Android;

            var context = new BuildContext(builder.Root, null);
            BuildTargetConfigurationPass.Instance.RunForTest(context);

            var targetSettings = builder.Root.GetComponent<PlatformTargetSettings>();
            Assert.IsNotNull(targetSettings, "A PlatformTargetSettings component must be added to carry the requested build target.");
            Assert.AreEqual(Models.BuildTarget.Android, targetSettings.buildTarget, "The requested build target must be written onto PlatformTargetSettings.");
            Assert.AreEqual(Models.BuildTarget.Auto, NdmfSessionState.BuildTarget, "The session-wide request must be reset to Auto after being applied.");
        }

        /// <summary>
        /// When no session-wide build target is pending (Auto), the pass must not add a
        /// PlatformTargetSettings component that wasn't already there.
        /// </summary>
        [Test]
        public void Execute_DoesNotAddSettings_WhenSessionBuildTargetIsAuto()
        {
            builder = new NdmfTestAvatarBuilder();
            NdmfSessionState.BuildTarget = Models.BuildTarget.Auto;

            var context = new BuildContext(builder.Root, null);
            BuildTargetConfigurationPass.Instance.RunForTest(context);

            var targetSettings = builder.Root.GetComponent<PlatformTargetSettings>();
            Assert.IsNull(targetSettings, "No PlatformTargetSettings should be added when there is no pending session build target.");
        }
    }
}
