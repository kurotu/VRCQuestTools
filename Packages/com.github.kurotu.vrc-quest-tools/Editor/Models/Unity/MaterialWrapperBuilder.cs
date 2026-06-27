// <copyright file="MaterialWrapperBuilder.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// MaterialBase builder.
    /// </summary>
    internal class MaterialWrapperBuilder
    {
        /// <summary>
        /// Shader categories.
        /// </summary>
        internal enum ShaderCategory
        {
#pragma warning disable SA1136 // Enum values should be on separate lines
#pragma warning disable SA1602 // Enumeration items should be documented
            UTS2, Arktoon, Standard, Unlit, Quest, Sunao, AXCS, LilToon, Poiyomi, VirtualLens2, Particle, Unverified,
#pragma warning restore SA1602 // Enumeration items should be documented
#pragma warning restore SA1136 // Enum values should be on separate lines
        }

        /// <summary>
        /// Create an instance of appropriate MaterialBase object by shader category.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <returns>Material wrapper object.</returns>
        internal virtual MaterialBase Build(Material material)
        {
            return Build(material, false);
        }

        /// <summary>
        /// Create an instance of appropriate MaterialBase object by shader category.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <param name="renderForParticleLikeRenderer">Whether the material is used by a particle-like renderer
        /// (ParticleSystemRenderer, TrailRenderer, or LineRenderer). When true and the shader is not a recognized
        /// particle shader, the material's rendered appearance is converted to a particle shader.</param>
        /// <returns>Material wrapper object.</returns>
        internal virtual MaterialBase Build(Material material, bool renderForParticleLikeRenderer)
        {
            var category = DetectShaderCategory(material);
            if (renderForParticleLikeRenderer && category != ShaderCategory.Particle)
            {
                return new RenderedParticleMaterial(material);
            }

            return BuildForCategory(material, category);
        }

        /// <summary>
        /// Creates a wrapper while ignoring particle conversion: recognized particle shaders are treated as
        /// generic materials. Used when a material has an explicit per-material convert setting that must be
        /// honored instead of automatic particle conversion.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <returns>Material wrapper object.</returns>
        internal MaterialBase BuildIgnoringParticleCategory(Material material)
        {
            var category = DetectShaderCategory(material);
            if (category == ShaderCategory.Particle)
            {
                category = ShaderCategory.Unverified;
            }

            return BuildForCategory(material, category);
        }

        private MaterialBase BuildForCategory(Material material, ShaderCategory category)
        {
            switch (category)
            {
                case ShaderCategory.UTS2:
                    return new UTS2Material(material);
                case ShaderCategory.Arktoon:
                case ShaderCategory.AXCS:
                    return new ArktoonMaterial(material);
                case ShaderCategory.Sunao:
                    return new SunaoMaterial(material);
                case ShaderCategory.LilToon:
                    return new LilToonMaterial(material);
                case ShaderCategory.Poiyomi:
                    return new PoiyomiMaterial(material);
                case ShaderCategory.VirtualLens2:
                    return new VirtualLens2Material(material);
                case ShaderCategory.Particle:
                    return new ParticleMaterial(material);
                case ShaderCategory.Standard:
                case ShaderCategory.Unlit:
                case ShaderCategory.Quest:
                case ShaderCategory.Unverified:
                    return new StandardMaterial(material);
                default:
                    throw new System.NotImplementedException($"MaterialWrapperBuilder.Build() not implemented for {typeof(ShaderCategory).Name}.{System.Enum.GetName(typeof(ShaderCategory), category)}");
            }
        }

        /// <summary>
        /// Detects shader category for a material.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <returns>Detected shader category.</returns>
        internal virtual ShaderCategory DetectShaderCategory(Material material)
        {
            // Shader name may be changed for Shader Blocking System (case sensitive). So compare shader name case insensitive.
            // https://niwaka.fanbox.cc/posts/1612078
            var shaderName = material.shader.name.ToLower();
            var shaderPath = AssetDatabase.GetAssetPath(material.shader).ToLower();
            if (shaderName == "Standard".ToLower() || shaderName == "Standard (Specular setup)".ToLower() || shaderName.StartsWith("Standard/".ToLower()))
            {
                return ShaderCategory.Standard;
            }
            if (shaderName.StartsWith("UnityChanToonShader".ToLower()))
            {
                return ShaderCategory.UTS2;
            }
            if (shaderName.StartsWith("arktoon/".ToLower()))
            {
                return ShaderCategory.Arktoon;
            }
            if (shaderName.StartsWith("ArxCharacterShaders/".ToLower()))
            {
                return ShaderCategory.AXCS;
            }
            if (shaderName.StartsWith("Unlit/".ToLower()))
            {
                return ShaderCategory.Unlit;
            }

            // Particle shaders must be detected before VRChat/Mobile/ (Quest) so that
            // "VRChat/Mobile/Particles/*" is captured here instead of being treated as a Quest material.
            if (IsParticleShader(shaderName))
            {
                return ShaderCategory.Particle;
            }
            if (shaderName.StartsWith("VRChat/Mobile/".ToLower()))
            {
                return ShaderCategory.Quest;
            }
            if (shaderName.StartsWith("Sunao Shader/".ToLower()))
            {
                return ShaderCategory.Sunao;
            }
            if (shaderName.Contains("liltoon"))
            {
                return ShaderCategory.LilToon;
            }
            if (shaderPath.EndsWith(".lilcontainer"))
            {
                return ShaderCategory.LilToon;
            }
            if (shaderName.Contains("poiyomi"))
            {
                return ShaderCategory.Poiyomi;
            }
            if (shaderName.StartsWith("VirtualLens2/".ToLower()))
            {
                return ShaderCategory.VirtualLens2;
            }
            return ShaderCategory.Unverified;
        }

        /// <summary>
        /// Determines whether the shader name represents a particle shader.
        /// </summary>
        /// <param name="shaderNameLower">Lower-cased shader name.</param>
        /// <returns>true when the shader is a particle shader.</returns>
        private static bool IsParticleShader(string shaderNameLower)
        {
            // "particles/*"                : Unity built-in and "Particles/Standard Unlit|Surface".
            // "legacy shaders/particles/*" : Legacy particle shaders.
            // "mobile/particles/*"         : Mobile particle shaders.
            // "vrchat/mobile/particles/*"  : VRChat mobile particle shaders (Additive/Multiply/Alpha Blended).
            return shaderNameLower.StartsWith("particles/")
                || shaderNameLower.StartsWith("legacy shaders/particles/")
                || shaderNameLower.StartsWith("mobile/particles/")
                || shaderNameLower.StartsWith("vrchat/mobile/particles/");
        }
    }
}
