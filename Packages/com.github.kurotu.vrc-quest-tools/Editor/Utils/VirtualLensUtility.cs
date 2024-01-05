using System;
using System.Reflection;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for VirtualLens.
    /// </summary>
    internal class VirtualLensUtility
    {
        /// <summary>
        /// Type object of VirtualLensSettings.
        /// </summary>
        internal static readonly Type VirtualLensSettingsType = SystemUtility.GetTypeByName(VirtualLensSettingsFullName);

        private const string VirtualLensSettingsFullName = "VirtualLens2.VirtualLensSettings";

        /// <summary>
        /// Enum for RemoteOnlyMode.
        /// </summary>
        internal enum RemoteOnlyMode
        {
#pragma warning disable SA1602
            ForceDisable = 0,
            ForceEnable = 1,
            MobileOnly = 2,
#pragma warning restore SA1602
        }

        /// <summary>
        /// Reflection proxy for VirtualLensSettings.
        /// </summary>
        internal class VirtualLensSettingsProxy
        {
            /// <summary>
            /// Reference to VirtualLensSettings component.
            /// </summary>
            internal readonly Component Component;

            private static readonly FieldInfo RemoteOnlyModeField = VirtualLensSettingsType?.GetField("remoteOnlyMode", BindingFlags.Public | BindingFlags.Instance);

            /// <summary>
            /// Initializes a new instance of the <see cref="VirtualLensSettingsProxy"/> class.
            /// Initialize a new instance of the <see cref="VirtualLensSettingsProxy"/> class.
            /// </summary>
            /// <param name="component">VirtualLensSettings component.</param>
            internal VirtualLensSettingsProxy(Component component)
            {
                this.Component = component;
            }

            /// <summary>
            /// Sets set remoteOnlyMode.
            /// </summary>
#pragma warning disable SA1300 // 命名スタイル
            internal RemoteOnlyMode remoteOnlyMode
#pragma warning restore SA1300 // 命名スタイル
            {
                set
                {
                    if (RemoteOnlyModeField == null)
                    {
                        Debug.LogWarning("VirtualLensSettings.remoteOnlyMode is not found.");
                        return;
                    }
                    RemoteOnlyModeField.SetValue(Component, (int)value);
                }
            }
        }
    }
}
