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
using UnityEditor.SceneManagement;
#if !UNITY_2021_2_OR_NEWER
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
            var i18n = VRCQuestToolsSettings.I18nResource;
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
#if VQT_HAS_MA_CONVERT_CONSTRAINTS
                        if (avatar.GameObject.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>() == null)
                        {
                            using (var horizontal = new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.HelpBox(i18n.AlertForMAConvertConstraints, MessageType.Warning);
                                if (GUILayout.Button(i18n.AddLabel, GUILayout.Height(38), GUILayout.Width(60)))
                                {
                                    OnClickAddConvertConstraintsButton(descriptor);
                                }
                            }
                        }
#else
                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.HelpBox(i18n.AlertForUnityConstraintsConversion, MessageType.Warning);
                        }
#endif
                    }
#endif

                    if (VRCSDKUtility.HasMissingNetworkIds(avatar.AvatarDescriptor) && avatar.GameObject.GetComponent<NetworkIDAssigner>() == null)
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
                    editorState.foldOutAdditionalMaterialSettings = MaterialConversionGUI.Draw(so, editorState.foldOutAdditionalMaterialSettings, additionalMaterialConvertSettingsReorderableList);

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
                    EditorGUILayout.PropertyField(so.FindProperty("removeAvatarDynamics"), new GUIContent(i18n.AvatarConverterRemoveAvatarDynamicsLabel, i18n.AvatarConverterRemoveAvatarDynamicsTooltip));

                    if (converterSettings.removeAvatarDynamics)
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
                    }

                    EditorGUILayout.Space(12);
                }

                editorState.foldOutAdvancedSettings = Views.EditorGUIUtility.Foldout(i18n.AdvancedConverterSettingsLabel, editorState.foldOutAdvancedSettings);
                if (editorState.foldOutAdvancedSettings)
                {
                    EditorGUILayout.PropertyField(so.FindProperty("animatorOverrideControllers"), new GUIContent(i18n.AnimationOverrideLabel, i18n.AnimationOverrideTooltip));
                    EditorGUILayout.PropertyField(so.FindProperty("removeVertexColor"), new GUIContent(i18n.RemoveVertexColorLabel, i18n.RemoveVertexColorTooltip));
                    EditorGUILayout.PropertyField(so.FindProperty("removeExtraMaterialSlots"), new GUIContent(i18n.RemoveExtraMaterialSlotsLabel, i18n.RemoveExtraMaterialSlotsTooltip));
                    EditorGUILayout.PropertyField(so.FindProperty("compressExpressionsMenuIcons"), new GUIContent("[NDMF] " + i18n.CompressExpressionsMenuIconsLabel, i18n.CompressExpressionsMenuIconsTooltip));
                    EditorGUILayout.PropertyField(so.FindProperty("ndmfPhase"), new GUIContent("[NDMF] " + i18n.NdmfPhaseLabel, i18n.NdmfPhaseTooltip));
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
#if VQT_HAS_MA_CONVERT_CONSTRAINTS
                    if (descriptor.gameObject.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>() != null)
                    {
                        componentsToBeAlearted = componentsToBeAlearted.Where(c => !(c is UnityEngine.Animations.IConstraint)).ToArray();
                    }
#endif
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

                Views.EditorGUIUtility.HorizontalDivider(2);

                if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    canConvert = false;
                    Views.EditorGUIUtility.HelpBoxGUI(MessageType.Error, () =>
                    {
                        EditorGUILayout.LabelField(i18n.ErrorForPrefabStage, EditorStyles.wordWrappedMiniLabel);
                        if (GUILayout.Button(i18n.ExitPrefabStageButtonLabel))
                        {
                            OnClickPrefabStageExitButton();
                        }
                        EditorGUILayout.Space(2);
                    });
                }
                else
                {
#if VQT_HAS_VRCSDK_NO_PRECHECK
                    EditorGUILayout.HelpBox(i18n.InfoForNdmfConversion2, MessageType.Info);
#else
                    EditorGUILayout.HelpBox(i18n.InfoForNdmfConversion, MessageType.Info);
#endif
#if VQT_HAS_NDMF
                    if (GUILayout.Button(i18n.OpenAvatarBuilder, GUILayout.Height(38)))
                    {
                        var typeName = "KRT.VRCQuestTools.Ndmf.AvatarBuilderWindow";
                        var type = SystemUtility.GetTypeByName(typeName) ?? throw new System.InvalidProgramException($"Type not found: {typeName}");
                        EditorWindow.GetWindow(type).Show();
                    }
#endif

                    EditorGUILayout.Space();

#if VQT_HAS_NDMF
                    EditorGUILayout.HelpBox(i18n.ManualConversionWarning, MessageType.Warning);
                    using (var disabled = new EditorGUI.DisabledGroupScope(!canConvert))
                    {
                        if (GUILayout.Button(i18n.ManualConvertButtonLabel))
                        {
                            OnClickConvertButton(descriptor);
                        }
                    }
#else
                    using (var disabled = new EditorGUI.DisabledGroupScope(!canConvert))
                    {
                        if (GUILayout.Button(i18n.ConvertButtonLabel))
                        {
                            OnClickConvertButton(descriptor);
                        }
                    }
#endif
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
            var i18n = VRCQuestToolsSettings.I18nResource;
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

#if VQT_HAS_MA_CONVERT_CONSTRAINTS
        private void OnClickAddConvertConstraintsButton(VRC_AvatarDescriptor avatar)
        {
            Undo.AddComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>(avatar.gameObject);
        }
#endif

        private void OnClickAssignNetIdsButton(VRC_AvatarDescriptor avatar)
        {
            VRCSDKUtility.AssignNetworkIdsToPhysBones(avatar);
        }

        private void OnClickAttachNetworkIDAssignerButton(VRC_AvatarDescriptor descriptor)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            Undo.AddComponent<NetworkIDAssigner>(descriptor.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(descriptor.gameObject);
            EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.NetworkIdAssignerAttached, "OK");
        }

        private void OnClickRegenerateTexturesButton(VRC_AvatarDescriptor avatar, IMaterialConvertSettings convertSetting)
        {
            var toonLitSetting = convertSetting is ToonLitConvertSettings ? convertSetting as ToonLitConvertSettings : null;
            var targetAvatar = new Models.VRChat.VRChatAvatar(avatar);

            TextureProgressCallback progressCallback = (int total, int index, Material material, Material _) =>
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                var progress = (float)index / total;
                EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"{i18n.GeneratingTexturesDialogMessage} : {index + 1}/{total}", progress);
            };
            var outputPath = GetOutputPath(avatar);
            Directory.CreateDirectory(outputPath);
            var callback = new ProgressCallback()
            {
                onTextureProgress = progressCallback,
            };
            try
            {
                VRCQuestTools.AvatarConverter.GenerateAndroidTextures(targetAvatar.Materials, true, outputPath, converterSettings, progressCallback);
            }
            catch (System.Exception exception)
            {
                HandleConversionException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
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

        private void OnClickPrefabStageExitButton()
        {
            StageUtility.GoToMainStage();
        }

        private void OnClickConvertButton(VRC_AvatarDescriptor avatar)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
#if VQT_HAS_MA_CONVERT_CONSTRAINTS
    #if VQT_HAS_VRCSDK_NO_PRECHECK
            if (new VRChatAvatar(avatar).HasUnityConstraints && avatar.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>() == null)
            {
                if (EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.ConfirmationForMAConvertConstraints, i18n.YesLabel, i18n.NoLabel))
                {
                    Undo.AddComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>(avatar.gameObject);
                }
            }
    #else
            if (new VRChatAvatar(avatar).HasUnityConstraints)
            {
                if (!EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.ConfirmationForUnityConstraints, i18n.YesLabel, i18n.NoLabel))
                {
                    return;
                }
            }
    #endif
#endif

            var progressCallback = new ProgressCallback
            {
                onTextureProgress = (total, index, material, _) =>
                {
                    var progress = (float)index / total;
                    EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"{i18n.GeneratingTexturesDialogMessage} : {index + 1}/{total}", progress);
                },
                onAnimationClipProgress = (total, index, clip, _) =>
                {
                    var progress = (float)index / total;
                    EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"Converting AnimationCilps : {index}/{total}", progress);
                },
                onRuntimeAnimatorProgress = (total, index, controller, _) =>
                {
                    var progress = (float)index / total;
                    EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"Converting AnimatorControllers : {index + 1}/{total}", progress);
                },
            };

            VRChatAvatar questAvatar;
            var sw = new System.Diagnostics.Stopwatch();
            try
            {
                sw.Start();
                questAvatar = VRCQuestTools.AvatarConverter.ConvertForQuest(converterSettings, VRCQuestTools.ComponentRemover, true, GetOutputPath(avatar), progressCallback);
                sw.Stop();
                Debug.Log($"[{VRCQuestTools.Name}] Converted avatar for Android in {sw.ElapsedMilliseconds}ms");
            }
            catch (System.Exception exception)
            {
                HandleConversionException(exception);
                return;
            }
            finally
            {
                sw.Stop();
                EditorUtility.ClearProgressBar();
            }
            if (questAvatar != null)
            {
                Selection.activeGameObject = questAvatar.GameObject;

                if (TargetComponent.removeAvatarDynamics)
                {
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
                }

                converterSettings.AvatarDescriptor.gameObject.SetActive(false);
            }
        }

        private void HandleConversionException(System.Exception exception)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var message = i18n.AvatarConverterFailedDialogMessage;
            var dialogException = exception;
            Object context = null;
            switch (exception)
            {
                case MaterialConversionException e:
                    message = $"{i18n.MaterialExceptionDialogMessage}\n" +
                        "\n" +
                        $"Material: {AssetDatabase.GetAssetPath(e.source)}\n" +
                        $"Shader: {e.source.shader.name}";
                    dialogException = e.InnerException;
                    context = e.source;
                    break;
                case AnimationClipConversionException e:
                    message = $"{i18n.AnimationClipExceptionDialogMessage}\n" +
                        $"\n" +
                        $"AnimationClip: {e.source.name}";
                    dialogException = e.InnerException;
                    context = e.source;
                    break;
                case AnimatorControllerConversionException e:
                    message = $"{i18n.AnimatorControllerExceptionDialogMessage}\n" +
                        $"\n" +
                        $"AnimatorController: {e.source.name}";
                    dialogException = e.InnerException;
                    context = e.source;
                    break;
                case InvalidReplacementMaterialException e:
                    message = $"{i18n.InvalidReplacementMaterialExceptionDialogMessage}\n" +
                        $"\n" +
                        $"Material: {e.replacementMaterial.name}\n" +
                        $"Shader: {e.replacementMaterial.shader.name}";
                    dialogException = e;
                    context = e.component;
                    break;
            }
            if (exception.InnerException != null)
            {
                Debug.LogException(exception.InnerException, context);
            }
            Debug.LogException(exception, context);
            DisplayErrorDialog(message, dialogException);
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
                "\n" +
                exception.StackTrace.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, string.Empty);
            return EditorUtility.DisplayDialog(VRCQuestTools.Name, m, "OK");
        }
    }
}
