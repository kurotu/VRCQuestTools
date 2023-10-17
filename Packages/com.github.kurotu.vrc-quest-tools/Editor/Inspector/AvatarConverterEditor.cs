using System;
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
        private I18nBase i18n;
        private AvatarPerformanceStatsLevelSet statsLevelSet;
        [SerializeField]
        private bool foldOutEstimatedPerf = false;

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
            }
            else
            {
                EditorGUILayout.HelpBox("Must be children of avatar", MessageType.Error);
                canConvert = false;
            }

            using (var disabled = new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Target Avatar", converter.destinationAvatar, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("マテリアル変換設定", EditorStyles.boldLabel);
            MaterialSettingGUI(converter.defaultMaterialConvertSetting);
            if (GUILayout.Button(i18n.GenerateQuestTexturesLabel))
            {
                OnClickRegenerateTexturesButton();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Avatar Dynamics 設定", EditorStyles.boldLabel);
            var so = new SerializedObject(target);
            so.Update();
            var m_physBones = so.FindProperty("physBonesToKeep");
            EditorGUILayout.PropertyField(m_physBones, new GUIContent("PhysBones", "Set PhysBones to keep while conversion."));
            var m_physBoneColliders = so.FindProperty("physBoneCollidersToKeep");
            EditorGUILayout.PropertyField(m_physBoneColliders, new GUIContent("PhysBone Colliders", "Set PhysBone Colliders to keep while conversion."));
            var m_contacts = so.FindProperty("contactsToKeep");
            EditorGUILayout.PropertyField(m_contacts, new GUIContent("Contact Senders & Receivers", "Set Contacts to keep while conversion."));
            so.ApplyModifiedProperties();
            AvatarDynamicsPerformanceGUI(converter);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(i18n.AdvancedConverterSettingsLabel, EditorStyles.boldLabel);

            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("animatorOverrideControllers"), true);
            so.ApplyModifiedProperties();

            converter.removeVertexColor = EditorGUILayout.Toggle(new GUIContent(i18n.RemoveVertexColorLabel, i18n.RemoveVertexColorTooltip), converter.removeVertexColor);

            EditorGUILayout.Space();

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
            if (foldOutEstimatedPerf)
            {
                GUIRatingPanel(stats, AvatarPerformanceCategory.PhysBoneComponentCount, i18n);
                GUIRatingPanel(stats, AvatarPerformanceCategory.PhysBoneTransformCount, i18n);
                GUIRatingPanel(stats, AvatarPerformanceCategory.PhysBoneColliderCount, i18n);
                GUIRatingPanel(stats, AvatarPerformanceCategory.PhysBoneCollisionCheckCount, i18n);
                GUIRatingPanel(stats, AvatarPerformanceCategory.ContactCount, i18n);
            }
            else
            {
                var ratings = new PerformanceRating[]
                {
                    Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, AvatarPerformanceCategory.PhysBoneComponentCount),
                    Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, AvatarPerformanceCategory.PhysBoneTransformCount),
                    Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, AvatarPerformanceCategory.PhysBoneColliderCount),
                    Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, AvatarPerformanceCategory.PhysBoneCollisionCheckCount),
                    Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, AvatarPerformanceCategory.ContactCount),
                };
                var worst = ratings.Max();
                Views.EditorGUIUtility.PerformanceRatingPanel(worst, $"Avatar Dynamics: {worst}", worst == PerformanceRating.VeryPoor ? "Can't upload for Quest" : null);
            }
        }

        private void GUIRatingPanel(Models.VRChat.AvatarDynamics.PerformanceStats stats, AvatarPerformanceCategory category, I18nBase i18n)
        {
            var rating = Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, category);
            var categoryName = string.Empty;
            var veryPoorViolation = string.Empty;
            var value = 0;
            var maximum = 0;
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
            Views.EditorGUIUtility.PerformanceRatingPanel(rating, label, rating == PerformanceRating.VeryPoor ? veryPoorViolation : null);
        }

        private void ToonLitSettingGUI(ToonLitConvertSetting setting)
        {
            setting.generateQuestTextures = EditorGUILayout.Toggle(i18n.GenerateQuestTexturesLabel, setting.generateQuestTextures);
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
