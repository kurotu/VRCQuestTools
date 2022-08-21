using UnityEditor;
using UnityEditor.Build;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for Switch Platform.
    /// </summary>
    [InitializeOnLoad]
    internal static class SwitchPlatformMenu
    {
        static SwitchPlatformMenu()
        {
            UpdateMenuItems();
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.SwitchPlatformToPC, false, (int)VRCQuestToolsMenus.MenuPriorities.SwitchPlatformToPC)]
        private static void SwitchPlatformToPC()
        {
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.SwitchPlatformToPC, true)]
        private static bool ValidatePCMenu()
        {
            return EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64;
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.SwitchPlatformToAndroid, false, (int)VRCQuestToolsMenus.MenuPriorities.SwitchPlatformToAndroid)]
        private static void SwitchPlatformToAndroid()
        {
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.SwitchPlatformToAndroid, true)]
        private static bool ValidateAndroidMenu()
        {
            return EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android;
        }

        private static void UpdateMenuItems()
        {
            var isPC = EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64;
            Menu.SetChecked(VRCQuestToolsMenus.MenuPaths.SwitchPlatformToPC, isPC);
            Menu.SetChecked(VRCQuestToolsMenus.MenuPaths.SwitchPlatformToAndroid, !isPC);
        }
    }
}
