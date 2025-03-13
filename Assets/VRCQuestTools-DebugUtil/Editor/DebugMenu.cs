using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
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

        [MenuItem("GameObject/VRCQuestTools/[NDMF] Manual Bake without Cache", false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectNdmfManualBakeWithAndroidSettings + 1)]
        private static void ClearCacheThenManualBake()
        {
            CacheManager.Texture.Clear();
            EditorApplication.ExecuteMenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfManualBakeWithAndroidSettings);
        }
    }
}
