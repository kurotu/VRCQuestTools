using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace KRTQuestTools
{
    public static class MaterialConverter
    {
        enum ShaderCategory
        {
            Generic, // Standard, Arktoon
            QuestAvatar
        }

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

        public static Texture GetEmissionMap(Material material)
        {
            switch (IdentifyShader(material.shader))
            {
                case ShaderCategory.Generic:
                    return material.GetTexture("_EmissionMap");
                case ShaderCategory.QuestAvatar:
                    return null;
                default:
                    return null;
            }
        }

        public static Color GetEmissionColor(Material material)
        {
            return material.GetColor("_EmissionColor");
        }

        static ShaderCategory IdentifyShader(Shader shader)
        {
            if (shader.name.StartsWith("VRChat/Mobile/"))
            {
                return ShaderCategory.QuestAvatar;
            }
            return ShaderCategory.Generic;
        }
    }
}
