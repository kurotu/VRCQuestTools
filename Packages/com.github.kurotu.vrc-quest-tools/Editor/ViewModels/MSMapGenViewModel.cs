// <copyright file="MSMapGenViewModel.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.ViewModels
{
    /// <summary>
    /// ViewModel for MSMapGenWindow.
    /// </summary>
    [Serializable]
    internal class MSMapGenViewModel
    {
        /// <summary>
        /// Source metallic map.
        /// </summary>
        public Texture2D metallicMap;

        /// <summary>
        /// Source smoothness / roughness map.
        /// </summary>
        public Texture2D smoothnessMap;

        /// <summary>
        /// Should invert smoothness map.
        /// </summary>
        public bool invertSmoothness;

        /// <summary>
        /// Gets a value indicating whether a window shows a generate button.
        /// </summary>
        internal bool DisableGenerateButton => (smoothnessMap == null) && (metallicMap == null);

        /// <summary>
        /// Generate and save MSMap.
        /// </summary>
        /// <param name="destPath">File path to save.</param>
        internal void GenerateMetallicSmoothness(string destPath)
        {
            using (var baker = DisposableObject.New(new Material(Shader.Find("Hidden/VRCQuestTools/MetallicSmoothnes"))))
            using (var metallic = DisposableObject.New(AssetUtility.LoadUncompressedTexture(metallicMap)))
            using (var smoothness = DisposableObject.New(AssetUtility.LoadUncompressedTexture(smoothnessMap)))
            {
                var width = Math.Max(metallic.Object?.width ?? 4, smoothness.Object?.width ?? 4);
                var height = Math.Max(metallic.Object?.height ?? 4, smoothness.Object?.height ?? 4);

                var dstTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                baker.Object.SetTexture("_MetallicMap", metallic.Object);
                baker.Object.SetInt("_InvertMetallic", 0);
                baker.Object.SetTexture("_SmoothnessMap", smoothness.Object);
                baker.Object.SetInt("_InvertSmoothness", invertSmoothness ? 1 : 0);

                // Remember active render texture
                var activeRenderTexture = RenderTexture.active;
                try
                {
                    Graphics.Blit(metallic.Object, dstTexture, baker.Object);
                    var request = AsyncGPUReadback.Request(dstTexture, 0, TextureFormat.RGBA32);
                    Texture2D outTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    using (var data = request.GetData<Color32>())
                    {
                        outTexture.LoadRawTextureData(data);
                        outTexture.Apply();
                    }

                    AssetUtility.SaveUncompressedTexture(destPath, outTexture, false);
                }
                finally
                {
                    RenderTexture.active = activeRenderTexture;
                    RenderTexture.ReleaseTemporary(dstTexture);
                }
            }
        }
    }
}
