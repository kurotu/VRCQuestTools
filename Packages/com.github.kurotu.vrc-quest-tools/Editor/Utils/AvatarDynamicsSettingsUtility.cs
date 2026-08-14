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

            ApplyCore(converterSettings.AvatarDescriptor, providersToKeep, collidersToKeep, contactsToKeep, true);

            Undo.RecordObject(converterSettings, ApplyUndoName);
            ClearLegacyArraysInPlace(converterSettings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(converterSettings);

            Undo.CollapseUndoOperations(undoGroup);
        }

        /// <summary>
        /// Applies avatar dynamics settings to PlatformComponentRemover components and clears legacy fields,
        /// without recording Undo operations. Use this for GameObjects loaded via
        /// <see cref="PrefabUtility.LoadPrefabContents(string)"/>, which the Undo system cannot target,
        /// or for GameObjects in a scene that is about to be saved and closed as a batch operation.
        /// </summary>
        /// <param name="converterSettings">AvatarConverterSettings to update.</param>
        /// <param name="providersToKeep">PhysBone providers to keep.</param>
        /// <param name="collidersToKeep">PhysBone colliders to keep.</param>
        /// <param name="contactsToKeep">Contacts to keep.</param>
        internal static void ApplyWithoutUndo(
            AvatarConverterSettings converterSettings,
            VRCPhysBoneProviderBase[] providersToKeep,
            VRCPhysBoneCollider[] collidersToKeep,
            ContactBase[] contactsToKeep)
        {
            ApplyCore(converterSettings.AvatarDescriptor, providersToKeep, collidersToKeep, contactsToKeep, false);

            ClearLegacyArraysInPlace(converterSettings);
            EditorUtility.SetDirty(converterSettings);

            // Needed when converterSettings lives on a prefab instance (e.g. inside a scene file being
            // migrated as a batch): without this, clearing the legacy fields above is lost on save,
            // since Unity only persists prefab-instance field changes recorded as instance overrides.
            PrefabUtility.RecordPrefabInstancePropertyModifications(converterSettings);
        }

        // Nulls out every element instead of assigning a new, shorter array. A prefab variant (or a
        // prefab instance anywhere - in a scene, in another project) can override one of these arrays
        // with its own, different, but EQUAL-LENGTH content; Unity only records a per-index override
        // in that case, not an Array.size override. If this prefab's array is later shrunk, that
        // now-out-of-range per-index override is silently dropped by Unity's prefab merge, and the
        // dependent variant/instance's own (unrelated) legacy settings are lost right along with it -
        // even if that variant/instance is never touched by this migration. Keeping the length stable
        // avoids that entirely; HasLegacyAvatarDynamicsSettings already treats an all-null array as
        // "no legacy settings" (KRT.VRCQuestTools.Components.AvatarConverterSettings.HasLegacyAvatarDynamicsSettings),
        // and every consumer of these arrays already filters out nulls.
        private static void ClearLegacyArraysInPlace(AvatarConverterSettings converterSettings)
        {
            for (var i = 0; i < converterSettings.physBonesToKeep.Length; i++)
            {
                converterSettings.physBonesToKeep[i] = null;
            }

            for (var i = 0; i < converterSettings.physBoneCollidersToKeep.Length; i++)
            {
                converterSettings.physBoneCollidersToKeep[i] = null;
            }

            for (var i = 0; i < converterSettings.contactsToKeep.Length; i++)
            {
                converterSettings.contactsToKeep[i] = null;
            }
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

            ApplyCore(avatarDescriptor, providersToKeep, collidersToKeep, contactsToKeep, true);

            Undo.CollapseUndoOperations(undoGroup);
        }

        private static void ApplyCore(
            VRC_AvatarDescriptor avatarDescriptor,
            VRCPhysBoneProviderBase[] providersToKeep,
            VRCPhysBoneCollider[] collidersToKeep,
            ContactBase[] contactsToKeep,
            bool recordUndo)
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
                var remover = GetOrAddPlatformComponentRemover(component.gameObject, recordUndo);
                if (recordUndo)
                {
                    Undo.RecordObject(remover, ApplyUndoName);
                }

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
                    if (recordUndo)
                    {
                        Undo.RecordObject(remover, ApplyUndoName);
                    }

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
                    if (recordUndo)
                    {
                        Undo.DestroyObjectImmediate(remover);
                    }
                    else
                    {
                        Object.DestroyImmediate(remover);
                    }
                }
            }
        }

        private static PlatformComponentRemover GetOrAddPlatformComponentRemover(GameObject gameObject, bool recordUndo)
        {
            var remover = gameObject.GetComponent<PlatformComponentRemover>();
            if (remover == null)
            {
                remover = recordUndo ? Undo.AddComponent<PlatformComponentRemover>(gameObject) : gameObject.AddComponent<PlatformComponentRemover>();
            }

            return remover;
        }
    }
}
