// <copyright file="AvatarConverterWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// EditorWindow for AvatarConverter.
    /// </summary>
    internal class AvatarConverterWindow : EditorWindow
    {
        [SerializeField]
        private GameObject targetRoot;

        [SerializeField]
        private Vector2 scrollPosition = Vector2.zero;

        [SerializeField]
        private bool addMaConvertConstraints = true;

        [SerializeField]
        private bool addMaSyncParameterSequence = true;

        [SerializeField]
        private bool addAaoTraceAndOptimize = true;

        private Editor editor;

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
            window.targetRoot = avatar.gameObject;
            window.Show();
        }

        private void OnEnable()
        {
            titleContent.text = "Setup Avatar for Mobile";
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUI.BeginChangeCheck();
            var descriptor = targetRoot == null ? null : targetRoot.GetComponent<VRC_AvatarDescriptor>();
            descriptor = (VRC_AvatarDescriptor)EditorGUILayout.ObjectField(i18n.AvatarLabel, descriptor, typeof(VRC_AvatarDescriptor), true);
            targetRoot = descriptor == null ? null : descriptor.gameObject;
            if (targetRoot == null)
            {
                return;
            }
            if (EditorGUI.EndChangeCheck() || editor == null || editor.target == null)
            {
                var converterSettings = targetRoot.GetComponentInChildren<AvatarConverterSettings>(true);
                if (converterSettings == null)
                {
                    var hasMaConvertConstraints = ModularAvatarUtility.IsModularAvatarImported();
                    var hasMaSyncParameterSequence = ModularAvatarUtility.SupportsSyncParameterSequenceComponent();
                    var hasAaoTraceAndOptimize = AvatarOptimizerUtility.IsAvatarOptimizerImported();
                    var hasMaConvertConstraintsComponent = hasMaConvertConstraints && ModularAvatarUtility.HasConvertConstraintsComponent(targetRoot);
                    var hasMaSyncParameterSequenceComponent = hasMaSyncParameterSequence && ModularAvatarUtility.HasSyncParameterSequenceComponent(targetRoot);
                    var hasAaoTraceAndOptimizeComponent = hasAaoTraceAndOptimize && AvatarOptimizerUtility.HasTraceAndOptimizeComponent(targetRoot);

                    EditorGUILayout.LabelField(i18n.BeginConvertSettingsButtonDescription, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.Space();

                    if (hasMaConvertConstraints)
                    {
                        addMaConvertConstraints = DrawOptionalComponentToggle("MA Convert Constraints", addMaConvertConstraints, hasMaConvertConstraintsComponent);
                    }

                    if (hasMaSyncParameterSequence)
                    {
                        addMaSyncParameterSequence = DrawOptionalComponentToggle("MA Sync Parameter Sequence", addMaSyncParameterSequence, hasMaSyncParameterSequenceComponent);
                    }

                    if (hasAaoTraceAndOptimize)
                    {
                        addAaoTraceAndOptimize = DrawOptionalComponentToggle("AAO Trace and Optimize", addAaoTraceAndOptimize, hasAaoTraceAndOptimizeComponent);
                    }

                    editor = null;
                    if (GUILayout.Button(i18n.BeginConvertSettingsButtonLabel))
                    {
                        OnClickAttachAvatarConverterButton();
                    }
                    return;
                }
                editor = Editor.CreateEditor(converterSettings);
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scroll.scrollPosition;

                if (editor != null)
                {
                    editor.OnInspectorGUI();
                    EditorGUILayout.Space();
                }
            }
        }

        private void OnClickAttachAvatarConverterButton()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add AvatarConverterSettings");
            var converterSettings = Undo.AddComponent<AvatarConverterSettings>(targetRoot);
            editor = Editor.CreateEditor(converterSettings);
            if (converterSettings.assignNetworkIds)
            {
                EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.PhysBoneSyncReminder, "OK");
            }

            if (addMaConvertConstraints && ModularAvatarUtility.IsModularAvatarImported() && !ModularAvatarUtility.HasConvertConstraintsComponent(targetRoot))
            {
                ModularAvatarUtility.AddConvertConstraintsComponent(targetRoot);
            }

            if (addMaSyncParameterSequence && ModularAvatarUtility.SupportsSyncParameterSequenceComponent() && !ModularAvatarUtility.HasSyncParameterSequenceComponent(targetRoot))
            {
                ModularAvatarUtility.AddSyncParameterSequenceComponent(targetRoot);
            }

            if (addAaoTraceAndOptimize && AvatarOptimizerUtility.IsAvatarOptimizerImported() && !AvatarOptimizerUtility.HasTraceAndOptimizeComponent(targetRoot))
            {
                AvatarOptimizerUtility.AddTraceAndOptimizeComponent(targetRoot);
            }

            Undo.CollapseUndoOperations(group);
        }

        private static bool DrawOptionalComponentToggle(string label, bool enabledByUser, bool alreadyExists)
        {
            var value = alreadyExists || enabledByUser;
            using (new EditorGUI.DisabledScope(alreadyExists))
            {
                value = EditorGUILayout.ToggleLeft(label, value);
            }

            return alreadyExists || value;
        }
    }
}
