using KRT.VRCQuestTools.Menus;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Menu for avatar builder.
    /// </summary>
    internal static class AvatarBuilderMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.ShowAvatarBuilder, false, (int)VRCQuestToolsMenus.MenuPriorities.ShowAvatarBuilder)]
        private static void InitFromMenu()
        {
            EditorWindow.GetWindow<AvatarBuilderWindow>().Show();
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfManualBakeWithAndroidSettings, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectNdmfManualBakeWithAndroidSettings)]
        private static void ManualBake()
        {
            NdmfPluginUtility.ManualBakeWithAndroidSettings(Selection.activeGameObject);
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfManualBakeWithAndroidSettings, true)]
        private static bool ManualBakeValidate()
        {
            var target = Selection.activeGameObject;
            if (target == null)
            {
                return false;
            }
            return target.GetComponent<VRCAvatarDescriptor>() != null;
        }
    }
}
