// <copyright file="MeshModifierImporter.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace KRT.VRCQuestTools.Importers
{
    /// <summary>
    /// Duplicates and imports a mesh and applies modifications such as removing vertex colors.
    /// </summary>
    [ScriptedImporter(1, "vqtmesh")]
    public class MeshModifierImporter : ScriptedImporter
    {
        /// <summary>
        /// The source mesh to modify during import.
        /// </summary>
        public Mesh source;

        /// <summary>
        /// If true, vertex colors will be removed from the imported mesh.
        /// </summary>
        public bool removeVertexColor;

        /// <inheritdoc/>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (source == null)
            {
                Logger.Log($"[{nameof(MeshModifierImporter)}] No source mesh assigned for import at path: {ctx.assetPath}");
                ctx.AddObjectToAsset("main", new Mesh());
                return;
            }

            Mesh modifiedMesh = Object.Instantiate(source);
            modifiedMesh.name = source.name;
            var sourcePath = AssetDatabase.GetAssetPath(source);
            if (!string.IsNullOrEmpty(sourcePath))
            {
                ctx.DependsOnSourceAsset(sourcePath);
            }

            if (removeVertexColor)
            {
                modifiedMesh.colors = null;
                modifiedMesh.colors32 = null;
            }

            ctx.AddObjectToAsset("main", modifiedMesh);
            ctx.SetMainObject(modifiedMesh);
        }
    }
}
