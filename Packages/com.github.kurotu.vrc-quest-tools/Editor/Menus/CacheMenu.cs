using KRT.VRCQuestTools.Utils;
using UnityEditor;

namespace KRT.VRCQuestTools.Menus
{
    /// <summary>
    /// Menu for cache.
    /// </summary>
    internal static class CacheMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.ClearTextureCache, false, (int)VRCQuestToolsMenus.MenuPriorities.ClearTextureCache)]
        private static void ClearCache()
        {
            CacheManager.Texture.Clear();
        }
    }
}
