// <copyright file="ParticleGenerator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Class for particle material generator.
    /// Converts particle shaders into avatar-compatible VRChat/Mobile/Particles/* (or Toon Lit).
    /// </summary>
    internal class ParticleGenerator : IMaterialGenerator
    {
        private readonly IMaterialConvertSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleGenerator"/> class.
        /// </summary>
        /// <param name="settings">Convert settings selected for the material (texture format/size are reused).</param>
        internal ParticleGenerator(IMaterialConvertSettings settings)
        {
            this.settings = settings;
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateMaterial(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
            var particle = (ParticleMaterial)material;
            var newMaterial = particle.CreateConvertedMaterial();
            if (ShouldGenerateTextures() && !particle.ShouldUseOriginalMainTexture)
            {
                return GenerateParticleTexture(particle, saveTextureAsPng, texturesPath, (texture) =>
                {
                    newMaterial.mainTexture = texture;
                    completion?.Invoke(newMaterial);
                });
            }

            // Reuse the original _MainTex reference (already set by CreateConvertedMaterial).
            return new ResultRequest<UnityEngine.Object>(null, (_) =>
            {
                completion?.Invoke(newMaterial);
            });
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateTextures(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveAsPng, string texturesPath, Action completion)
        {
            var particle = (ParticleMaterial)material;
            if (!ShouldGenerateTextures() || particle.ShouldUseOriginalMainTexture)
            {
                return new ResultRequest<UnityEngine.Object>(null, (_) =>
                {
                    completion?.Invoke();
                });
            }

            return GenerateParticleTexture(particle, saveAsPng, texturesPath, (_) =>
            {
                completion?.Invoke();
            });
        }

        private AsyncCallbackRequest GenerateParticleTexture(ParticleMaterial material, bool saveAsPng, string texturesPath, Action<Texture2D> completion)
        {
            var platformOverride = material.GetToonLitPlatformOverride();
            var maxTextureSize = GetMaxTextureSize(platformOverride);
            // Use a dedicated texture type so the cache key does not collide with the "main" texture of a
            // normal Toon Lit/Toon Standard conversion of the same material (the cache key is otherwise
            // identical because ParticleGenerator reuses the selected convert settings type).
            return MaterialGeneratorUtility.GenerateTexture(
                material.Material,
                settings,
                "particle",
                saveAsPng,
                texturesPath,
                (compl) => material.GenerateParticleImage(maxTextureSize, compl),
                completion,
                platformOverride);
        }

        private bool ShouldGenerateTextures()
        {
            switch (settings)
            {
                case IToonLitConvertSettings toonLit:
                    return toonLit.GenerateQuestTextures;
                case ToonStandardConvertSettings toonStandard:
                    return toonStandard.generateQuestTextures;
                default:
                    return true;
            }
        }

        private int GetMaxTextureSize((int MaxTextureSize, TextureFormat Format)? platformOverride)
        {
            if (platformOverride.HasValue)
            {
                return platformOverride.Value.MaxTextureSize;
            }
            switch (settings)
            {
                case IToonLitConvertSettings toonLit:
                    return (int)toonLit.MaxTextureSize;
                case ToonStandardConvertSettings toonStandard:
                    return (int)toonStandard.maxTextureSize;
                default:
                    return (int)TextureSizeLimit.Max1024x1024;
            }
        }
    }
}
