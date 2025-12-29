using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3A.Editor;

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
            var i18n = VRCQuestToolsSettings.I18nResource;
            var avatar = Selection.activeGameObject;
            NdmfSessionState.BuildTarget = Models.BuildTarget.Android;
            try
            {
                if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var sdkBuilder))
                {
                    await sdkBuilder.BuildAndTest(avatar);
                    EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.BuildAndTestSucceeded(avatar.name), "OK");
                }
                else
                {
                    Logger.LogError(i18n.BuildAndTestRequiresSdkControlPanel);
                    EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.BuildAndTestRequiresSdkControlPanel, "OK");
                }
            }
            catch (System.Exception e)
            {
                Logger.LogException(e);
                EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.BuildAndTestFailed(e.Message), "OK");
            }
            finally
            {
                NdmfSessionState.BuildTarget = Models.BuildTarget.Auto;
            }
        }

        [MenuItem(VRCQuestToolsMenus.GameObjectMenuPaths.NdmfBuildAndTestWithAndroidSettings, true)]
        private static bool BuildAndTestValidate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return false;
            }
            if (EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
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
