using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Pass to resize menu icons in expressions menu.
    /// </summary>
    public class MenuIconsResizerPass : Pass<MenuIconsResizerPass>
    {
        private const int MaxActionTextureSize = 256;

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var menu = context.AvatarDescriptor.expressionsMenu;
            if (menu == null)
            {
                return;
            }

            var icons = VRCSDKUtility.GetTexturesFromMenu(menu);
            if (icons.Length == 0)
            {
                return;
            }

            var resizer = context.AvatarRootObject.GetComponentInChildren<MenuIconsResizer>(true);
            var settings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (resizer == null && settings == null)
            {
                return;
            }

            var target = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);

            var maxSize = MaxActionTextureSize;
            if (resizer != null)
            {
                var resizeMode = target == BuildTarget.PC ? resizer.resizeModePC : resizer.resizeModeAndroid;
                if (resizeMode != MenuIconsResizer.TextureResizeMode.DoNotResize)
                {
                    maxSize = (int)resizeMode;
                }
            }

            var compressTextures =
                (resizer != null ? resizer.compressTextures : false)
                || (settings != null ? settings.compressExpressionsMenuIcons : false);

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
                if (compressTextures && AssetUtility.IsUncompressedFormat(texture.format))
                {
                    return true;
                }
                return false;
            }
            if (icons.FirstOrDefault(NeedToProcess) == null)
            {
                return;
            }

            var newMenu = VRCSDKUtility.DuplicateExpressionsMenu(menu);
            context.AvatarDescriptor.expressionsMenu = newMenu;
#if VQT_HAS_NDMF_ERROR_REPORT
            ObjectRegistry.RegisterReplacedObject(menu, newMenu);
#endif

            VRCSDKUtility.ResizeExpressionMenuIcons(newMenu, maxSize, compressTextures, (oldTex, newTex) =>
            {
                AssetUtility.CompressTextureForBuildTarget(newTex, UnityEditor.EditorUserBuildSettings.activeBuildTarget);
#if VQT_HAS_NDMF_ERROR_REPORT
                ObjectRegistry.RegisterReplacedObject(oldTex, newTex);
#endif
            });
        }
    }
}
