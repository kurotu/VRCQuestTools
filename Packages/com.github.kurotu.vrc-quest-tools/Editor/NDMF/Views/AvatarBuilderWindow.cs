using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Core;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// AvatarBuilderWindow invokes build and upload process for the selected avatar with NDMF.
    /// </summary>
    internal class AvatarBuilderWindow : EditorWindow
    {
        private IVRCSdkAvatarBuilderApi sdkBuilder;
        private GameObject targetAvatar;
        private string targetBlueprintId;
        private VRCAvatar? uploadedVrcAvatar;

        private string sdkBuildProgress = string.Empty;
        private (string Status, float Percentage) sdkUploadProgress = (string.Empty, 0.0f);
        private Exception lastException = null;
        [NonSerialized]
        private bool buildSecceeded = false;
        [NonSerialized]
        private bool uploadSecceeded = false;

        private Vector2 scrollPosition;
        private SynchronizationContext mainContext;

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

        private bool CanStartLocalBuild
        {
            get
            {
                if (targetAvatar == null)
                {
                    return false;
                }
                if (!targetAvatar.activeInHierarchy)
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

                return true;
            }
        }

        private bool CanStartUpload
        {
            get
            {
                if (!CanStartLocalBuild)
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

                return true;
            }
        }

        private void OnEnable()
        {
            Debug.Log("AvatarBuilderWindow.OnEnable1");
            Debug.Log(targetAvatar);
            mainContext = SynchronizationContext.Current;
            titleContent = new GUIContent("VQT Avatar Builder");
            OnSdkPanelEnable(null, EventArgs.Empty);
            VRCSdkControlPanel.OnSdkPanelEnable += OnSdkPanelEnable;
            VRCSdkControlPanel.OnSdkPanelDisable += OnSdkPanelDisable;
            VRCSdkControlPanel.InitAccount();
            mainContext.Post(_ => OnChangeBlueprintId(targetBlueprintId), null);
        }

        private void OnDisable()
        {
            Debug.Log("AvatarBuilderWindow.OnDisable");
            sdkBuilder = null;
            VRCSdkControlPanel.OnSdkPanelEnable -= OnSdkPanelEnable;
            VRCSdkControlPanel.OnSdkPanelDisable -= OnSdkPanelDisable;
        }

        private void OnFocus()
        {
            var pipeline = targetAvatar?.GetComponent<PipelineManager>();
            if (pipeline == null)
            {
                targetBlueprintId = null;
            }
            else
            {
                targetBlueprintId = pipeline.blueprintId;
            }
            OnChangeBlueprintId(targetBlueprintId);
        }

        private void OnGUI()
        {
            Views.EditorGUIUtility.LanguageSelector();
            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowDescription, MessageType.None);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!CanSelectAvatar))
            {
                var avatars = FindActiveAvatarsFromScene();
                var selectedAvatarIndex = targetAvatar != null ? Array.IndexOf(avatars, targetAvatar) : -1;
                if (selectedAvatarIndex < 0)
                {
                    selectedAvatarIndex = 0;
                }
                selectedAvatarIndex = EditorGUILayout.Popup(i18n.AvatarLabel, selectedAvatarIndex, avatars.Select(a => a.name).ToArray());
                targetAvatar = avatars[selectedAvatarIndex];

                var newBlueprintId = targetAvatar.GetComponent<PipelineManager>().blueprintId;
                if (newBlueprintId != targetBlueprintId)
                {
                    targetBlueprintId = newBlueprintId;
                    uploadedVrcAvatar = null;
                    OnChangeBlueprintId(targetBlueprintId);
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
                        if (uploadedVrcAvatar.HasValue)
                        {
                            name = uploadedVrcAvatar.Value.Name;
                            desc = uploadedVrcAvatar.Value.Description;
                            tags = string.Join(", ", uploadedVrcAvatar.Value.Tags.Select(t => VRCSDKUtility.GetContentTagLabel(t)).ToArray());
                            visibility = uploadedVrcAvatar.Value.ReleaseStatus;
                        }
                        else
                        {
                            name = AvatarBuilderSessionState.AvatarName;
                            desc = AvatarBuilderSessionState.AvatarDesc;
                            tags = AvatarBuilderSessionState.AvatarTags;
                            visibility = AvatarBuilderSessionState.AvatarReleaseStatus;
                        }
                        if (name == string.Empty)
                        {
                            name = "(No name)";
                        }
                        if (desc == string.Empty)
                        {
                            desc = "(No description)";
                        }
                        var tagLabels = tags == string.Empty ? new string[] { "(No tags)" } : tags.Trim('|').Split('|').Select(t => VRCSDKUtility.GetContentTagLabel(t)).ToArray();

                        EditorGUILayout.LabelField("Name", name, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Description", desc, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Content Tags", string.Join(", ", tagLabels), EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Visibility", visibility, EditorStyles.wordWrappedLabel);
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
                    return;
                }

                SdkBuilderProgressBar();

                EditorGUILayout.Space();

                var isAndroidEditor = EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android;
                using (new EditorGUI.DisabledScope(!CanStartLocalBuild))
                {
                    using (new EditorGUI.DisabledScope(isAndroidEditor))
                    {
                        EditorGUILayout.LabelField(i18n.AvatarBuilderWindowOfflineTestingLabel, EditorStyles.largeLabel);
                        EditorGUILayout.LabelField(i18n.AvatarBuilderWindowOfflineTestingDescription(targetAvatar.name), EditorStyles.wordWrappedMiniLabel);
                        if (GUILayout.Button("Build & Test on PC"))
                        {
                            OnClickBuildAndTestOnPC();
                        }
                        EditorGUILayout.Space();
                    }
                }

                using (new EditorGUI.DisabledScope(!CanStartUpload))
                {
                    using (new EditorGUI.DisabledScope(!isAndroidEditor))
                    {
                        EditorGUILayout.LabelField(i18n.AvatarBuilderWindowOnlinePublishingLabel, EditorStyles.largeLabel);
                        EditorGUILayout.LabelField(i18n.AvatarBuilderWindowOnlinePublishingDescription, EditorStyles.wordWrappedMiniLabel);
                        if (!uploadedVrcAvatar.HasValue && (string.IsNullOrEmpty(AvatarBuilderSessionState.AvatarName) || string.IsNullOrEmpty(AvatarBuilderSessionState.AvatarThumbPath)))
                        {
                            EditorGUILayout.HelpBox(i18n.AvatarBuilderWindowRequiresAvatarNameAndThumb, MessageType.Error);
                        }
                        if (GUILayout.Button("Build & Publish for Android"))
                        {
                            OnClickBuildAndPublishForAndroid();
                        }
                        EditorGUILayout.Space();
                    }
                }

                using (new EditorGUI.DisabledScope(IsBuilding))
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
            if (sdkBuilder.BuildState != SdkBuildState.Idle)
            {
                EditorGUILayout.Space();
                var progress = 0.0f;
                switch (sdkBuilder.BuildState)
                {
                    case SdkBuildState.Idle:
                        progress = 0.0f;
                        break;
                    case SdkBuildState.Building:
                        progress = 0.0f;
                        break;
                    case SdkBuildState.Success:
                        progress = 1.0f;
                        break;
                    case SdkBuildState.Failure:
                        progress = 0.0f;
                        break;
                }
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, sdkBuildProgress);
            }

            if (sdkBuilder.UploadState != SdkUploadState.Idle)
            {
                EditorGUILayout.Space();
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), sdkUploadProgress.Percentage, sdkUploadProgress.Status);
            }
        }

        private void OnClickOpenSdkControlPanel()
        {
            GetWindow<VRCSdkControlPanel>().Show();
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
                Debug.Log("GetVRCAvatar " + a.Result.HasValue);
                uploadedVrcAvatar = a.Result;
                PostRepaint();
            });
        }

        private void PostRepaint()
        {
            mainContext.Post(_ => Repaint(), null);
        }

        private void OnClickBuildAndTestOnPC()
        {
            SetBuildTarget(Models.BuildTarget.Android);

            var avatar = targetAvatar;
            var task = BuildAndTest(avatar);
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
            var task = BuildAndPublish(avatar);
            task.ContinueWith(t =>
            {
                SetBuildTarget(Models.BuildTarget.Auto);
                PostRepaint();
            });
        }

#if VQT_HAS_NDMF
        private void OnClickManualBakeWithConverterSettings()
        {
            SetBuildTarget(Models.BuildTarget.Android);
            try
            {
                nadena.dev.ndmf.AvatarProcessor.ProcessAvatarUI(targetAvatar);
            }
            finally
            {
                SetBuildTarget(Models.BuildTarget.Auto);
            }
        }
#endif

        private void OnSdkPanelEnable(object sender, EventArgs e)
        {
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
            var avatars = VRCSDKUtility.GetAvatarsFromScene(SceneManager.GetActiveScene());
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
            sdkBuildProgress = "Building Avatar";
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
                var avatar = await GetVRCAvatar(pipeline.blueprintId);
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
    }
}
