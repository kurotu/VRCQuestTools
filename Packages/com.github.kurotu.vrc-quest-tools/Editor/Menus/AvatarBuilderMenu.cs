#if !VQT_HAS_NDMF
using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Models;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for avatar builder.
    /// </summary>
    internal static class AvatarBuilderMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.ShowAvatarBuilder, false, (int)VRCQuestToolsMenus.MenuPriorities.ShowAvatarBuilder)]
        private static void InitFromMenu()
        {
            EditorUtility.DisplayDialog(VRCQuestTools.Name, VRCQuestToolsSettings.I18nResource.FeatureRequiresNdmf, "OK");
        }
    }
}
#endif
