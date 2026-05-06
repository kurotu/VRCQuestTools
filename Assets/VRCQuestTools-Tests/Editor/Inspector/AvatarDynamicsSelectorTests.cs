// <copyright file="AvatarDynamicsSelectorTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable CS0618

using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Views;
using NUnit.Framework;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Tests for AvatarDynamicsSelectorWindow.ApplyDynamicsSettings.
    /// Verifies that the Apply logic configures PlatformComponentRemover correctly
    /// instead of writing to legacy physBonesToKeep fields (issue #185).
    /// </summary>
    public class AvatarDynamicsSelectorTests
    {
        private GameObject avatarRoot;
        private AvatarConverterSettings converterSettings;
        private GameObject boneObject;
        private VRCPhysBone physBone;
        private VRCPhysBoneCollider physBoneCollider;
        private ContactReceiver contact;

        /// <summary>
        /// Creates a simple avatar hierarchy with one PhysBone, one Collider, and one Contact for each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            avatarRoot = new GameObject("TestAvatar");
            avatarRoot.AddComponent<VRCAvatarDescriptor>();
            converterSettings = avatarRoot.AddComponent<AvatarConverterSettings>();

            boneObject = new GameObject("BoneObject");
            boneObject.transform.SetParent(avatarRoot.transform);
            physBone = boneObject.AddComponent<VRCPhysBone>();
            physBoneCollider = boneObject.AddComponent<VRCPhysBoneCollider>();
            contact = boneObject.AddComponent<ContactReceiver>();
        }

        /// <summary>
        /// Destroys the test avatar hierarchy after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (avatarRoot != null)
            {
                Object.DestroyImmediate(avatarRoot);
            }
        }

        /// <summary>
        /// When all components are deselected (none kept), PCR should be created with removeOnAndroid=true
        /// for all dynamics components.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_AllDeselected_CreatesRemoveOnAndroidPCR()
        {
            AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                converterSettings,
                new VRCPhysBoneProviderBase[0],
                new VRCPhysBoneCollider[0],
                new ContactBase[0]);

            var remover = boneObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNotNull(remover, "PlatformComponentRemover should be added to the bone object");

            var pbSetting = System.Array.Find(remover.componentSettings, s => s.component == physBone);
            Assert.IsNotNull(pbSetting, "PhysBone setting should exist in PCR");
            Assert.IsTrue(pbSetting.removeOnAndroid, "PhysBone should be marked for Android removal");

            var colliderSetting = System.Array.Find(remover.componentSettings, s => s.component == physBoneCollider);
            Assert.IsNotNull(colliderSetting, "PhysBoneCollider setting should exist in PCR");
            Assert.IsTrue(colliderSetting.removeOnAndroid, "PhysBoneCollider should be marked for Android removal");

            var contactSetting = System.Array.Find(remover.componentSettings, s => s.component == contact);
            Assert.IsNotNull(contactSetting, "Contact setting should exist in PCR");
            Assert.IsTrue(contactSetting.removeOnAndroid, "Contact should be marked for Android removal");
        }

        /// <summary>
        /// When all components are selected (all kept), no PCR should remain on the avatar.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_AllSelected_NoPCROnAvatar()
        {
            var provider = new VRCPhysBoneProvider(physBone);

            AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                converterSettings,
                new VRCPhysBoneProviderBase[] { provider },
                new VRCPhysBoneCollider[] { physBoneCollider },
                new ContactBase[] { contact });

            var remover = boneObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNull(remover, "PlatformComponentRemover should not be present when all components are kept");
        }

        /// <summary>
        /// After Apply, the legacy physBonesToKeep field should be cleared to empty.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_ClearsLegacyFields()
        {
            converterSettings.physBonesToKeep = new VRCPhysBone[] { physBone };
            converterSettings.physBoneCollidersToKeep = new VRCPhysBoneCollider[] { physBoneCollider };
            converterSettings.contactsToKeep = new ContactBase[] { contact };

            AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                converterSettings,
                new VRCPhysBoneProviderBase[] { new VRCPhysBoneProvider(physBone) },
                new VRCPhysBoneCollider[] { physBoneCollider },
                new ContactBase[] { contact });

            Assert.AreEqual(0, converterSettings.physBonesToKeep.Length, "physBonesToKeep should be cleared");
            Assert.AreEqual(0, converterSettings.physBoneCollidersToKeep.Length, "physBoneCollidersToKeep should be cleared");
            Assert.AreEqual(0, converterSettings.contactsToKeep.Length, "contactsToKeep should be cleared");
        }

        /// <summary>
        /// When PhysBone is deselected (to remove) but Collider is selected (to keep),
        /// only the PhysBone should be marked for Android removal.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_PartialSelection_OnlyDeselectedRemovedOnAndroid()
        {
            AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                converterSettings,
                new VRCPhysBoneProviderBase[0],
                new VRCPhysBoneCollider[] { physBoneCollider },
                new ContactBase[] { contact });

            var remover = boneObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNotNull(remover, "PlatformComponentRemover should be added");

            var pbSetting = System.Array.Find(remover.componentSettings, s => s.component == physBone);
            Assert.IsNotNull(pbSetting, "PhysBone setting should exist");
            Assert.IsTrue(pbSetting.removeOnAndroid, "PhysBone should be marked for Android removal");

            var colliderSetting = System.Array.Find(remover.componentSettings, s => s.component == physBoneCollider);
            if (colliderSetting != null)
            {
                Assert.IsFalse(colliderSetting.removeOnAndroid, "Collider should NOT be marked for Android removal");
            }

            var contactSetting = System.Array.Find(remover.componentSettings, s => s.component == contact);
            if (contactSetting != null)
            {
                Assert.IsFalse(contactSetting.removeOnAndroid, "Contact should NOT be marked for Android removal");
            }
        }

        /// <summary>
        /// When Apply is called twice: first removing a component, then keeping it,
        /// the existing PCR entry should be updated (removeOnAndroid=false) and PCR removed if empty.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_ReApplyWithAllKept_RemovesPCR()
        {
            // First: deselect PhysBone (mark for removal)
            AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                converterSettings,
                new VRCPhysBoneProviderBase[0],
                new VRCPhysBoneCollider[] { physBoneCollider },
                new ContactBase[] { contact });

            Assert.IsNotNull(boneObject.GetComponent<PlatformComponentRemover>(), "PCR should exist after first apply");

            // Second: keep all
            AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                converterSettings,
                new VRCPhysBoneProviderBase[] { new VRCPhysBoneProvider(physBone) },
                new VRCPhysBoneCollider[] { physBoneCollider },
                new ContactBase[] { contact });

            Assert.IsNull(boneObject.GetComponent<PlatformComponentRemover>(), "PCR should be removed when all components are kept");
        }

        /// <summary>
        /// Components on multiple GameObjects are each configured with their own PCR.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_MultipleGameObjects_EachGetsOwnPCR()
        {
            var otherBoneObject = new GameObject("OtherBone");
            otherBoneObject.transform.SetParent(avatarRoot.transform);
            var otherPhysBone = otherBoneObject.AddComponent<VRCPhysBone>();

            try
            {
                // Keep original physBone, remove otherPhysBone
                AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                    converterSettings,
                    new VRCPhysBoneProviderBase[] { new VRCPhysBoneProvider(physBone) },
                    new VRCPhysBoneCollider[] { physBoneCollider },
                    new ContactBase[] { contact });

                var removerOnBone = boneObject.GetComponent<PlatformComponentRemover>();
                Assert.IsNull(removerOnBone, "Original bone object should not have PCR (all its dynamics are kept)");

                var removerOnOther = otherBoneObject.GetComponent<PlatformComponentRemover>();
                Assert.IsNotNull(removerOnOther, "Other bone object should have PCR");

                var setting = System.Array.Find(removerOnOther.componentSettings, s => s.component == otherPhysBone);
                Assert.IsNotNull(setting, "otherPhysBone setting should exist in PCR");
                Assert.IsTrue(setting.removeOnAndroid, "otherPhysBone should be marked for Android removal");
            }
            finally
            {
                Object.DestroyImmediate(otherBoneObject);
            }
        }

        /// <summary>
        /// Existing PCR with removeOnPC=true is preserved even when removeOnAndroid is cleared.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_PreservesRemoveOnPC()
        {
            // Add PCR manually with removeOnPC=true
            var existingRemover = boneObject.AddComponent<PlatformComponentRemover>();
            existingRemover.UpdateComponentSettings();
            var pbSetting = System.Array.Find(existingRemover.componentSettings, s => s.component == physBone);
            if (pbSetting != null)
            {
                pbSetting.removeOnPC = true;
            }

            // Apply keeping all (removeOnAndroid should become false, removeOnPC should remain)
            AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                converterSettings,
                new VRCPhysBoneProviderBase[] { new VRCPhysBoneProvider(physBone) },
                new VRCPhysBoneCollider[] { physBoneCollider },
                new ContactBase[] { contact });

            var remover = boneObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNotNull(remover, "PCR with removeOnPC=true should still exist");
            var setting = System.Array.Find(remover.componentSettings, s => s.component == physBone);
            Assert.IsNotNull(setting);
            Assert.IsTrue(setting.removeOnPC, "removeOnPC should be preserved");
            Assert.IsFalse(setting.removeOnAndroid, "removeOnAndroid should be false");
        }

        /// <summary>
        /// AvatarConverterSettings.Reset should leave physBonesToKeep empty (new mode default).
        /// </summary>
        [Test]
        public void AvatarConverterSettings_Reset_LegacyFieldsEmpty()
        {
            // In new mode, physBonesToKeep should be empty by default after Reset()
            // (Reset is called when AddComponent is invoked in the editor, simulated by checking defaults)
            var newSettings = avatarRoot.AddComponent<AvatarConverterSettings>();
            Assert.AreEqual(0, newSettings.physBonesToKeep.Length, "physBonesToKeep should be empty by default");
            Assert.AreEqual(0, newSettings.physBoneCollidersToKeep.Length, "physBoneCollidersToKeep should be empty by default");
            Assert.AreEqual(0, newSettings.contactsToKeep.Length, "contactsToKeep should be empty by default");
            Object.DestroyImmediate(newSettings);
        }

        /// <summary>
        /// After ApplyDynamicsSettings, DynamicsSettingsConfiguredViaPCR should be set to true
        /// to prevent mode detection ambiguity with empty legacy arrays.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_SetsMigrationFlag()
        {
            Assert.IsFalse(converterSettings.DynamicsSettingsConfiguredViaPCR, "Flag should be false before first apply");

            AvatarDynamicsSelectorWindow.ApplyDynamicsSettings(
                converterSettings,
                new VRCPhysBoneProviderBase[0],
                new VRCPhysBoneCollider[0],
                new ContactBase[0]);

            Assert.IsTrue(converterSettings.DynamicsSettingsConfiguredViaPCR, "Flag should be true after ApplyDynamicsSettings");
        }
    }
}
