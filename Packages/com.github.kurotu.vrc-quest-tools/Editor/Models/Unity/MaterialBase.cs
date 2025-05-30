// <copyright file="MaterialBase.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Utils;
using UnityEngine;
using UnityEngine.Rendering;

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
                newMat.mainTextureOffset = Material.mainTextureOffset;
                newMat.mainTextureScale = Material.mainTextureScale;
            }
            return newMat;
        }

        /// <summary>
        /// Generates an image for Toon Lit main texture.
        /// </summary>
        /// <param name="settings">Setting object.</param>
        /// <param name="completion">Completion action.</param>
        /// <returns>Request to wait.</returns>
        internal virtual AsyncCallbackRequest GenerateToonLitImage(IToonLitConvertSettings settings, System.Action<Texture2D> completion)
        {
            var maxTextureSize = (int)settings.MaxTextureSize;
            var mainTexture = Material.mainTexture ?? Texture2D.whiteTexture;
            var width = mainTexture.width;
            var height = mainTexture.height;
            if (maxTextureSize > 0)
            {
                width = System.Math.Min(maxTextureSize, width);
                height = System.Math.Min(maxTextureSize, height);
            }

            using (var disposables = new CompositeDisposable())
            using (var baker = DisposableObject.New(Object.Instantiate(Material)))
            {
#if UNITY_2022_1_OR_NEWER
                baker.Object.parent = null;
#endif
                baker.Object.shader = ToonLitBakeShader;
                baker.Object.SetFloat("_VQT_MainTexBrightness", settings.MainTextureBrightness);
                baker.Object.SetFloat("_VQT_GenerateShadow", settings.GenerateShadowFromNormalMap ? 1 : 0);
                foreach (var name in Material.GetTexturePropertyNames())
                {
                    var t = Material.GetTexture(name);
                    if (t is Cubemap)
                    {
                        continue;
                    }
                    if (TextureUtility.IsNormalMapAsset(t))
                    {
                        continue;
                    }
                    var tex = TextureUtility.LoadUncompressedTexture(t);
                    disposables.Add(DisposableObject.New(tex));
                    baker.Object.SetTexture(name, tex);
                }

                return TextureUtility.BakeTexture(mainTexture, true, width, height, true, baker.Object, completion);
            }
        }
    }
}
