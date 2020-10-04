using System.Linq;
using UnityEngine;

namespace KRTQuestTools
{
    public static class MaterialConverter
    {
        public static Material Convert(Material material, Shader newShader)
        {
            if (IsUsableForQuestAvatar(material))
            {
                return new Material(material);
            }
            return new Material(material)
            {
                shader = newShader,
                shaderKeywords = null
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
