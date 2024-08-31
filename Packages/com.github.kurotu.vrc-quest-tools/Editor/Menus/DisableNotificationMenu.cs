using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    internal static class DisableNotificationMenu
    {

        private const string PrefsDisableNotification = "VRCQuestTools.DisableNotification";

        internal static bool DisableNotificationState
        {
            get { return EditorPrefs.GetBool(PrefsDisableNotification, false); }
            set { EditorPrefs.SetBool(PrefsDisableNotification, value); }
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.DisableNotification, false, (int)VRCQuestToolsMenus.MenuPriorities.DisableNotification)]
        private static void DisableNotification()
        {
            DisableNotificationState = !DisableNotificationState;
        }

        [MenuItem(VRCQuestToolsMenus.MenuPaths.DisableNotification, true)]
        private static bool DisableNotificationValidate()
        {
            Menu.SetChecked(VRCQuestToolsMenus.MenuPaths.DisableNotification, DisableNotificationState);
            return true;
        }
    }
}
