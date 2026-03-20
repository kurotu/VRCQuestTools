// Tests for AssetUtility.CreateAsset<T> method.

using System.IO;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.Utils
{
    [TestFixture]
    internal class AssetUtilityCreateAssetTests
    {
        private const string TestDir = "Assets/VRCQuestTools-Tests/Fixtures/TempAssetTests";

        [SetUp]
        public void SetUp()
        {
            if (!Directory.Exists(TestDir))
            {
                Directory.CreateDirectory(TestDir);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up all test assets
            if (Directory.Exists(TestDir))
            {
                var files = Directory.GetFiles(TestDir);
                foreach (var f in files)
                {
                    AssetDatabase.DeleteAsset(f.Replace("\\", "/"));
                }
            }
            // Also clean up tmp_vqt
            if (Directory.Exists("Assets/tmp_vqt"))
            {
                AssetDatabase.DeleteAsset("Assets/tmp_vqt");
            }
        }

        [Test]
        public void CreateAsset_NewPath_CreatesAsset()
        {
            var mat = new Material(Shader.Find("Standard"));
            var path = $"{TestDir}/TestCreateNew.mat";
            try
            {
                // Ensure path is clean
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }

                var result = AssetUtility.CreateAsset(mat, path);
                Assert.IsNotNull(result);
                Assert.IsTrue(File.Exists(path));
            }
            finally
            {
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        [Test]
        public void CreateAsset_ExistingPath_OverwritesAsset()
        {
            var path = $"{TestDir}/TestOverwrite.mat";
            try
            {
                // Create initial asset
                var mat1 = new Material(Shader.Find("Standard"));
                mat1.color = Color.red;
                AssetDatabase.CreateAsset(mat1, path);
                AssetDatabase.SaveAssets();
                Assert.IsTrue(File.Exists(path));

                // Overwrite with new asset
                var mat2 = new Material(Shader.Find("Standard"));
                mat2.color = Color.blue;
                var result = AssetUtility.CreateAsset(mat2, path);
                Assert.IsNotNull(result);
            }
            finally
            {
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        [Test]
        public void CreateAsset_WithPostCreateAction_InvokesAction()
        {
            var path = $"{TestDir}/TestPostAction.mat";
            var actionCalled = false;
            try
            {
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }

                var mat = new Material(Shader.Find("Standard"));
                var result = AssetUtility.CreateAsset(mat, path, (m) =>
                {
                    actionCalled = true;
                });
                Assert.IsTrue(actionCalled);
                Assert.IsNotNull(result);
            }
            finally
            {
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        [Test]
        public void CreateAsset_OverwriteWithPostAction_InvokesAction()
        {
            var path = $"{TestDir}/TestOverwriteAction.mat";
            var actionCalled = false;
            try
            {
                // Create initial asset first
                var mat1 = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat1, path);
                AssetDatabase.SaveAssets();

                // Overwrite with post action
                var mat2 = new Material(Shader.Find("Standard"));
                var result = AssetUtility.CreateAsset(mat2, path, (m) =>
                {
                    actionCalled = true;
                });
                Assert.IsTrue(actionCalled);
            }
            finally
            {
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }
    }
}
