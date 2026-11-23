// <copyright file="VRCQuestToolsEditorOnlyEditorBase.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Base class for editor of VRCQuestToolsEditorOnly components.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    public abstract class VRCQuestToolsEditorOnlyEditorBase<T> : Editor
        where T : VRCQuestToolsEditorOnly
    {
        /// <summary>
        /// Gets target object as T.
        /// </summary>
        protected T TargetComponent => (T)target;

        /// <summary>
        /// Gets description of the component.
        /// This will be displayed at the top of the inspector.
        /// </summary>
        protected abstract string Description { get; }

        /// <inheritdoc />
        public sealed override void OnInspectorGUI()
        {
#if VQT_VRCSDK_AVATARS_NO_RESET_GUI_STATE_BUG
            try
            {
                DrawInspectorGUI();
            }
            finally
            {
                // Workaround for VRChat SDK 3.10.6+ bug: VRC Avatar Descriptor inspector becomes partially unclickable
                // when an inspector of a component below it changes EditorGUIUtility.labelWidth.
                // https://feedback.vrchat.com/sdk-bug-reports/p/3106-use-all-tracking-keys-toggle-may-be-unclickable-and-the-left-half-of-field
                // https://github.com/anatawa12/AvatarOptimizer/pull/1816
                UnityEditor.EditorGUIUtility.labelWidth = 0;
            }
#else
            DrawInspectorGUI();
#endif
        }

        /// <summary>
        /// Called after header GUI is drawed in OnInspectorGUI.
        /// </summary>
        public abstract void OnInspectorGUIInternal();

        private void DrawInspectorGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            Views.EditorGUIUtility.LanguageSelector();
            Views.EditorGUIUtility.UpdateNotificationPanel();

            Views.EditorGUIUtility.HorizontalDivider(2);

            if (target is IExperimentalComponent)
            {
                EditorGUILayout.HelpBox(i18n.ExperimentalComponentWarning, MessageType.Warning);
                Views.EditorGUIUtility.HorizontalDivider(2);
            }

            var description = Description;
            if (target is INdmfComponent)
            {
                description = "[NDMF] " + description;
            }
            else if (target is AvatarConverterSettings)
            {
#if VQT_HAS_NDMF
                description = "[NDMF] " + description;
#endif
            }
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);

#if !VQT_HAS_NDMF
            if (target is IManualConversionComponent)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(i18n.ComponentRequiresNdmfForBuild, MessageType.Info);
            }
            else if (target is INdmfComponent)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(i18n.ComponentRequiresNdmf, MessageType.Warning);
            }
#endif

            if (target is IAvatarRootComponent && !VRCSDKUtility.IsAvatarRoot(TargetComponent.gameObject))
            {
                EditorGUILayout.HelpBox(i18n.AvatarRootComponentMustBeOnAvatarRoot, MessageType.Error);
                return;
            }

#if VQT_HAS_NDMF
            if ((target is IPlatformDependentComponent || target is AvatarConverterSettings) && !ComponentUtility.AvatarHasPlatformTargetSettings(TargetComponent.gameObject))
            {
                EditorGUILayout.HelpBox(i18n.PlatformTargetSettingsIsRequiredToEnforcePlatform, MessageType.Info);
            }
#endif

            EditorGUILayout.Space();

            OnInspectorGUIInternal();
        }
    }
}
