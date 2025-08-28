// <copyright file="PhysBonesRemoveViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Models.VRChat.PhysBoneProviders;
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
        private List<Component> physBonesToKeep = new List<Component>();

        [SerializeField]
        private List<Component> physBoneCollidersToKeep = new List<Component>();

        [SerializeField]
        private List<Component> contactsToKeep = new List<Component>();

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
        internal bool DoesSelectAllPhysBones => Avatar.GetPhysBoneComponents().Length == physBonesToKeep.Count;

        /// <summary>
        /// Gets a value indicating whether all PhysBoneColliders are selected.
        /// </summary>
        internal bool DoesSelectAllPhysBoneColliders => Avatar.GetPhysBoneColliders().Length == physBoneCollidersToKeep.Count;

        /// <summary>
        /// Gets a value indicating whether all ContactReceiver and ContactSender are selected.
        /// </summary>
        internal bool DoesSelectAllContacts => Avatar.GetContacts().Length == contactsToKeep.Count;

        /// <summary>
        /// Gets selected PhysBones to keep.
        /// </summary>
        internal IEnumerable<IVRCPhysBoneProvider> PhysBonesToKeep
        {
            get
            {
                return physBonesToKeep
                    .Cast<VRCPhysBone>()
                    .Select(pb => new VRCPhysBoneProvider(pb))
                    .Cast<IVRCPhysBoneProvider>();
            }
        }

        /// <summary>
        /// Gets selected PhysBoneColliders to keep.
        /// </summary>
        internal IEnumerable<Component> PhysBoneCollidersToKeep => physBoneCollidersToKeep;

        /// <summary>
        /// Gets selected ContactReceivers and ContactSenders to keep.
        /// </summary>
        internal IEnumerable<Component> ContactsToKeep => contactsToKeep;

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

            SelectAllPhysBones(true);
            SelectAllPhysBoneColliders(true);
            SelectAllContacts(true);
        }

        /// <summary>
        /// Set selected PhysBones.
        /// </summary>
        /// <param name="physBones">Components.</param>
        internal void SetSelectedPhysBones(IEnumerable<IVRCPhysBoneProvider> physBones)
        {
            physBonesToKeep.Clear();
            physBonesToKeep.AddRange(physBones.Select(pb => pb.Component));
        }

        /// <summary>
        /// Select a PhysBone.
        /// </summary>
        /// <param name="physBone">Target component.</param>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectPhysBone(IVRCPhysBoneProvider physBone, bool select)
        {
            if (select)
            {
                if (!physBonesToKeep.Contains(physBone.Component))
                {
                    physBonesToKeep.Add(physBone.Component);
                }
            }
            else
            {
                physBonesToKeep.Remove(physBone.Component);
            }
        }

        /// <summary>
        /// Select all PhysBones.
        /// </summary>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectAllPhysBones(bool select)
        {
            physBonesToKeep.Clear();
            foreach (var b in Avatar.GetPhysBones())
            {
                SelectPhysBone(b, select);
            }
        }

        /// <summary>
        /// Set selected PhysBoneColliders.
        /// </summary>
        /// <param name="physBoneColliders">Components.</param>
        internal void SetSelectedPhysBoneColliders(IEnumerable<Component> physBoneColliders)
        {
            physBoneCollidersToKeep.Clear();
            physBoneCollidersToKeep.AddRange(physBoneColliders);
        }

        /// <summary>
        /// Select a PhysBoneCollider.
        /// </summary>
        /// <param name="collider">Target component.</param>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectPhysBoneCollider(Component collider, bool select)
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
        internal void SetSelectedContacts(IEnumerable<Component> contacts)
        {
            contactsToKeep.Clear();
            contactsToKeep.AddRange(contacts);
        }

        /// <summary>
        /// Select a ContactReceiver or a ContactSender.
        /// </summary>
        /// <param name="contact">Target component.</param>
        /// <param name="select">true to select, false to deselect.</param>
        internal void SelectContact(Component contact, bool select)
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
            var pbToKeep = physBonesToKeep.Cast<VRCPhysBone>().ToArray();
            var pbcToKeep = physBoneCollidersToKeep.Select(c => (VRCPhysBoneCollider)c).ToArray();
            var cToKeep = contactsToKeep.Select(c => (ContactBase)c).ToArray();
            VRCSDKUtility.DeleteAvatarDynamicsComponents(Avatar, pbToKeep, pbcToKeep, cToKeep);
            Undo.IncrementCurrentGroup();
        }

        /// <summary>
        /// Deselect all components which doen't exist in the target avatar.
        /// </summary>
        internal void DeselectRemovedComponents()
        {
            var physbones = Avatar.GetPhysBoneComponents();
            physBonesToKeep.RemoveAll(c => !physbones.Contains(c));

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
            var originalComponents = Avatar.GetPhysBones().Select(pb => pb.Component).Take(PhysBonesToKeep.Count());
            return PhysBonesToKeep.Select(pb => pb.Component).SequenceEqual(originalComponents);
        }
    }
}
