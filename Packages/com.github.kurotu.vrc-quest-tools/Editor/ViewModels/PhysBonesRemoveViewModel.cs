// <copyright file="PhysBonesRemoveViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for PhysBonesRemoveWindow.
    /// </summary>
    [Serializable]
    internal class PhysBonesRemoveViewModel
    {
        [SerializeField]
        private GameObject avatarRoot;

        [SerializeField]
        private List<VRCPhysBoneProviderBase> physBonesToKeep = new List<VRCPhysBoneProviderBase>();

        [SerializeField]
        private List<VRCPhysBoneCollider> physBoneCollidersToKeep = new List<VRCPhysBoneCollider>();

        [SerializeField]
        private List<ContactBase> contactsToKeep = new List<ContactBase>();

        /// <summary>
        /// Gets the target avatar.
        /// </summary>
        internal VRChatAvatar Avatar
        {
            get
            {
                if (avatarRoot == null)
                {
                    return null;
                }
                return new VRChatAvatar(avatarRoot.GetComponent<VRC_AvatarDescriptor>());
            }
        }

        /// <summary>
        /// Gets a value indicating whether all PhysBones are selected.
        /// </summary>
        internal bool DoesSelectAllPhysBones => Avatar.GetPhysBoneProviders().Length == physBonesToKeep.Count;

        /// <summary>
        /// Gets a value indicating whether all PhysBoneColliders are selected.
        /// </summary>
        internal bool DoesSelectAllPhysBoneColliders => Avatar.GetPhysBoneColliders().Length == physBoneCollidersToKeep.Count;

        /// <summary>
        /// Gets a value indicating whether all ContactReceiver and ContactSender are selected.
        /// </summary>
        internal bool DoesSelectAllContacts => Avatar.GetContacts().Length == contactsToKeep.Count;

        /// <summary>
        /// Gets selected PhysBones to keep as providers for abstraction layer.
        /// </summary>
        internal IEnumerable<VRCPhysBoneProviderBase> PhysBoneProvidersToKeep => physBonesToKeep;

        /// <summary>
        /// Gets selected PhysBoneColliders to keep.
        /// </summary>
        internal IEnumerable<VRCPhysBoneCollider> PhysBoneCollidersToKeep => physBoneCollidersToKeep;

        /// <summary>
        /// Gets selected ContactReceivers and ContactSenders to keep.
        /// </summary>
        internal IEnumerable<ContactBase> ContactsToKeep => contactsToKeep;

        /// <summary>
        /// Select a target avatar.
        /// </summary>
        /// <param name="avatar">Avatar to remove Avatar Dynamics components.</param>
        internal void SelectAvatar(VRC_AvatarDescriptor avatar)
        {
            if (avatar == null)
            {
                avatarRoot = null;
            }
            else
            {
                avatarRoot = avatar.gameObject;
            }
            physBonesToKeep.Clear();
            physBoneCollidersToKeep.Clear();
            contactsToKeep.Clear();

            SelectAllPhysBoneProviders(true);
            SelectAllPhysBoneColliders(true);
            SelectAllContacts(true);
        }

        /// <summary>
        /// Set selected PhysBones using providers.
        /// </summary>
        /// <param name="physBoneProviders">Provider components.</param>
        internal void SetSelectedPhysBoneProviders(IEnumerable<VRCPhysBoneProviderBase> physBoneProviders)
        {
            physBonesToKeep.Clear();
            physBonesToKeep.AddRange(physBoneProviders);
        }

        /// <summary>
        /// Select a PhysBone.
        /// </summary>
        /// <param name="physBone">Target component.</param>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectPhysBoneProvider(VRCPhysBoneProviderBase physBone, bool select)
        {
            if (select)
            {
                if (!physBonesToKeep.Contains(physBone))
                {
                    physBonesToKeep.Add(physBone);
                }
            }
            else
            {
                physBonesToKeep.Remove(physBone);
            }
        }

        /// <summary>
        /// Select all PhysBones using providers.
        /// </summary>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectAllPhysBoneProviders(bool select)
        {
            physBonesToKeep.Clear();
            foreach (var provider in Avatar.GetPhysBoneProviders())
            {
                SelectPhysBoneProvider(provider, select);
            }
        }

        /// <summary>
        /// Set selected PhysBoneColliders.
        /// </summary>
        /// <param name="physBoneColliders">Components.</param>
        internal void SetSelectedPhysBoneColliders(IEnumerable<VRCPhysBoneCollider> physBoneColliders)
        {
            physBoneCollidersToKeep.Clear();
            physBoneCollidersToKeep.AddRange(physBoneColliders);
        }

        /// <summary>
        /// Select a PhysBoneCollider.
        /// </summary>
        /// <param name="collider">Target component.</param>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectPhysBoneCollider(VRCPhysBoneCollider collider, bool select)
        {
            if (select)
            {
                if (!physBoneCollidersToKeep.Contains(collider))
                {
                    physBoneCollidersToKeep.Add(collider);
                }
            }
            else
            {
                physBoneCollidersToKeep.Remove(collider);
            }
        }

        /// <summary>
        /// Select all PhysBoneColliders.
        /// </summary>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectAllPhysBoneColliders(bool select)
        {
            foreach (var c in Avatar.GetPhysBoneColliders())
            {
                SelectPhysBoneCollider(c, select);
            }
        }

        /// <summary>
        /// Set selected ContactReceivers and ContactSenders.
        /// </summary>
        /// <param name="contacts">Components.</param>
        internal void SetSelectedContacts(IEnumerable<ContactBase> contacts)
        {
            contactsToKeep.Clear();
            contactsToKeep.AddRange(contacts);
        }

        /// <summary>
        /// Select a ContactReceiver or a ContactSender.
        /// </summary>
        /// <param name="contact">Target component.</param>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectContact(ContactBase contact, bool select)
        {
            if (select)
            {
                if (!contactsToKeep.Contains(contact))
                {
                    contactsToKeep.Add(contact);
                }
            }
            else
            {
                contactsToKeep.Remove(contact);
            }
        }

        /// <summary>
        /// Select all ContactReceivers and ContactSenders.
        /// </summary>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectAllContacts(bool select)
        {
            foreach (var c in Avatar.GetContacts())
            {
                SelectContact(c, select);
            }
        }

        /// <summary>
        /// Delete all unselected components.
        /// </summary>
        internal void DeleteComponents()
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Remove Avatar Dynamics Components");
            var pbToKeep = physBonesToKeep.ToArray();
            var pbcToKeep = physBoneCollidersToKeep.ToArray();
            var cToKeep = contactsToKeep.ToArray();
            VRCSDKUtility.DeleteAvatarDynamicsComponents(Avatar, pbToKeep, pbcToKeep, cToKeep);
            Undo.IncrementCurrentGroup();
        }

        /// <summary>
        /// Deselect all components which doen't exist in the target avatar.
        /// </summary>
        internal void DeselectRemovedComponents()
        {
            physBonesToKeep.RemoveAll(p => p.GetPhysBones().Length == 0);

            var colliders = Avatar.GetPhysBoneColliders();
            physBoneCollidersToKeep.RemoveAll(c => !colliders.Contains(c));

            var contacts = Avatar.GetContacts();
            contactsToKeep.RemoveAll(c => !contacts.Contains(c));
        }

        /// <summary>
        /// Gets whether selected physbones order matches with the orignal list.
        /// </summary>
        /// <returns>true when matched.</returns>
        internal bool SelectedPhysBonesOrderMatchesWithOriginal()
        {
            var originalPhysBones = Avatar.GetPhysBoneProviders().Select(p => p.Component as VRCPhysBone).Where(pb => pb != null);
            var selectedPhysBones = PhysBoneProvidersToKeep.Select(p => p.Component as VRCPhysBone).Where(pb => pb != null);
            return selectedPhysBones.SequenceEqual(originalPhysBones.Take(selectedPhysBones.Count()));
        }
    }
}
