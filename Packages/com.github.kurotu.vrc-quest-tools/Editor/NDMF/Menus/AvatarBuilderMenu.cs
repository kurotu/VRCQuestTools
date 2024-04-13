using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Views;
using UnityEditor;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Menu for avatar builder.
    /// </summary>
    internal static class AvatarBuilderMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.RootMenu + "Show Avatar Builder for Android", false, (int)VRCQuestToolsMenus.MenuPriorities.ConvertAvatarForQuest + 1)]
        private static void InitFromMenu()
        {
            EditorWindow.GetWindow<AvatarBuilderWindow>().Show();
        }
    }
}
