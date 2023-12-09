using System.IO;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
using static KRT.VRCQuestTools.Models.VRChat.AvatarConverter;
using static KRT.VRCQuestTools.Utils.VRCSDKUtility.Reflection;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Inspector for <see cref="AvatarConverterSettings"/>.
    /// </summary>
    [CustomEditor(typeof(AvatarConverterSettings))]
    public class AvatarConverterSettingsEditor : Editor
    {
        [SerializeField]
        private bool foldOutEstimatedPerf = false;

        private AvatarConverterSettings converterSettings;
        private I18nBase i18n;
        private AvatarPerformanceStatsLevelSet statsLevelSet;
        private PerformanceRating avatarDynamicsPerfLimit;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            i18n = VRCQuestToolsSettings.I18nResource;

            var so = new SerializedObject(target);
            so.Update();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(i18n.ExitPlayModeToEdit, MessageType.Warning);
            }

            using (var disabledInPlayMode = new EditorGUI.DisabledGroupScope(EditorApplication.isPlaying))
            {
                bool canConvert = true;

                converterSettings = (AvatarConverterSettings)target;
                var descriptor = converterSettings.AvatarDescriptor;
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
                                OnClickConvertToPhysBonesButton(descriptor);
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
                                OnClickAssignNetIdsButton(descriptor);
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
                        Views.EditorGUIUtility.HelpBoxGUI(MessageType.Warning, () =>
                        {
                            var style = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
                            style.normal.textColor = EditorStyles.helpBox.normal.textColor;
                            EditorGUILayout.LabelField(i18n.AlertForMultiplePhysBones, style);
                            using (var disabled = new EditorGUI.DisabledGroupScope(true))
                            {
                                foreach (var obj in multiPbObjs)
                                {
                                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                                }
                            }
                            EditorGUILayout.Space(2);
                        });
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(i18n.AvatarConverterMustBeOnAvatarRoot, MessageType.Error);
                    canConvert = false;
                }

                using (var disabled = new EditorGUI.DisabledGroupScope(true))
                {
                    var path = GetOutputPath(descriptor);
                    if (Directory.Exists(path))
                    {
                        EditorGUILayout.ObjectField(i18n.SaveToLabel, AssetDatabase.LoadAssetAtPath<DefaultAsset>(path), typeof(DefaultAsset), false);
                    }
                    else
                    {
                        EditorGUILayout.TextField(i18n.SaveToLabel, path);
                    }
                    EditorGUILayout.PropertyField(so.FindProperty("destinationAvatar"), new GUIContent(i18n.ConvertedAvatarLabel));
                }
                EditorGUILayout.PropertyField(so.FindProperty("overwriteDestinationAvatar"), new GUIContent(i18n.OverwriteDestinationAvatarLabel, i18n.OverwriteDestinationAvatarTooltip));

                EditorGUILayout.Space();

                EditorGUILayout.LabelField(i18n.AvatarConverterMaterialConvertSettingLabel, EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(so.FindProperty("defaultMaterialConvertSetting"), new GUIContent(i18n.AvatarConverterDefaultMaterialConvertSettingLabel));

                var additionalMaterialConvertSettings = so.FindProperty("additionalMaterialConvertSettings");
                var additionalMaterialConvertCount = additionalMaterialConvertSettings.arraySize;
                EditorGUILayout.PropertyField(additionalMaterialConvertSettings, new GUIContent(i18n.AvatarConverterAdditionalMaterialConvertSettingsLabel));
                if (additionalMaterialConvertSettings.arraySize > additionalMaterialConvertCount)
                {
                    for (var i = additionalMaterialConvertCount; i < additionalMaterialConvertSettings.arraySize; i++)
                    {
                        var e = additionalMaterialConvertSettings.GetArrayElementAtIndex(i);
                        e.FindPropertyRelative("targetMaterial").objectReferenceValue = null;
                        e.FindPropertyRelative("materialConvertSettings").managedReferenceValue = new ToonLitConvertSettings();
                    }
                }

                if (GUILayout.Button(i18n.UpdateTexturesLabel))
                {
                    OnClickRegenerateTexturesButton(descriptor, converterSettings.defaultMaterialConvertSetting);
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField(i18n.AvatarConverterAvatarDynamicsSettingLabel, EditorStyles.boldLabel);
                if (GUILayout.Button(i18n.AvatarConverterAvatarDynamicsSettingLabel))
                {
                    OnClickSelectAvatarDynamicsComponentsButton(descriptor);
                }

                var m_physBones = so.FindProperty("physBonesToKeep");
                EditorGUILayout.PropertyField(m_physBones, new GUIContent("PhysBones", i18n.AvatarConverterPhysBonesTooltip));
                var m_physBoneColliders = so.FindProperty("physBoneCollidersToKeep");
                EditorGUILayout.PropertyField(m_physBoneColliders, new GUIContent("PhysBone Colliders", i18n.AvatarConverterPhysBoneCollidersTooltip));
                var m_contacts = so.FindProperty("contactsToKeep");
                EditorGUILayout.PropertyField(m_contacts, new GUIContent("Contact Senders & Receivers", i18n.AvatarConverterContactsTooltip));
                AvatarDynamicsPerformanceGUI(converterSettings);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField(i18n.AdvancedConverterSettingsLabel, EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(so.FindProperty("animatorOverrideControllers"), new GUIContent("Animator Override Controllers", i18n.AnimationOverrideTooltip));
                EditorGUILayout.PropertyField(so.FindProperty("removeVertexColor"), new GUIContent(i18n.RemoveVertexColorLabel, i18n.RemoveVertexColorTooltip));

                EditorGUILayout.Space();

                if (descriptor)
                {
                    var componentsToBeAlearted = VRCQuestTools.ComponentRemover.GetUnsupportedComponentsInChildren(descriptor.gameObject, true);
                    if (componentsToBeAlearted.Count() > 0)
                    {
                        Views.EditorGUIUtility.HelpBoxGUI(MessageType.Warning, () =>
                        {
                            EditorGUILayout.LabelField(i18n.AlertForComponents, EditorStyles.wordWrappedMiniLabel);
                            using (var disabled = new EditorGUI.DisabledGroupScope(true))
                            {
                                foreach (var c in componentsToBeAlearted)
                                {
                                    EditorGUILayout.ObjectField(c, typeof(Component), true);
                                }
                            }
                            EditorGUILayout.Space(2);
                        });
                    }
                }

                using (var disabled = new EditorGUI.DisabledGroupScope(!canConvert))
                {
                    if (GUILayout.Button(i18n.ConvertButtonLabel))
                    {
                        OnClickConvertButton(descriptor);
                    }
                }
            }

            so.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            statsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            avatarDynamicsPerfLimit = PerformanceRating.Poor;
        }

        private void AvatarDynamicsPerformanceGUI(AvatarConverterSettings converterSettings)
        {
            var original = converterSettings.AvatarDescriptor;
            if (original == null)
            {
                return;
            }
            var pbToKeep = converterSettings.physBonesToKeep.Where(x => x != null).Select(pb => new PhysBone(pb)).ToArray();
            var pbcToKeep = converterSettings.physBoneCollidersToKeep.Where(x => x != null).Select(pbc => new PhysBoneCollider(pbc)).ToArray();
            var contactsToKeep = converterSettings.contactsToKeep.Where(x => x != null).Select(c => new ContactBase(c)).ToArray();
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
                    Views.EditorGUIUtility.PerformanceRatingPanel(stats, statsLevelSet, category, i18n);
                }
            }
        }

        private void OnClickConvertToPhysBonesButton(VRC_AvatarDescriptor avatar)
        {
            Selection.activeGameObject = avatar.gameObject;
            EditorApplication.ExecuteMenuItem("VRChat SDK/Utilities/Convert DynamicBones To PhysBones");
        }

        private void OnClickAssignNetIdsButton(VRC_AvatarDescriptor avatar)
        {
            VRCSDKUtility.AssignNetworkIdsToPhysBones(avatar);
        }

        private void OnClickRegenerateTexturesButton(VRC_AvatarDescriptor avatar, IMaterialConvertSettings convertSetting)
        {
            var toonLitSetting = convertSetting is ToonLitConvertSettings ? convertSetting as ToonLitConvertSettings : null;
            var targetAvatar = new Models.VRChat.VRChatAvatar(avatar);

            TextureProgressCallback progressCallback = (int total, int index, System.Exception exception, Material material) =>
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                if (exception != null)
                {
                    var message = $"{i18n.MaterialExceptionDialogMessage}\n" +
                        "\n" +
                        $"Material: {AssetDatabase.GetAssetPath(material)}\n" +
                        $"Shader: {material.shader.name}";
                    DisplayErrorDialog(message, exception);
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    var progress = (float)index / total;
                    EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"{i18n.GeneratingTexturesDialogMessage} : {index + 1}/{total}", progress);
                }
            };
            var outputPath = GetOutputPath(avatar);
            Directory.CreateDirectory(outputPath);
            var callback = new ProgressCallback()
            {
                onTextureProgress = progressCallback,
            };
            VRCQuestTools.AvatarConverter.GenerateAndroidTextures(targetAvatar.Materials, outputPath, converterSettings, progressCallback);
            EditorUtility.ClearProgressBar();
        }

        private void OnClickSelectAvatarDynamicsComponentsButton(VRC_AvatarDescriptor avatar)
        {
            var window = EditorWindow.GetWindow<AvatarDynamicsSelectorWindow>();
            window.converterSettings = converterSettings;
            window.physBonesToKeep = converterSettings.physBonesToKeep;
            window.physBoneCollidersToKeep = converterSettings.physBoneCollidersToKeep;
            window.contactsToKeep = converterSettings.contactsToKeep;
            window.Show();
        }

        private void OnClickConvertButton(VRC_AvatarDescriptor avatar)
        {
            var progressCallback = new ProgressCallback
            {
                onTextureProgress = (total, index, exception, material) =>
                {
                    var i18n = VRCQuestToolsSettings.I18nResource;
                    if (exception != null)
                    {
                        var message = $"{i18n.MaterialExceptionDialogMessage}\n" +
                            "\n" +
                            $"Material: {AssetDatabase.GetAssetPath(material)}\n" +
                            $"Shader: {material.shader.name}";
                        DisplayErrorDialog(message, exception);
                        EditorUtility.ClearProgressBar();
                    }
                    else
                    {
                        var progress = (float)index / total;
                        EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"{i18n.GeneratingTexturesDialogMessage} : {index + 1}/{total}", progress);
                    }
                },
                onAnimationClipProgress = (total, index, exception, clip) =>
                {
                    var i18n = VRCQuestToolsSettings.I18nResource;
                    if (exception != null)
                    {
                        var message = $"{i18n.AnimationClipExceptionDialogMessage}\n" +
                            $"\n" +
                            $"AnimationClip: {clip.name}";
                        DisplayErrorDialog(message, exception);
                        EditorUtility.ClearProgressBar();
                    }
                    else
                    {
                        var progress = (float)index / total;
                        EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"Converting AnimationCilps : {index}/{total}", progress);
                    }
                },
                onRuntimeAnimatorProgress = (total, index, exception, controller) =>
                {
                    var i18n = VRCQuestToolsSettings.I18nResource;
                    if (exception != null)
                    {
                        var message = $"{i18n.AnimatorControllerExceptionDialogMessage}\n" +
                            $"\n" +
                            $"AnimatorController: {controller.name}";
                        DisplayErrorDialog(message, exception);
                        EditorUtility.ClearProgressBar();
                    }
                    else
                    {
                        var progress = (float)index / total;
                        EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"Converting AnimatorControllers : {index + 1}/{total}", progress);
                    }
                },
            };

            var questAvatar = VRCQuestTools.AvatarConverter.ConvertForQuest(converterSettings, GetOutputPath(avatar), VRCQuestTools.ComponentRemover, progressCallback);
            EditorUtility.ClearProgressBar();
            if (questAvatar != null)
            {
                Selection.activeGameObject = questAvatar.GameObject;

                var converted = questAvatar;
                var stats = VRCSDKUtility.CalculatePerformanceStats(converted.GameObject, true);
                var ratings = new PerformanceRating[]
                {
                    stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneComponentCount),
                    stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneTransformCount),
                    stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneColliderCount),
                    stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneCollisionCheckCount),
                    stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ContactCount),
                };

                if (ratings.Contains(PerformanceRating.VeryPoor))
                {
                    EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.AlertForAvatarDynamicsPerformance, "OK");
                    PhysBonesRemoveWindow.ShowWindow(converted.AvatarDescriptor);
                }

                // Overwrite existing converted avatar.
                if (converterSettings.destinationAvatar != null && converterSettings.overwriteDestinationAvatar)
                {
                    var destinationIndex = converterSettings.destinationAvatar.transform.GetSiblingIndex();
                    converted.GameObject.transform.SetParent(converterSettings.destinationAvatar.transform.parent);
                    converted.GameObject.transform.SetSiblingIndex(destinationIndex);
                    DestroyImmediate(converterSettings.destinationAvatar.gameObject);
                }
                converterSettings.destinationAvatar = converted.AvatarDescriptor;

                converterSettings.AvatarDescriptor.gameObject.SetActive(false);
            }
        }

        private string GetOutputPath(VRC_AvatarDescriptor avatar)
        {
            var outputPath = $"Assets/VRCQuestToolsOutput/{avatar.name}-{GlobalObjectId.GetGlobalObjectIdSlow(avatar).targetObjectId}";
            return outputPath;
        }

        private bool DisplayErrorDialog(string message, System.Exception exception)
        {
            var m = $"{message}\n" +
                "\n" +
                $"{exception.GetType().Name}: {exception.Message}\n" +
                exception.StackTrace.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, string.Empty);
            return EditorUtility.DisplayDialog(VRCQuestTools.Name, m, "OK");
        }
    }
}
