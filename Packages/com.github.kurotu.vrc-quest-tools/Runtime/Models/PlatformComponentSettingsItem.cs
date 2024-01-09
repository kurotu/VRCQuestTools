using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// PlatformComponentSettingsItem is a class that stores the settings of the component to be enabled depending on the platform.
    /// </summary>
    [Serializable]
    public class PlatformComponentSettingsItem
    {
        /// <summary>
        /// Target component.
        /// </summary>
        public Component component;

        /// <summary>
        /// Whether to enable on PC.
        /// </summary>
        public bool enabledOnPC = true;

        /// <summary>
        /// Whether to enable on Android.
        /// </summary>
        public bool enabledOnAndroid = true;
    }
}
