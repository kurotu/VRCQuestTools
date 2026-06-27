// <copyright file="ParticleMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using BlendMode = UnityEngine.Rendering.BlendMode;
using BlendOp = UnityEngine.Rendering.BlendOp;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents a particle shader material to be converted into an avatar-compatible
    /// VRChat/Mobile/Particles/* (or Toon Lit) shader.
    /// </summary>
    internal class ParticleMaterial : MaterialBase
    {
        private const string ToonLitShaderName = "VRChat/Mobile/Toon Lit";
        private const string AdditiveShaderName = "VRChat/Mobile/Particles/Additive";
        private const string MultiplyShaderName = "VRChat/Mobile/Particles/Multiply";

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal ParticleMaterial(Material material)
            : base(material)
        {
            Blend = DetectBlend();
        }

        /// <summary>
        /// Blend type of a particle material.
        /// </summary>
        internal enum ParticleBlend
        {
            /// <summary>Fully opaque (src). Converted to Toon Lit.</summary>
            Opaque,

            /// <summary>Alpha-tested cutout. Converted to Additive with baked alpha threshold.</summary>
            Cutout,

            /// <summary>Alpha-blended fade (dst(1-a)+src*a). Converted to Additive.</summary>
            Fade,

            /// <summary>Premultiplied transparent (dst(1-a)+src). Converted to Additive with alpha baked to 1.</summary>
            Transparent,

            /// <summary>Additive (dst+src*a). Converted to Additive.</summary>
            Additive,

            /// <summary>Subtractive (dst*(1-src)). Converted to Multiply with inverted RGB.</summary>
            Subtractive,

            /// <summary>Modulate (dst*src). Converted to Multiply.</summary>
            Modulate,
        }

        /// <summary>
        /// Gets the detected blend type.
        /// </summary>
        internal ParticleBlend Blend { get; }

        /// <summary>
        /// Gets the destination (avatar-compatible) shader name.
        /// </summary>
        internal virtual string DestinationShaderName
        {
            get
            {
                switch (Blend)
                {
                    case ParticleBlend.Opaque:
                        return ToonLitShaderName;
                    case ParticleBlend.Subtractive:
                    case ParticleBlend.Modulate:
                        return MultiplyShaderName;
                    default:
                        return AdditiveShaderName;
                }
            }
        }

        /// <summary>
        /// Gets the tint color, absorbing the property name difference between
        /// Standard Particle (_Color) and Legacy/Mobile Particle (_TintColor).
        /// </summary>
        internal Color Tint
        {
            get
            {
                if (Material.HasProperty("_Color"))
                {
                    return Material.GetColor("_Color");
                }
                if (Material.HasProperty("_TintColor"))
                {
                    return Material.GetColor("_TintColor");
                }
                return Color.white;
            }
        }

        /// <summary>
        /// Gets a value indicating whether RGB should be inverted (Subtractive).
        /// </summary>
        internal bool Invert => Blend == ParticleBlend.Subtractive;

        /// <summary>
        /// Gets a value indicating whether the alpha threshold should be baked (Cutout).
        /// </summary>
        internal bool UseCutout => Blend == ParticleBlend.Cutout;

        /// <summary>
        /// Gets the alpha cutoff value for Cutout.
        /// </summary>
        internal float Cutoff => Material.HasProperty("_Cutoff") ? Material.GetFloat("_Cutoff") : 0.5f;

        /// <summary>
        /// Gets a value indicating whether the alpha should be baked to 1 (Transparent premultiplied).
        /// </summary>
        internal bool OverrideAlphaToOne => Blend == ParticleBlend.Transparent;

        /// <summary>
        /// Gets a value indicating whether emission should be baked additively.
        /// Only applies to materials converted to the Additive shader.
        /// </summary>
        internal bool BakeEmission => DestinationShaderName == AdditiveShaderName && HasEmission();

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Shader.Find("Hidden/VRCQuestTools/Particle");

        /// <summary>
        /// Gets a value indicating whether the original _MainTex reference can be reused without baking.
        /// Both the logical no-op condition and the unity_builtin_extra condition must hold.
        /// </summary>
        internal virtual bool ShouldUseOriginalMainTexture => CanSkipBakeLogically && IsMainTextureUnityBuiltinExtra();

        /// <summary>
        /// Gets a value indicating whether texture processing is logically unnecessary.
        /// </summary>
        private bool CanSkipBakeLogically =>
            IsTintWhite() &&
            IsMainTextureTilingIdentity() &&
            !BakeEmission &&
            !Invert &&
            !UseCutout &&
            !OverrideAlphaToOne;

        /// <summary>
        /// Creates the converted avatar-compatible material (without baked textures).
        /// </summary>
        /// <returns>Converted material.</returns>
        internal Material CreateConvertedMaterial()
        {
            var newShader = Shader.Find(DestinationShaderName);
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
            if (newMat.HasProperty("_MainTex"))
            {
                newMat.mainTexture = Material.mainTexture;
                newMat.mainTextureScale = MainTextureScale;
                newMat.mainTextureOffset = MainTextureOffset;
            }
            return newMat;
        }

        /// <summary>
        /// Generates a baked main texture for the converted particle material.
        /// </summary>
        /// <param name="maxTextureSize">Max texture size (0 for no limit).</param>
        /// <param name="completion">Completion action.</param>
        /// <returns>Request to wait.</returns>
        internal virtual AsyncCallbackRequest GenerateParticleImage(int maxTextureSize, System.Action<Texture2D> completion)
        {
            var mainTexture = Material.mainTexture ?? Texture2D.whiteTexture;

            using (var disposables = new CompositeDisposable())
            using (var baker = DisposableObject.New(Object.Instantiate(Material)))
            {
                baker.Object.parent = null;
                baker.Object.shader = ToonLitBakeShader;
                baker.Object.SetColor("_VQT_Tint", Tint);
                baker.Object.SetFloat("_VQT_Invert", Invert ? 1 : 0);
                baker.Object.SetFloat("_VQT_CutoffEnabled", UseCutout ? 1 : 0);
                baker.Object.SetFloat("_VQT_Cutoff", Mathf.Clamp01(Cutoff));
                baker.Object.SetFloat("_VQT_AlphaToOne", OverrideAlphaToOne ? 1 : 0);
                baker.Object.SetFloat("_VQT_Emission", BakeEmission ? 1 : 0);

                // Reset _MainTex UV tiling on the baker so the bake produces the raw texture appearance.
                // UV tiling is applied at runtime via mainTextureScale/Offset on the output material.
                baker.Object.SetTextureScale("_MainTex", Vector2.one);
                baker.Object.SetTextureOffset("_MainTex", Vector2.zero);

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

                    // Built-in textures (e.g. Default-Particle) are often non-readable and cannot be
                    // instantiated for an uncompressed copy (LoadUncompressedTexture would throw).
                    // The baker already references the original (from Instantiate(Material)) and the GPU
                    // can sample it during Graphics.Blit, so leave such textures as-is.
                    var path = AssetDatabase.GetAssetPath(t);
                    if (path == "Resources/unity_builtin_extra" || path == "Library/unity default resources")
                    {
                        continue;
                    }

                    var tex = TextureUtility.LoadUncompressedTexture(t);
                    if (tex != t)
                    {
                        disposables.Add(DisposableObject.New(tex));
                    }
                    baker.Object.SetTexture(name, tex);
                }

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

        private static ParticleBlend InferBlendFromState(BlendMode src, BlendMode dst, BlendOp op)
        {
            // Reverse-subtract (dst - src) darkens the background like the Subtractive mode.
            if (op == BlendOp.ReverseSubtract)
            {
                return ParticleBlend.Subtractive;
            }

            // Multiply/Modulate: the destination (framebuffer) color is used as a blend factor.
            // e.g. DstColor/Zero (Legacy Multiply), DstColor/OneMinusSrcAlpha (Standard Particles Modulate),
            // or Zero/SrcColor (VRChat Mobile Multiply).
            if (src == BlendMode.DstColor || dst == BlendMode.SrcColor)
            {
                return ParticleBlend.Modulate;
            }

            switch (dst)
            {
                case BlendMode.One:
                    // SrcAlpha One or One One: additive accumulation.
                    return ParticleBlend.Additive;
                case BlendMode.OneMinusSrcAlpha:
                    // One OneMinusSrcAlpha: premultiplied transparent. SrcAlpha OneMinusSrcAlpha: fade.
                    return src == BlendMode.One ? ParticleBlend.Transparent : ParticleBlend.Fade;
                case BlendMode.OneMinusSrcColor:
                    // Zero OneMinusSrcColor: dst*(1-src).
                    return ParticleBlend.Subtractive;
                case BlendMode.Zero:
                    // One Zero: opaque (DstColor Zero is already handled as Modulate above).
                    return ParticleBlend.Opaque;
                default:
                    // Unknown blend: fall back to an additive approximation.
                    return ParticleBlend.Fade;
            }
        }

        private static ParticleBlend ModeToBlend(int mode)
        {
            // Unity Standard Particle shader _Mode enum.
            switch (mode)
            {
                case 0: return ParticleBlend.Opaque;
                case 1: return ParticleBlend.Cutout;
                case 2: return ParticleBlend.Fade;
                case 3: return ParticleBlend.Transparent;
                case 4: return ParticleBlend.Additive;
                case 5: return ParticleBlend.Subtractive;
                case 6: return ParticleBlend.Modulate;
                default: return ParticleBlend.Fade;
            }
        }

        private ParticleBlend DetectBlend()
        {
            // Prefer the real blend state (_SrcBlend/_DstBlend/_BlendOp) so that shader name changes
            // (e.g. Shader Blocking System) or manual blend overrides are respected.
            if (Material.HasProperty("_SrcBlend") && Material.HasProperty("_DstBlend"))
            {
                var src = (BlendMode)(int)Material.GetFloat("_SrcBlend");
                var dst = (BlendMode)(int)Material.GetFloat("_DstBlend");
                var op = Material.HasProperty("_BlendOp") ? (BlendOp)(int)Material.GetFloat("_BlendOp") : BlendOp.Add;
                var blend = InferBlendFromState(src, dst, op);

                // Opaque and Cutout share the same One/Zero blend state; disambiguate via alpha test.
                if (blend == ParticleBlend.Opaque && IsAlphaTestEnabled())
                {
                    return ParticleBlend.Cutout;
                }
                return blend;
            }

            // Fall back to the Standard Particle _Mode property.
            if (Material.HasProperty("_Mode"))
            {
                return ModeToBlend((int)Material.GetFloat("_Mode"));
            }

            // Fall back to the shader name (Legacy/Mobile/VRChat fixed-function particle shaders).
            return DetectBlendFromShaderName();
        }

        private ParticleBlend DetectBlendFromShaderName()
        {
            var name = Material.shader.name.ToLower();
            if (name.Contains("additive"))
            {
                return ParticleBlend.Additive;
            }
            if (name.Contains("multiply"))
            {
                return ParticleBlend.Modulate;
            }

            // Alpha Blended / VertexLit Blended / others: approximate with Additive.
            return ParticleBlend.Fade;
        }

        private bool IsAlphaTestEnabled()
        {
            if (Material.IsKeywordEnabled("_ALPHATEST_ON"))
            {
                return true;
            }
            return Material.HasProperty("_Mode") && (int)Material.GetFloat("_Mode") == 1;
        }

        private bool HasEmission()
        {
            if (Material.HasProperty("_EmissionColor"))
            {
                var color = Material.GetColor("_EmissionColor");
                if (color.maxColorComponent > 0f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsTintWhite()
        {
            var t = Tint;
            return Mathf.Approximately(t.r, 1f) && Mathf.Approximately(t.g, 1f) && Mathf.Approximately(t.b, 1f) && Mathf.Approximately(t.a, 1f);
        }

        private bool IsMainTextureTilingIdentity()
        {
            return MainTextureScale == Vector2.one && MainTextureOffset == Vector2.zero;
        }

        private bool IsMainTextureUnityBuiltinExtra()
        {
            if (Material.mainTexture == null)
            {
                return false;
            }
            var path = AssetDatabase.GetAssetPath(Material.mainTexture);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            return path == "Resources/unity_builtin_extra" || path == "Library/unity default resources";
        }
    }
}
