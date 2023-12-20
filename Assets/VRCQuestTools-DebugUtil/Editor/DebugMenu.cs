using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Models;
using UnityEditor;

namespace KRT.VRCQuestTools.Debug
{
    /// <summary>
    /// Menu for debug.
    /// </summary>
    internal static class DebugMenu
    {
        private const string DebugMenuRoot = VRCQuestToolsMenus.MenuPaths.RootMenu + "Debug/";

        [MenuItem(DebugMenuRoot + "Clear Skipped Version")]
        private static void ClearSkippedVersion()
        {
            VRCQuestToolsSettings.SkippedVersion = new SemVer(0, 0, 0);
        }
    }
}
