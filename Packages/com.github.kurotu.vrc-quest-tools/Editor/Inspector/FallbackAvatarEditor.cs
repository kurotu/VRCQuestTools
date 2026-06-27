// <copyright file="FallbackAvatarEditor.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor for FallbackAvatar.
    /// </summary>
    [CustomEditor(typeof(FallbackAvatar))]
    internal class FallbackAvatarEditor : VRCQuestToolsEditorOnlyEditorBase<FallbackAvatar>
    {
        /// <inheritdoc/>
        protected override string Description => VRCQuestToolsSettings.I18nResource.FallbackAvatarEditorDescription;

        /// <inheritdoc/>
        public override void OnInspectorGUIInternal()
        {
            // This is a marker component, so there is no GUI to display.
        }
    }
}
