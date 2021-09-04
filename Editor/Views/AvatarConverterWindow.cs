// <copyright file="AvatarConverterWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
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
    /// EditorWindow for AvatarConverter.
    /// </summary>
    internal class AvatarConverterWindow : EditorWindow
    {
        [SerializeField]
        private AvatarConverterViewModel model = new AvatarConverterViewModel();
        private Vector2 scrollPosition;
        private VRChatAvatar.ProgressCallback progressCallback;

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
            progressCallback = new Models.VRChat.VRChatAvatar.ProgressCallback
            {
                onTextureProgress = (total, index, exception, material) =>
                {
                    var i18n = VRCQuestToolsSettings.I18nResource;
                    if (exception != null)
                    {
                        var message = $"{i18n.MaterialExceptionDialogMessage}\n" +
                            "\n" +
                            $"Material: {AssetDatabase.GetAssetPath(material)}\n" +
                            $"Shader: {material.shader.name}\n" +
                            "\n" +
                            $"Exception: {exception.Message}";
                        EditorUtility.DisplayDialog(VRCQuestTools.Name, message, "OK");
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
                            $"AnimationClip: {clip.name}\n" +
                            $"\n" +
                            $"Exception: {exception.Message}";
                        EditorUtility.DisplayDialog(VRCQuestTools.Name, message, "OK");
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
                            $"AnimatorController: {controller.name}\n" +
                            $"\n" +
                            $"Exception: {exception.Message}";
                        EditorUtility.DisplayDialog(VRCQuestTools.Name, message, "OK");
                        EditorUtility.ClearProgressBar();
                    }
                    else
                    {
                        var progress = (float)index / total;
                        EditorUtility.DisplayProgressBar(VRCQuestTools.Name, $"Converting AnimatorControllers : {index + 1}/{total}", progress);
                    }
                },
            };
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

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                model.generateQuestTextures = EditorGUILayout.BeginToggleGroup(i18n.GenerateQuestTexturesLabel, model.generateQuestTextures);
                var message = $"{i18n.QuestTexturesDescription}\n\n" +
                    $"{i18n.VerifiedShadersLabel}: Standard, UTS2, arktoon, Sunao";
                EditorGUILayout.HelpBox(message, MessageType.Info);
                if (model.TargetAvatarDescriptor != null)
                {
                    var unverifiedMaterials = model.UnverifiedShaderMaterials;
                    if (model.generateQuestTextures && unverifiedMaterials.Length > 0)
                    {
                        var message2 = $"{i18n.WarningForUnverifiedShaders}\n\n" +
                            $"{string.Join("\n", unverifiedMaterials.Select(m => $"  - {m.name} ({m.shader.name})"))}";
                        EditorGUILayout.HelpBox(message2, MessageType.Error);
                    }
                }

                model.texturesSizeLimit = (AvatarConverterViewModel.TexturesSizeLimit)EditorGUILayout.EnumPopup(i18n.TexturesSizeLimitLabel, model.texturesSizeLimit);

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
                if (GUILayout.Button(i18n.SelectButtonLabel))
                {
                    OnClickSelectButton();
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(i18n.WarningForPerformance, MessageType.Info);
            EditorGUILayout.HelpBox(i18n.WarningForAppearance, MessageType.Warning);
            if (model.TargetAvatarDescriptor != null)
            {
                var componentsToBeAlearted = model.UnsupportedComponents
                    .Select(c => c.GetType().Name)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToArray();
                if (componentsToBeAlearted.Count() > 0)
                {
                    EditorGUILayout.HelpBox(i18n.AlertForComponents + "\n\n" + string.Join("\n", componentsToBeAlearted.Select(c => $"  - {c}")), MessageType.Error);
                }

                if (model.HasAnimatedMaterials)
                {
                    EditorGUILayout.HelpBox(i18n.AlertForMaterialAnimation, MessageType.Error);
                }
            }

            EditorGUI.BeginDisabledGroup(model.TargetAvatarDescriptor == null);
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
            model.outputPath = $"{ArtifactsRootDir}/{avatar.name}";
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
            }
        }
    }
}
