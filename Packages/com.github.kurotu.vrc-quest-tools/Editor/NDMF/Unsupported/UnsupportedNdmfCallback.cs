#pragma warning disable SA1300 // Elements should should begin with an uppercase letter

using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Callback object for unsupported NDMF.
    /// </summary>
    internal class UnsupportedNdmfCallback : IVRCSDKPreprocessAvatarCallback
    {
#if VQT_NDMF_LEGACY
        private const string RequiredNdmfVersion = "1.5.0";
#endif
#if VQT_NDMF_BREAKING
        private const string BreakingNdmfVersion = "2.0.0";
#endif

        /// <summary>
        /// Gets execution order to process before NDMF.
        /// </summary>
        public int callbackOrder => int.MinValue;

        /// <summary>
        /// Execute before build an avatar.
        /// </summary>
        /// <param name="avatarGameObject">Avatar to be built.</param>
        /// <returns>true.</returns>
        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
#if VQT_NDMF_LEGACY
            var message = i18n.UnsupportedNdmfCallbackMessageLegacy(RequiredNdmfVersion);
#elif VQT_NDMF_BREAKING
            var message = i18n.UnsupportedNdmfCallbackMessageBreaking(BreakingNdmfVersion);
#endif
            Debug.LogError($"[{VRCQuestTools.Name}] " + message);
            EditorUtility.DisplayDialog(VRCQuestTools.Name, message, "OK");
            return false;
        }
    }
}
