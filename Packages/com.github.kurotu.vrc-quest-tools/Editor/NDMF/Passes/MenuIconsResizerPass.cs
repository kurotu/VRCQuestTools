using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Pass to resize menu icons in expressions menu.
    /// </summary>
    public class MenuIconsResizerPass : Pass<MenuIconsResizerPass>
    {
        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var menu = context.AvatarDescriptor.expressionsMenu;
            if (menu == null)
            {
                return;
            }

            var resizer = context.AvatarRootObject.GetComponentInChildren<MenuIconsResizer>(true);
            if (resizer == null)
            {
                return;
            }

            var target = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            if (target == BuildTarget.PC && resizer.resizeModePC == MenuIconsResizer.TextureResizeMode.Keep)
            {
                return;
            }
            if (target == BuildTarget.Android && resizer.resizeModeAndroid == MenuIconsResizer.TextureResizeMode.Keep)
            {
                return;
            }

            var maxSize = target == BuildTarget.PC ? (int)resizer.resizeModePC : (int)resizer.resizeModeAndroid;

            var newMenu = VRCSDKUtility.DuplicateExpressionsMenu(menu);
            context.AvatarDescriptor.expressionsMenu = newMenu;
#if VQT_HAS_NDMF_ERROR_REPORT
            ObjectRegistry.RegisterReplacedObject(menu, newMenu);
#endif

            VRCSDKUtility.ResizeExpressionMenuIcons(newMenu, maxSize, (oldTex, newTex) =>
            {
                AssetUtility.CompressTextureForBuildTarget(newTex, UnityEditor.EditorUserBuildSettings.activeBuildTarget);
#if VQT_HAS_NDMF_ERROR_REPORT
                ObjectRegistry.RegisterReplacedObject(oldTex, newTex);
#endif
            });
        }
    }
}
