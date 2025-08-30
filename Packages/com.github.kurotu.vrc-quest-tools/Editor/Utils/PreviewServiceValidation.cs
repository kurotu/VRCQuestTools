// <copyright file="PreviewServiceValidation.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Views;

namespace KRT.VRCQuestTools.Validation
{
    /// <summary>
    /// Validation script to test PhysBone preview functionality.
    /// </summary>
    internal static class PreviewServiceValidation
    {
        /// <summary>
        /// Menu item to test the preview service initialization.
        /// </summary>
        [MenuItem("VRCQuestTools/Test/Initialize Preview Service")]
        internal static void TestInitializePreviewService()
        {
            Debug.Log("Testing AvatarDynamicsPreviewService initialization...");
            
            try
            {
                AvatarDynamicsPreviewService.Initialize();
                Debug.Log("✓ Preview service initialized successfully");
                
                AvatarDynamicsPreviewService.SetPreviewComponent(null);
                Debug.Log("✓ SetPreviewComponent(null) works");
                
                AvatarDynamicsPreviewService.Cleanup();
                Debug.Log("✓ Preview service cleanup successful");
                
                Debug.Log("🎉 All preview service tests passed!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Preview service test failed: {e.Message}");
            }
        }

        /// <summary>
        /// Menu item to test opening AvatarDynamicsSelectorWindow.
        /// </summary>
        [MenuItem("VRCQuestTools/Test/Open Avatar Dynamics Selector")]
        internal static void TestOpenAvatarDynamicsSelector()
        {
            try
            {
                var window = EditorWindow.GetWindow<AvatarDynamicsSelectorWindow>();
                window.Show();
                Debug.Log("✓ AvatarDynamicsSelectorWindow opened successfully with preview service integration");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to open AvatarDynamicsSelectorWindow: {e.Message}");
            }
        }

        /// <summary>
        /// Menu item to test opening PhysBonesRemoveWindow.
        /// </summary>
        [MenuItem("VRCQuestTools/Test/Open PhysBones Remove Window")]
        internal static void TestOpenPhysBonesRemoveWindow()
        {
            try
            {
                PhysBonesRemoveWindow.ShowWindow();
                Debug.Log("✓ PhysBonesRemoveWindow opened successfully with preview service integration");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to open PhysBonesRemoveWindow: {e.Message}");
            }
        }
    }
}
#endif