using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Pass to resize menu icons in expressions menu.
    /// </summary>
    public class MenuIconResizerPass : Pass<MenuIconResizerPass>
    {
        private const int MaxActionTextureSize = 256;

        /// <inheritdoc/>
        public override string DisplayName => "Resize menu icons";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var avatarDescriptor = context.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null)
            {
                Logger.LogWarning($"No VRCAvatarDescriptor found in the avatar root object. Skipping menu icon resizing.");
                return;
            }

            var menu = avatarDescriptor.expressionsMenu;
            if (menu == null)
            {
                return;
            }

            var icons = VRCSDKUtility.GetTexturesFromMenu(menu);
            if (icons.Length == 0)
            {
                return;
            }

            var resizer = context.AvatarRootObject.GetComponentInChildren<MenuIconResizer>(true);

            var target = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);

            var maxSize = MaxActionTextureSize;
            if (resizer != null)
            {
                var resizeMode = target == BuildTarget.PC ? resizer.resizeModePC : resizer.resizeModeAndroid;
                if (resizeMode != MenuIconResizer.TextureResizeMode.DoNotResize)
                {
                    maxSize = (int)resizeMode;
                }
            }

            var compressTextures =
                (resizer != null ? resizer.compressTextures : false)
                || context.GetState<NdmfState>().compressExpressionsMenuIcons;

            if (maxSize == MaxActionTextureSize && compressTextures == false)
            {
                return;
            }

            bool NeedToProcess(Texture2D texture)
            {
                if (texture.width > maxSize)
                {
                    return true;
                }
                if (texture.height > maxSize)
                {
                    return true;
                }
                if (compressTextures && TextureUtility.IsUncompressedFormat(texture.format))
                {
                    return true;
                }
                return false;
            }
            if (icons.FirstOrDefault(NeedToProcess) == null)
            {
                return;
            }

            var objectRegistry = context.GetState<NdmfObjectRegistry>();

            var newMenu = VRCSDKUtility.DuplicateExpressionsMenu(menu);
            avatarDescriptor.expressionsMenu = newMenu;
            objectRegistry.RegisterReplacedObject(menu, newMenu);

            VRCSDKUtility.ResizeExpressionMenuIcons(newMenu, maxSize, compressTextures, (oldTex, newTex) =>
            {
                TextureUtility.CompressTextureForBuildTarget(newTex, UnityEditor.EditorUserBuildSettings.activeBuildTarget, (TextureFormat)resizer.mobileTextureFormat);
                objectRegistry.RegisterReplacedObject(oldTex, newTex);
            });
        }
    }
}
