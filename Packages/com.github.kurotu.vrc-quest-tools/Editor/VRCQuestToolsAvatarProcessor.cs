#pragma warning disable SA1300 // Elements should should begin with an uppercase letter

#if VQT_HAS_VRCSDK_BASE

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Callback object for VRCSDK build.
    /// </summary>
    internal class VRCQuestToolsAvatarProcessor : IVRCSDKPreprocessAvatarCallback
    {
        /// <summary>
        /// Gets execution order for the moment.
        /// </summary>
        public int callbackOrder => -2000; // AAO: -1025 https://vpm.anatawa12.com/avatar-optimizer/ja/docs/developers/make-your-components-compatible-with-aao/#remove-component

        /// <summary>
        /// Execute before build an avatar.
        /// </summary>
        /// <param name="avatarGameObject">Avatar to be built.</param>
        /// <returns>true.</returns>
        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            if (VRCSDKUtility.CountMissingComponentsInChildren(avatarGameObject, true) > 0)
            {
                var i18n = VRCQuestToolsSettings.I18nResource;
                var originalName = Regex.Replace(avatarGameObject.name, @"\(Clone\)$", string.Empty);
                var result = EditorUtility.DisplayDialog("VRCQuestTools", i18n.MissingRemoverOnBuildDialogMessage(originalName), i18n.YesLabel, i18n.AbortLabel);
                if (!result)
                {
                    return false;
                }
                VRCSDKUtility.RemoveMissingComponentsInChildren(avatarGameObject, true);
            }

            var removers = avatarGameObject.GetComponentsInChildren<VertexColorRemover>(true);
            foreach (var r in removers)
            {
                r.RemoveVertexColor();
            }

#if !VQT_HAS_NDMF
            var components = avatarGameObject.GetComponentsInChildren<VRCQuestToolsEditorOnly>(true);
            foreach (var c in components)
            {
                Object.DestroyImmediate(c);
            }
#endif

            return true;
        }
    }
}

#endif
