// <copyright file="RenderedParticleMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents a material that is used by a ParticleSystem but does not use a recognized particle
    /// shader. Its rendered appearance (including transparency) is baked and converted to the
    /// VRChat/Mobile/Particles/Additive shader.
    /// </summary>
    internal class RenderedParticleMaterial : ParticleMaterial
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderedParticleMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal RenderedParticleMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override string DestinationShaderName => "VRChat/Mobile/Particles/Additive";

        /// <summary>
        /// The rendered result already reflects the material's UV tiling, so the output uses identity tiling.
        /// </summary>
        internal override Vector2 MainTextureScale => Vector2.one;

        /// <inheritdoc/>
        internal override Vector2 MainTextureOffset => Vector2.zero;

        /// <summary>
        /// The rendered appearance always needs to be baked; the original texture cannot be reused.
        /// </summary>
        internal override bool ShouldUseOriginalMainTexture => false;

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateParticleImage(int maxTextureSize, System.Action<Texture2D> completion)
        {
            var mainTexture = Material.mainTexture ?? Texture2D.whiteTexture;

            using (var baker = DisposableObject.New(Object.Instantiate(Material)))
            {
                baker.Object.parent = null;

                var width = mainTexture.width;
                var height = mainTexture.height;
                if (maxTextureSize > 0)
                {
                    width = System.Math.Min(maxTextureSize, width);
                    height = System.Math.Min(maxTextureSize, height);
                }

                // Render the material with its own shader so the baked texture reflects the material's
                // appearance including transparency (alpha). The render target is cleared to transparent
                // first so blended shaders do not composite over uninitialized memory. Particle (vertex)
                // color is intentionally not baked; it stays at runtime via the Additive shader's primary.
                return TextureUtility.BakeTexture(mainTexture, true, width, height, true, baker.Object, clearRenderTarget: true, completion);
            }
        }
    }
}
