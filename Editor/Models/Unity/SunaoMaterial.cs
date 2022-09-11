// <copyright file="SunaoMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Threading.Tasks;
using ImageMagick;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Sunao Shader material.
    /// </summary>
    internal class SunaoMaterial : MaterialBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SunaoMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal SunaoMaterial(Material material)
            : base(material)
        {
        }

#pragma warning disable SA1136 // Enum values should be on separate lines
        private enum SunaoDecalMode
        {
            Override, Add, Multiply, MultiplyMono, EmissiveAdd, EmissiveOverride,
        }

        private enum SunaoDecalMirrorMode
        {
            Normal, Fixed, Mirror1, Mirror2, CopyMirror, CopyFixed,
        }

        private enum SunaoEmissionMode
        {
            Add, Multiply, Minus,
        }
#pragma warning restore SA1136 // Enum values should be on separate lines

#pragma warning disable SA1516 // Elements should be separated by blank line

        // Main Color & Texture Maps
        private float Brightness => Material.GetFloat("_Bright");
        private float AnimationSpeed => Material.GetFloat("_UVAnimation");
        private bool UseTextureAnimation => AnimationSpeed > 0.0f;
        private int AnimationXSize => Mathf.FloorToInt(Material.GetFloat("_UVAnimX"));
        private int AnimationYSize => Mathf.FloorToInt(Material.GetFloat("_UVAnimY"));
        private bool AnimationOtherTextureMaps => Material.GetFloat("_UVAnimOtherTex") > 0.0f;

        // Decal
        private bool DecalEnable => Material.GetFloat("_DecalEnable") > 0.0f;
        private Texture DecalTexture => Material.GetTexture("_DecalTex");
        private Color DecalColor => Material.GetColor("_DecalColor");
        private float DecalPositionX => Material.GetFloat("_DecalPosX");
        private float DecalPositionY => Material.GetFloat("_DecalPosY");
        private float DecalScaleX => Material.GetFloat("_DecalSizeX");
        private float DecalScaleY => Material.GetFloat("_DecalSizeY");

        /// <summary>Gets decal rotation in degree, clockwise.</summary>
        private float DecalRotation => Material.GetFloat("_DecalRotation");
        private SunaoDecalMode DecalMode => GetEnum<SunaoDecalMode>(Material.GetFloat("_DecalMode"));
        private SunaoDecalMirrorMode DecalMirrorMode => GetEnum<SunaoDecalMirrorMode>(Material.GetFloat("_DecalMirror"));

        /// <summary>Gets decal brightness offset. Use with MultiplyMono.</summary>
        private float DecalBrightnessOffset => Material.GetFloat("_DecalBright");

        /// <summary>Gets decal emission intensity. Use with EmissiveAdd or EmissiveOverride.</summary>
        private float DecalEmissionIntensity => Material.GetFloat("_DecalEmission");
        private float DecalAnimationSpeed => Material.GetFloat("_DecalAnimation");
        private bool UseDecalAnimation => DecalAnimationSpeed > 0.0f;
        private int DecalAnimationXSize => Mathf.FloorToInt(Material.GetFloat("_DecalAnimX"));
        private int DecalAnimationYSize => Mathf.FloorToInt(Material.GetFloat("_DecalAnimY"));

        // Emission
        private bool EmissionEnable => Material.GetFloat("_EmissionEnable") > 0.0f;
        private Texture EmissionMap => Material.GetTexture("_EmissionMap");
        private Color EmissionColor => Material.GetColor("_EmissionColor");
        private float EmissionIntensity => Material.GetFloat("_Emission");
        private Texture SecondEmissionMap => Material.GetTexture("_EmissionMap2");
        private SunaoEmissionMode EmissionMode => GetEnum<SunaoEmissionMode>(Material.GetFloat("_EmissionMode"));
        private float EmissionAnimationSpeed => Material.GetFloat("_EmissionAnimation");
        private bool UseEmissionAnimation => EmissionAnimationSpeed > 0.0f;
        private int EmissionAnimationXSize => Mathf.FloorToInt(Material.GetFloat("_EmissionAnimX"));
        private int EmissionAnimationYSize => Mathf.FloorToInt(Material.GetFloat("_EmissionAnimY"));

        // Gamma Fix
        private bool GammaFixEnable => Material.GetFloat("_EnableGammaFix") > 0.5f;
        private float GammaR => Material.GetFloat("_GammaR");
        private float GammaG => Material.GetFloat("_GammaG");
        private float GammaB => Material.GetFloat("_GammaB");

        // Brightness Fix
        private bool BrightnessFixEnable => Material.GetFloat("_EnableBlightFix") > 0.5;
        private float OutputBrightness => Material.GetFloat("_BlightOutput");
        private float BrightnessOffset => Material.GetFloat("_BlightOffset");

        // Output Limitter
        private bool OutputLimitterEnable => Material.GetFloat("_LimitterEnable") > 0.5;
        private float LimitterMax => Material.GetFloat("_LimitterMax");
#pragma warning restore SA1516 // Elements should be separated by blank line

        /// <inheritdoc/>
        internal override Texture2D GenerateToonLitImage()
        {
            using (var image = GenerateToonLitMagickImage())
            {
                return MagickImageUtility.MagickImageToTexture2D(image);
            }
        }

        private MagickImage GenerateToonLitMagickImage()
        {
            using (var disposables = new CompositeDisposable())
            {
                var main = CompositeMainImage();

                if (DecalEnable)
                {
                    var decal = CompositeDecalImage();
                    disposables.Add(decal);

                    MagickImageUtility.ResizeForLarger(main, decal);

                    var decalScaleToAspectFill = main.Width / (float)Math.Min(decal.Width, decal.Height);
                    var decalPercentageX = new Percentage(decalScaleToAspectFill * DecalScaleX * 100.0);
                    var decalPercentageY = new Percentage(decalScaleToAspectFill * DecalScaleY * 100.0);
                    decal.Scale(decalPercentageX, decalPercentageY);
                    decal.BackgroundColor = MagickColors.Transparent;
                    decal.Rotate(DecalRotation);

                    var decalX = Mathf.RoundToInt(main.Width * DecalPositionX - decal.Width / 2);
                    var decalY = main.Height - Mathf.RoundToInt(main.Height * DecalPositionY + decal.Height / 2);

                    // Draw basic decal as decalCanvas with mirror mode
                    var decalCanvas = new MagickImage(MagickColors.Transparent, main.Width, main.Height);
                    disposables.Add(decalCanvas);
                    switch (DecalMirrorMode)
                    {
                        case SunaoDecalMirrorMode.Normal:

                        // 仕様上、ミラー側をコントロールすることができないので通常と同じにする
                        case SunaoDecalMirrorMode.Fixed:
                        case SunaoDecalMirrorMode.Mirror1:
                            decalCanvas.Composite(decal, decalX, decalY, CompositeOperator.Over);
                            break;

                        // ミラー側のデカールを反転させることが設定の意図なので、反転させる
                        case SunaoDecalMirrorMode.Mirror2:
                            decal.Flop();
                            decalCanvas.Composite(decal, decalX, decalY, CompositeOperator.Over);
                            break;
                        case SunaoDecalMirrorMode.CopyMirror:
                            using (var halfDecal = new MagickImage(MagickColors.Transparent, main.Width, main.Height))
                            {
                                var leftIsNormal = DecalPositionX < 0.5;
                                halfDecal.Composite(decal, decalX, decalY, CompositeOperator.Over);
                                var crop = new MagickGeometry
                                {
                                    X = leftIsNormal ? 0 : decalCanvas.Width / 2,
                                    Y = 0,
                                    Width = decalCanvas.Width / 2,
                                    Height = decalCanvas.Height,
                                };
                                halfDecal.Crop(crop);

                                var normalX = leftIsNormal ? 0 : decalCanvas.Width / 2;
                                var copyX = leftIsNormal ? decalCanvas.Width / 2 : 0;
                                decalCanvas.Composite(halfDecal, normalX, 0, CompositeOperator.Over);
                                halfDecal.Flop();
                                decalCanvas.Composite(halfDecal, copyX, 0, CompositeOperator.Over);
                            }
                            break;
                        case SunaoDecalMirrorMode.CopyFixed:
                            using (var leftDecal = new MagickImage(MagickColors.Transparent, main.Width / 2, main.Height))
                            {
                                if (DecalPositionX < 0.5)
                                {
                                    leftDecal.Composite(decal, decalX, decalY, CompositeOperator.Over);
                                }
                                else
                                {
                                    using (var mirror = decal.Clone())
                                    {
                                        mirror.Flop();
                                        leftDecal.Composite(mirror, Mathf.RoundToInt(main.Width * (1 - DecalPositionX) - decal.Width / 2), decalY, CompositeOperator.Over);
                                    }
                                }
                                decalCanvas.Composite(leftDecal, 0, 0, CompositeOperator.Over);
                            }
                            using (var rightDecal = new MagickImage(MagickColors.Transparent, main.Width, main.Height))
                            {
                                if (DecalPositionX < 0.5)
                                {
                                    rightDecal.Composite(decal, Mathf.RoundToInt(main.Width * (1 - DecalPositionX) - decal.Width / 2), 0, CompositeOperator.Over);
                                }
                                else
                                {
                                    using (var mirror = decal.Clone())
                                    {
                                        mirror.Flop();
                                        rightDecal.Composite(mirror, decalX, decalY, CompositeOperator.Over);
                                    }
                                }
                                rightDecal.Crop(new MagickGeometry
                                {
                                    X = main.Width / 2,
                                    Y = 0,
                                    Width = decalCanvas.Width / 2,
                                    Height = decalCanvas.Height,
                                });
                                decalCanvas.Composite(rightDecal, main.Width / 2, 0, CompositeOperator.Over);
                            }
                            break;
                        default:
                            throw new Exception($"Unhandled DecalMirrorMode: {DecalMirrorMode}");
                    }

                    // Apply decal
                    switch (DecalMode)
                    {
                        case SunaoDecalMode.Override:
                            main.Composite(decalCanvas, CompositeOperator.Over);
                            break;
                        case SunaoDecalMode.Add:
                            main.Composite(decalCanvas, CompositeOperator.Plus);
                            break;
                        case SunaoDecalMode.Multiply:
                            main.Composite(decalCanvas, CompositeOperator.Multiply);
                            break;
                        case SunaoDecalMode.MultiplyMono:
                            using (var mono = CompositeMainForMultiplyMonoDecalMode())
                            {
                                decalCanvas.Composite(mono, CompositeOperator.Multiply);
                            }
                            main.Composite(decalCanvas, CompositeOperator.Over);
                            break;
                        case SunaoDecalMode.EmissiveAdd:
                            using (var pc = decalCanvas.GetPixels())
                            {
                                var values = pc.GetValues();
                                var channels = pc.Channels;
                                Parallel.For(0, values.Length / channels, (index) =>
                                {
                                    var baseIndex = index * channels;
                                    var r = (float)values[baseIndex + 0];
                                    var g = (float)values[baseIndex + 1];
                                    var b = (float)values[baseIndex + 2];
                                    var a = (float)values[baseIndex + 3];
                                    var mono = (0.2126f * r) + (0.7152f * g) + (0.0722f * b);
                                    values[baseIndex + 0] = Saturate(r * a / Quantum.Max);
                                    values[baseIndex + 1] = Saturate(g * a / Quantum.Max);
                                    values[baseIndex + 2] = Saturate(b * a / Quantum.Max);
                                    values[baseIndex + 3] = Saturate(a);
                                });
                                pc.SetPixels(values);
                            }
                            main.Composite(decalCanvas, CompositeOperator.Plus);
                            break;
                        case SunaoDecalMode.EmissiveOverride:
                            main.Composite(decalCanvas, CompositeOperator.Over);
                            using (var pc = decalCanvas.GetPixels())
                            {
                                var values = pc.GetValues();
                                var channels = pc.Channels;
                                var intensity = DecalEmissionIntensity;
                                Parallel.For(0, values.Length / channels, (index) =>
                                {
                                    var baseIndex = index * channels;
                                    var r = (float)values[baseIndex + 0];
                                    var g = (float)values[baseIndex + 1];
                                    var b = (float)values[baseIndex + 2];
                                    var a = (float)values[baseIndex + 3];
                                    values[baseIndex + 0] = Saturate(r * a * intensity / Quantum.Max);
                                    values[baseIndex + 1] = Saturate(g * a * intensity / Quantum.Max);
                                    values[baseIndex + 2] = Saturate(b * a * intensity / Quantum.Max);
                                });
                                pc.SetPixels(values);
                            }
                            main.Composite(decalCanvas, CompositeOperator.Plus);
                            break;
                        default:
                            throw new Exception($"Unhandled DecalMode: {DecalMode}");
                    }
                }

                // Apply emission
                if (EmissionEnable)
                {
                    var emission = CompositeEmissionImage();
                    disposables.Add(emission);
                    MagickImageUtility.ResizeForLarger(main, emission);
                    switch (EmissionMode)
                    {
                        case SunaoEmissionMode.Add:
                            main.Composite(emission, CompositeOperator.Plus);
                            break;
                        case SunaoEmissionMode.Multiply:
                            using (var multiply = new MagickImage(MagickColors.White, emission.Width, emission.Height))
                            using (var emi = main.Clone())
                            using (var reflectionOffset = new MagickImage(MagickColor.FromRgb(13, 13, 13), multiply.Width, multiply.Height))
                            {
                                multiply.Composite(emission, CompositeOperator.MinusSrc);
                                emi.Composite(reflectionOffset, CompositeOperator.Plus);
                                emi.Composite(emission, CompositeOperator.Multiply);

                                main.Composite(multiply, CompositeOperator.Multiply);
                                main.Composite(emi, CompositeOperator.Plus);
                            }
                            break;
                        case SunaoEmissionMode.Minus:
                            main.Composite(emission, CompositeOperator.MinusSrc);
                            break;
                        default:
                            throw new Exception($"Unhandled EmissionMode: {EmissionMode}");
                    }
                }

                // Apply gamma
                if (GammaFixEnable)
                {
                    var powR = 1.0f / (1.0f / Mathf.Max(GammaR, 0.00001f));
                    var powG = 1.0f / (1.0f / Mathf.Max(GammaG, 0.00001f));
                    var powB = 1.0f / (1.0f / Mathf.Max(GammaB, 0.00001f));
                    using (var pc = main.GetPixels())
                    {
                        var values = pc.GetValues();
                        var channels = pc.Channels;
                        Parallel.For(0, values.Length / channels, (index) =>
                        {
                            var baseIndex = index * channels;
                            var r = Mathf.Pow(values[baseIndex + 0] / (float)Quantum.Max, powR) * Quantum.Max;
                            var g = Mathf.Pow(values[baseIndex + 1] / (float)Quantum.Max, powG) * Quantum.Max;
                            var b = Mathf.Pow(values[baseIndex + 2] / (float)Quantum.Max, powB) * Quantum.Max;
                            values[baseIndex + 0] = Saturate(r);
                            values[baseIndex + 1] = Saturate(g);
                            values[baseIndex + 2] = Saturate(b);
                        });
                        pc.SetPixels(values);
                    }
                }

                // Apply brightness
                if (BrightnessFixEnable)
                {
                    using (var pc = main.GetPixels())
                    {
                        var offset = BrightnessOffset * Quantum.Max;
                        var values = pc.GetValues();
                        var channels = pc.Channels;
                        var brightness = OutputBrightness;
                        Parallel.For(0, values.Length / channels, (index) =>
                        {
                            var baseIndex = index * channels;
                            var r = values[baseIndex + 0] * brightness + offset;
                            var g = values[baseIndex + 1] * brightness + offset;
                            var b = values[baseIndex + 2] * brightness + offset;
                            values[baseIndex + 0] = Saturate(r);
                            values[baseIndex + 1] = Saturate(g);
                            values[baseIndex + 2] = Saturate(b);
                        });
                        pc.SetPixels(values);
                    }
                }

                // Apply limitter
                if (OutputLimitterEnable)
                {
                    using (var pc = main.GetPixels())
                    {
                        var limit = LimitterMax;
                        var values = pc.GetValues();
                        var channels = pc.Channels;
                        Parallel.For(0, values.Length / channels, (index) =>
                        {
                            // OUT.rgb  = min(OUT.rgb , _LimitterMax) の詳細が分からない
                            // hsv化してv^2を制限するとそれっぽくなる
                            var baseIndex = index * channels;
                            var r = values[baseIndex + 0];
                            var g = values[baseIndex + 1];
                            var b = values[baseIndex + 2];
                            var c = new Color(
                                r / (float)Quantum.Max,
                                g / (float)Quantum.Max,
                                b / (float)Quantum.Max);
                            Color.RGBToHSV(c, out float h, out float s, out float v);
                            var limitedV2 = Mathf.Min(limit, v * v);
                            var limited = Color.HSVToRGB(h, s, (float)Math.Sqrt(limitedV2));
                            values[baseIndex + 0] = (ushort)(limited.r * Quantum.Max);
                            values[baseIndex + 1] = (ushort)(limited.g * Quantum.Max);
                            values[baseIndex + 2] = (ushort)(limited.b * Quantum.Max);
                        });
                        pc.SetPixels(values);
                    }
                }

                return main;
            }
        }

        private static ushort Saturate(float value)
        {
            var rounded = Mathf.RoundToInt(value);
            if (rounded > Quantum.Max)
            {
                return Quantum.Max;
            }
            if (rounded < 0.0f)
            {
                return 0;
            }
            return (ushort)rounded;
        }

        private MagickImage CompositeMainImage()
        {
            using (var disposables = new CompositeDisposable())
            {
                var mainTexture = MagickImageUtility.GetMagickImage(Material.mainTexture) ?? new MagickImage(MagickColors.White, 2, 2);
                disposables.Add(mainTexture);

                var coloredMain = MagickImageUtility.Multiply(mainTexture, Material.color);
                disposables.Add(coloredMain);

                var brightnessColor = new Color(Brightness, Brightness, Brightness);
                var brightnessApplied = MagickImageUtility.Multiply(coloredMain, brightnessColor);
                disposables.Add(brightnessApplied);

                if (UseTextureAnimation)
                {
                    int newW = brightnessApplied.Width / AnimationXSize;
                    int newH = brightnessApplied.Height / AnimationYSize;
                    brightnessApplied.Crop(newW, newH);
                    MagickImageUtility.ResizeToSquare(brightnessApplied);
                }

                using (var result = brightnessApplied.Clone())
                {
                    return new MagickImage(result);
                }
            }
        }

        private MagickImage CompositeDecalImage()
        {
            using (var decalTexture = MagickImageUtility.GetMagickImage(DecalTexture))
            {
                var coloredDecal = MagickImageUtility.Multiply(decalTexture, DecalColor);
                if (UseDecalAnimation)
                {
                    int newW = coloredDecal.Width / DecalAnimationXSize;
                    int newH = coloredDecal.Height / DecalAnimationYSize;
                    coloredDecal.Crop(newW, newH);
                }
                return coloredDecal;
            }
        }

        private MagickImage CompositeMainForMultiplyMonoDecalMode()
        {
            var main = CompositeMainImage();
            using (var mainPixels = main.GetPixels())
            {
                var values = mainPixels.GetValues();
                var channels = mainPixels.Channels;
                var offset = DecalBrightnessOffset;
                Parallel.For(0, values.Length / mainPixels.Channels, (index) =>
                {
                    var baseIndex = index * channels;
                    var r = (float)values[baseIndex + 0];
                    var g = (float)values[baseIndex + 1];
                    var b = (float)values[baseIndex + 2];
                    var mono = Mathf.Max(r, g, b);
                    var v = Saturate(mono + offset * Quantum.Max);
                    values[baseIndex + 0] = v;
                    values[baseIndex + 1] = v;
                    values[baseIndex + 2] = v;
                });
                mainPixels.SetPixels(values);
            }
            return main;
        }

        private MagickImage CompositeEmissionImage()
        {
            using (var emap = MagickImageUtility.GetMagickImage(EmissionMap) ?? new MagickImage(MagickColors.White, 2, 2))
            using (var e1 = MagickImageUtility.Multiply(emap, EmissionColor))
            using (var e2 = MagickImageUtility.GetMagickImage(SecondEmissionMap) ?? new MagickImage(MagickColors.White, 2, 2))
            {
                if (UseEmissionAnimation)
                {
                    int newW = e1.Width / EmissionAnimationXSize;
                    int newH = e1.Height / EmissionAnimationYSize;
                    e1.Crop(newW, newH);
                    MagickImageUtility.ResizeToSquare(e1);
                }

                var size = Math.Max(e1.Width, e2.Width);
                e1.Resize(size, size);
                e2.Resize(size, size);
                using (var emission1 = new MagickImage(MagickColors.Black, size, size))
                using (var emission2 = new MagickImage(MagickColors.Black, size, size))
                {
                    emission1.Composite(e1, CompositeOperator.Over);
                    emission2.Composite(e2, CompositeOperator.Over);

                    var result = new MagickImage(MagickColors.Black, size, size);
                    result.Composite(emission1, CompositeOperator.Over);
                    result.Composite(emission2, CompositeOperator.Multiply);
                    using (var pc = result.GetPixels())
                    {
                        var values = pc.GetValues();
                        var channels = pc.Channels;
                        var intensity = EmissionIntensity;
                        Parallel.For(0, values.Length / channels, (index) =>
                        {
                            var baseIndex = index * channels;
                            var r = values[baseIndex + 0];
                            var g = values[baseIndex + 1];
                            var b = values[baseIndex + 2];
                            values[baseIndex + 0] = Saturate(r * intensity);
                            values[baseIndex + 1] = Saturate(g * intensity);
                            values[baseIndex + 2] = Saturate(b * intensity);
                        });
                        pc.SetPixels(values);
                    }
                    return result;
                }
            }
        }

        private T GetEnum<T>(float value)
        {
            return (T)Enum.ToObject(typeof(T), (int)value);
        }
    }
}
