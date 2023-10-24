using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
using static KRT.VRCQuestTools.Utils.VRCSDKUtility.Reflection;
using static UnityEngine.UI.Image;

namespace KRT.VRCQuestTools.Views
{
    internal class AvatarDynamicsSelectorWindow : EditorWindow
    {
        internal AvatarConverter converter;
        internal VRCPhysBone[] physBonesToKeep = { };
        internal VRCPhysBoneCollider[] physBoneCollidersToKeep = { };
        internal VRC.Dynamics.ContactBase[] contactsToKeep = { };
        [NonSerialized]
        internal bool canceled = false;

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
            canceled = false;
        }

        private void OnGUI()
        {
            i18n = VRCQuestToolsSettings.I18nResource;
            if (converter == null)
            {
                EditorGUILayout.LabelField("Referenced AvatarConverter is missing.");
                if (GUILayout.Button(i18n.CloseLabel))
                {
                    Close();
                }
                return;
            }

            EditorGUILayout.LabelField(i18n.SelectComponentsToKeep, EditorStyles.wordWrappedLabel);
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutPhysBones, "PhysBones"))
                {
                    foldoutPhysBones = foldout.foldout;
                    if (foldoutPhysBones)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var pbs = converter.RootAvatar.GetComponentsInChildren<VRCPhysBone>(true);
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
                                        physBonesToKeep = pbs.ToArray();
                                    }
                                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                    {
                                        physBonesToKeep = new VRCPhysBone[] { };
                                    }
                                }
                                physBonesToKeep = EditorGUIUtility.ObjectSelectorList(pbs, physBonesToKeep);
                            }
                        }
                    }
                }

                EditorGUILayout.Space();

                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutPhysBonesColliders, "PhysBone Colliders"))
                {
                    foldoutPhysBonesColliders = foldout.foldout;
                    if (foldoutPhysBonesColliders)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var colliders = converter.RootAvatar.GetComponentsInChildren<VRCPhysBoneCollider>(true);
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
                                physBoneCollidersToKeep = EditorGUIUtility.ObjectSelectorList(colliders, physBoneCollidersToKeep);
                            }
                        }
                    }
                }

                EditorGUILayout.Space();

                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutContacts, "Contact Senders & Contact Receivers"))
                {
                    foldoutContacts = foldout.foldout;
                    if (foldoutContacts)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var contacts = converter.RootAvatar.GetComponentsInChildren<VRC.Dynamics.ContactBase>(true);
                            if (contacts.Length == 0)
                            {
                                EditorGUILayout.LabelField("No Contact Senders & Contact Receivers found.");
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
                                contactsToKeep = EditorGUIUtility.ObjectSelectorList(contacts, contactsToKeep);
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.EstimatedPerformanceStats, EditorStyles.boldLabel);
            var pbToKeep = physBonesToKeep.Where(x => x != null).Select(pb => new PhysBone(pb)).ToArray();
            var pbcToKeep = physBoneCollidersToKeep.Where(x => x != null).Select(pbc => new PhysBoneCollider(pbc)).ToArray();
            var cToKeep = contactsToKeep.Where(x => x != null).Select(c => new VRCSDKUtility.Reflection.ContactBase(c)).ToArray();
            var stats = Models.VRChat.AvatarDynamics.CalculatePerformanceStats(converter.RootAvatar.gameObject, pbToKeep, pbcToKeep, cToKeep);
            var categories = new AvatarPerformanceCategory[]
            {
                AvatarPerformanceCategory.PhysBoneComponentCount,
                AvatarPerformanceCategory.PhysBoneTransformCount,
                AvatarPerformanceCategory.PhysBoneColliderCount,
                AvatarPerformanceCategory.PhysBoneCollisionCheckCount,
                AvatarPerformanceCategory.ContactCount,
            };
            var ratings = categories.ToDictionary(x => x, x => Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, x));
            foreach (var category in categories)
            {
                EditorGUIUtility.PerformanceRatingPanel(stats, statsLevelSet, category, i18n);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(i18n.ApplyButtonLabel))
            {
                converter.physBonesToKeep = physBonesToKeep;
                converter.physBoneCollidersToKeep = physBoneCollidersToKeep;
                converter.contactsToKeep = contactsToKeep;
                Close();
            }

            EditorGUILayout.Space();
        }
    }
}
