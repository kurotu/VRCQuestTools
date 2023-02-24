#pragma warning disable SA1300 // Elements should should begin with an uppercase letter

#if VRC_SDK_VRCSDK3

using KRT.VRCQuestTools.Components;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Callback object for VRCSDK build.
    /// </summary>
    internal class AvatarProcessor : IVRCSDKPreprocessAvatarCallback
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
