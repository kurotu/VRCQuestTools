// <copyright file="IsolatedEditorSceneTest.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#if UNITY_EDITOR
using System;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KRT.VRCQuestTools.TestUtilities
{
    /// <summary>
    /// Provides a temporary isolated editor scene for tests so they don't modify
    /// the user's currently open scene. Base class SetUp creates an additive
    /// empty scene and makes it active when possible; otherwise it creates a
    /// temporary root GameObject in the active scene. TearDown cleans up the
    /// root and restores the previous active scene.
    /// </summary>
    public class IsolatedEditorSceneTest
    {
        private Scene previousScene;
        private Scene testScene;
        protected GameObject TestRoot { get; private set; }
        private bool createdTempScene;

        [SetUp]
        public void IsolatedSceneSetUp()
        {
            previousScene = EditorSceneManager.GetActiveScene();

            // Try to create an additive editor scene; if the active scene is
            // unsaved or the editor forbids it, fall back to creating a root
            // GameObject in the existing active scene.
            createdTempScene = false;
            try
            {
                testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SetActiveScene(testScene);
                createdTempScene = true;
            }
            catch (InvalidOperationException)
            {
                // Editor prevented creating an additive scene (untitled/unsaved).
                // Fall back to using the current active scene and a temporary root GameObject.
                createdTempScene = false;
            }

            // Create a root GameObject inside the active scene; derived tests
            // should parent their test hierarchy under this root so TearDown can
            // reliably clean it up.
            TestRoot = new GameObject("VQT_TestRoot_" + Guid.NewGuid().ToString("N"));
            if (createdTempScene && testScene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(TestRoot, testScene);
            }
        }

        [TearDown]
        public void IsolatedSceneTearDown()
        {
            if (TestRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(TestRoot);
                TestRoot = null;
            }

            if (createdTempScene && testScene.IsValid())
            {
                // Close and unload the temporary scene (remove it from the hierarchy)
                EditorSceneManager.CloseScene(testScene, true);
            }

            if (previousScene.IsValid())
            {
                EditorSceneManager.SetActiveScene(previousScene);
            }
        }
    }
}
#endif