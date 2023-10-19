using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
using static KRT.VRCQuestTools.Utils.VRCSDKUtility.Reflection;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Inspector for <see cref="AvatarConverter"/>.
    /// </summary>
    [CustomEditor(typeof(AvatarConverter))]
    public class AvatarConverterEditor : Editor
    {
        [SerializeField]
        private bool foldOutEstimatedPerf = false;

        private I18nBase i18n;
        private AvatarPerformanceStatsLevelSet statsLevelSet;
        private PerformanceRating avatarDynamicsPerfLimit;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            bool canConvert = true;

            i18n = VRCQuestToolsSettings.I18nResource;

            var converter = (AvatarConverter)target;
            var descriptor = converter.gameObject.GetComponentInParent<VRC_AvatarDescriptor>();
            if (descriptor)
            {
                var avatar = new Models.VRChat.VRChatAvatar(descriptor);
                if (avatar.HasDynamicBoneComponents)
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.HelpBox(i18n.AlertForDynamicBoneConversion, MessageType.Warning);
                        if (GUILayout.Button(i18n.ConvertButtonLabel, GUILayout.Height(38), GUILayout.Width(60)))
                        {
                            OnClickConvertToPhysBonesButton();
                        }
                    }
                }
                if (VRCSDKUtility.HasMissingNetworkIds(avatar.AvatarDescriptor))
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.HelpBox(i18n.AlertForMissingNetIds, MessageType.Warning);
                        if (GUILayout.Button(i18n.AssignButtonLabel, GUILayout.Height(38), GUILayout.Width(60)))
                        {
                            OnClickAssignNetIdsButton();
                        }
                    }
                }

                var pbs = descriptor.GetComponentsInChildren(VRCSDKUtility.PhysBoneType, true);
                var multiPbObjs = pbs
                    .Select(pb => pb.gameObject)
                    .Where(go => go.GetComponents(VRCSDKUtility.PhysBoneType).Count() >= 2)
                    .Distinct()
                    .ToArray();
                if (multiPbObjs.Length > 0)
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        var message = $"{i18n.AlertForMultiplePhysBones}\n\n" +
                            $"{string.Join("\n", multiPbObjs.Select(x => $"  - {x.name}"))}";
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox(i18n.AvatarConverterMustBeChildrenOfAvatar, MessageType.Error);
                canConvert = false;
            }

            using (var disabled = new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Target Avatar", converter.destinationAvatar, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.AvatarConverterMaterialConvertSettingLabel, EditorStyles.boldLabel);
            MaterialSettingGUI(converter.defaultMaterialConvertSetting);
            if (GUILayout.Button(i18n.GenerateQuestTexturesLabel))
            {
                OnClickRegenerateTexturesButton();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.AvatarConverterAvatarDynamicsSettingLabel, EditorStyles.boldLabel);
            var so = new SerializedObject(target);
            so.Update();
            var m_physBones = so.FindProperty("physBonesToKeep");
            EditorGUILayout.PropertyField(m_physBones, new GUIContent("PhysBones", i18n.AvatarConverterPhysBonesTooltip));
            var m_physBoneColliders = so.FindProperty("physBoneCollidersToKeep");
            EditorGUILayout.PropertyField(m_physBoneColliders, new GUIContent("PhysBone Colliders", i18n.AvatarConverterPhysBoneCollidersTooltip));
            var m_contacts = so.FindProperty("contactsToKeep");
            EditorGUILayout.PropertyField(m_contacts, new GUIContent("Contact Senders & Receivers", i18n.AvatarConverterContactsTooltip));
            so.ApplyModifiedProperties();
            AvatarDynamicsPerformanceGUI(converter);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.AdvancedConverterSettingsLabel, EditorStyles.boldLabel);

            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("animatorOverrideControllers"), new GUIContent("Animator Override Controllers", i18n.AnimationOverrideTooltip));
            so.ApplyModifiedProperties();

            converter.removeVertexColor = EditorGUILayout.Toggle(new GUIContent(i18n.RemoveVertexColorLabel, i18n.RemoveVertexColorTooltip), converter.removeVertexColor);

            EditorGUILayout.Space();

            var componentsToBeAlearted = VRCQuestTools.ComponentRemover.GetUnsupportedComponentsInChildren(descriptor.gameObject, true)
                .Select(c => c.GetType().Name)
                .Distinct()
                .OrderBy(s => s)
                .ToArray();
            if (componentsToBeAlearted.Count() > 0)
            {
                EditorGUILayout.HelpBox(i18n.AlertForComponents + "\n\n" + string.Join("\n", componentsToBeAlearted.Select(c => $"  - {c}")), MessageType.Warning);
                EditorGUILayout.Space();
            }

            using (var disabled = new EditorGUI.DisabledGroupScope(!canConvert))
            {
                if (GUILayout.Button(i18n.ConvertButtonLabel))
                {
                    OnClickConvertButton();
                }
            }
        }

        private void OnEnable()
        {
            statsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            avatarDynamicsPerfLimit = PerformanceRating.Poor;
        }

        private void MaterialSettingGUI(IMaterialConvertSetting setting)
        {
            switch (setting)
            {
                case ToonLitConvertSetting toonLitSetting:
                    ToonLitSettingGUI(toonLitSetting);
                    break;
            }
        }

        private void AvatarDynamicsPerformanceGUI(AvatarConverter converter)
        {
            var original = converter.gameObject.GetComponentInParent<VRC_AvatarDescriptor>();
            if (original == null)
            {
                return;
            }
            var pbToKeep = converter.physBonesToKeep.Where(x => x != null).Select(pb => new PhysBone(pb)).ToArray();
            var pbcToKeep = converter.physBoneCollidersToKeep.Where(x => x != null).Select(pbc => new PhysBoneCollider(pbc)).ToArray();
            var contactsToKeep = converter.contactsToKeep.Where(x => x != null).Select(c => new ContactBase(c)).ToArray();
            var stats = Models.VRChat.AvatarDynamics.CalculatePerformanceStats(original.gameObject, pbToKeep, pbcToKeep, contactsToKeep);

            foldOutEstimatedPerf = EditorGUILayout.Foldout(foldOutEstimatedPerf, i18n.EstimatedPerformanceStats);
            var categories = new AvatarPerformanceCategory[]
            {
                AvatarPerformanceCategory.PhysBoneComponentCount,
                AvatarPerformanceCategory.PhysBoneTransformCount,
                AvatarPerformanceCategory.PhysBoneColliderCount,
                AvatarPerformanceCategory.PhysBoneCollisionCheckCount,
                AvatarPerformanceCategory.ContactCount,
            };
            var ratings = categories.ToDictionary(x => x, x => Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, x));
            var worst = ratings.Values.Max();
            Views.EditorGUIUtility.PerformanceRatingPanel(worst, $"Avatar Dynamics: {worst}", worst > avatarDynamicsPerfLimit ? i18n.AvatarDynamicsPreventsUpload : null);
            foreach (var category in categories)
            {
                if (foldOutEstimatedPerf || ratings[category] > avatarDynamicsPerfLimit)
                {
                    GUIRatingPanel(stats, category, i18n);
                }
            }
        }

        private void GUIRatingPanel(Models.VRChat.AvatarDynamics.PerformanceStats stats, AvatarPerformanceCategory category, I18nBase i18n)
        {
            var rating = Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, category);
            string categoryName;
            string veryPoorViolation;
            int value;
            int maximum;
            switch (category)
            {
                case AvatarPerformanceCategory.PhysBoneComponentCount:
                    categoryName = "PhysBones Components";
                    veryPoorViolation = i18n.PhysBonesWillBeRemovedAtRunTime;
                    value = stats.PhysBonesCount;
                    maximum = statsLevelSet.poor.physBone.componentCount;
                    break;
                case AvatarPerformanceCategory.PhysBoneTransformCount:
                    categoryName = "PhysBones Affected Transforms";
                    veryPoorViolation = i18n.PhysBonesTransformsShouldBeReduced;
                    value = stats.PhysBonesTransformCount;
                    maximum = statsLevelSet.poor.physBone.transformCount;
                    break;
                case AvatarPerformanceCategory.PhysBoneColliderCount:
                    categoryName = "PhysBones Colliders";
                    veryPoorViolation = i18n.PhysBoneCollidersWillBeRemovedAtRunTime;
                    value = stats.PhysBonesColliderCount;
                    maximum = statsLevelSet.poor.physBone.colliderCount;
                    break;
                case AvatarPerformanceCategory.PhysBoneCollisionCheckCount:
                    categoryName = "PhysBones Collision Check Count";
                    veryPoorViolation = i18n.PhysBonesCollisionCheckCountShouldBeReduced;
                    value = stats.PhysBonesCollisionCheckCount;
                    maximum = statsLevelSet.poor.physBone.collisionCheckCount;
                    break;
                case AvatarPerformanceCategory.ContactCount:
                    categoryName = "Avatar Dynamics Contacts";
                    veryPoorViolation = i18n.ContactsWillBeRemovedAtRunTime;
                    value = stats.ContactsCount;
                    maximum = statsLevelSet.poor.contactCount;
                    break;
                default: throw new InvalidOperationException();
            }
            var label = $"{categoryName}: {value} ({i18n.Maximum}: {maximum})";
            Views.EditorGUIUtility.PerformanceRatingPanel(rating, label, rating > avatarDynamicsPerfLimit ? veryPoorViolation : null);
        }

        private void ToonLitSettingGUI(ToonLitConvertSetting setting)
        {
            setting.generateQuestTextures = EditorGUILayout.Toggle(new GUIContent(i18n.GenerateQuestTexturesLabel, i18n.QuestTexturesDescription), setting.generateQuestTextures);
            using (var disabled = new EditorGUI.DisabledGroupScope(!setting.generateQuestTextures))
            {
                var sizes = new int[] { 0, 256, 512, 1024, 2048 };
                var names = sizes.Select(x => x == 0 ? "No Limits" : $"Max {x}x{x}").ToArray();
                setting.maxTextureSize = EditorGUILayout.IntPopup(i18n.TexturesSizeLimitLabel, setting.maxTextureSize, names, sizes);
                setting.mainTextureBrightness = EditorGUILayout.Slider(new GUIContent(i18n.MainTextureBrightnessLabel, i18n.MainTextureBrightnessTooltip), setting.mainTextureBrightness, 0.0f, 1.0f);
            }
        }

        private void OnClickConvertToPhysBonesButton()
        {
            throw new NotImplementedException();
        }

        private void OnClickAssignNetIdsButton()
        {
            throw new NotImplementedException();
        }

        private void OnClickRegenerateTexturesButton()
        {
            throw new NotImplementedException();
        }

        private void OnClickConvertButton()
        {
            throw new NotImplementedException();
        }
    }
}
