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
    internal abstract class MaterialBase
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

        /// <summary>
        /// Gets shader to bake Toon Lit texture.
        /// </summary>
        internal abstract Shader ToonLitBakeShader { get; }

        /// <summary>
        /// Gets the main texture scale. Override to extract UV tiling from shader-specific properties.
        /// </summary>
        internal virtual Vector2 MainTextureScale => Material.mainTextureScale;

        /// <summary>
        /// Gets the main texture offset. Override to extract UV tiling from shader-specific properties.
        /// </summary>
        internal virtual Vector2 MainTextureOffset => Material.mainTextureOffset;

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
        /// Gets the platform override settings for Toon Lit texture.
        /// </summary>
        /// <returns>Platform override settings, or null if none.</returns>
        internal virtual (int MaxTextureSize, TextureFormat Format)? GetToonLitPlatformOverride()
        {
            // Just use the platform override from main texture
            return TextureUtility.GetBestPlatformOverrideSettings(Material.mainTexture);
        }

        /// <summary>
        /// Generates an image for Toon Lit main texture.
        /// </summary>
        /// <param name="settings">Setting object.</param>
        /// <param name="completion">Completion action.</param>
        /// <returns>Request to wait.</returns>
        internal virtual AsyncCallbackRequest GenerateToonLitImage(IToonLitConvertSettings settings, System.Action<Texture2D> completion)
        {
            var mainTexture = Material.mainTexture ?? Texture2D.whiteTexture;

            using (var disposables = new CompositeDisposable())
            using (var baker = DisposableObject.New(Object.Instantiate(Material)))
            {
                baker.Object.parent = null;
                baker.Object.shader = ToonLitBakeShader;
                baker.Object.SetFloat("_VQT_MainTexBrightness", settings.MainTextureBrightness);
                baker.Object.SetFloat("_VQT_GenerateShadow", settings.GenerateShadowFromNormalMap ? 1 : 0);

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
                    disposables.Add(DisposableObject.New(tex));
                    baker.Object.SetTexture(name, tex);
                }

                // Check platform override settings from source textures used in baking
                var platformOverride = TextureUtility.GetBestPlatformOverrideSettings(texturesForOverride.ToArray());
                var maxTextureSize = platformOverride?.MaxTextureSize ?? (int)settings.MaxTextureSize;

                var width = mainTexture.width;
                var height = mainTexture.height;
                if (maxTextureSize > 0)
                {
                    width = System.Math.Min(maxTextureSize, width);
                    height = System.Math.Min(maxTextureSize, height);
                }

                return TextureUtility.BakeTexture(mainTexture, true, width, height, true, baker.Object, completion);
            }
        }
    }
}
