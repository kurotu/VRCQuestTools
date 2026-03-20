// <copyright file="PhysBonesRemoveViewModelTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="PhysBonesRemoveViewModel"/>.
    /// </summary>
    public class PhysBonesRemoveViewModelTests
    {
        private GameObject avatarGo;
        private VRCAvatarDescriptor descriptor;

        [SetUp]
        public void SetUp()
        {
            avatarGo = new GameObject("TestAvatar");
            descriptor = avatarGo.AddComponent<VRCAvatarDescriptor>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(avatarGo);
        }

        [Test]
        public void SelectAvatar_SetsAvatar()
        {
            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            Assert.IsNotNull(vm.Avatar);
        }

        [Test]
        public void DoesSelectAllPhysBones_NoPhysBones_ReturnsTrue()
        {
            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            Assert.IsTrue(vm.DoesSelectAllPhysBones);
        }

        [Test]
        public void DoesSelectAllPhysBoneColliders_NoColliders_ReturnsTrue()
        {
            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            Assert.IsTrue(vm.DoesSelectAllPhysBoneColliders);
        }

        [Test]
        public void DoesSelectAllContacts_NoContacts_ReturnsTrue()
        {
            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            Assert.IsTrue(vm.DoesSelectAllContacts);
        }

        [Test]
        public void SelectAvatar_WithPhysBones_SelectsAll()
        {
            var child = new GameObject("Bone");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBone>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            Assert.IsTrue(vm.DoesSelectAllPhysBones);
        }

        [Test]
        public void SelectAllPhysBoneProviders_DeselectAll()
        {
            var child = new GameObject("Bone");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBone>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SelectAllPhysBoneProviders(false);
            Assert.IsFalse(vm.DoesSelectAllPhysBones);
        }

        [Test]
        public void SelectAllPhysBoneProviders_ReselectAll()
        {
            var child = new GameObject("Bone");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBone>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SelectAllPhysBoneProviders(false);
            vm.SelectAllPhysBoneProviders(true);
            Assert.IsTrue(vm.DoesSelectAllPhysBones);
        }

        [Test]
        public void PhysBoneProvidersToKeep_AllSelected_ReturnsAll()
        {
            var child = new GameObject("Bone");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBone>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            var toKeep = vm.PhysBoneProvidersToKeep.ToArray();
            Assert.AreEqual(1, toKeep.Length);
        }

        [Test]
        public void PhysBoneProvidersToKeep_NoneSelected_ReturnsEmpty()
        {
            var child = new GameObject("Bone");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBone>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SelectAllPhysBoneProviders(false);
            var toKeep = vm.PhysBoneProvidersToKeep.ToArray();
            Assert.AreEqual(0, toKeep.Length);
        }

        [Test]
        public void SelectAllPhysBoneColliders_DeselectAll()
        {
            var child = new GameObject("Collider");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBoneCollider>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SelectAllPhysBoneColliders(false);
            Assert.IsFalse(vm.DoesSelectAllPhysBoneColliders);
        }

        [Test]
        public void SelectAllContacts_DeselectAll()
        {
            var child = new GameObject("Contact");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<ContactReceiver>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SelectAllContacts(false);
            Assert.IsFalse(vm.DoesSelectAllContacts);
        }

        [Test]
        public void ContactsToKeep_AllSelected_ReturnsAll()
        {
            var child = new GameObject("Contact");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<ContactReceiver>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            var toKeep = vm.ContactsToKeep.ToArray();
            Assert.AreEqual(1, toKeep.Length);
        }

        [Test]
        public void PhysBoneCollidersToKeep_AllSelected_ReturnsAll()
        {
            var child = new GameObject("Collider");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBoneCollider>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            var toKeep = vm.PhysBoneCollidersToKeep.ToArray();
            Assert.AreEqual(1, toKeep.Length);
        }

        [Test]
        public void SelectedPhysBonesOrderMatchesWithOriginal_AllSelected_ReturnsTrue()
        {
            var child1 = new GameObject("Bone1");
            child1.transform.SetParent(avatarGo.transform);
            child1.AddComponent<VRCPhysBone>();

            var child2 = new GameObject("Bone2");
            child2.transform.SetParent(avatarGo.transform);
            child2.AddComponent<VRCPhysBone>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            Assert.IsTrue(vm.SelectedPhysBonesOrderMatchesWithOriginal());
        }

        [Test]
        public void DeselectRemovedComponents_NoRemovedComponents_KeepsAll()
        {
            var child = new GameObject("Bone");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBone>();

            var colliderChild = new GameObject("Collider");
            colliderChild.transform.SetParent(avatarGo.transform);
            colliderChild.AddComponent<VRCPhysBoneCollider>();

            var contactChild = new GameObject("Contact");
            contactChild.transform.SetParent(avatarGo.transform);
            contactChild.AddComponent<ContactReceiver>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.DeselectRemovedComponents();

            Assert.IsTrue(vm.DoesSelectAllPhysBones);
            Assert.IsTrue(vm.DoesSelectAllPhysBoneColliders);
            Assert.IsTrue(vm.DoesSelectAllContacts);
        }

        [Test]
        public void SelectedPhysBonesOrderMatchesWithOriginal_NoneSelected_ReturnsTrue()
        {
            var child = new GameObject("Bone");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<VRCPhysBone>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SelectAllPhysBoneProviders(false);
            Assert.IsTrue(vm.SelectedPhysBonesOrderMatchesWithOriginal());
        }

        [Test]
        public void SetSelectedPhysBoneProviders_OverwritesPrevious()
        {
            var child1 = new GameObject("Bone1");
            child1.transform.SetParent(avatarGo.transform);
            child1.AddComponent<VRCPhysBone>();

            var child2 = new GameObject("Bone2");
            child2.transform.SetParent(avatarGo.transform);
            child2.AddComponent<VRCPhysBone>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            var providers = vm.Avatar.GetPhysBoneProviders();
            vm.SetSelectedPhysBoneProviders(new[] { providers[0] });
            Assert.AreEqual(1, vm.PhysBoneProvidersToKeep.Count());
        }

        [Test]
        public void SetSelectedPhysBoneColliders_OverwritesPrevious()
        {
            var child1 = new GameObject("Collider1");
            child1.transform.SetParent(avatarGo.transform);
            child1.AddComponent<VRCPhysBoneCollider>();

            var child2 = new GameObject("Collider2");
            child2.transform.SetParent(avatarGo.transform);
            child2.AddComponent<VRCPhysBoneCollider>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            var colliders = vm.Avatar.GetPhysBoneColliders();
            vm.SetSelectedPhysBoneColliders(new[] { colliders[0] });
            Assert.AreEqual(1, vm.PhysBoneCollidersToKeep.Count());
        }

        [Test]
        public void SetSelectedContacts_OverwritesPrevious()
        {
            var child = new GameObject("Contact");
            child.transform.SetParent(avatarGo.transform);
            child.AddComponent<ContactReceiver>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SetSelectedContacts(new ContactBase[0]);
            Assert.AreEqual(0, vm.ContactsToKeep.Count());
        }

        [Test]
        public void SelectContact_IndividualToggle()
        {
            var child = new GameObject("Contact");
            child.transform.SetParent(avatarGo.transform);
            var contact = child.AddComponent<ContactReceiver>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SelectContact(contact, false);
            Assert.AreEqual(0, vm.ContactsToKeep.Count());
            vm.SelectContact(contact, true);
            Assert.AreEqual(1, vm.ContactsToKeep.Count());
            // Select again should not duplicate
            vm.SelectContact(contact, true);
            Assert.AreEqual(1, vm.ContactsToKeep.Count());
        }

        [Test]
        public void SelectPhysBoneCollider_IndividualToggle()
        {
            var child = new GameObject("Collider");
            child.transform.SetParent(avatarGo.transform);
            var collider = child.AddComponent<VRCPhysBoneCollider>();

            var vm = new PhysBonesRemoveViewModel();
            vm.SelectAvatar(descriptor);
            vm.SelectPhysBoneCollider(collider, false);
            Assert.AreEqual(0, vm.PhysBoneCollidersToKeep.Count());
            vm.SelectPhysBoneCollider(collider, true);
            Assert.AreEqual(1, vm.PhysBoneCollidersToKeep.Count());
            // Select again should not duplicate
            vm.SelectPhysBoneCollider(collider, true);
            Assert.AreEqual(1, vm.PhysBoneCollidersToKeep.Count());
        }

        [Test]
        public void SelectAvatar_Null_ThrowsNullReference()
        {
            var vm = new PhysBonesRemoveViewModel();
            // SelectAvatar(null) sets avatarRoot to null, then SelectAll* methods
            // access Avatar property which returns null, causing NRE
            Assert.Throws<System.NullReferenceException>(() =>
            {
                vm.SelectAvatar(null);
            });
        }
    }
}
