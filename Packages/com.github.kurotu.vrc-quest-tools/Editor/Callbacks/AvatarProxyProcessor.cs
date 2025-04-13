using System.Linq;
using KRT.VRCQuestTools.Components;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;

namespace KRT.VRCQuestTools.Callbacks
{
    class AvatarProxyProcessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => int.MinValue; // Process as soon as possible.

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            // Check avatar proxy.
            var avatarProxy = avatarGameObject.GetComponent<AvatarProxy>();
            if (avatarProxy == null)
            {
                return true;
            }

            // Resolve build target.
            var platformTarget = avatarGameObject.GetComponent<PlatformTargetSettings>();
            bool isMobile;
            if (platformTarget == null || platformTarget.buildTarget == Models.BuildTarget.Auto)
            {
                isMobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
            }
            else
            {
                if (platformTarget.buildTarget == Models.BuildTarget.Android)
                {
                    isMobile = true;
                }
                else
                {
                    isMobile = false;
                }
            }

            if (!isMobile)
            {
                return true;
            }

            // Check avatar proxy settings.
            if (avatarProxy.mobileAvatar == null)
            {
                Debug.LogError($"AvatarProxy {avatarProxy.name} does not have a proxy avatar. Please add one to the avatar proxy.", avatarProxy);
                return false;
            }
            if (avatarProxy.mobileAvatar.GetType() != typeof(VRCAvatarDescriptor))
            {
                Debug.LogError($"AvatarProxy {avatarProxy.name} has a different type of avatar descriptor than the root avatar. Please use the same type of avatar descriptor.", avatarProxy);
                return false;
            }

            // Create proxy.
            var proxy = Object.Instantiate(avatarProxy.mobileAvatar.gameObject);
            proxy.hideFlags = HideFlags.HideAndDontSave;
            proxy.transform.position = avatarGameObject.transform.position;
            proxy.transform.rotation = avatarGameObject.transform.rotation;

            try
            {
                // Remove original objects.
                while (avatarGameObject.transform.childCount > 0)
                {
                    var child = avatarGameObject.transform.GetChild(0);
                    Object.DestroyImmediate(child.gameObject);
                }
                var originalComponents = avatarGameObject.GetComponents<Component>()
                    .Where(c => c is not Transform)
                    .Where(c => c is not VRCAvatarDescriptor)
                    .Where(c => c is not PipelineManager)
                    .Where(c => c is not Animator)
                    .Where(c => c is not VRCTestMarker);
                foreach (var component in originalComponents)
                {
                    Object.DestroyImmediate(component);
                }

                // Move proxy children to avatar.
                while (proxy.transform.childCount > 0)
                {
                    var child = proxy.transform.GetChild(0);
                    child.SetParent(avatarGameObject.transform);
                }

                // Copy proxy components to avatar.
                var proxyComponents = proxy.GetComponents<Component>()
                        .Where(c => c is not Transform)
                        .Where(c => c is not VRCAvatarDescriptor)
                        .Where(c => c is not PipelineManager)
                        .Where(c => c is not Animator)
                        .Where(c => c is not AvatarProxy);
                foreach (var component in proxyComponents)
                {
                    var c = avatarGameObject.AddComponent(component.GetType());
                    EditorUtility.CopySerialized(component, c);
                }
                EditorUtility.CopySerialized(proxy.GetComponent<VRCAvatarDescriptor>(), avatarGameObject.GetComponent<VRCAvatarDescriptor>());
                EditorUtility.CopySerialized(proxy.GetComponent<Animator>(), avatarGameObject.GetComponent<Animator>());
                return true;
            }
            finally
            {
                Object.DestroyImmediate(proxy);
            }
        }
    }
}
