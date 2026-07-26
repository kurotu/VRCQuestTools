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
        private const string DevDebugLogDefaultAppliedKey = "KRT.VRCQuestTools.Debug.DevDebugLogDefaultApplied";

        // Debug logging defaults to on in this dev project (unlike the shipped package, which defaults to off).
        // Only applied once so a later manual toggle via the Settings menu sticks.
        [InitializeOnLoadMethod]
        private static void EnableDebugLogByDefault()
        {
            if (EditorUserSettings.GetConfigValue(DevDebugLogDefaultAppliedKey) == null)
            {
                VRCQuestToolsSettings.IsDebugLogEnabled = true;
                Logger.UseDebug = true;
                EditorUserSettings.SetConfigValue(DevDebugLogDefaultAppliedKey, "TRUE");
            }
        }

        [MenuItem(DebugMenuRoot + "Clear Skipped Version")]
        private static void ClearSkippedVersion()
        {
            VRCQuestToolsSettings.SkippedVersion = new SemVer(0, 0, 0);
        }

        [MenuItem("GameObject/VRCQuestTools/[NDMF] Manual Bake without Cache", false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectNdmfManualBakeWithMobileSettings + 1)]
        private static void ClearCacheThenManualBake()
        {
            CacheManager.Texture.Clear();
            EditorApplication.ExecuteMenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfManualBakeWithMobileSettings);
        }
    }
}
