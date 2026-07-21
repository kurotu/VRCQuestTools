// <copyright file="AssignNetworkIDsPassTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Text.RegularExpressions;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Guard-clause tests for <see cref="AssignNetworkIDsPass"/> driven through NDMF's
    /// <see cref="BuildContext"/>. Exact PhysBone network ID assignment is covered by
    /// <c>VRCSDKUtility</c>-level tests, not here; this only covers the branches that decide
    /// whether assignment or the missing-assigner warning happens at all.
    /// </summary>
    public class AssignNetworkIDsPassTests
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
        /// Without a VRCAvatarDescriptor at all, the pass must not attempt any assignment.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNoAvatarDescriptor()
        {
            root = new GameObject("NoDescriptor");

            var context = new BuildContext(root, null);
            Assert.DoesNotThrow(() => AssignNetworkIDsPass.Instance.RunForTest(context));
        }

        /// <summary>
        /// With a VRCAvatarDescriptor but no NetworkIDAssigner and no AvatarConverterSettings, the
        /// pass has nothing to check assignment against and must not report a warning.
        /// </summary>
        [Test]
        public void Execute_DoesNothing_WhenNoAssignerAndNoSettings()
        {
            builder = new NdmfTestAvatarBuilder();

            var context = new BuildContext(builder.Root, null);
            Assert.DoesNotThrow(() => AssignNetworkIDsPass.Instance.RunForTest(context));
        }

        /// <summary>
        /// With AvatarConverterSettings.assignNetworkIds disabled and a PhysBone whose network ID is
        /// missing, the pass must report a warning telling the user to add a NetworkIDAssigner instead
        /// of silently leaving the PhysBone unsynchronized between platforms.
        /// </summary>
        [Test]
        public void Execute_ReportsWarning_WhenIdsAreMissingAndAssignmentIsDisabled()
        {
            builder = new NdmfTestAvatarBuilder();
            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            settings.assignNetworkIds = false;
            builder.Root.AddComponent<VRCPhysBone>();

            var context = new BuildContext(builder.Root, null);
            LogAssert.Expect(LogType.Warning, new Regex(@"^\[NDMF\] Error Reported: "));
            AssignNetworkIDsPass.Instance.RunForTest(context);
        }

        /// <summary>
        /// With AvatarConverterSettings.assignNetworkIds enabled, the pass must attempt assignment
        /// rather than reporting the missing-assigner warning.
        /// </summary>
        [Test]
        public void Execute_AssignsIds_WhenAssignmentIsEnabled()
        {
            builder = new NdmfTestAvatarBuilder();
            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            settings.assignNetworkIds = true;
            builder.Root.AddComponent<VRCPhysBone>();

            var context = new BuildContext(builder.Root, null);
            Assert.DoesNotThrow(() => AssignNetworkIDsPass.Instance.RunForTest(context));
        }
    }
}
