using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Menu for NDMF operations.
    /// </summary>
    internal static class AvatarBuilderMenu
    {
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

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfBuildAndTestWithAndroidSettings, false, (int)VRCQuestToolsMenus.GameObjectMenuPriorities.GameObjectNdmfBuildAndTestWithAndroidSettings)]
        private static async void BuildAndTest()
        {
            var avatar = Selection.activeGameObject;
            NdmfSessionState.BuildTarget = BuildTarget.Android;
            try
            {
                if (VRCSdkControlPanel.TryGetBuilder(out var sdkBuilder))
                {
                    await sdkBuilder.BuildAndTest(avatar);
                }
                else
                {
                    Logger.LogError("VRChat SDK Control Panel is not available. Please open it first.");
                    EditorUtility.DisplayDialog(VRCQuestTools.Name, "VRChat SDK Control Panel is not available. Please open it first.", "OK");
                }
            }
            catch (System.Exception e)
            {
                Logger.LogException(e);
                EditorUtility.DisplayDialog(VRCQuestTools.Name, $"Failed to build and test avatar: {e.Message}", "OK");
            }
            finally
            {
                NdmfSessionState.BuildTarget = BuildTarget.Auto;
            }
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfBuildAndTestWithAndroidSettings, true)]
        private static bool BuildAndTestValidate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return false;
            }
            var target = Selection.activeGameObject;
            if (target == null)
            {
                return false;
            }
            if (!target.activeInHierarchy)
            {
                return false;
            }
            return target.GetComponent<VRCAvatarDescriptor>() != null;
        }
    }
}
