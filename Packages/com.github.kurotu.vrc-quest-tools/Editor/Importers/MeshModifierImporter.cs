// <copyright file="MeshModifierImporter.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace KRT.VRCQuestTools.Importers
{
    /// <summary>
    /// Duplicates and imports a mesh and applies modifications such as removing vertex colors.
    /// </summary>
    [ScriptedImporter(1, Extension)]
    public class MeshModifierImporter : ScriptedImporter
    {
        /// <summary>
        /// The file extension for MeshModifierImporter.
        /// </summary>
        public const string Extension = "vqtmesh";

        /// <summary>
        /// The source mesh to modify during import.
        /// </summary>
        public Mesh source;

        /// <summary>
        /// If true, vertex colors will be removed from the imported mesh.
        /// </summary>
        public bool removeVertexColor;

        /// <summary>
        /// Creates a .vqtmesh asset with specified modifications.
        /// </summary>
        /// <param name="sourceMesh">The source mesh to be modified.</param>
        /// <param name="path">The file path where the .vqtmesh asset will be created.</param>
        /// <param name="removeVertexColor">A boolean indicating if vertex colors should be removed.</param>
        /// <exception cref="System.ArgumentException">Thrown when the provided path does not end with .vqtmesh.</exception>
        public static void CreateAsset(Mesh sourceMesh, string path, bool removeVertexColor)
        {
            if (!path.EndsWith("." + Extension))
            {
                throw new System.ArgumentException($"Path must end with .{Extension} extension.", nameof(path));
            }
            File.Create(path).Dispose();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            var importer = GetAtPath(path) as MeshModifierImporter;
            var serializedImporter = new SerializedObject(importer);
            serializedImporter.FindProperty(nameof(source)).objectReferenceValue = sourceMesh;
            serializedImporter.FindProperty(nameof(removeVertexColor)).boolValue = removeVertexColor;
            serializedImporter.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        /// <inheritdoc/>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (source == null)
            {
                Logger.LogDebug($"[{nameof(MeshModifierImporter)}] No source mesh assigned for import at path: {ctx.assetPath}");
                ctx.AddObjectToAsset("main", new Mesh());
                return;
            }

            Mesh modifiedMesh = Object.Instantiate(source);
            var sourcePath = AssetDatabase.GetAssetPath(source);
            if (!string.IsNullOrEmpty(sourcePath))
            {
                ctx.DependsOnSourceAsset(sourcePath);
            }

            if (removeVertexColor)
            {
                modifiedMesh.colors32 = null;
            }

            ctx.AddObjectToAsset("main", modifiedMesh);
            ctx.SetMainObject(modifiedMesh);
        }
    }
}
