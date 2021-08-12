// <copyright file="MaterialConverter.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    public static class MaterialConverter
    {
        public static Material Convert(Material material, Shader newShader)
        {
            if (IsUsableForQuestAvatar(material))
            {
                return new Material(material);
            }
            return new Material(newShader)
            {
                color = material.color,
                doubleSidedGI = material.doubleSidedGI,
                enableInstancing = true, // https://docs.vrchat.com/docs/quest-content-optimization#avatars-and-worlds
                globalIlluminationFlags = material.globalIlluminationFlags,
                hideFlags = material.hideFlags,
                mainTexture = material.mainTexture,
                mainTextureOffset = material.mainTextureOffset,
                mainTextureScale = material.mainTextureScale,
                name = $"{material.name}_{newShader.name.Split('/').Last()}",
                renderQueue = material.renderQueue,
                shader = newShader,
                shaderKeywords = null,
            };
        }

        private static bool IsUsableForQuestAvatar(Material material)
        {
            var usableShaders = new string[] {
                "Standard Lite", "Bumped Diffuse", "Bumped Mapped Specular", "Diffuse",
                "MatCap Lit", "Toon Lit", "Particles/Additive", "Particles/Multiply"
            }.Select(s => $"VRChat/Mobile/{s}");
            return usableShaders.Contains(material.shader.name);
        }
    }
}
