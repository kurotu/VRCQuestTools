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
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase.Validation.Performance;
using AvatarPerformanceStatsLevelSet = VRC.SDKBase.Validation.Performance.Stats.AvatarPerformanceStatsLevelSet;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

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
        [SerializeField]
        private Vector2 scrollPosition;
        [SerializeField]
        private bool showPhysBones = true;
        [SerializeField]
        private bool showPhysBoneColliders = true;
        [SerializeField]
        private bool showContacts = true;
        [SerializeField]
        private bool foldoutEstimatedPerformance = true;

        // Per-group foldout states keyed by relative path label. Default: open (true).
        private Dictionary<string, bool> physBoneGroupFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> colliderGroupFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> contactGroupFoldouts = new Dictionary<string, bool>();

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
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            foldedContentPanel = new GUIStyle()
            {
                padding =
                {
                    left = 16,
                },
            };
            statsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            AvatarDynamicsPreviewService.Initialize();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AvatarDynamicsPreviewService.Cleanup();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                model.ResetSelectionAfterPlayMode();
            }
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            if (EditorApplication.isPlaying)
            {
                // The referenced PhysBones/Colliders/Contacts are destroyed by NDMF's Play Mode avatar
                // processing, so they look "missing" while playing even though the selection is
                // preserved (see OnPlayModeStateChanged) and will reappear on returning to Edit Mode.
                EditorGUILayout.HelpBox(i18n.ExitPlayModeToEdit, MessageType.Warning);
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

            AvatarDynamicsPreviewService.BeginPreviewFrame(this);

            var avatarRoot = model.Avatar.AvatarDescriptor.gameObject;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.SelectComponentsToKeep, EditorStyles.wordWrappedLabel);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                if (showPhysBones = EditorGUILayout.BeginFoldoutHeaderGroup(showPhysBones, new GUIContent("PhysBones", i18n.PhysBonesListTooltip)))
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(foldedContentPanel))
                    {
                        var physBones = model.Avatar.GetPhysBoneProviders().OrderBy(p => VRCSDKUtility.GetFullPathInHierarchy(p.GameObject)).ToArray();
                        if (physBones.Length > 0)
                        {
                            using (var horizontal = new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                {
                                    model.SelectAllPhysBoneProviders(true);
                                }
                                if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                {
                                    model.SelectAllPhysBoneProviders(false);
                                }
                            }
                            var selected = model.PhysBoneProvidersToKeep.ToArray();
                            selected = Views.EditorGUIUtility.GroupedAvatarDynamicsComponentSelectorList(physBones, selected, avatarRoot, physBoneGroupFoldouts);
                            model.SetSelectedPhysBoneProviders(selected);
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
                            var selected = model.PhysBoneCollidersToKeep.Select(c => new VRCPhysBoneColliderProvider(c)).ToArray();
                            selected = Views.EditorGUIUtility.GroupedAvatarDynamicsComponentSelectorList(colliders.Select(c => new VRCPhysBoneColliderProvider(c)).ToArray(), selected, avatarRoot, colliderGroupFoldouts);
                            model.SetSelectedPhysBoneColliders(selected.Select(provider => provider.Component).Cast<VRCPhysBoneCollider>());
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No PhysBone Colliders found.");
                        }
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                var contactsHeader = "Non-Local Contact Senders & Non-Local Contact Receivers";
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
                            var selected = model.ContactsToKeep.Select(c => new VRCContactBaseProvider(c)).ToArray();
                            selected = Views.EditorGUIUtility.GroupedAvatarDynamicsComponentSelectorList(contacts.Select(c => new VRCContactBaseProvider(c)).ToArray(), selected, avatarRoot, contactGroupFoldouts);
                            model.SetSelectedContacts(selected.Select(provider => provider.Component).Cast<VRC.Dynamics.ContactBase>());
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

            // Update the fallback scene preview with the current selection (drawn while no row is hovered).
            AvatarDynamicsPreviewService.SetSelectedPreviewComponents(
                model.PhysBoneProvidersToKeep.Cast<IVRCAvatarDynamicsProvider>()
                    .Concat(model.ContactsToKeep.Where(c => c != null).Select(c => (IVRCAvatarDynamicsProvider)new VRCContactBaseProvider(c))));

            using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutEstimatedPerformance, new GUIContent(i18n.EstimatedPerformanceStats)))
            {
                foldoutEstimatedPerformance = foldout.Foldout;
                if (foldoutEstimatedPerformance)
                {
                    var stats = model.Avatar.EstimatePerformanceStats(
                        model.PhysBoneProvidersToKeep.ToArray(),
                        model.PhysBoneCollidersToKeep.ToArray(),
                        model.ContactsToKeep.ToArray(),
                        true);
                    foreach (var category in VRCSDKUtility.AvatarDynamicsPerformanceCategories)
                    {
                        EditorGUIUtility.PerformanceRatingPanel(stats, statsLevelSet, category, i18n);
                    }
                }
            }

            EditorGUILayout.Space(8);

            if (EditorGUIUtility.LargeButton(i18n.DeleteUnselectedComponents))
            {
                OnClickDelete();
            }

            EditorGUILayout.Space(4);

            if (EditorGUIUtility.LargeButton(i18n.SetPlatformComponentRemoverButtonLabel))
            {
                OnClickSetPlatformComponentRemover();
            }

            EditorGUILayout.Space(8);

            AvatarDynamicsPreviewService.EndPreviewFrame();
        }

        private void OnSelectAvatar(VRC_AvatarDescriptor avatar)
        {
            model.SelectAvatar(avatar);
        }

        private void OnClickDelete()
        {
            model.DeleteComponents();
        }

        private void OnClickSetPlatformComponentRemover()
        {
            AvatarDynamicsSettingsUtility.Apply(
                model.Avatar.AvatarDescriptor,
                model.PhysBoneProvidersToKeep.ToArray(),
                model.PhysBoneCollidersToKeep.ToArray(),
                model.ContactsToKeep.ToArray());
        }
    }
}
