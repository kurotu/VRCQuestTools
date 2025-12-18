// <copyright file="FallbackAvatar.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Marker component to set avatar as fallback after upload if performance requirements are met.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Fallback Avatar")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/fallback-avatar?lang=auto")]
    [DisallowMultipleComponent]
    public class FallbackAvatar : VRCQuestToolsEditorOnly, IAvatarRootComponent
    {
    }
}
