// <copyright file="MaterialBase.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Abstract class for material wrapper.
    /// </summary>
    internal abstract class MaterialBase : IAlbedoSource
    {
        /// <summary>
        /// Internal material.
        /// </summary>
        internal readonly Material Material;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBase"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal MaterialBase(Material material)
        {
            Material = material;
        }

        /// <inheritdoc/>
        (Vector2 Scale, Vector2 Offset) IAlbedoSource.MainTextureST => (MainTextureScale, MainTextureOffset);

        /// <summary>
        /// Gets shader to bake the albedo texture.
        /// </summary>
        internal abstract Shader AlbedoBakeShader { get; }

        /// <summary>
        /// Gets the main texture scale. Override to extract UV tiling from shader-specific properties.
        /// </summary>
        internal virtual Vector2 MainTextureScale => Material.mainTextureScale;

        /// <summary>
        /// Gets the main texture offset. Override to extract UV tiling from shader-specific properties.
        /// </summary>
        internal virtual Vector2 MainTextureOffset => Material.mainTextureOffset;

        /// <inheritdoc/>
        bool IAlbedoSource.RequiresAlbedoTexture(AlbedoBakeRequest request) => RequiresAlbedoTexture(request);

        /// <inheritdoc/>
        AsyncCallbackRequest IAlbedoSource.GenerateAlbedo(AlbedoBakeRequest request, System.Action<Texture2D> completion) => GenerateAlbedo(request, completion);

        /// <inheritdoc/>
        (int MaxTextureSize, TextureFormat Format)? IAlbedoSource.GetAlbedoPlatformOverride(AlbedoBakeRequest request) => GetAlbedoPlatformOverride(request);

        /// <summary>
        /// Convert internal material to Toon Lit.
        /// </summary>
        /// <returns>Converted material.</returns>
        internal Material ConvertToToonLit()
        {
            var newShader = Shader.Find("VRChat/Mobile/Toon Lit");
            var hasMainTexProp = Material.GetTexturePropertyNames().Contains("_MainTex");
            var newMat = new Material(newShader)
            {
                doubleSidedGI = Material.doubleSidedGI,
                enableInstancing = true, // https://docs.vrchat.com/docs/quest-content-optimization#avatars-and-worlds
                globalIlluminationFlags = Material.globalIlluminationFlags,
                hideFlags = Material.hideFlags,
                name = $"{Material.name}_{newShader.name.Split('/').Last()}",
                renderQueue = Material.renderQueue,
                shader = newShader,
                shaderKeywords = null,
            };
            if (hasMainTexProp)
            {
                newMat.mainTexture = Material.mainTexture;
                newMat.mainTextureOffset = MainTextureOffset;
                newMat.mainTextureScale = MainTextureScale;
            }
            return newMat;
        }

        /// <summary>
        /// Gets whether the albedo needs a texture for the request. When false, the main color alone represents the albedo.
        /// </summary>
        /// <param name="request">Bake request.</param>
        /// <returns>True when a texture is needed.</returns>
        internal virtual bool RequiresAlbedoTexture(AlbedoBakeRequest request) => true;

        /// <summary>
        /// Gets the platform override settings of the source textures which the albedo is baked from.
        /// </summary>
        /// <param name="request">Bake request.</param>
        /// <returns>Platform override settings, or null if none.</returns>
        internal virtual (int MaxTextureSize, TextureFormat Format)? GetAlbedoPlatformOverride(AlbedoBakeRequest request)
        {
            // Just use the platform override from main texture
            return TextureUtility.GetBestPlatformOverrideSettings(Material.mainTexture);
        }

        /// <summary>
        /// Bakes the albedo (main texture).
        /// </summary>
        /// <param name="request">Bake request.</param>
        /// <param name="completion">Completion action.</param>
        /// <returns>Request to wait.</returns>
        internal virtual AsyncCallbackRequest GenerateAlbedo(AlbedoBakeRequest request, System.Action<Texture2D> completion)
        {
            var mainTexture = Material.mainTexture ?? Texture2D.whiteTexture;
            var bakeMainTexture = mainTexture;

            using (var disposables = new CompositeDisposable())
            using (var baker = DisposableObject.New(Object.Instantiate(Material)))
            {
                baker.Object.parent = null;
                baker.Object.shader = AlbedoBakeShader;
                baker.Object.SetFloat("_VQT_MainTexBrightness", request.Brightness);
                baker.Object.SetFloat("_VQT_GenerateShadow", request.ShadeFromNormalMap ? 1 : 0);

                // Reset _MainTex UV tiling on the baker so the bake produces the raw texture appearance
                // (no embedded tiling). UV tiling is applied at runtime via mainTextureScale/Offset on
                // the output Toon Lit material set by ConvertToToonLit(). Embedding tiling here and also
                // setting mainTextureScale would cause double-tiling at runtime.
                baker.Object.SetTextureScale("_MainTex", Vector2.one);
                baker.Object.SetTextureOffset("_MainTex", Vector2.zero);

                // Collect textures for platform override analysis
                var texturesForOverride = new List<Texture>();
                foreach (var name in Material.GetTexturePropertyNames())
                {
                    var t = Material.GetTexture(name);
                    if (t == null)
                    {
                        continue;
                    }
                    if (t is Cubemap)
                    {
                        continue;
                    }
                    if (TextureUtility.IsNormalMapAsset(t))
                    {
                        continue;
                    }
                    texturesForOverride.Add(t);
                    var tex = TextureUtility.LoadUncompressedTexture(t);
                    // The loader can return borrowed generated textures and runtime buffers.
                    if (tex != t)
                    {
                        disposables.Add(DisposableObject.New(tex));
                    }
                    baker.Object.SetTexture(name, tex);
                    if (t == mainTexture)
                    {
                        bakeMainTexture = tex;
                    }
                }

                // Check platform override settings from source textures used in baking
                var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(texturesForOverride.ToArray());
                var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)request.MaxTextureSize;

                var width = mainTexture.width;
                var height = mainTexture.height;
                if (maxTextureSize > 0)
                {
                    width = System.Math.Min(maxTextureSize, width);
                    height = System.Math.Min(maxTextureSize, height);
                }

                return TextureUtility.BakeTexture(bakeMainTexture, true, width, height, true, baker.Object, completion);
            }
        }
    }
}
