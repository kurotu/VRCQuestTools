// <copyright file="TextureUVTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for <see cref="TextureUV"/>.
    /// </summary>
    public class TextureUVTests
    {
        /// <summary>
        /// Test that a texture sampled with the main texture UV and no own tiling needs no remap.
        /// </summary>
        [Test]
        public void MainUVIdentity_NeedsNoRemap()
        {
            var mainST = (new Vector2(2, 3), new Vector2(0.5f, 0.25f));
            Assert.IsNull(TextureUV.MainUVIdentity.GetRemapFromMainTextureSpace(mainST));
        }

        /// <summary>
        /// Test that a main-UV based texture is remapped only by its own tiling, regardless of the main tiling.
        /// </summary>
        [Test]
        public void MainUVWithTiling_RemapsByOwnTiling()
        {
            var mainST = (new Vector2(2, 3), new Vector2(0.5f, 0.25f));
            var uv = new TextureUV(TextureUVBasis.MainUV, new Vector2(4, 4), new Vector2(0.1f, 0.2f));

            var remap = uv.GetRemapFromMainTextureSpace(mainST);

            Assert.IsTrue(remap.HasValue);
            Assert.AreEqual(new Vector2(4, 4), remap.Value.Scale);
            Assert.AreEqual(new Vector2(0.1f, 0.2f), remap.Value.Offset);
        }

        /// <summary>
        /// Test that a mesh-UV based texture with the main tiling needs no remap.
        /// </summary>
        [Test]
        public void MeshUV0SameAsMain_NeedsNoRemap()
        {
            var mainST = (new Vector2(2, 3), new Vector2(0.5f, 0.25f));
            var uv = new TextureUV(TextureUVBasis.MeshUV0, new Vector2(2, 3), new Vector2(0.5f, 0.25f));
            Assert.IsNull(uv.GetRemapFromMainTextureSpace(mainST));
        }

        /// <summary>
        /// Test that a mesh-UV based texture is remapped through the mesh UV.
        /// </summary>
        [Test]
        public void MeshUV0Different_MapsThroughMeshUV()
        {
            var mainST = (Scale: new Vector2(2, 4), Offset: new Vector2(0.5f, 0.25f));
            var uv = new TextureUV(TextureUVBasis.MeshUV0, new Vector2(3, 1), new Vector2(0.1f, 0.0f));

            var remap = uv.GetRemapFromMainTextureSpace(mainST);
            Assert.IsTrue(remap.HasValue);

            var meshUV = new Vector2(0.3f, 0.7f);
            var mainUV = Vector2.Scale(meshUV, mainST.Scale) + mainST.Offset;
            var expected = Vector2.Scale(meshUV, uv.Scale) + uv.Offset;
            var remapped = Vector2.Scale(mainUV, remap.Value.Scale) + remap.Value.Offset;
            Assert.AreEqual(expected.x, remapped.x, 1e-5f);
            Assert.AreEqual(expected.y, remapped.y, 1e-5f);
        }

        /// <summary>
        /// Test that a main-UV based texture is converted to mesh-UV tiling.
        /// </summary>
        [Test]
        public void MainUV_ToMeshUV0Tiling_ComposesMainTiling()
        {
            var mainST = (Scale: new Vector2(2, 4), Offset: new Vector2(0.5f, 0.25f));
            var uv = new TextureUV(TextureUVBasis.MainUV, new Vector2(3, 1), new Vector2(0.1f, 0.2f));

            var meshST = uv.ToMeshUV0Tiling(mainST);

            var meshUV = new Vector2(0.3f, 0.7f);
            var mainUV = Vector2.Scale(meshUV, mainST.Scale) + mainST.Offset;
            var expected = Vector2.Scale(mainUV, uv.Scale) + uv.Offset;
            var actual = Vector2.Scale(meshUV, meshST.Scale) + meshST.Offset;
            Assert.AreEqual(expected.x, actual.x, 1e-5f);
            Assert.AreEqual(expected.y, actual.y, 1e-5f);
        }
    }
}
