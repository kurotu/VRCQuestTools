using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;
using VRC.Core;
using VRC.SDK3A.Editor;
using VRC.SDKBase;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// AvatarBuilderWindow invokes build and upload process for the selected avatar with NDMF.
    /// </summary>
    internal class AvatarBuilderWindow : EditorWindow
    {
        private const string SdkBuildProgressStartingBuild = "Building Avatar";

        private IVRCSdkAvatarBuilderApi sdkBuilder;
        private VRC_AvatarDescriptor targetAvatar;
        private string targetBlueprintId;
        private VRCAvatar? uploadedVrcAvatar;
        private bool tryToSetFallbackFlag = false;

        private string sdkBuildProgress = string.Empty;
        private (string Status, float Percentage) sdkUploadProgress = (string.Empty, 0.0f);
        private Exception lastException = null;
        [NonSerialized]
        private bool buildSecceeded = false;
        [NonSerialized]
        private bool uploadSecceeded = false;
        [NonSerialized]
        private string thumbnailUri;
        [NonSerialized]
        private Texture2D thumbnail;
        private string noImageThumbnailPath;

        private Vector2 scrollPosition;
        private SynchronizationContext mainContext;

        private bool IsPlayMode => EditorApplication.isPlayingOrWillChangePlaymode;

        private bool IsBuilding
        {
            get
            {
                if (sdkBuilder == null)
                {
                    return false;
                }
                if (sdkBuilder.BuildState != SdkBuildState.Idle)
                {
                    return true;
                }
                if (sdkBuilder.UploadState != SdkUploadState.Idle)
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanSelectAvatar => !IsBuilding;

        private bool CanStartBuild
        {
            get
            {
                if (IsPlayMode)
                {
                    return false;
                }
                if (targetAvatar == null)
                {
                    return false;
                }
                if (!targetAvatar.gameObject.activeInHierarchy)
                {
                    return false;
                }
                if (sdkBuilder == null)
                {
                    return false;
                }
                if (IsBuilding)
                {
                    return false;
                }
                if (sdkBuildProgress == SdkBuildProgressStartingBuild)
                {
                    return false;
                }
                return true;
            }
        }

        private bool CanStartLocalBuild
        {
            get
            {
                if (!CanStartBuild)
                {
                    return false;
                }
                return EditorUserBuildSettings.activeBuildTarget != UnityEditor.BuildTarget.iOS;
            }
        }

        private bool CanStartUpload
        {
            get
            {
                if (!CanStartBuild)
                {
                    return false;
                }

                if (uploadedVrcAvatar == null)
                {
                    if (string.IsNullOrEmpty(AvatarBuilderSessionState.AvatarName))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(AvatarBuilderSessionState.AvatarThumbPath))
                    {
                        return false;
                    }
                }

                return EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS;
            }
        }

        private bool CanStartManualBake
        {
            get
            {
                if (IsPlayMode)
                {
                    return false;
                }
                if (IsBuilding)
                {
                    return false;
                }

                return true;
            }
        }

        private void OnEnable()
        {
            mainContext = SynchronizationContext.Current;
            titleContent = new GUIContent("VQT Avatar Builder");
            noImageThumbnailPath = AssetDatabase.GUIDToAssetPath("c7a6f55b56f65934e9489e35a10808cf");
            thumbnail = LoadNoThumbnailImage();
            OnSdkPanelEnable(null, EventArgs.Empty);
            VRCSdkControlPanel.OnSdkPanelEnable += OnSdkPanelEnable;
            VRCSdkControlPanel.OnSdkPanelDisable += OnSdkPanelDisable;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            mainContext.Post(_ => OnChangeBlueprintId(targetBlueprintId), null);
        }

        private void OnDisable()
        {
            sdkBuilder = null;
            VRCSdkControlPanel.OnSdkPanelEnable -= OnSdkPanelEnable;
            VRCSdkControlPanel.OnSdkPanelDisable -= OnSdkPanelDisable;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            thumbnail = null;
        }

        private void OnFocus()
        {
            var pipeline = targetAvatar?.GetComponent<PipelineManager>();
            if (pipeline == null)
            {
                targetBlueprintId = null;
                OnChangeBlueprintId(targetBlueprintId);
            }
            else
            {
                if (targetBlueprintId != pipeline.blueprintId || !uploadedVrcAvatar.HasValue)
                {
                    OnChangeBlueprintId(pipeline.blueprintId);
                }
                targetBlueprintId = pipeline.blueprintId;
            }
        }

        private void OnGUI()
        {
            Views.EditorGUIUtility.LanguageSelector();
            Views.EditorGUIUtility.UpdateNotificationPanel();

            Views.EditorGUIUtility.HorizontalDivider(2);

            var i18n = VRCQuestToolsSettings.I18nResource;

            if (IsPlayMode)
            {
                EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowExitPlayMode, MessageType.Warning);
                return;
            }

            if (sdkBuilder == null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowRequiresControlPanel, MessageType.Warning);
                    if (GUILayout.Button(i18n.OpenLabel, GUILayout.Height(38), GUILayout.Width(60)))
                    {
                        OnClickOpenSdkControlPanel();
                    }
                }
                EditorGUILayout.Space();
            }

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowExitPrefabStage, MessageType.Warning);
                    if (GUILayout.Button(i18n.ExitLabel, GUILayout.Height(38), GUILayout.Width(60)))
                    {
                        OnClickExitPrefabStage();
                    }
                }
                return;
            }

            using (new EditorGUI.DisabledScope(!CanSelectAvatar))
            {
                var avatars = FindActiveAvatarsFromScene();
                if (avatars.Length == 0)
                {
                    EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowNoActiveAvatarsFound, MessageType.Warning);
                    return;
                }

                EditorGUILayout.LabelField(i18n.AvatarBuilderWindowSelectAvatar, EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space();
                try
                {
                    targetAvatar = VRCSDKUtility.GetSdkControlPanelSelectedAvatar();
                }
                catch (NotSupportedException)
                {
                    EditorGUILayout.HelpBox(i18n.IncompatibleSDK, MessageType.Error);
                    return;
                }
                if (targetAvatar == null)
                {
                    return;
                }

                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    var index = avatars.ToList().IndexOf(targetAvatar.gameObject);
                    var selectedIndex = EditorGUILayout.Popup("Selected Avatar", index, avatars.Select(a => a.name).ToArray());
                    if (ccs.changed)
                    {
                        targetAvatar = avatars[selectedIndex].GetComponent<VRC_AvatarDescriptor>();
                        VRCSdkControlPanelAvatarBuilder.SelectAvatar(targetAvatar);
                    }

                    var newBlueprintId = targetAvatar.GetComponent<PipelineManager>().blueprintId;
                    if (newBlueprintId != targetBlueprintId || ccs.changed)
                    {
                        targetBlueprintId = newBlueprintId;
                        uploadedVrcAvatar = null;
                        OnChangeBlueprintId(targetBlueprintId);
                    }
                }
            }

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                if (targetAvatar != null)
                {
                    using (new EditorGUILayout.VerticalScope("Box"))
                    {
                        string name;
                        string desc;
                        string tags;
                        string visibility;
                        string newThumbnailUri;
                        if (uploadedVrcAvatar.HasValue)
                        {
                            name = uploadedVrcAvatar.Value.Name;
                            desc = uploadedVrcAvatar.Value.Description;
                            tags = string.Join("|", uploadedVrcAvatar.Value.Tags);
                            visibility = uploadedVrcAvatar.Value.ReleaseStatus;
                            newThumbnailUri = uploadedVrcAvatar.Value.ThumbnailImageUrl;
                        }
                        else
                        {
                            name = AvatarBuilderSessionState.AvatarName;
                            desc = AvatarBuilderSessionState.AvatarDesc;
                            tags = AvatarBuilderSessionState.AvatarTags;
                            visibility = AvatarBuilderSessionState.AvatarReleaseStatus;
                            newThumbnailUri = AvatarBuilderSessionState.AvatarThumbPath;

                            if (string.IsNullOrEmpty(newThumbnailUri))
                            {
                                newThumbnailUri = noImageThumbnailPath;
                            }
                        }

                        if (thumbnail == null)
                        {
                            thumbnail = LoadNoThumbnailImage();
                        }
                        if (thumbnailUri != newThumbnailUri)
                        {
                            if (uploadedVrcAvatar.HasValue)
                            {
                                VRCApi.GetImage(newThumbnailUri).ContinueWith(t =>
                                {
                                    var tex = t.Result;
                                    if (tex != null)
                                    {
                                        thumbnail = tex;
                                        Repaint();
                                    }
                                });
                            }
                            else
                            {
                                var data = System.IO.File.ReadAllBytes(newThumbnailUri);
                                thumbnail.LoadImage(data);
                            }
                            thumbnailUri = newThumbnailUri;
                        }

                        if (name == string.Empty)
                        {
                            name = "(No name)";
                        }
                        if (desc == string.Empty)
                        {
                            desc = "(No description)";
                        }
                        var contentTags = tags.Trim('|').Split('|').Where(t => t.StartsWith("content_")).ToArray();
                        var contentTagLabels = contentTags.Length == 0
                            ? new string[] { "(No tags)" }
                            : contentTags.Select(t => VRCSDKUtility.GetContentTagLabel(t)).ToArray();
                        var isFallback = tags.Contains(VRCSDKUtility.AvatarContentTag.Fallback);
                        EditorGUILayout.LabelField("Name", name, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Description", desc, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Content Tags", string.Join(", ", contentTagLabels), EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Visibility", System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(visibility.ToLower()), EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Fallback", isFallback ? "Yes" : "No", EditorStyles.wordWrappedLabel);

                        const int thumbnailHeight = 140;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PrefixLabel("Thumbnail");
                            var texRect = EditorGUILayout.GetControlRect();
                            texRect.height = thumbnailHeight;
                            texRect.width = thumbnail.width * texRect.height / thumbnail.height;
                            GUI.DrawTexture(texRect, thumbnail);
                        }
                        EditorGUILayout.Space(thumbnailHeight - EditorGUIUtility.singleLineHeight);
                    }
                }

                if (uploadSecceeded || buildSecceeded)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var message = uploadSecceeded ? i18n.AvatarBuilderWindowSucceededUpload : i18n.AvatarBuilderWindowSucceededBuild;
                        EditorGUILayout.HelpBox(message, MessageType.Info);
                        if (GUILayout.Button(i18n.DismissLabel, GUILayout.Height(38), GUILayout.Width(60)))
                        {
                            buildSecceeded = false;
                            uploadSecceeded = false;
                        }
                    }
                }

                if (lastException != null)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowFailedBuild + "\n" + lastException.Message, MessageType.Error);
                        if (GUILayout.Button(i18n.DismissLabel, GUILayout.Height(38), GUILayout.Width(60)))
                        {
                            lastException = null;
                        }
                    }
                }

                SdkBuilderProgressBar();

                EditorGUILayout.Space();

                using (new EditorGUI.DisabledScope(!CanStartLocalBuild))
                {
                    var targetName = EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android ? "Android" : "PC";
                    EditorGUILayout.LabelField(i18n.AvatarBuilderWindowOfflineTestingLabel(targetName), EditorStyles.largeLabel);
                    EditorGUILayout.LabelField(i18n.AvatarBuilderWindowOfflineTestingDescription(targetAvatar.name), EditorStyles.wordWrappedMiniLabel);
                    if (GUILayout.Button($"Build & Test on {targetName}"))
                    {
                        OnClickBuildAndTest();
                    }
                    EditorGUILayout.Space();
                }

                using (new EditorGUI.DisabledScope(!CanStartUpload))
                {
                    var targetName = EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS ? "iOS" : "Android";
                    EditorGUILayout.LabelField(i18n.AvatarBuilderWindowOnlinePublishingLabel(targetName), EditorStyles.largeLabel);
                    EditorGUILayout.LabelField(i18n.AvatarBuilderWindowOnlinePublishingDescription, EditorStyles.wordWrappedMiniLabel);
#if VQT_HAS_VRCSDK_NO_PRECHECK
                    EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowSdkNoPrecheck, MessageType.Info);
#endif
                    if (!uploadedVrcAvatar.HasValue && (string.IsNullOrEmpty(AvatarBuilderSessionState.AvatarName) || string.IsNullOrEmpty(AvatarBuilderSessionState.AvatarThumbPath)))
                    {
                        EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowRequiresAvatarNameAndThumb, MessageType.Error);
                    }
                    tryToSetFallbackFlag = EditorGUILayout.ToggleLeft(new GUIContent(i18n.AvatarBuilderWindowSetAsFallbackIfPossible, i18n.AvatarBuilderWindowSetAsFallbackIfPossibleTooltip), tryToSetFallbackFlag);
                    if (GUILayout.Button($"Build & Publish for {targetName}"))
                    {
                        OnClickBuildAndPublishForAndroid();
                    }
                    EditorGUILayout.Space();
                }

                using (new EditorGUI.DisabledScope(!CanStartManualBake))
                {
                    EditorGUILayout.LabelField(i18n.AvatarBuilderWindowNdmfManualBakingLabel, EditorStyles.largeLabel);
                    EditorGUILayout.LabelField(i18n.AvatarBuilderWindowNdmfManualBakingDescription, EditorStyles.wordWrappedMiniLabel);
                    if (GUILayout.Button("Manual Bake"))
                    {
                        OnClickManualBakeWithConverterSettings();
                    }
                    EditorGUILayout.Space();
                }
            }
        }

        private void SdkBuilderProgressBar()
        {
            if (sdkBuilder == null)
            {
                return;
            }
            var i18n = VRCQuestToolsSettings.I18nResource;
            if (sdkBuilder.BuildState != SdkBuildState.Idle)
            {
                var progress = 0.0f;
                switch (sdkBuilder.BuildState)
                {
                    case SdkBuildState.Idle:
                        progress = 0.0f;
                        break;
                    case SdkBuildState.Building:
                        progress = 0.5f;
                        break;
                    case SdkBuildState.Success:
                        progress = 1.0f;
                        break;
                    case SdkBuildState.Failure:
                        progress = 1.0f;
                        break;
                }
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(i18n.AvatarBuilderWindowBuildingProgressLabel, EditorStyles.largeLabel);
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, sdkBuildProgress);
            }
#if VQT_HAS_VRCSDK_COPYRIGHT_AGREEMENT_2
            else
            {
                if (sdkBuildProgress == SdkBuildProgressStartingBuild)
                {
                    EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowCopyrightAgreementHelp, MessageType.Warning);
                }
            }
#endif

            if (sdkBuilder.UploadState != SdkUploadState.Idle)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(i18n.AvatarBuilderWindowUploadingProgressLabel, EditorStyles.largeLabel);
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), sdkUploadProgress.Percentage, sdkUploadProgress.Status);
#if VQT_HAS_VRCSDK_COPYRIGHT_AGREEMENT && !VQT_HAS_VRCSDK_COPYRIGHT_AGREEMENT_2
                // awaiting copyright ownership agreement
                if (sdkUploadProgress.Percentage == 0.0f && sdkUploadProgress.Status == "")
                {
                    EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowCopyrightAgreementHelp, MessageType.Warning);
                }
#endif
            }
        }

        private void OnClickOpenSdkControlPanel()
        {
            GetWindow<VRCSdkControlPanel>().Show();
        }

        private void OnClickExitPrefabStage()
        {
            StageUtility.GoToMainStage();
        }

        private void OnChangeBlueprintId(string blueprintId)
        {
            if (string.IsNullOrEmpty(blueprintId))
            {
                uploadedVrcAvatar = null;
                PostRepaint();
                return;
            }
            GetVRCAvatar(blueprintId).ContinueWith(a =>
            {
                uploadedVrcAvatar = a.Result;
                if (a.Result.HasValue)
                {
                    tryToSetFallbackFlag = a.Result.Value.Tags.Contains(VRCSDKUtility.AvatarContentTag.Fallback);
                }
                PostRepaint();
            });
        }

        private void PostRepaint()
        {
            mainContext?.Post(_ => Repaint(), null);
        }

        private void OnClickBuildAndTest()
        {
            SetBuildTarget(Models.BuildTarget.Android);

            var avatar = targetAvatar;
            var task = BuildAndTest(avatar.gameObject);
            task.ContinueWith(t =>
            {
                SetBuildTarget(Models.BuildTarget.Auto);
                PostRepaint();
            });
        }

        private void OnClickBuildAndPublishForAndroid()
        {
            SetBuildTarget(Models.BuildTarget.Android);

            var avatar = targetAvatar;
            var task = BuildAndPublish(avatar.gameObject);
            task.ContinueWith(t =>
            {
                SetBuildTarget(Models.BuildTarget.Auto);
                PostRepaint();
            });
        }

        private void OnClickManualBakeWithConverterSettings()
        {
            NdmfPluginUtility.ManualBakeWithAndroidSettings(targetAvatar.gameObject);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                OnFocus();
            }
        }

        private void OnSdkPanelEnable(object sender, EventArgs e)
        {
            if (sdkBuilder != null)
            {
                return;
            }
            if (VRCSdkControlPanel.TryGetBuilder(out sdkBuilder))
            {
                sdkBuilder.OnSdkBuildProgress += OnSdkBuildProgress;
                sdkBuilder.OnSdkUploadProgress += OnSdkUploadProgress;
            }
            PostRepaint();
        }

        private void OnSdkPanelDisable(object sender, EventArgs e)
        {
            if (sdkBuilder != null)
            {
                sdkBuilder.OnSdkBuildProgress -= OnSdkBuildProgress;
                sdkBuilder.OnSdkUploadProgress -= OnSdkUploadProgress;
                sdkBuilder = null;
            }
            PostRepaint();
        }

        private void OnSdkBuildProgress(object sender, string progress)
        {
            sdkBuildProgress = progress;
            PostRepaint();
        }

        private void OnSdkUploadProgress(object sender, (string Status, float Percentage) progress)
        {
            sdkUploadProgress = progress;
            PostRepaint();
        }

        private GameObject[] FindActiveAvatarsFromScene()
        {
            var avatars = VRCSDKUtility.GetAvatarsFromLoadedScenes();
            return avatars
                .Where(a => a.gameObject.activeInHierarchy)
                .Where(a => VRCSDKUtility.IsAvatarRoot(a.gameObject))
                .Select(a => a.gameObject)
                .ToArray();
        }

        private void PrepareBuild()
        {
            lastException = null;
            buildSecceeded = false;
            uploadSecceeded = false;
            sdkBuildProgress = SdkBuildProgressStartingBuild;
            sdkUploadProgress = (string.Empty, 0.0f);
        }

        private async Task BuildAndTest(GameObject avatar)
        {
            PrepareBuild();
            try
            {
                await sdkBuilder.BuildAndTest(avatar);
                buildSecceeded = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                lastException = e;
            }
            finally
            {
                sdkBuildProgress = string.Empty;
                var pipeline = avatar.GetComponent<PipelineManager>();
                targetBlueprintId = pipeline.blueprintId;
                OnChangeBlueprintId(targetBlueprintId);
            }
        }

        private async Task BuildAndPublish(GameObject avatarObject)
        {
            PrepareBuild();
            var pipeline = avatarObject.GetComponent<PipelineManager>();
            try
            {
                var blueprintId = pipeline.blueprintId;
                var avatar = await GetVRCAvatar(blueprintId);
                var isNewAvatar = !avatar.HasValue;
                string thumbPath = null;
                if (isNewAvatar)
                {
                    avatar = new VRCAvatar
                    {
                        Name = AvatarBuilderSessionState.AvatarName,
                        Description = AvatarBuilderSessionState.AvatarDesc,
                        Tags = AvatarBuilderSessionState.AvatarTags.Trim('|').Split('|').ToList(),
                        ReleaseStatus = AvatarBuilderSessionState.AvatarReleaseStatus,
                    };
                    thumbPath = AvatarBuilderSessionState.AvatarThumbPath;
                }
                await sdkBuilder.BuildAndUpload(avatarObject, avatar.Value, thumbPath);
                if (tryToSetFallbackFlag)
                {
                    var overallRating = NdmfSessionState.LastActualPerformanceRating[blueprintId];
                    if (VRCSDKUtility.IsAllowedForFallbackAvatar(overallRating))
                    {
                        if (!avatar.Value.Tags.Contains(VRCSDKUtility.AvatarContentTag.Fallback))
                        {
                            Debug.Log($"[{VRCQuestTools.Name}] Setting avatar as fallback");
                            uploadedVrcAvatar = await VRCApi.SetAvatarAsFallback(blueprintId, avatar.Value);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[{VRCQuestTools.Name}] The avatar is not allowed to be set as a fallback avatar: {overallRating}");
                        EditorUtility.DisplayDialog(VRCQuestTools.Name, VRCQuestToolsSettings.I18nResource.AvatarBuilderWindowFallbackNotAllowed(overallRating.ToString()), "OK");
                    }
                }
                uploadSecceeded = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                lastException = e;
            }
            finally
            {
                sdkBuildProgress = string.Empty;
                targetBlueprintId = pipeline.blueprintId;
                OnChangeBlueprintId(targetBlueprintId);
            }
        }

        private void SetBuildTarget(Models.BuildTarget target)
        {
            NdmfSessionState.BuildTarget = target;
        }

        private async Task<VRCAvatar?> GetVRCAvatar(string blueprintId)
        {
            if (string.IsNullOrEmpty(blueprintId))
            {
                return null;
            }
            try
            {
                return await VRCApi.GetAvatar(blueprintId);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get VRCAvatar ({e.GetType().Name}): {e.Message}");

                // Debug.LogException(e);
                return null;
            }
        }

        private Texture2D LoadNoThumbnailImage()
        {
            var tex = new Texture2D(4, 4);
            var data = System.IO.File.ReadAllBytes(noImageThumbnailPath);
            tex.LoadImage(data);
            return tex;
        }
    }
}
