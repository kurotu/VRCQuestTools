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
        public int callbackOrder => 1000;

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

            var components = avatarGameObject.GetComponentsInChildren<VertexColorRemover>(true);
            foreach (var c in components)
            {
                c.RemoveVertexColor();
                Object.DestroyImmediate(c);
            }
            return true;
        }
    }
}

#endif
