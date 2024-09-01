#pragma warning disable SA1300 // Elements should should begin with an uppercase letter

using System.Text.RegularExpressions;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

namespace KRT.VRCQuestTools.NonDestructive
{
    /// <summary>
    /// Callback object for VRCSDK build without NDMF.
    /// </summary>
    internal class VRCQuestToolsAvatarProcessor : IVRCSDKPreprocessAvatarCallback
    {
        /// <summary>
        /// Gets execution order for the moment.
        /// </summary>
        ///
        /// -1100: LI(without NDMF) https://github.com/lilxyzw/lilycalInventory/blob/1.0.1/Editor/VRChat/VRChatProcessor.cs
        /// -10000: VRCFury https://github.com/VRCFury/VRCFury/blob/com.vrcfury.vrcfury/1.1014.0/com.vrcfury.vrcfury/Editor/VF/Hooks/VrcPreuploadHook.cs
        public int callbackOrder => -12000;

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

            AssignNetworkIDs(avatarGameObject);

            var removers = avatarGameObject.GetComponentsInChildren<VertexColorRemover>(true);
            foreach (var r in removers)
            {
                r.RemoveVertexColor();
            }

            var components = avatarGameObject.GetComponentsInChildren<VRCQuestToolsEditorOnly>(true);
            foreach (var c in components)
            {
                Object.DestroyImmediate(c);
            }

            return true;
        }

        private static void AssignNetworkIDs(GameObject avatarGameObject)
        {
            var assigner = avatarGameObject.GetComponent<NetworkIDAssigner>();
            if (assigner == null)
            {
                return;
            }
            var descriptor = avatarGameObject.GetComponent<VRC_AvatarDescriptor>();

            VRCSDKUtility.AssignNetworkIdsToPhysBonesByHierarchyHash(descriptor);
        }
    }
}
