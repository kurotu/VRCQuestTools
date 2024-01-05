using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Editor state for AvatarConverterSettingsEditor.
    /// </summary>
    internal class AvatarConverterSettingsEditorState : ScriptableSingleton<AvatarConverterSettingsEditorState>
    {
        /// <summary>
        /// Whether to show the foldout for material settings.
        /// </summary>
        [SerializeField]
        internal bool foldOutMaterialSettings = false;

        /// <summary>
        /// Whether to show the foldout for additional material settings.
        /// </summary>
        [SerializeField]
        internal bool foldOutAdditionalMaterialSettings = false;

        /// <summary>
        /// Whether to show the foldout for avatar dynamics.
        /// </summary>
        [SerializeField]
        internal bool foldOutAvatarDynamics = false;

        /// <summary>
        /// Whether to show the foldout for estimated performance.
        /// </summary>
        [SerializeField]
        internal bool foldOutEstimatedPerf = false;

        /// <summary>
        /// Whether to show the foldout for advanced settings.
        /// </summary>
        [SerializeField]
        internal bool foldOutAdvancedSettings = false;
    }
}
