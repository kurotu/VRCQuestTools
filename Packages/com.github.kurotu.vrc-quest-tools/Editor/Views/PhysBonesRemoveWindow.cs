// <copyright file="PhysBonesRemoveWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

#if VQT_HAS_VRCSDK_BASE
using VRC.SDKBase.Validation.Performance;

using AvatarPerformanceStatsLevelSet = VRC.SDKBase.Validation.Performance.Stats.AvatarPerformanceStatsLevelSet;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using AvatarPerformanceCategory = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceCategory;
using AvatarPerformanceStatsLevelSet = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet;
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Window for Remove PhysBones menu.
    /// </summary>
    internal class PhysBonesRemoveWindow : EditorWindow
    {
        private GUIStyle foldedContentPanel;

        [SerializeField]
        private PhysBonesRemoveViewModel model = new PhysBonesRemoveViewModel();
        private Vector2 scrollPosition;
        private bool showPhysBones = true;
        private bool showPhysBoneColliders = true;
        private bool showContacts = true;

        private AvatarPerformanceStatsLevelSet statsLevelSet;

        /// <summary>
        /// Show a window then set the avatar as a target.
        /// </summary>
        /// <param name="avatar">Target component.</param>
        internal static void ShowWindow(VRC_AvatarDescriptor avatar)
        {
            var window = GetWindow<PhysBonesRemoveWindow>();
            if (window.model.Avatar?.AvatarDescriptor != avatar)
            {
                window.model.SelectAvatar(avatar);
            }
            window.Show();
        }

        /// <summary>
        /// Show a window.
        /// </summary>
        internal static void ShowWindow()
        {
            var window = GetWindow<PhysBonesRemoveWindow>();
            window.Show();
        }

        private void OnEnable()
        {
            titleContent.text = "PhysBones Remover";
            foldedContentPanel = new GUIStyle()
            {
                padding =
                {
                    left = 16,
                },
            };
            statsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            if (!VRCSDKUtility.IsPhysBonesImported())
            {
                EditorGUILayout.LabelField(i18n.PhysBonesSDKRequired);
                return;
            }

            var selectedAvatar = (VRC_AvatarDescriptor)EditorGUILayout.ObjectField(i18n.AvatarLabel, model.Avatar?.AvatarDescriptor, typeof(VRC_AvatarDescriptor), true);
            if (model.Avatar?.AvatarDescriptor != selectedAvatar)
            {
                OnSelectAvatar(selectedAvatar);
            }
            if (model.Avatar == null)
            {
                return;
            }
            model.DeselectRemovedComponents();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.SelectComponentsToKeep, EditorStyles.wordWrappedLabel);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                if (showPhysBones = EditorGUILayout.BeginFoldoutHeaderGroup(showPhysBones, new GUIContent("PhysBones", i18n.PhysBonesListTooltip)))
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(foldedContentPanel))
                    {
                        var physBones = model.Avatar.GetPhysBones().OrderBy(p => VRCSDKUtility.GetFullPathInHierarchy(p.gameObject)).ToArray();
                        if (physBones.Length > 0)
                        {
                            using (var horizontal = new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                {
                                    model.SelectAllPhysBones(true);
                                }
                                if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                {
                                    model.SelectAllPhysBones(false);
                                }
                            }
                            var selected = model.PhysBonesToKeep.ToArray();
                            selected = Views.EditorGUIUtility.AvatarDynamicsComponentSelectorList(model.Avatar.GetPhysBones(), selected);
                            model.SetSelectedPhysBones(selected);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No PhysBones found.");
                        }
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (showPhysBoneColliders = EditorGUILayout.BeginFoldoutHeaderGroup(showPhysBoneColliders, new GUIContent("PhysBone Colliders", i18n.PhysBonesListTooltip)))
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(foldedContentPanel))
                    {
                        var colliders = model.Avatar.GetPhysBoneColliders();
                        if (colliders.Length > 0)
                        {
                            using (var horizontal = new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                {
                                    model.SelectAllPhysBoneColliders(true);
                                }
                                if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                {
                                    model.SelectAllPhysBoneColliders(false);
                                }
                            }
                            var selected = model.PhysBoneCollidersToKeep.ToArray();
                            selected = Views.EditorGUIUtility.AvatarDynamicsComponentSelectorList(model.Avatar.GetPhysBoneColliders(), selected);
                            model.SetSelectedPhysBoneColliders(selected);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No PhysBone Colliders found.");
                        }
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

#if VQT_HAS_VRCSDK_LOCAL_CONTACT_SENDER
                var contactsHeader = "Non-Local Contact Senders & Non-Local Contact Receivers";
#elif VQT_HAS_VRCSDK_LOCAL_CONTACT_RECEIVER
                var contactsHeader = "Contact Senders & Non-Local Contact Receivers";
#else
                var contactsHeader = "Contact Senders & Contact Receivers";
#endif
                if (showContacts = EditorGUILayout.BeginFoldoutHeaderGroup(showContacts, new GUIContent(contactsHeader, i18n.PhysBonesListTooltip)))
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(foldedContentPanel))
                    {
                        var contacts = model.Avatar.GetNonLocalContacts();
                        if (contacts.Length > 0)
                        {
                            using (var horizontal = new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                {
                                    model.SelectAllContacts(true);
                                }
                                if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                {
                                    model.SelectAllContacts(false);
                                }
                            }
                            var selected = model.ContactsToKeep.ToArray();
                            selected = Views.EditorGUIUtility.AvatarDynamicsComponentSelectorList(contacts, selected);
                            model.SetSelectedContacts(selected);
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"No {contactsHeader} found.");
                        }
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Space();

            if (VRCSDKUtility.HasMissingNetworkIds(model.Avatar.AvatarDescriptor))
            {
                EditorGUILayout.HelpBox(i18n.PhysBonesShouldHaveNetworkID, MessageType.Warning);
            }

            EditorGUILayout.Space();

            var stats = model.Avatar.EstimatePerformanceStats(
                model.PhysBonesToKeep.Select(c => new VRCSDKUtility.Reflection.PhysBone(c)).ToArray(),
                model.PhysBoneCollidersToKeep.Select(c => new VRCSDKUtility.Reflection.PhysBoneCollider(c)).ToArray(),
                model.ContactsToKeep.Select(c => new VRCSDKUtility.Reflection.ContactBase(c)).ToArray(),
                true);
            EditorGUILayout.LabelField(i18n.EstimatedPerformanceStats, EditorStyles.boldLabel);
            foreach (var category in VRCSDKUtility.AvatarDynamicsPerformanceCategories)
            {
                EditorGUIUtility.PerformanceRatingPanel(stats, statsLevelSet, category, i18n);
            }

            if (GUILayout.Button(i18n.DeleteUnselectedComponents))
            {
                OnClickDelete();
            }

            EditorGUILayout.Space();
        }

        private void OnSelectAvatar(VRC_AvatarDescriptor avatar)
        {
            model.SelectAvatar(avatar);
        }

        private void OnClickDelete()
        {
            model.DeleteComponents();
        }
    }
}
