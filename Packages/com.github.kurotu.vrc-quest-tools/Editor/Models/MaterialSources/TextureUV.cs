// <copyright file="TextureUV.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Coordinate which a source shader transforms with a texture's tiling and offset.
    /// </summary>
    internal enum TextureUVBasis
    {
        /// <summary>
        /// Mesh UV0: <c>uv0 * scale + offset</c>.
        /// </summary>
        MeshUV0,

        /// <summary>
        /// Main texture UV, which already has the main texture's tiling and offset: <c>(uv0 * mainScale + mainOffset) * scale + offset</c>.
        /// lilToon samples most secondary textures this way.
        /// </summary>
        MainUV,
    }

    /// <summary>
    /// How a source shader samples a texture.
    /// </summary>
    internal readonly struct TextureUV
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextureUV"/> struct.
        /// </summary>
        /// <param name="basis">Coordinate which the tiling and offset apply to.</param>
        /// <param name="scale">Tiling.</param>
        /// <param name="offset">Offset.</param>
        internal TextureUV(TextureUVBasis basis, Vector2 scale, Vector2 offset)
        {
            Basis = basis;
            Scale = scale;
            Offset = offset;
        }

        /// <summary>
        /// Gets mesh UV0 without tiling and offset.
        /// </summary>
        internal static TextureUV MeshUV0Identity => new TextureUV(TextureUVBasis.MeshUV0, Vector2.one, Vector2.zero);

        /// <summary>
        /// Gets main texture UV without additional tiling and offset.
        /// </summary>
        internal static TextureUV MainUVIdentity => new TextureUV(TextureUVBasis.MainUV, Vector2.one, Vector2.zero);

        /// <summary>
        /// Gets the coordinate which the tiling and offset apply to.
        /// </summary>
        internal TextureUVBasis Basis { get; }

        /// <summary>
        /// Gets the tiling.
        /// </summary>
        internal Vector2 Scale { get; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        internal Vector2 Offset { get; }

        /// <summary>
        /// Gets the tiling and offset which give the same sampling when applied to mesh UV0,
        /// for targets which transform every texture from mesh UV0.
        /// </summary>
        /// <param name="mainST">Tiling and offset of the main texture.</param>
        /// <returns>Tiling and offset on mesh UV0.</returns>
        internal (Vector2 Scale, Vector2 Offset) ToMeshUV0Tiling((Vector2 Scale, Vector2 Offset) mainST)
        {
            if (Basis == TextureUVBasis.MeshUV0)
            {
                return (Scale, Offset);
            }

            // (uv0 * mainScale + mainOffset) * scale + offset
            return (Vector2.Scale(mainST.Scale, Scale), Vector2.Scale(mainST.Offset, Scale) + Offset);
        }

        /// <summary>
        /// Computes the transform which maps a coordinate in the main texture's space (mesh UV0 with the main tiling and offset applied)
        /// to this texture's coordinate.
        /// </summary>
        /// <param name="mainST">Tiling and offset of the main texture.</param>
        /// <returns>Scale and offset, or null when no remap applies: the coordinates are identical, or the main tiling is zero and
        /// mesh UV0 cannot be recovered from the main texture's space.</returns>
        internal (Vector2 Scale, Vector2 Offset)? GetRemapFromMainTextureSpace((Vector2 Scale, Vector2 Offset) mainST)
        {
            if (Basis == TextureUVBasis.MainUV)
            {
                if (Scale == Vector2.one && Offset == Vector2.zero)
                {
                    return null;
                }
                return (Scale, Offset);
            }

            if (Scale == mainST.Scale && Offset == mainST.Offset)
            {
                return null;
            }
            if (Mathf.Approximately(mainST.Scale.x, 0.0f) || Mathf.Approximately(mainST.Scale.y, 0.0f))
            {
                return null;
            }

            // uv0 = (t - mainOffset) / mainScale, then uv0 * scale + offset.
            var scale = new Vector2(Scale.x / mainST.Scale.x, Scale.y / mainST.Scale.y);
            var offset = Offset - Vector2.Scale(mainST.Offset, scale);
            return (scale, offset);
        }
    }
}
