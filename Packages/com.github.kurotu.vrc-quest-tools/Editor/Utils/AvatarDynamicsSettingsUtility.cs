// <copyright file="AvatarDynamicsSettingsUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for applying Avatar Dynamics keep lists to <see cref="PlatformComponentRemover"/> components.
    /// </summary>
    internal static class AvatarDynamicsSettingsUtility
    {
        private const string ApplyUndoName = "Apply Avatar Dynamics Settings";

        /// <summary>
        /// Applies avatar dynamics settings to PlatformComponentRemover components and clears legacy fields.
        /// Components not in the keep lists will be configured for Android removal.
        /// </summary>
        /// <param name="converterSettings">AvatarConverterSettings to update.</param>
        /// <param name="providersToKeep">PhysBone providers to keep.</param>
        /// <param name="collidersToKeep">PhysBone colliders to keep.</param>
        /// <param name="contactsToKeep">Contacts to keep.</param>
        internal static void Apply(
            AvatarConverterSettings converterSettings,
            VRCPhysBoneProviderBase[] providersToKeep,
            VRCPhysBoneCollider[] collidersToKeep,
            ContactBase[] contactsToKeep)
        {
            Undo.SetCurrentGroupName(ApplyUndoName);
            var undoGroup = Undo.GetCurrentGroup();

            ApplyCore(converterSettings.AvatarDescriptor, providersToKeep, collidersToKeep, contactsToKeep);

            Undo.RecordObject(converterSettings, ApplyUndoName);
            converterSettings.physBonesToKeep = new VRCPhysBone[0];
            converterSettings.physBoneCollidersToKeep = new VRCPhysBoneCollider[0];
            converterSettings.contactsToKeep = new ContactBase[0];
            PrefabUtility.RecordPrefabInstancePropertyModifications(converterSettings);

            Undo.CollapseUndoOperations(undoGroup);
        }

        /// <summary>
        /// Applies avatar dynamics settings to PlatformComponentRemover components.
        /// Components not in the keep lists will be configured for Android removal.
        /// </summary>
        /// <param name="avatarDescriptor">Target avatar descriptor.</param>
        /// <param name="providersToKeep">PhysBone providers to keep.</param>
        /// <param name="collidersToKeep">PhysBone colliders to keep.</param>
        /// <param name="contactsToKeep">Contacts to keep.</param>
        internal static void Apply(
            VRC_AvatarDescriptor avatarDescriptor,
            VRCPhysBoneProviderBase[] providersToKeep,
            VRCPhysBoneCollider[] collidersToKeep,
            ContactBase[] contactsToKeep)
        {
            Undo.SetCurrentGroupName(ApplyUndoName);
            var undoGroup = Undo.GetCurrentGroup();

            ApplyCore(avatarDescriptor, providersToKeep, collidersToKeep, contactsToKeep);

            Undo.CollapseUndoOperations(undoGroup);
        }

        private static void ApplyCore(
            VRC_AvatarDescriptor avatarDescriptor,
            VRCPhysBoneProviderBase[] providersToKeep,
            VRCPhysBoneCollider[] collidersToKeep,
            ContactBase[] contactsToKeep)
        {
            var avatarRoot = avatarDescriptor.gameObject;
            var physBonesToKeep = providersToKeep.SelectMany(p => p.GetPhysBones()).ToArray();

            var allPhysBones = avatarRoot.GetComponentsInChildren<VRCPhysBone>(true);
            var allColliders = avatarRoot.GetComponentsInChildren<VRCPhysBoneCollider>(true);
            var allContacts = new VRChatAvatar(avatarDescriptor).GetNonLocalContacts();

            var physBonesToRemove = allPhysBones.Except(physBonesToKeep).ToArray();
            var collidersToRemove = allColliders.Except(collidersToKeep).ToArray();
            var contactsToRemove = allContacts.Except(contactsToKeep).ToArray();

            foreach (var component in physBonesToRemove.Cast<Component>().Concat(collidersToRemove).Concat(contactsToRemove))
            {
                var remover = GetOrAddPlatformComponentRemover(component.gameObject);
                Undo.RecordObject(remover, ApplyUndoName);
                remover.UpdateComponentSettings();
                var setting = System.Array.Find(remover.componentSettings, s => s.component == component);
                if (setting != null)
                {
                    setting.removeOnAndroid = true;
                }

                EditorUtility.SetDirty(remover);
                PrefabUtility.RecordPrefabInstancePropertyModifications(remover);
            }

            foreach (var component in physBonesToKeep.Cast<Component>().Concat(collidersToKeep).Concat(contactsToKeep))
            {
                var remover = component.gameObject.GetComponent<PlatformComponentRemover>();
                if (remover != null)
                {
                    Undo.RecordObject(remover, ApplyUndoName);
                    var setting = System.Array.Find(remover.componentSettings, s => s.component == component);
                    if (setting != null)
                    {
                        setting.removeOnAndroid = false;
                    }

                    EditorUtility.SetDirty(remover);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(remover);
                }
            }

            var allRemovers = avatarRoot.GetComponentsInChildren<PlatformComponentRemover>(true);
            foreach (var remover in allRemovers)
            {
                bool hasEffect = System.Array.Exists(remover.componentSettings, s => s.removeOnAndroid || s.removeOnPC);
                if (!hasEffect)
                {
                    Undo.DestroyObjectImmediate(remover);
                }
            }
        }

        private static PlatformComponentRemover GetOrAddPlatformComponentRemover(GameObject gameObject)
        {
            var remover = gameObject.GetComponent<PlatformComponentRemover>();
            if (remover == null)
            {
                remover = Undo.AddComponent<PlatformComponentRemover>(gameObject);
            }

            return remover;
        }
    }
}
