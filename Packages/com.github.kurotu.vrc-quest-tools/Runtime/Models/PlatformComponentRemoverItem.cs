using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// PlatformComponentRemoverItem is a class that stores whether the component should be removed depending on the platform.
    /// </summary>
    [Serializable]
    public class PlatformComponentRemoverItem
    {
        /// <summary>
        /// Target component.
        /// </summary>
        public Component component;

        /// <summary>
        /// Whether to remove on PC.
        /// </summary>
        public bool removeOnPC;

        /// <summary>
        /// Whether to remove on Android.
        /// </summary>
        public bool removeOnAndroid;
    }
}
