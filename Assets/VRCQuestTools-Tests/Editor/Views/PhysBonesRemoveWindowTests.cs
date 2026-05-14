// <copyright file="PhysBonesRemoveWindowTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Tests for AvatarDynamicsSettingsUtility.Apply(VRC_AvatarDescriptor, ...)
    /// used by PhysBonesRemoveWindow's "Set Platform Component Remover" button.
    /// </summary>
    public class PhysBonesRemoveWindowTests
    {
        private GameObject avatarRoot;
        private VRCAvatarDescriptor descriptor;
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
            descriptor = avatarRoot.AddComponent<VRCAvatarDescriptor>();

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
        /// When all components are deselected, PCR should be created with removeOnAndroid=true
        /// for all dynamics components.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_AllDeselected_CreatesRemoveOnAndroidPCR()
        {
            AvatarDynamicsSettingsUtility.Apply(
                descriptor,
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

            AvatarDynamicsSettingsUtility.Apply(
                descriptor,
                new VRCPhysBoneProviderBase[] { provider },
                new VRCPhysBoneCollider[] { physBoneCollider },
                new ContactBase[] { contact });

            var remover = boneObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNull(remover, "PlatformComponentRemover should not be present when all components are kept");
        }

        /// <summary>
        /// When PhysBone is deselected but Collider and Contact are selected,
        /// only the PhysBone should be marked for Android removal.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_PartialSelection_OnlyDeselectedRemovedOnAndroid()
        {
            AvatarDynamicsSettingsUtility.Apply(
                descriptor,
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
        /// Calling Apply twice: first deselecting a component, then keeping it,
        /// should clear removeOnAndroid and remove PCR if no longer needed.
        /// </summary>
        [Test]
        public void ApplyDynamicsSettings_ReApplyWithAllKept_RemovesPCR()
        {
            // First: deselect PhysBone
            AvatarDynamicsSettingsUtility.Apply(
                descriptor,
                new VRCPhysBoneProviderBase[0],
                new VRCPhysBoneCollider[] { physBoneCollider },
                new ContactBase[] { contact });

            Assert.IsNotNull(boneObject.GetComponent<PlatformComponentRemover>(), "PCR should exist after first apply");

            // Second: keep all
            AvatarDynamicsSettingsUtility.Apply(
                descriptor,
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
                AvatarDynamicsSettingsUtility.Apply(
                    descriptor,
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
            var existingRemover = boneObject.AddComponent<PlatformComponentRemover>();
            existingRemover.UpdateComponentSettings();
            var pbSetting = System.Array.Find(existingRemover.componentSettings, s => s.component == physBone);
            if (pbSetting != null)
            {
                pbSetting.removeOnPC = true;
            }

            AvatarDynamicsSettingsUtility.Apply(
                descriptor,
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
        /// SelectAvatar should deselect components that are configured to remove on Android by PCR.
        /// </summary>
        [Test]
        public void SelectAvatar_WithPcrRemoveOnAndroid_DeselectsMarkedComponents()
        {
            var remover = boneObject.AddComponent<PlatformComponentRemover>();
            remover.UpdateComponentSettings();
            foreach (var setting in remover.componentSettings)
            {
                if (setting.component == physBone || setting.component == physBoneCollider || setting.component == contact)
                {
                    setting.removeOnAndroid = true;
                }
            }

            var model = new PhysBonesRemoveViewModel();
            model.SelectAvatar(descriptor);

            Assert.IsFalse(
                model.PhysBoneProvidersToKeep.SelectMany(provider => provider.GetPhysBones()).Contains(physBone),
                "PhysBone marked for Android removal should be deselected");
            Assert.IsFalse(
                model.PhysBoneCollidersToKeep.Contains(physBoneCollider),
                "PhysBoneCollider marked for Android removal should be deselected");
            Assert.IsFalse(
                model.ContactsToKeep.Contains(contact),
                "Contact marked for Android removal should be deselected");
        }
    }
}
