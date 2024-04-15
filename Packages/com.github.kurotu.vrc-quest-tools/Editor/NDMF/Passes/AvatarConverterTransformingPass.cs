using System.IO;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Convert the avatar with AvatarConverterSettings component in NDMF.
    /// </summary>
    internal class AvatarConverterTransformingPass : Pass<AvatarConverterTransformingPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Convert avatar for Android";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var settings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (settings == null)
            {
                return;
            }

            var assetContainerPath = AssetDatabase.GetAssetPath(context.AssetContainer);
            var assetDirectory = Path.GetDirectoryName(assetContainerPath).Replace('\\', '/') + "/" + Path.GetFileNameWithoutExtension(assetContainerPath) + "_VQT";
            Directory.CreateDirectory(assetDirectory);
            VRCQuestTools.AvatarConverter.ConvertForQuestInPlace(settings, VRCQuestTools.ComponentRemover, false, assetDirectory, new Models.VRChat.AvatarConverter.ProgressCallback()
            {
                onTextureProgress = (_, __, e, material) =>
                {
                    if (e != null)
                    {
                        Debug.LogError("Failed to convert texture for material: " + material.name, material);
                        Debug.LogException(e, material);
                    }
                },
                onAnimationClipProgress = (_, __, e, clip) =>
                {
                    if (e != null)
                    {
                        Debug.LogError("Failed to convert animation clip: " + clip.name, clip);
                        Debug.LogException(e, clip);
                    }
                },
                onRuntimeAnimatorProgress = (_, __, e, controller) =>
                {
                    if (e != null)
                    {
                        Debug.LogError("Failed to convert runtime animator controller: " + controller.name, controller);
                        Debug.LogException(e, controller);
                    }
                },
            });
        }
    }
}
