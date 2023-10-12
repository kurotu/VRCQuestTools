// <copyright file="AvatarConverterWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using UnityEditor;
using UnityEngine;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3 || VQT_HAS_VRCSDK_BASE
using VRC.SDKBase.Validation.Performance;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using AvatarPerformanceCategory = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceCategory;
using PerformanceRating = KRT.VRCQuestTools.Mocks.Mock_PerformanceRating;
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// EditorWindow for AvatarConverter.
    /// </summary>
    internal class AvatarConverterWindow : EditorWindow
    {
        [SerializeField]
        private AvatarConverterViewModel model = new AvatarConverterViewModel();
        private Vector2 scrollPosition;
        private AvatarConverter.ProgressCallback progressCallback;
        private string[] invalidCharStrings;

        /// <summary>
        /// Show a window.
        /// </summary>
        internal static void ShowWindow()
        {
            var window = (AvatarConverterWindow)GetWindow(typeof(AvatarConverterWindow));
            window.Show();
        }

        /// <summary>
        /// Show a window with a target PC avatar.
        /// </summary>
        /// <param name="avatar">Target PC avatar.</param>
        internal static void ShowWindow(VRC_AvatarDescriptor avatar)
        {
            var window = (AvatarConverterWindow)GetWindow(typeof(AvatarConverterWindow));
            window.model.TargetAvatarDescriptor = avatar;
            window.SetArtifactsPath(avatar);
            window.Show();
        }

        private void OnEnable()
        {
            titleContent.text = "Convert Avatar for Quest";
            progressCallback = new AvatarConverter.ProgressCallback
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
            model.AvatarConverter = VRCQuestTools.AvatarConverter;
            invalidCharStrings = Path.GetInvalidFileNameChars()
                        .Where(c => !char.IsControl(c))
                        .Select(c => new string(c, 1))
                        .ToArray();
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var selectedAvatar = (VRC_AvatarDescriptor)EditorGUILayout.ObjectField(i18n.AvatarLabel, model.TargetAvatarDescriptor, typeof(VRC_AvatarDescriptor), true);
            if (selectedAvatar == null)
            {
                model.outputPath = string.Empty;
            }
            else if (model.TargetAvatarDescriptor != selectedAvatar)
            {
                SetArtifactsPath(selectedAvatar);
            }
            model.TargetAvatarDescriptor = selectedAvatar;

            if (model.TargetAvatarDescriptor != null)
            {
                if (VRCSDKUtility.IsPhysBonesImported() && model.HasDynamicBones)
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.HelpBox(i18n.AlertForDynamicBoneConversion, MessageType.Warning);
                        if (GUILayout.Button(i18n.ConvertButtonLabel, GUILayout.Height(38), GUILayout.MinWidth(60)))
                        {
                            OnClickConvertToPhysBonesButton();
                        }
                    }
                }

#if VQT_VRCSDK_HAS_NETWORK_ID
                if (model.HasMissingNetIDs)
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.HelpBox(i18n.AlertForMissingNetIds, MessageType.Warning);
                        if (GUILayout.Button(i18n.AssignButtonLabel, GUILayout.Height(38), GUILayout.MinWidth(60)))
                        {
                            OnClickAssignNetIdsButton();
                        }
                    }
                }

                var gos = model.GameObjectsWithMultiplePhysBones;
                if (gos.Length > 0)
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        var message = $"{i18n.AlertForMultiplePhysBones}\n\n" +
                            $"{string.Join("\n", model.GameObjectsWithMultiplePhysBones.Select(x => $"  - {x.name}"))}";
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    }
                }
#endif
            }

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                model.generateQuestTextures = EditorGUILayout.BeginToggleGroup(i18n.GenerateQuestTexturesLabel, model.generateQuestTextures);
                var message = $"{i18n.QuestTexturesDescription}\n\n" +
                    $"{i18n.SupportedShadersLabel}: Standard, UTS2, arktoon, AXCS, Sunao, lilToon";
                EditorGUILayout.HelpBox(message, MessageType.Info);
                if (model.TargetAvatarDescriptor != null)
                {
                    var unverifiedMaterials = model.UnverifiedShaderMaterials;
                    if (model.generateQuestTextures && unverifiedMaterials.Length > 0)
                    {
                        var message2 = $"{i18n.WarningForUnsupportedShaders}\n\n" +
                            $"{string.Join("\n", unverifiedMaterials.Select(m => $"  - {m.name} ({m.shader.name})"))}";
                        EditorGUILayout.HelpBox(message2, MessageType.Warning);
                    }
                }

                model.texturesSizeLimit = (AvatarConverterViewModel.TexturesSizeLimit)EditorGUILayout.EnumPopup(i18n.TexturesSizeLimitLabel, model.texturesSizeLimit);
                model.mainTextureBrightness = EditorGUILayout.Slider(new GUIContent(i18n.MainTextureBrightnessLabel, i18n.MainTextureBrightnessTooltip), model.mainTextureBrightness, 0.0f, 1.0f);

                EditorGUILayout.Space();

                if (GUILayout.Button(i18n.UpdateTexturesLabel))
                {
                    OnClickUpdateTexturesButton();
                }
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                model.outputPath = EditorGUILayout.TextField(i18n.SaveToLabel, model.outputPath);
                if (model.HasInvalidCharsInOutputPath)
                {
                    EditorGUILayout.HelpBox(i18n.InvalidCharsInOutputPath + "\n" + string.Join(" ", invalidCharStrings), MessageType.Error);
                }
                if (GUILayout.Button(i18n.SelectButtonLabel))
                {
                    OnClickSelectButton();
                }
            }
            EditorGUILayout.EndVertical();

#if VRC_SDK_VRCSDK3 || VQT_HAS_VRCSDK_BASE
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField(i18n.AdvancedConverterSettingsLabel);

                EditorGUILayout.Space();

                model.removeVertexColor = EditorGUILayout.ToggleLeft(new GUIContent(i18n.RemoveVertexColorLabel, i18n.RemoveVertexColorTooltip), model.removeVertexColor  );

                EditorGUILayout.Space();

                var so = new SerializedObject(this);
                so.Update();
                var m_model = so.FindProperty("model");
                var m_overrideControllers = m_model.FindPropertyRelative("overrideControllers");
                EditorGUILayout.PropertyField(m_overrideControllers, new GUIContent(i18n.AnimationOverrideLabel, i18n.AnimationOverrideTooltip));
                so.ApplyModifiedProperties();

                if (model.OverrideControllersHasUnsupportedMaterials)
                {
                    EditorGUILayout.HelpBox(i18n.AnimationOverrideMaterialErrorMessage, MessageType.Error);
                }
            }
#endif

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(i18n.WarningForPerformance, MessageType.Info);
            EditorGUILayout.HelpBox(i18n.WarningForAppearance, MessageType.Info);
            if (model.TargetAvatarDescriptor != null)
            {
                var componentsToBeAlearted = model.UnsupportedComponents
                    .Select(c => c.GetType().Name)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToArray();
                if (componentsToBeAlearted.Count() > 0)
                {
                    EditorGUILayout.HelpBox(i18n.AlertForComponents + "\n\n" + string.Join("\n", componentsToBeAlearted.Select(c => $"  - {c}")), MessageType.Warning);
                }

                if (model.HasAnimatedMaterials)
                {
                    EditorGUILayout.HelpBox(i18n.AlertForMaterialAnimation, MessageType.Info);
                }
            }

            EditorGUI.BeginDisabledGroup(!model.CanConvertAvatar);
            {
                if (GUILayout.Button(i18n.ConvertButtonLabel))
                {
                    OnClickConvertButton();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.EndScrollView();
        }

        private void SetArtifactsPath(VRC_AvatarDescriptor avatar)
        {
            const string ArtifactsRootDir = "Assets/KRT/QuestAvatars";
            var invalidChars = Path.GetInvalidFileNameChars();

            var name = new string(avatar.name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

            // replace leading spaces with underscores
            name = Regex.Replace(name, @"^ +", m => new string('_', m.Length));

            // replace trailing spaces with underscores
            name = Regex.Replace(name, @" +$", m => new string('_', m.Length));

            model.outputPath = $"{ArtifactsRootDir}/{name}";
        }

        private void OnClickUpdateTexturesButton()
        {
            model.UpdateTextures(progressCallback.onTextureProgress);
            EditorUtility.ClearProgressBar();
        }

        private void OnClickSelectButton()
        {
            var split = model.outputPath.Split('/');
            var folder = string.Join("/", split.Where((s, i) => i <= split.Length - 2));
            var defaultName = split.Last();
            var dest = EditorUtility.SaveFolderPanel("QuestAvatars", folder, defaultName);
            if (dest != string.Empty)
            {
                model.outputPath = "Assets" + dest.Remove(0, Application.dataPath.Length);
            }
        }

        private void OnClickConvertButton()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var questAvatar = model.ConvertAvatar(
                () =>
                {
                    var altDir = AssetDatabase.GenerateUniqueAssetPath(model.outputPath);
                    var option = EditorUtility.DisplayDialogComplex(
                        i18n.OverwriteWarningDialogTitle,
                        i18n.OverwriteWarningDialogMessage(model.outputPath),
                        i18n.OverwriteWarningDialogButtonOK,
                        i18n.OverwriteWarningDialogButtonCancel,
                        i18n.OverwriteWarningDialogButtonUseAltDir(altDir));
                    switch (option)
                    {
                        case 0: // OK
                            return true;
                        case 1: // Cancel
                            return false;
                        case 2: // Alt
                            model.outputPath = altDir;
                            return true;
                    }
                    return true;
                },
                progressCallback);
            EditorUtility.ClearProgressBar();
            if (questAvatar != null)
            {
                EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.CompletedDialogMessage(model.TargetAvatarDescriptor.name), "OK");
                Selection.activeGameObject = questAvatar;

                var converted = new VRChatAvatar(questAvatar.GetComponent<VRC_AvatarDescriptor>());
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
        }

        private void OnClickConvertToPhysBonesButton()
        {
            model.ConvertDynamicBonesToPhysBones();
        }

        private void OnClickAssignNetIdsButton()
        {
            model.AssignNetIdsToPhysBones();
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
