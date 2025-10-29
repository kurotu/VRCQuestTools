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
using UnityEditorInternal;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
using static KRT.VRCQuestTools.Models.VRChat.AvatarConverter;

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
        private GUIStyle foldoutContentStyle;

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

                    if (avatar.HasUnityConstraints)
                    {
                        if (ModularAvatarUtility.IsModularAvatarImported())
                        {
                            if (!ModularAvatarUtility.HasConvertConstraintsComponent(avatar.GameObject))
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
                        }
                        else
                        {
                            using (var horizontal = new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.HelpBox(i18n.AlertForUnityConstraintsConversion, MessageType.Warning);
                            }
                        }
                    }

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

                    var pbs = descriptor.GetComponentsInChildren<VRCPhysBone>(true);
                    var multiPbObjs = pbs
                        .Select(pb => pb.gameObject)
                        .Where(go => go.GetComponents<VRCPhysBone>().Count() >= 2)
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
                    additionalMaterialConvertSettingsReorderableList ??= MaterialConversionGUI.CreateAdditionalMaterialConvertSettingsList(so, so.FindProperty(nameof(AvatarConverterSettings.additionalMaterialConvertSettings)));
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

                        // Initialize foldout content style if needed
                        if (foldoutContentStyle == null)
                        {
                            foldoutContentStyle = new GUIStyle()
                            {
                                padding = new RectOffset(16, 0, 0, 0),
                            };
                        }

                        var avatar = new VRChatAvatar(descriptor);

                        // PhysBones Section
                        if (editorState.foldOutPhysBones = EditorGUILayout.BeginFoldoutHeaderGroup(editorState.foldOutPhysBones, new GUIContent(i18n.AvatarConverterPhysBonesHeader, i18n.AvatarConverterPhysBonesTooltip)))
                        {
                            using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                            {
                                var allPhysBones = avatar.GetPhysBoneProviders();
                                if (allPhysBones.Length > 0)
                                {
                                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button(new GUIContent(i18n.SelectAllButtonLabel, i18n.SelectAllPhysBonesButtonTooltip)))
                                        {
                                            Undo.RecordObject(converterSettings, "Select All PhysBones");
                                            converterSettings.physBonesToKeep = allPhysBones.SelectMany(p => p.GetPhysBones()).ToArray();
                                            EditorUtility.SetDirty(converterSettings);
                                        }
                                        if (GUILayout.Button(new GUIContent(i18n.DeselectAllButtonLabel, i18n.DeselectAllPhysBonesButtonTooltip)))
                                        {
                                            Undo.RecordObject(converterSettings, "Deselect All PhysBones");
                                            converterSettings.physBonesToKeep = new VRCPhysBone[] { };
                                            EditorUtility.SetDirty(converterSettings);
                                        }
                                    }
                                    var selectedProviders = converterSettings.physBonesToKeep.Where(pb => pb != null).Select(pb => new VRCPhysBoneProvider(pb)).Cast<VRCPhysBoneProviderBase>().ToArray();
                                    var newSelectedProviders = Views.EditorGUIUtility.AvatarDynamicsComponentSelectorList(allPhysBones, selectedProviders);
                                    if (!selectedProviders.SequenceEqual(newSelectedProviders))
                                    {
                                        Undo.RecordObject(converterSettings, "Update PhysBones Selection");
                                        converterSettings.physBonesToKeep = newSelectedProviders.SelectMany(p => p.GetPhysBones()).ToArray();
                                        EditorUtility.SetDirty(converterSettings);
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(i18n.NoPhysBonesFound);
                                }
                                EditorGUILayout.Space();
                            }
                        }
                        EditorGUILayout.EndFoldoutHeaderGroup();

                        // PhysBone Colliders Section
                        if (editorState.foldOutPhysBoneColliders = EditorGUILayout.BeginFoldoutHeaderGroup(editorState.foldOutPhysBoneColliders, new GUIContent(i18n.AvatarConverterPhysBoneCollidersHeader, i18n.AvatarConverterPhysBoneCollidersTooltip)))
                        {
                            using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                            {
                                var allColliders = avatar.GetPhysBoneColliders();
                                if (allColliders.Length > 0)
                                {
                                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button(new GUIContent(i18n.SelectAllButtonLabel, i18n.SelectAllPhysBoneCollidersButtonTooltip)))
                                        {
                                            Undo.RecordObject(converterSettings, "Select All PhysBone Colliders");
                                            converterSettings.physBoneCollidersToKeep = allColliders.ToArray();
                                            EditorUtility.SetDirty(converterSettings);
                                        }
                                        if (GUILayout.Button(new GUIContent(i18n.DeselectAllButtonLabel, i18n.DeselectAllPhysBoneCollidersButtonTooltip)))
                                        {
                                            Undo.RecordObject(converterSettings, "Deselect All PhysBone Colliders");
                                            converterSettings.physBoneCollidersToKeep = new VRCPhysBoneCollider[] { };
                                            EditorUtility.SetDirty(converterSettings);
                                        }
                                    }
                                    var allColliderProviders = allColliders.Select(c => new VRCPhysBoneColliderProvider(c)).ToArray();
                                    var selectedColliderProviders = converterSettings.physBoneCollidersToKeep.Where(c => c != null).Select(c => new VRCPhysBoneColliderProvider(c)).ToArray();
                                    var newSelectedColliderProviders = Views.EditorGUIUtility.AvatarDynamicsComponentSelectorList(allColliderProviders, selectedColliderProviders);
                                    if (!selectedColliderProviders.SequenceEqual(newSelectedColliderProviders))
                                    {
                                        Undo.RecordObject(converterSettings, "Update PhysBone Colliders Selection");
                                        converterSettings.physBoneCollidersToKeep = newSelectedColliderProviders.Select(p => p.Component).Cast<VRCPhysBoneCollider>().ToArray();
                                        EditorUtility.SetDirty(converterSettings);
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(i18n.NoPhysBoneCollidersFound);
                                }
                                EditorGUILayout.Space();
                            }
                        }
                        EditorGUILayout.EndFoldoutHeaderGroup();

                        // Contacts Section
                        if (editorState.foldOutContacts = EditorGUILayout.BeginFoldoutHeaderGroup(editorState.foldOutContacts, new GUIContent(i18n.AvatarConverterContactsHeader, i18n.AvatarConverterContactsTooltip)))
                        {
                            using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                            {
                                var allContacts = avatar.GetNonLocalContacts();
                                if (allContacts.Length > 0)
                                {
                                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button(new GUIContent(i18n.SelectAllButtonLabel, i18n.SelectAllContactsButtonTooltip)))
                                        {
                                            Undo.RecordObject(converterSettings, "Select All Contacts");
                                            converterSettings.contactsToKeep = allContacts.ToArray();
                                            EditorUtility.SetDirty(converterSettings);
                                        }
                                        if (GUILayout.Button(new GUIContent(i18n.DeselectAllButtonLabel, i18n.DeselectAllContactsButtonTooltip)))
                                        {
                                            Undo.RecordObject(converterSettings, "Deselect All Contacts");
                                            converterSettings.contactsToKeep = new VRC.Dynamics.ContactBase[] { };
                                            EditorUtility.SetDirty(converterSettings);
                                        }
                                    }
                                    var allContactProviders = allContacts.Select(c => new VRCContactBaseProvider(c)).ToArray();
                                    var selectedContactProviders = converterSettings.contactsToKeep.Where(c => c != null).Select(c => new VRCContactBaseProvider(c)).ToArray();
                                    var newSelectedContactProviders = Views.EditorGUIUtility.AvatarDynamicsComponentSelectorList(allContactProviders, selectedContactProviders);
                                    if (!selectedContactProviders.SequenceEqual(newSelectedContactProviders))
                                    {
                                        Undo.RecordObject(converterSettings, "Update Contacts Selection");
                                        converterSettings.contactsToKeep = newSelectedContactProviders.Select(p => p.Component).Cast<VRC.Dynamics.ContactBase>().ToArray();
                                        EditorUtility.SetDirty(converterSettings);
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(i18n.NoContactsFound);
                                }
                                EditorGUILayout.Space();
                            }
                        }
                        EditorGUILayout.EndFoldoutHeaderGroup();

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
                    if (ModularAvatarUtility.IsModularAvatarImported() && ModularAvatarUtility.HasConvertConstraintsComponent(descriptor.gameObject))
                    {
                        componentsToBeAlearted = componentsToBeAlearted.Where(c => !(c is UnityEngine.Animations.IConstraint)).ToArray();
                    }
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
                    EditorGUILayout.HelpBox(i18n.InfoForNdmfConversion2, MessageType.Info);
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
            var pbToKeep = converterSettings.physBonesToKeep.Where(x => x != null).ToArray();
            var pbcToKeep = converterSettings.physBoneCollidersToKeep.Where(x => x != null).ToArray();
            var contactsToKeep = converterSettings.contactsToKeep.Where(x => x != null).ToArray();
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

        private void OnClickAddConvertConstraintsButton(VRC_AvatarDescriptor avatar)
        {
            ModularAvatarUtility.AddConvertConstraintsComponent(avatar.gameObject);
        }

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
            window.physBoneProvidersToKeep = converterSettings.physBonesToKeep.Select(p => new VRCPhysBoneProvider(p)).ToArray();
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
            if (ModularAvatarUtility.IsModularAvatarImported() && new VRChatAvatar(avatar).HasUnityConstraints && !ModularAvatarUtility.HasConvertConstraintsComponent(avatar.gameObject))
            {
                if (EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.ConfirmationForMAConvertConstraints, i18n.YesLabel, i18n.NoLabel))
                {
                    ModularAvatarUtility.AddConvertConstraintsComponent(avatar.gameObject);
                }
            }

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
                Logger.Log($"Converted avatar for Android in {sw.ElapsedMilliseconds}ms");
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
                case PackageCompatibilityException e:
                    message = e.LocalizedMessage;
                    dialogException = e;
                    break;
                case MaterialConversionException e:
                    if (e.InnerException is PackageCompatibilityException packageException)
                    {
                        message = packageException.LocalizedMessage;
                    }
                    else
                    {
                        message = $"{i18n.MaterialExceptionDialogMessage}\n" +
                            "\n" +
                            $"Material: {AssetDatabase.GetAssetPath(e.SourceObject)}\n" +
                            $"Shader: {e.SourceObject.shader.name}";
                    }
                    dialogException = e.InnerException;
                    context = e.SourceObject;
                    break;
                case AnimationClipConversionException e:
                    message = $"{i18n.AnimationClipExceptionDialogMessage}\n" +
                        $"\n" +
                        $"AnimationClip: {e.SourceObject.name}";
                    dialogException = e.InnerException;
                    context = e.SourceObject;
                    break;
                case AnimatorControllerConversionException e:
                    message = $"{i18n.AnimatorControllerExceptionDialogMessage}\n" +
                        $"\n" +
                        $"AnimatorController: {e.SourceObject.name}";
                    dialogException = e.InnerException;
                    context = e.SourceObject;
                    break;
                case InvalidReplacementMaterialException e:
                    message = $"{i18n.InvalidReplacementMaterialExceptionDialogMessage}\n" +
                        $"\n" +
                        $"Material: {e.ReplacementMaterial.name}\n" +
                        $"Shader: {e.ReplacementMaterial.shader.name}";
                    dialogException = e;
                    context = e.Component;
                    break;
            }
            if (exception.InnerException != null)
            {
                Logger.LogException(exception.InnerException, context);
            }
            Logger.LogException(exception, context);
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
