// <copyright file="AvatarConverterWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
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
        internal VRC_AvatarDescriptor target;
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
            window.target = avatar;
            window.Show();
        }

        private void OnEnable()
        {
            titleContent.text = "Convert Avatar for Quest";
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            EditorGUI.BeginChangeCheck();
            target = (VRC_AvatarDescriptor)EditorGUILayout.ObjectField(i18n.AvatarLabel, target, typeof(VRC_AvatarDescriptor), true);
            if (target == null)
            {
                return;
            }
            var converter = target.GetComponentInChildren<AvatarConverter>(true);
            if (converter == null)
            {
                if (GUILayout.Button(i18n.AddAvatarConverterButtonLabel(target.name)))
                {
                    OnClickAttachAvatarConverterButton();
                }
                return;
            }
            if (EditorGUI.EndChangeCheck() || editor == null)
            {
                editor = Editor.CreateEditor(converter);
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
            var converter = target.gameObject.AddComponent<AvatarConverter>();
            editor = Editor.CreateEditor(converter);
        }
    }
}
