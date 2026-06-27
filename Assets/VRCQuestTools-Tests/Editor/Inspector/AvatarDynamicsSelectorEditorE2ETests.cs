// <copyright file="AvatarDynamicsSelectorEditorE2ETests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable CS0618

using System.Collections;
using System.Linq;
using EditorDriver;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// E2E tests for AvatarConverterSettingsEditor.InitializeSelectorWindow.
    /// Verifies that pre-existing legacy array elements are reflected in the
    /// AvatarDynamicsSelectorWindow initial state when the selector is opened
    /// (issue #185).
    /// </summary>
    public class AvatarDynamicsSelectorEditorE2ETests
    {
        private Driver driver;
        private GameObject avatarRoot;
        private VRCAvatarDescriptor descriptor;
        private AvatarConverterSettings converterSettings;
        private GameObject boneObject;
        private VRCPhysBone physBone;
        private VRCPhysBoneCollider physBoneCollider;
        private ContactReceiver contact;

        /// <summary>
        /// Creates a simple avatar hierarchy and a fresh Driver for each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            driver = new Driver();

            avatarRoot = new GameObject("TestAvatar");
            descriptor = avatarRoot.AddComponent<VRCAvatarDescriptor>();
            converterSettings = avatarRoot.AddComponent<AvatarConverterSettings>();

            boneObject = new GameObject("BoneObject");
            boneObject.transform.SetParent(avatarRoot.transform);
            physBone = boneObject.AddComponent<VRCPhysBone>();
            physBoneCollider = boneObject.AddComponent<VRCPhysBoneCollider>();
            contact = boneObject.AddComponent<ContactReceiver>();
        }

        /// <summary>
        /// Disposes the Driver (closes windows) and destroys the test hierarchy.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            driver?.Dispose();
            if (avatarRoot != null)
            {
                Object.DestroyImmediate(avatarRoot);
            }
        }

        /// <summary>
        /// When physBonesToKeep contains a non-null entry (legacy mode),
        /// InitializeSelectorWindow must populate physBoneProvidersToKeep
        /// from the legacy array so the selector shows the previously chosen bone.
        /// </summary>
        [UnityTest]
        public IEnumerator LegacyPhysBones_InitializeSelectorWindow_ProvidersMatchLegacyArray()
        {
            converterSettings.physBonesToKeep = new[] { physBone };

            var handle = driver.OpenWindow<AvatarDynamicsSelectorWindow>();
            yield return null;
            var window = (AvatarDynamicsSelectorWindow)handle.Window;

            AvatarConverterSettingsEditor.InitializeSelectorWindow(window, converterSettings, descriptor);

            var providers = handle.GetFieldValue<VRCPhysBoneProviderBase[]>("physBoneProvidersToKeep");
            Assert.AreEqual(1, providers.Length, "Should have exactly one provider from legacy array");
            Assert.IsTrue(
                providers[0].GetPhysBones().Contains(physBone),
                "Provider should wrap the legacy physBone");
        }

        /// <summary>
        /// When physBoneCollidersToKeep contains a non-null entry (legacy mode),
        /// InitializeSelectorWindow must copy the legacy collider array to the window.
        /// </summary>
        [UnityTest]
        public IEnumerator LegacyColliders_InitializeSelectorWindow_CollidersMatchLegacyArray()
        {
            converterSettings.physBoneCollidersToKeep = new[] { physBoneCollider };

            var handle = driver.OpenWindow<AvatarDynamicsSelectorWindow>();
            yield return null;
            var window = (AvatarDynamicsSelectorWindow)handle.Window;

            AvatarConverterSettingsEditor.InitializeSelectorWindow(window, converterSettings, descriptor);

            var collidersToKeep = handle.GetFieldValue<VRCPhysBoneCollider[]>("physBoneCollidersToKeep");
            Assert.AreEqual(1, collidersToKeep.Length, "Should have exactly one collider from legacy array");
            Assert.AreEqual(physBoneCollider, collidersToKeep[0], "Collider should match the legacy entry");
        }

        /// <summary>
        /// When contactsToKeep contains a non-null entry (legacy mode),
        /// InitializeSelectorWindow must copy the legacy contact array to the window.
        /// </summary>
        [UnityTest]
        public IEnumerator LegacyContacts_InitializeSelectorWindow_ContactsMatchLegacyArray()
        {
            converterSettings.contactsToKeep = new ContactBase[] { contact };

            var handle = driver.OpenWindow<AvatarDynamicsSelectorWindow>();
            yield return null;
            var window = (AvatarDynamicsSelectorWindow)handle.Window;

            AvatarConverterSettingsEditor.InitializeSelectorWindow(window, converterSettings, descriptor);

            var contactsToKeep = handle.GetFieldValue<ContactBase[]>("contactsToKeep");
            Assert.AreEqual(1, contactsToKeep.Length, "Should have exactly one contact from legacy array");
            Assert.AreEqual(contact, contactsToKeep[0], "Contact should match the legacy entry");
        }

        /// <summary>
        /// When all legacy arrays are empty (fresh avatar / PCR mode),
        /// InitializeSelectorWindow selects all dynamics components by default
        /// (because no PCR marks any of them for Android removal).
        /// </summary>
        [UnityTest]
        public IEnumerator FreshAvatar_InitializeSelectorWindow_AllPhysBonesInitiallySelected()
        {
            // No legacy entries; no PlatformComponentRemover on the hierarchy.
            var handle = driver.OpenWindow<AvatarDynamicsSelectorWindow>();
            yield return null;
            var window = (AvatarDynamicsSelectorWindow)handle.Window;

            AvatarConverterSettingsEditor.InitializeSelectorWindow(window, converterSettings, descriptor);

            var providers = handle.GetFieldValue<VRCPhysBoneProviderBase[]>("physBoneProvidersToKeep");
            Assert.IsTrue(
                providers.SelectMany(p => p.GetPhysBones()).Contains(physBone),
                "PhysBone should be selected by default on a fresh avatar (PCR mode with no removal settings)");
        }

        /// <summary>
        /// When all legacy arrays are empty (fresh avatar / PCR mode),
        /// InitializeSelectorWindow selects all colliders by default.
        /// </summary>
        [UnityTest]
        public IEnumerator FreshAvatar_InitializeSelectorWindow_AllCollidersInitiallySelected()
        {
            var handle = driver.OpenWindow<AvatarDynamicsSelectorWindow>();
            yield return null;
            var window = (AvatarDynamicsSelectorWindow)handle.Window;

            AvatarConverterSettingsEditor.InitializeSelectorWindow(window, converterSettings, descriptor);

            var collidersToKeep = handle.GetFieldValue<VRCPhysBoneCollider[]>("physBoneCollidersToKeep");
            Assert.IsTrue(
                collidersToKeep.Contains(physBoneCollider),
                "PhysBoneCollider should be selected by default on a fresh avatar");
        }

        /// <summary>
        /// When a PlatformComponentRemover marks a PhysBone for Android removal (PCR mode),
        /// InitializeSelectorWindow must exclude that PhysBone from physBoneProvidersToKeep.
        /// </summary>
        [UnityTest]
        public IEnumerator PCRMode_PhysBoneMarkedForRemoval_NotSelectedInWindow()
        {
            // Add a PCR that marks physBone for Android removal.
            var remover = boneObject.AddComponent<PlatformComponentRemover>();
            remover.UpdateComponentSettings();
            var setting = System.Array.Find(remover.componentSettings, s => s.component == physBone);
            if (setting != null)
            {
                setting.removeOnAndroid = true;
            }

            var handle = driver.OpenWindow<AvatarDynamicsSelectorWindow>();
            yield return null;
            var window = (AvatarDynamicsSelectorWindow)handle.Window;

            AvatarConverterSettingsEditor.InitializeSelectorWindow(window, converterSettings, descriptor);

            var providers = handle.GetFieldValue<VRCPhysBoneProviderBase[]>("physBoneProvidersToKeep");
            Assert.IsFalse(
                providers.SelectMany(p => p.GetPhysBones()).Contains(physBone),
                "PhysBone marked for Android removal by PCR should not be in the initial selection");
        }
    }
}
