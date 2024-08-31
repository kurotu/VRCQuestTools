using System.IO;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Views;
using UnityEditor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditorInternal;
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
    public class AvatarConverterSettingsEditor : VRCQuestToolsEditorOnlyEditorBase<AvatarConverterSettings>
    {
        private AvatarConverterSettings converterSettings;

        private I18nBase i18n;
        private AvatarConverterSettingsEditorState editorState;

        private ReorderableList additionalMaterialConvertSettingsReorderableList;
        private AvatarPerformanceStatsLevelSet statsLevelSet;
        private PerformanceRating avatarDynamicsPerfLimit;

        /// <inheritdoc/>
        protected override string Description
        {
            get
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
#if VQT_HAS_NDMF
                return i18n.AvatarConverterSettingsEditorDescriptionNDMF;
#else
                return i18n.AvatarConverterSettingsEditorDescription;
#endif
            }
        }

        /// <inheritdoc/>
        public override void OnInspectorGUIInternal()
        {
            i18n = VRCQuestToolsSettings.I18nResource;
            if (editorState == null)
            {
                editorState = AvatarConverterSettingsEditorState.instance;
            }

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
#if VQT_HAS_VRCSDK_CONSTRAINTS
                    if (avatar.HasUnityConstraints)
                    {
                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.HelpBox(i18n.AlertForUnityConstraintsConversion, MessageType.Warning);
                        }
                    }
#endif
                    if (VRCSDKUtility.HasMissingNetworkIds(avatar.AvatarDescriptor))
                    {
#if VQT_HAS_NDMF
                        if (avatar.GameObject.GetComponent<NetworkIDAssigner>() == null)
                        {
                            using (var horizontal = new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.HelpBox(i18n.InfoForNetworkIdAssigner, MessageType.Info);
                                if (GUILayout.Button(i18n.AttachButtonLabel, GUILayout.Height(38), GUILayout.Width(60)))
                                {
                                    OnClickAttachNetworkIDAssignerButton(descriptor);
                                }
                            }
                        }
#else
                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.HelpBox(i18n.AlertForMissingNetIds, MessageType.Warning);
                            if (GUILayout.Button(i18n.AssignButtonLabel, GUILayout.Height(38), GUILayout.Width(60)))
                            {
                                OnClickAssignNetIdsButton(descriptor);
                            }
                        }
#endif
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
                }

                EditorGUILayout.Space(12);

                editorState.foldOutMaterialSettings = Views.EditorGUIUtility.Foldout(i18n.AvatarConverterMaterialConvertSettingsLabel, editorState.foldOutMaterialSettings);
                if (editorState.foldOutMaterialSettings)
                {
                    var defaultMaterialConvertSettings = so.FindProperty("defaultMaterialConvertSettings");
                    EditorGUILayout.PropertyField(defaultMaterialConvertSettings, new GUIContent(i18n.AvatarConverterDefaultMaterialConvertSettingLabel));

                    var additionalMaterialConvertSettings = so.FindProperty("additionalMaterialConvertSettings");

                    var headerRect = new Rect(EditorGUILayout.GetControlRect());
                    using (var property = new EditorGUI.PropertyScope(headerRect, new GUIContent(i18n.AvatarConverterAdditionalMaterialConvertSettingsLabel), additionalMaterialConvertSettings))
                    {
                        editorState.foldOutAdditionalMaterialSettings = EditorGUI.Foldout(headerRect, editorState.foldOutAdditionalMaterialSettings, property.content, true);
                        if (editorState.foldOutAdditionalMaterialSettings)
                        {
                            if (additionalMaterialConvertSettingsReorderableList == null)
                            {
                                additionalMaterialConvertSettingsReorderableList = new ReorderableList(so, additionalMaterialConvertSettings, true, false, true, true);
                                additionalMaterialConvertSettingsReorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                                {
                                    EditorGUI.PropertyField(rect, additionalMaterialConvertSettings.GetArrayElementAtIndex(index));
                                    so.ApplyModifiedProperties();
                                };
                                additionalMaterialConvertSettingsReorderableList.elementHeightCallback = (index) =>
                                {
                                    var element = additionalMaterialConvertSettings.GetArrayElementAtIndex(index);
                                    return EditorGUI.GetPropertyHeight(element);
                                };
                                additionalMaterialConvertSettingsReorderableList.onAddCallback = (list) =>
                                {
                                    var index = list.serializedProperty.arraySize;
                                    list.serializedProperty.arraySize++;
                                    list.index = index;
                                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                                    element.managedReferenceValue = new AdditionalMaterialConvertSettings();
                                    so.ApplyModifiedProperties();
                                };
                                additionalMaterialConvertSettingsReorderableList.onRemoveCallback = (list) =>
                                {
                                    if (list.index < 0 || list.index >= list.serializedProperty.arraySize)
                                    {
                                        return;
                                    }
                                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                                    so.ApplyModifiedProperties();
                                };
                            }
                            additionalMaterialConvertSettingsReorderableList.DoLayoutList();
                        }
                    }

                    if (descriptor)
                    {
                        var targetAvatar = new VRChatAvatar(descriptor);
                        var unverifiedMaterials = targetAvatar.Materials
                            .Where(m => VRCQuestTools.AvatarConverter.MaterialWrapperBuilder.DetectShaderCategory(m) == MaterialWrapperBuilder.ShaderCategory.Unverified)
                            .ToArray();

                        if (unverifiedMaterials.Length > 0)
                        {
                            Views.EditorGUIUtility.HelpBoxGUI(MessageType.Warning, () =>
                            {
                                EditorGUILayout.LabelField(i18n.WarningForUnsupportedShaders, EditorStyles.wordWrappedMiniLabel);
                                EditorGUILayout.Space(1);
                                EditorGUILayout.LabelField($"{i18n.SupportedShadersLabel}: Standard, UTS2, arktoon, AXCS, Sunao, lilToon, Poiyomi", EditorStyles.wordWrappedMiniLabel);
                                EditorGUI.BeginDisabledGroup(true);
                                foreach (var m in unverifiedMaterials)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.ObjectField(m, typeof(Material), false);
                                    EditorGUILayout.ObjectField(m.shader, typeof(Shader), false);
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUI.EndDisabledGroup();
                            });
                        }
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button(i18n.UpdateTexturesLabel))
                    {
                        OnClickRegenerateTexturesButton(descriptor, converterSettings.defaultMaterialConvertSettings);
                    }
                    EditorGUILayout.Space(12);
                }

                var stats = EstimatePerformanceStats(converterSettings);

                editorState.foldOutAvatarDynamics = Views.EditorGUIUtility.Foldout(i18n.AvatarConverterAvatarDynamicsSettingsLabel, editorState.foldOutAvatarDynamics);
                if (editorState.foldOutAvatarDynamics)
                {
                    if (GUILayout.Button(i18n.AvatarConverterAvatarDynamicsSettingsLabel))
                    {
                        OnClickSelectAvatarDynamicsComponentsButton(descriptor);
                    }

                    var m_physBones = so.FindProperty("physBonesToKeep");
                    EditorGUILayout.PropertyField(m_physBones, new GUIContent("PhysBones to Keep", i18n.AvatarConverterPhysBonesTooltip));
                    var m_physBoneColliders = so.FindProperty("physBoneCollidersToKeep");
                    EditorGUILayout.PropertyField(m_physBoneColliders, new GUIContent("PhysBone Colliders to Keep", i18n.AvatarConverterPhysBoneCollidersTooltip));
                    var m_contacts = so.FindProperty("contactsToKeep");
                    EditorGUILayout.PropertyField(m_contacts, new GUIContent("Contact Senders & Receivers to Keep", i18n.AvatarConverterContactsTooltip));
                    AvatarDynamicsPerformanceGUI(stats);
                    EditorGUILayout.Space(12);
                }

                editorState.foldOutAdvancedSettings = Views.EditorGUIUtility.Foldout(i18n.AdvancedConverterSettingsLabel, editorState.foldOutAdvancedSettings);
                if (editorState.foldOutAdvancedSettings)
                {
                    EditorGUILayout.PropertyField(so.FindProperty("animatorOverrideControllers"), new GUIContent(i18n.AnimationOverrideLabel, i18n.AnimationOverrideTooltip));
                    EditorGUILayout.PropertyField(so.FindProperty("removeVertexColor"), new GUIContent(i18n.RemoveVertexColorLabel, i18n.RemoveVertexColorTooltip));
                }

                Views.EditorGUIUtility.HorizontalDivider(2);

                if (descriptor)
                {
                    if (GetAvatarDynamicsRating(stats) > avatarDynamicsPerfLimit)
                    {
                        Views.EditorGUIUtility.HelpBoxGUI(MessageType.Error, () =>
                        {
                            EditorGUILayout.LabelField(i18n.AlertForAvatarDynamicsPerformance, EditorStyles.wordWrappedMiniLabel);
                            if (GUILayout.Button(i18n.AvatarConverterAvatarDynamicsSettingsLabel))
                            {
                                OnClickSelectAvatarDynamicsComponentsButton(descriptor);
                            }
                            EditorGUILayout.Space(2);
                        });
                    }

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

                    var maComponentsToBeAlearted = ModularAvatarUtility.GetUnsupportedComponentsInChildren(descriptor.gameObject, true);
                    if (maComponentsToBeAlearted.Length > 0)
                    {
                        Views.EditorGUIUtility.HelpBoxGUI(MessageType.Warning, () =>
                        {
                            EditorGUILayout.LabelField(i18n.AlertForComponents, EditorStyles.wordWrappedMiniLabel);
                            using (var disabled = new EditorGUI.DisabledGroupScope(true))
                            {
                                foreach (var c in maComponentsToBeAlearted)
                                {
                                    EditorGUILayout.ObjectField(c, typeof(Component), true);
                                }
                            }
                            EditorGUILayout.Space(2);
                        });
                    }

                    if (stats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall) > PerformanceRating.Poor)
                    {
                        Views.EditorGUIUtility.HelpBoxGUI(MessageType.None, () =>
                        {
                            EditorGUILayout.LabelField(i18n.WarningForPerformance, EditorStyles.wordWrappedMiniLabel);
                            EditorGUILayout.Space(2);
                        });
                    }
                }

                Views.EditorGUIUtility.HelpBoxGUI(MessageType.None, () =>
                {
                    EditorGUILayout.LabelField(i18n.WarningForAppearance, EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.Space(2);
                });

                using (new EditorGUILayout.HorizontalScope())
                {
                    Views.EditorGUIUtility.HelpBoxGUI(MessageType.Info, () =>
                    {
                        EditorGUILayout.LabelField(i18n.InfoForNdmfConversion, EditorStyles.wordWrappedMiniLabel);
                        EditorGUILayout.Space(2);
                    });
#if VQT_HAS_NDMF
                    if (GUILayout.Button(i18n.OpenLabel, GUILayout.Height(38), GUILayout.Width(60)))
                    {
                        var typeName = "KRT.VRCQuestTools.Ndmf.AvatarBuilderWindow";
                        var type = SystemUtility.GetTypeByName(typeName) ?? throw new System.InvalidProgramException($"Type not found: {typeName}");
                        EditorWindow.GetWindow(type).Show();
                    }
#endif
                }

                if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    canConvert = false;
                    Views.EditorGUIUtility.HelpBoxGUI(MessageType.Error, () =>
                    {
                        EditorGUILayout.LabelField(i18n.ErrorForPrefabStage, EditorStyles.wordWrappedMiniLabel);
                    });
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

        private AvatarPerformanceStats EstimatePerformanceStats(AvatarConverterSettings converterSettings)
        {
            var original = converterSettings.AvatarDescriptor;
            if (original == null)
            {
                return null;
            }
            var pbToKeep = converterSettings.physBonesToKeep.Where(x => x != null).Select(pb => new PhysBone(pb)).ToArray();
            var pbcToKeep = converterSettings.physBoneCollidersToKeep.Where(x => x != null).Select(pbc => new PhysBoneCollider(pbc)).ToArray();
            var contactsToKeep = converterSettings.contactsToKeep.Where(x => x != null).Select(c => new ContactBase(c)).ToArray();
            var avatar = new VRChatAvatar(original);
            return avatar.EstimatePerformanceStats(pbToKeep, pbcToKeep, contactsToKeep);
        }

        private void AvatarDynamicsPerformanceGUI(AvatarPerformanceStats stats)
        {
            editorState.foldOutEstimatedPerf = EditorGUILayout.Foldout(editorState.foldOutEstimatedPerf, i18n.EstimatedPerformanceStats, true);
            var ratings = VRCSDKUtility.AvatarDynamicsPerformanceCategories.ToDictionary(x => x, stats.GetPerformanceRatingForCategory);
            var worst = ratings.Values.Max();
            if (editorState.foldOutEstimatedPerf || worst > avatarDynamicsPerfLimit)
            {
                Views.EditorGUIUtility.PerformanceRatingPanel(worst, $"Avatar Dynamics: {worst}", worst > avatarDynamicsPerfLimit ? i18n.AvatarDynamicsPreventsUpload : null);
            }
            foreach (var category in VRCSDKUtility.AvatarDynamicsPerformanceCategories)
            {
                if (editorState.foldOutEstimatedPerf || ratings[category] > avatarDynamicsPerfLimit)
                {
                    Views.EditorGUIUtility.PerformanceRatingPanel(stats, statsLevelSet, category, i18n);
                }
            }
        }

        private PerformanceRating GetAvatarDynamicsRating(AvatarPerformanceStats stats)
        {
            var ratings = VRCSDKUtility.AvatarDynamicsPerformanceCategories.Select(stats.GetPerformanceRatingForCategory);
            return ratings.Max();
        }

        private void OnClickConvertToPhysBonesButton(VRC_AvatarDescriptor avatar)
        {
            Selection.activeGameObject = avatar.gameObject;
            EditorApplication.ExecuteMenuItem("VRChat SDK/Utilities/Convert DynamicBones To PhysBones");
        }

        private void OnClickConvertToVRCConstraintsButton(VRC_AvatarDescriptor avatar)
        {
            Selection.activeGameObject = avatar.gameObject;
            EditorApplication.ExecuteMenuItem("VRChat SDK/Utilities/Convert Unity Constraints To VRChat Constraints");
        }

        private void OnClickAssignNetIdsButton(VRC_AvatarDescriptor avatar)
        {
            VRCSDKUtility.AssignNetworkIdsToPhysBones(avatar);
        }

        private void OnClickAttachNetworkIDAssignerButton(VRC_AvatarDescriptor descriptor)
        {
            descriptor.gameObject.AddComponent<NetworkIDAssigner>();
            PrefabUtility.RecordPrefabInstancePropertyModifications(descriptor.gameObject);
            EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.NetworkIdAssignerAttached, "OK");
        }

        private void OnClickRegenerateTexturesButton(VRC_AvatarDescriptor avatar, IMaterialConvertSettings convertSetting)
        {
            var toonLitSetting = convertSetting is ToonLitConvertSettings ? convertSetting as ToonLitConvertSettings : null;
            var targetAvatar = new Models.VRChat.VRChatAvatar(avatar);

            TextureProgressCallback progressCallback = (int total, int index, System.Exception exception, Material material, Material _) =>
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
            VRCQuestTools.AvatarConverter.GenerateAndroidTextures(targetAvatar.Materials, true, outputPath, converterSettings, progressCallback);
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
                onTextureProgress = (total, index, exception, material, _) =>
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
                onAnimationClipProgress = (total, index, exception, clip, _) =>
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
                onRuntimeAnimatorProgress = (total, index, exception, controller, _) =>
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

            var questAvatar = VRCQuestTools.AvatarConverter.ConvertForQuest(converterSettings, VRCQuestTools.ComponentRemover, true, GetOutputPath(avatar), progressCallback);
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

                converterSettings.AvatarDescriptor.gameObject.SetActive(false);
            }
        }

        private string GetOutputPath(VRC_AvatarDescriptor avatar)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var avatarName = new string(avatar.name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            var invalidLastChars = new char[] { '.', ' ' };
            avatarName = avatarName.TrimStart().TrimEnd(invalidLastChars);

            var outputPath = $"Assets/VRCQuestToolsOutput/{avatarName}";
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
