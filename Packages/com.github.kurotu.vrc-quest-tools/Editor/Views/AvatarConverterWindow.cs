// <copyright file="AvatarConverterWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

#if VQT_HAS_VRCSDK_BASE
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
        private GameObject targetRoot;

        [SerializeField]
        private Vector2 scrollPosition = Vector2.zero;

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
            titleContent.text = "Convert Avatar for Android";
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
                    var components = new string[]
                    {
                        "VQT Avatar Converter Settings",
                        "VQT Network ID Assigner",
#if VQT_HAS_MA_CONVERT_CONSTRAINTS
                        "MA Convert Constraints",
#endif
                    };
                    var componentsText = components.Select(c => $"  - {c}").Aggregate((a, b) => $"{a}\n{b}");
                    EditorGUILayout.HelpBox(i18n.BeginConvertSettingsButtonDescription + "\n" + componentsText, MessageType.Info);

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
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add AvatarConverterSettings");
            var converterSettings = Undo.AddComponent<AvatarConverterSettings>(targetRoot);
            editor = Editor.CreateEditor(converterSettings);
            if (targetRoot.gameObject.GetComponent<NetworkIDAssigner>() == null)
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                Undo.AddComponent<NetworkIDAssigner>(targetRoot);
                EditorUtility.DisplayDialog("VRCQuestTools", i18n.NetworkIdAssignerAttached, "OK");
            }

#if VQT_HAS_MA_CONVERT_CONSTRAINTS
            if (targetRoot.GetComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>() == null)
            {
                Undo.AddComponent<nadena.dev.modular_avatar.core.ModularAvatarConvertConstraints>(targetRoot);
            }
#endif

            Undo.CollapseUndoOperations(group);
        }
    }
}
