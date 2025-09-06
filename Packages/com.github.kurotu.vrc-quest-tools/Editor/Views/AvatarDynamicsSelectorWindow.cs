using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase.Validation.Performance.Stats;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Editor window for selecting avatar dynamics components to keep.
    /// </summary>
    internal class AvatarDynamicsSelectorWindow : EditorWindow
    {
        /// <summary>
        /// AvatarConverterSettings to edit.
        /// </summary>
        internal AvatarConverterSettings converterSettings;

        /// <summary>
        /// PhysBone providers to keep.
        /// </summary>
        internal VRCPhysBoneProviderBase[] physBoneProvidersToKeep = { };

        /// <summary>
        /// PhysBoneColliders to keep.
        /// </summary>
        internal VRCPhysBoneCollider[] physBoneCollidersToKeep = { };

        /// <summary>
        /// ContactSenders & ContactReceivers to keep.
        /// </summary>
        internal VRC.Dynamics.ContactBase[] contactsToKeep = { };

        [SerializeField]
        private Vector2 scrollPosition = Vector2.zero;
        [SerializeField]
        private bool foldoutPhysBones = true;
        [SerializeField]
        private bool foldoutPhysBonesColliders = true;
        [SerializeField]
        private bool foldoutContacts = true;

        private GUIStyle foldoutContentStyle;
        private I18nBase i18n;
        private AvatarPerformanceStatsLevelSet statsLevelSet;

        private void OnEnable()
        {
            titleContent = new GUIContent("Avatar Dynamics Selector");
            foldoutContentStyle = new GUIStyle()
            {
                padding = new RectOffset(16, 0, 0, 0),
            };
            statsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            AvatarDynamicsPreviewService.Initialize();
        }

        private void OnDisable()
        {
            AvatarDynamicsPreviewService.Cleanup();
        }

        private void OnGUI()
        {
            i18n = VRCQuestToolsSettings.I18nResource;
            if (converterSettings == null)
            {
                EditorGUILayout.LabelField("Referenced AvatarConverter is missing.");
                if (GUILayout.Button(i18n.CloseLabel))
                {
                    Close();
                }
                return;
            }

            var avatar = new VRChatAvatar(converterSettings.AvatarDescriptor);

            EditorGUILayout.LabelField(i18n.SelectComponentsToKeep, EditorStyles.wordWrappedLabel);
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutPhysBones, new GUIContent("PhysBones", i18n.PhysBonesListTooltip)))
                {
                    foldoutPhysBones = foldout.Foldout;
                    if (foldoutPhysBones)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var pbs = avatar.GetPhysBoneProviders();
                            if (pbs.Length == 0)
                            {
                                EditorGUILayout.LabelField("No PhysBones found.");
                            }
                            else
                            {
                                using (var horizontal = new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                    {
                                        physBoneProvidersToKeep = pbs.ToArray();
                                    }
                                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                    {
                                        physBoneProvidersToKeep = new VRCPhysBoneProviderBase[] { };
                                    }
                                }
                                physBoneProvidersToKeep = EditorGUIUtility.AvatarDynamicsComponentSelectorList(pbs, physBoneProvidersToKeep);
                            }
                        }
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space();

                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutPhysBonesColliders, new GUIContent("PhysBone Colliders", i18n.PhysBonesListTooltip)))
                {
                    foldoutPhysBonesColliders = foldout.Foldout;
                    if (foldoutPhysBonesColliders)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var colliders = converterSettings.AvatarDescriptor.GetComponentsInChildren<VRCPhysBoneCollider>(true);
                            if (colliders.Length == 0)
                            {
                                EditorGUILayout.LabelField("No PhysBone Colliders found.");
                            }
                            else
                            {
                                using (var horizontal = new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                    {
                                        physBoneCollidersToKeep = colliders.ToArray();
                                    }
                                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                    {
                                        physBoneCollidersToKeep = new VRCPhysBoneCollider[] { };
                                    }
                                }
                                var providers = colliders.Select(c => new VRCPhysBoneColliderProvider(c)).ToArray();
                                var selected = physBoneCollidersToKeep.Select(c => new VRCPhysBoneColliderProvider(c)).ToArray();
                                selected = EditorGUIUtility.AvatarDynamicsComponentSelectorList(providers, selected);
                                physBoneCollidersToKeep = selected.Select(p => p.Component).Cast<VRCPhysBoneCollider>().ToArray();
                            }
                        }
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space();

                var contactsHeader = "Non-Local Contact Senders & Non-Local Contact Receivers";
                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutContacts, new GUIContent(contactsHeader, i18n.PhysBonesListTooltip)))
                {
                    foldoutContacts = foldout.Foldout;
                    if (foldoutContacts)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var contacts = new VRChatAvatar(converterSettings.AvatarDescriptor).GetNonLocalContacts();
                            if (contacts.Length == 0)
                            {
                                EditorGUILayout.LabelField($"No {contactsHeader} found.");
                            }
                            else
                            {
                                using (var horizontal = new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                    {
                                        contactsToKeep = contacts.ToArray();
                                    }
                                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                    {
                                        contactsToKeep = new VRC.Dynamics.ContactBase[] { };
                                    }
                                }
                                var providers = contacts.Select(c => new VRCContactBaseProvider(c)).ToArray();
                                var selected = contactsToKeep.Select(c => new VRCContactBaseProvider(c)).ToArray();
                                selected = EditorGUIUtility.AvatarDynamicsComponentSelectorList(providers, selected);
                                contactsToKeep = selected.Select(p => p.Component).Cast<ContactBase>().ToArray();
                            }
                        }
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.EstimatedPerformanceStats, EditorStyles.boldLabel);
            var pbcToKeep = physBoneCollidersToKeep.Where(x => x != null).ToArray();
            var cToKeep = contactsToKeep.Where(x => x != null).ToArray();
            var stats = avatar.EstimatePerformanceStats(physBoneProvidersToKeep, pbcToKeep, cToKeep, true);
            var categories = VRCSDKUtility.AvatarDynamicsPerformanceCategories;
            foreach (var category in categories)
            {
                EditorGUIUtility.PerformanceRatingPanel(stats, statsLevelSet, category, i18n);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(i18n.ApplyButtonLabel))
            {
                converterSettings.physBonesToKeep = physBoneProvidersToKeep.SelectMany(p => p.GetPhysBones()).ToArray();
                converterSettings.physBoneCollidersToKeep = physBoneCollidersToKeep;
                converterSettings.contactsToKeep = contactsToKeep;
                PrefabUtility.RecordPrefabInstancePropertyModifications(converterSettings);

                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }

                Close();
            }

            EditorGUILayout.Space();
        }
    }
}
