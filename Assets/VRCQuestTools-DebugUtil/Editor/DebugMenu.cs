using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Debug
{
    /// <summary>
    /// Menu for debug.
    /// </summary>
    internal static class DebugMenu
    {
        private const string DebugMenuRoot = VRCQuestToolsMenus.MenuPaths.RootMenu + "Debug/";
        private const string UseDebugKey = "KRT.VRCQuestTools.UseDebug";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Logger.UseDebug = SessionState.GetBool(UseDebugKey, true);
            Menu.SetChecked(DebugMenuRoot + "Use Debug", Logger.UseDebug);
        }

        [MenuItem(DebugMenuRoot + "Use Debug")]
        private static void ToggleDebug()
        {
            Logger.UseDebug = !Logger.UseDebug;
            SessionState.SetBool(UseDebugKey, Logger.UseDebug);
            Menu.SetChecked(DebugMenuRoot + "Use Debug", Logger.UseDebug);
        }

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

        /// <summary>
        /// Menu item to test the preview service initialization.
        /// </summary>
        [MenuItem(DebugMenuRoot + "Test/Initialize Preview Service")]
        internal static void TestInitializePreviewService()
        {
            UnityEngine.Debug.Log("Testing AvatarDynamicsPreviewService initialization...");
            
            try
            {
                AvatarDynamicsPreviewService.Initialize();
                UnityEngine.Debug.Log("✓ Preview service initialized successfully");
                
                AvatarDynamicsPreviewService.ClearPreview();
                UnityEngine.Debug.Log("✓ ClearPreview() works");
                
                AvatarDynamicsPreviewService.Cleanup();
                UnityEngine.Debug.Log("✓ Preview service cleanup successful");
                
                UnityEngine.Debug.Log("🎉 All preview service tests passed!");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Preview service test failed: {e.Message}");
            }
        }

        /// <summary>
        /// Menu item to test opening AvatarDynamicsSelectorWindow.
        /// </summary>
        [MenuItem(DebugMenuRoot + "Test/Open Avatar Dynamics Selector")]
        internal static void TestOpenAvatarDynamicsSelector()
        {
            try
            {
                var window = EditorWindow.GetWindow<AvatarDynamicsSelectorWindow>();
                window.Show();
                UnityEngine.Debug.Log("✓ AvatarDynamicsSelectorWindow opened successfully with preview service integration");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Failed to open AvatarDynamicsSelectorWindow: {e.Message}");
            }
        }

        /// <summary>
        /// Menu item to test opening PhysBonesRemoveWindow.
        /// </summary>
        [MenuItem(DebugMenuRoot + "Test/Open PhysBones Remove Window")]
        internal static void TestOpenPhysBonesRemoveWindow()
        {
            try
            {
                PhysBonesRemoveWindow.ShowWindow();
                UnityEngine.Debug.Log("✓ PhysBonesRemoveWindow opened successfully with preview service integration");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Failed to open PhysBonesRemoveWindow: {e.Message}");
            }
        }
    }
}
