// <copyright file="MeshModifierImporterTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Importers
{
    /// <summary>
    /// Tests for MeshModifierImporter.
    /// </summary>
    public class MeshModifierImporterTests
    {
        private const string TempRoot = "Assets/VRCQuestTools-Tests/Temp";

        /// <summary>
        /// Imports mesh without removing vertex colors.
        /// </summary>
        [Test]
        public void ImportsMeshWithoutRemovingColors()
        {
            var tempFolder = CreateTempFolder();
            try
            {
                var sourcePath = CreateSourceMeshAsset(tempFolder);
                var sourceMesh = AssetDatabase.LoadAssetAtPath<Mesh>(sourcePath);
                var importedMesh = ImportMesh(tempFolder, sourceMesh, false);

                Assert.NotNull(importedMesh);
                Assert.AreEqual(sourceMesh.vertexCount, importedMesh.vertexCount);
                Assert.AreNotSame(sourceMesh, importedMesh);
                CollectionAssert.AreEqual(sourceMesh.colors, importedMesh.colors);
                CollectionAssert.AreEqual(sourceMesh.colors32, importedMesh.colors32);
            }
            finally
            {
                AssetDatabase.DeleteAsset(tempFolder);
            }
        }

        /// <summary>
        /// Imports mesh and removes vertex colors when enabled.
        /// </summary>
        [Test]
        public void RemovesVertexColorsWhenEnabled()
        {
            var tempFolder = CreateTempFolder();
            try
            {
                var sourcePath = CreateSourceMeshAsset(tempFolder);
                var sourceMesh = AssetDatabase.LoadAssetAtPath<Mesh>(sourcePath);
                Assert.Greater(sourceMesh.colors.Length, 0);
                Assert.Greater(sourceMesh.colors32.Length, 0);

                var importedMesh = ImportMesh(tempFolder, sourceMesh, true);

                Assert.NotNull(importedMesh);
                Assert.Zero(importedMesh.colors.Length);
                Assert.Zero(importedMesh.colors32.Length);
            }
            finally
            {
                AssetDatabase.DeleteAsset(tempFolder);
            }
        }

        /// <summary>
        /// Imports empty mesh when source is missing.
        /// </summary>
        [Test]
        public void UsesEmptyMeshWhenSourceMissing()
        {
            var tempFolder = CreateTempFolder();
            try
            {
                var importerPath = CreateImporterAsset(tempFolder);
                var importer = AssetImporter.GetAtPath(importerPath) as MeshModifierImporter;
                Assert.NotNull(importer);

                LogAssert.Expect(LogType.Log, new Regex($"\\[{nameof(MeshModifierImporter)}\\].*No source mesh assigned.*{Regex.Escape(importerPath)}"));
                importer.SaveAndReimport();

                var importedMesh = LoadImportedMesh(importerPath);
                Assert.NotNull(importedMesh);
                Assert.Zero(importedMesh.vertexCount);
            }
            finally
            {
                AssetDatabase.DeleteAsset(tempFolder);
            }
        }

        private static string CreateTempFolder()
        {
            var folder = $"{TempRoot}/{Guid.NewGuid():N}";
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            return folder;
        }

        private static string CreateSourceMeshAsset(string folder)
        {
            var mesh = CreateMeshWithVertexColors();
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, "sourceMesh.asset").Replace("\\", "/"));
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.ImportAsset(path);
            return path;
        }

        private static string CreateImporterAsset(string folder)
        {
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, "meshToModify.vqtmesh").Replace("\\", "/"));
            File.WriteAllText(path, string.Empty);
            AssetDatabase.ImportAsset(path);
            return path;
        }

        private static Mesh ImportMesh(string folder, Mesh sourceMesh, bool removeVertexColor)
        {
            var importerPath = CreateImporterAsset(folder);
            var importer = AssetImporter.GetAtPath(importerPath) as MeshModifierImporter;
            Assert.NotNull(importer);

            // Use SerializedObject to ensure importer settings are persisted before reimport.
            var serializedImporter = new SerializedObject(importer);
            serializedImporter.FindProperty("source").objectReferenceValue = sourceMesh;
            serializedImporter.FindProperty("removeVertexColor").boolValue = removeVertexColor;
            serializedImporter.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();

            return LoadImportedMesh(importerPath);
        }

        private static Mesh LoadImportedMesh(string importerPath)
        {
            var main = AssetDatabase.LoadAssetAtPath<Mesh>(importerPath);
            if (main != null)
            {
                return main;
            }

            return AssetDatabase.LoadAllAssetsAtPath(importerPath).OfType<Mesh>().FirstOrDefault();
        }

        private static Mesh CreateMeshWithVertexColors()
        {
            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
            };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.colors = new[]
            {
                Color.red,
                Color.green,
                Color.blue,
            };
            mesh.colors32 = new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(0, 255, 0, 255),
                new Color32(0, 0, 255, 255),
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
