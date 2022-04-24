// <copyright file="PhysBonesRemoveWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Window for Remove PhysBones menu.
    /// </summary>
    internal class PhysBonesRemoveWindow : EditorWindow
    {
        private const int CheckBoxWidth = 16;
        private GUIStyle foldedContentPanel;

        [SerializeField]
        private PhysBonesRemoveViewModel model = new PhysBonesRemoveViewModel();
        private Vector2 scrollPosition;
        private bool showPhysBones = true;
        private bool showPhysBoneColliders = true;
        private bool showContacts = true;

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

        private static Transform GetRootTransform(Component component)
        {
            var type = component.GetType();
            if (type == VRCSDKUtility.PhysBoneType)
            {
                var bone = new VRCSDKUtility.Reflection.PhysBone(component);
                return bone.RootTransform;
            }

            if (type == VRCSDKUtility.PhysBoneColliderType)
            {
                var collider = new VRCSDKUtility.Reflection.PhysBoneCollider(component);
                return collider.RootTransform;
            }

            if (type == VRCSDKUtility.ContactReceiverType || type == VRCSDKUtility.ContactSenderType)
            {
                var contact = new VRCSDKUtility.Reflection.ContactBase(component);
                return contact.RootTransform;
            }

            return null;
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
        }

        private void OnGUI()
        {
#if UNITY_2019_1_OR_NEWER
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

                if (showPhysBones = EditorGUILayout.BeginFoldoutHeaderGroup(showPhysBones, new GUIContent($"PhysBones Components ({model.PhysBonesToKeep.Count()}/{VRCSDKUtility.PoorPhysBonesCountLimit})", i18n.PhysBonesListTooltip)))
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(foldedContentPanel))
                    {
                        var physBones = model.Avatar.GetPhysBones();
                        if (physBones.Length > 0)
                        {
                            var allSelected = GUIToggleAllField(i18n.KeepAll, model.DoesSelectAllPhysBones);
                            if (allSelected)
                            {
                                model.SelectAllPhysBones(true);
                            }
                            if (model.DoesSelectAllPhysBones && !allSelected)
                            {
                                model.SelectAllPhysBones(false);
                            }
                            foreach (var c in physBones)
                            {
                                var selected = GUIToggleComponentField(model.PhysBonesToKeep.Contains(c), c);
                                model.SelectPhysBone(c, selected);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No PhysBones Components");
                        }
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (showPhysBoneColliders = EditorGUILayout.BeginFoldoutHeaderGroup(showPhysBoneColliders, new GUIContent($"PhysBones Colliders ({model.PhysBoneCollidersToKeep.Count()}/{VRCSDKUtility.PoorPhysBoneCollidersCountLimit})", i18n.PhysBonesListTooltip)))
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(foldedContentPanel))
                    {
                        var colliders = model.Avatar.GetPhysBoneColliders();
                        if (colliders.Length > 0)
                        {
                            var allSelected = GUIToggleAllField(i18n.KeepAll, model.DoesSelectAllPhysBoneColliders);
                            if (allSelected)
                            {
                                model.SelectAllPhysBoneColliders(true);
                            }
                            if (model.DoesSelectAllPhysBoneColliders && !allSelected)
                            {
                                model.SelectAllPhysBoneColliders(false);
                            }
                            foreach (var c in colliders)
                            {
                                var selected = GUIToggleComponentField(model.PhysBoneCollidersToKeep.Contains(c), c);
                                model.SelectPhysBoneCollider(c, selected);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No PhysBones Colliders");
                        }
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (showContacts = EditorGUILayout.BeginFoldoutHeaderGroup(showContacts, new GUIContent($"Avatar Dynamics Contacts ({model.ContactsToKeep.Count()}/{VRCSDKUtility.PoorContactsCountLimit})", i18n.PhysBonesListTooltip)))
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(foldedContentPanel))
                    {
                        var contacts = model.Avatar.GetContacts();
                        if (contacts.Length > 0)
                        {
                            var allSelected = GUIToggleAllField(i18n.KeepAll, model.DoesSelectAllContacts);
                            if (allSelected)
                            {
                                model.SelectAllContacts(true);
                            }
                            if (model.DoesSelectAllContacts && !allSelected)
                            {
                                model.SelectAllContacts(false);
                            }
                            foreach (var c in model.Avatar.GetContacts())
                            {
                                var selected = GUIToggleComponentField(model.ContactsToKeep.Contains(c), c);
                                model.SelectContact(c, selected);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No Avatar Dynamics Contacts");
                        }
                        EditorGUILayout.Space();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Space();

            if (model.PhysBonesToKeep.Count() > VRCSDKUtility.PoorPhysBonesCountLimit)
            {
                EditorGUILayout.HelpBox("PhysBones Components: Very Poor (Quest)\n" + i18n.PhysBonesWillBeRemovedAtRunTime, MessageType.Error);
            }
            if (model.PhysBoneCollidersToKeep.Count() > VRCSDKUtility.PoorPhysBoneCollidersCountLimit)
            {
                EditorGUILayout.HelpBox("PhysBones Colliders: Very Poor (Quest)\n" + i18n.PhysBoneCollidersWillBeRemovedAtRunTime, MessageType.Error);
            }
            if (model.ContactsToKeep.Count() > VRCSDKUtility.PoorContactsCountLimit)
            {
                EditorGUILayout.HelpBox("Avatar Dynamics Contacts: Very Poor (Quest)\n" + i18n.ContactsWillBeRemovedAtRunTime, MessageType.Error);
            }

            if (GUILayout.Button(i18n.DeleteUnselectedComponents))
            {
                OnClickDelete();
            }

            EditorGUILayout.Space();
#else
            EditorGUILayout.LabelField("Unity 2019 is required.");
#endif
        }

        private bool GUIToggleAllField(string label, bool allSelected)
        {
            using (var horizontal = new EditorGUILayout.HorizontalScope())
            {
                var selected = EditorGUILayout.Toggle(allSelected, GUILayout.Width(CheckBoxWidth));
                EditorGUILayout.LabelField(label);
                return selected;
            }
        }

        private bool GUIToggleComponentField(bool value, Component component)
        {
            using (var horizontal = new EditorGUILayout.HorizontalScope())
            {
                var selected = EditorGUILayout.Toggle(value, GUILayout.Width(CheckBoxWidth));
                GUILayout.Space(2);
                EditorGUILayout.ObjectField(component, component.GetType(), true);
                GUILayout.Space(2);
                EditorGUILayout.ObjectField(GetRootTransform(component), typeof(Transform), true);
                return selected;
            }
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
