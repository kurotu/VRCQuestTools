// <copyright file="SunaoMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using System;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    class SunaoMaterial : MaterialWrapper
    {
        private Material material;

        internal SunaoMaterial(Material material) : base()
        {
            this.material = material;
        }

        // Main Color & Texture Maps

        float Brightness => material.GetFloat("_Bright");
        float AnimationSpeed => material.GetFloat("_UVAnimation");
        bool UseTextureAnimation => AnimationSpeed > 0.0f;
        int AnimationXSize => Mathf.FloorToInt(material.GetFloat("_UVAnimX"));
        int AnimationYSize => Mathf.FloorToInt(material.GetFloat("_UVAnimY"));
        bool AnimationOtherTextureMaps => material.GetFloat("_UVAnimOtherTex") > 0.0f;

        // Decal

        bool DecalEnable => material.GetFloat("_DecalEnable") > 0.0f;
        Texture DecalTexture => material.GetTexture("_DecalTex");
        Color DecalColor => material.GetColor("_DecalColor");
        float DecalPositionX => material.GetFloat("_DecalPosX");
        float DecalPositionY => material.GetFloat("_DecalPosY");
        float DecalScaleX => material.GetFloat("_DecalSizeX");
        float DecalScaleY => material.GetFloat("_DecalSizeY");
        /// <summary>In degree, clockwise.</summary>
        float DecalRotation => material.GetFloat("_DecalRotation");
        SunaoDecalMode DecalMode => GetEnum<SunaoDecalMode>(material.GetFloat("_DecalMode"));
        SunaoDecalMirrorMode DecalMirrorMode => GetEnum<SunaoDecalMirrorMode>(material.GetFloat("_DecalMirror"));
        /// <summary>Use with MultiplyMono.</summary>
        float DecalBrightnessOffset => material.GetFloat("_DecalBright");
        /// <summary>Use with EmissiveAdd, EmissiveOverride.</summary>
        float DecalEmissionIntensity => material.GetFloat("_DecalEmission");
        float DecalAnimationSpeed => material.GetFloat("_DecalAnimation");
        bool UseDecalAnimation => DecalAnimationSpeed > 0.0f;
        int DecalAnimationXSize => Mathf.FloorToInt(material.GetFloat("_DecalAnimX"));
        int DecalAnimationYSize => Mathf.FloorToInt(material.GetFloat("_DecalAnimY"));

        // Emission

        bool EmissionEnable => material.GetFloat("_EmissionEnable") > 0.0f;
        Texture EmissionMap => material.GetTexture("_EmissionMap");
        Color EmissionColor => material.GetColor("_EmissionColor");
        float EmissionIntensity => material.GetFloat("_Emission");
        Texture SecondEmissionMap => material.GetTexture("_EmissionMap2");
        SunaoEmissionMode EmissionMode => GetEnum<SunaoEmissionMode>(material.GetFloat("_EmissionMode"));
        float EmissionAnimationSpeed => material.GetFloat("_EmissionAnimation");
        bool UseEmissionAnimation => EmissionAnimationSpeed > 0.0f;
        int EmissionAnimationXSize => Mathf.FloorToInt(material.GetFloat("_EmissionAnimX"));
        int EmissionAnimationYSize => Mathf.FloorToInt(material.GetFloat("_EmissionAnimY"));

        // Gamma Fix

        bool GammaFixEnable => material.GetFloat("_EnableGammaFix") > 0.5f;
        float GammaR => material.GetFloat("_GammaR");
        float GammaG => material.GetFloat("_GammaG");
        float GammaB => material.GetFloat("_GammaB");

        // Brightness Fix

        bool BrightnessFixEnable => material.GetFloat("_EnableBlightFix") > 0.5;
        float OutputBrightness => material.GetFloat("_BlightOutput");
        float BrightnessOffset => material.GetFloat("_BlightOffset");

        // Output Limitter

        bool OutputLimitterEnable => material.GetFloat("_LimitterEnable") > 0.5;
        float LimitterMax => material.GetFloat("_LimitterMax");

        public override MagickImage CompositeLayers()
        {
            using (var disposables = new CompositeDisposable())
            {
                var main = CompositeMainImage();

                if (DecalEnable)
                {
                    var decal = CompositeDecalImage();
                    disposables.Add(decal);

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
                                foreach (var p in pc)
                                {
                                    var c = p.ToColor();
                                    var mono = (0.2126f * c.R) + (0.7152f * c.G) + (0.0722f * c.B);
                                    var a = mono * DecalColor.a * DecalEmissionIntensity;
                                    p.SetChannel(3, Saturate(a));
                                    p.SetChannel(0, Saturate(c.R * a / Quantum.Max));
                                    p.SetChannel(1, Saturate(c.G * a / Quantum.Max));
                                    p.SetChannel(2, Saturate(c.B * a / Quantum.Max));
                                }
                            }
                            main.Composite(decalCanvas, CompositeOperator.Plus);
                            break;
                        case SunaoDecalMode.EmissiveOverride:
                            main.Composite(decalCanvas, CompositeOperator.Over);
                            using (var pc = decalCanvas.GetPixels())
                            {
                                foreach (var p in pc)
                                {
                                    var c = p.ToColor();
                                    p.SetChannel(0, Saturate(c.R * c.A * DecalEmissionIntensity / Quantum.Max));
                                    p.SetChannel(1, Saturate(c.G * c.A * DecalEmissionIntensity / Quantum.Max));
                                    p.SetChannel(2, Saturate(c.B * c.A * DecalEmissionIntensity / Quantum.Max));
                                }
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
                    emission.Resize(main.Width, main.Height);
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
                        foreach (var p in pc)
                        {
                            var r = Mathf.Pow(p.GetChannel(0) / (float)Quantum.Max, powR) * Quantum.Max;
                            var g = Mathf.Pow(p.GetChannel(1) / (float)Quantum.Max, powG) * Quantum.Max;
                            var b = Mathf.Pow(p.GetChannel(2) / (float)Quantum.Max, powB) * Quantum.Max;
                            p.SetChannel(0, Saturate(r));
                            p.SetChannel(1, Saturate(g));
                            p.SetChannel(2, Saturate(b));
                        }
                    }
                }

                // Apply brightness
                if (BrightnessFixEnable)
                {
                    using (var pc = main.GetPixels())
                    {
                        var offset = BrightnessOffset * Quantum.Max;
                        foreach (var p in pc)
                        {
                            var r = p.GetChannel(0) * OutputBrightness + offset;
                            var g = p.GetChannel(1) * OutputBrightness + offset;
                            var b = p.GetChannel(2) * OutputBrightness + offset;
                            p.SetChannel(0, Saturate(r));
                            p.SetChannel(1, Saturate(g));
                            p.SetChannel(2, Saturate(b));
                        }
                    }
                }

                // Apply limitter
                if (OutputLimitterEnable)
                {
                    using (var pc = main.GetPixels())
                    {
                        var limit = Saturate(LimitterMax * Quantum.Max);
                        foreach (var p in pc)
                        {
                            // OUT.rgb  = min(OUT.rgb , _LimitterMax) の詳細が分からない
                            // hsv化してv^2を制限するとそれっぽくなる
                            var c = new Color(
                                p.GetChannel(0) / (float)Quantum.Max,
                                p.GetChannel(1) / (float)Quantum.Max,
                                p.GetChannel(2) / (float)Quantum.Max
                            );
                            Color.RGBToHSV(c, out float h, out float s, out float v);
                            var limitedV2 = Mathf.Min(LimitterMax, v * v);
                            var limited = Color.HSVToRGB(h, s, (float)Math.Sqrt(limitedV2));
                            p.SetChannel(0, (ushort)(limited.r * Quantum.Max));
                            p.SetChannel(1, (ushort)(limited.g * Quantum.Max));
                            p.SetChannel(2, (ushort)(limited.b * Quantum.Max));
                        }
                    }
                }

                return main;
            }
        }

        private MagickImage CompositeMainImage()
        {
            using (var disposables = new CompositeDisposable())
            {
                var mainTexture = MaterialUtils.GetMagickImage(material.mainTexture);
                disposables.Add(mainTexture);

                var coloredMain = ImgProc.Multiply(mainTexture, material.color);
                disposables.Add(coloredMain);

                var brightnessColor = new Color(Brightness, Brightness, Brightness);
                var brightnessApplied = ImgProc.Multiply(coloredMain, brightnessColor);
                disposables.Add(brightnessApplied);

                if (UseTextureAnimation)
                {
                    int newW = brightnessApplied.Width / AnimationXSize;
                    int newH = brightnessApplied.Height / AnimationYSize;
                    brightnessApplied.Crop(newW, newH);
                    ImgProc.ResizeToSquare(brightnessApplied);
                }

                using (var result = brightnessApplied.Clone())
                {
                    return new MagickImage(result);
                }
            }
        }

        private MagickImage CompositeDecalImage()
        {
            using (var decalTexture = MaterialUtils.GetMagickImage(DecalTexture))
            {
                var coloredDecal = ImgProc.Multiply(decalTexture, DecalColor);
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
                foreach (var p in mainPixels)
                {
                    var mono = Mathf.Max(p.GetChannel(0), p.GetChannel(1), p.GetChannel(2));
                    var v = Mathf.Min(Quantum.Max, mono + DecalBrightnessOffset * Quantum.Max);
                    v = Mathf.Max(v, 0);
                    ushort value = (ushort)Mathf.RoundToInt(v);
                    p.SetChannel(0, value);
                    p.SetChannel(1, value);
                    p.SetChannel(2, value);
                    mainPixels.SetPixel(p);
                }
            }
            return main;
        }

        private MagickImage CompositeEmissionImage()
        {
            using (var emap = MaterialUtils.GetMagickImage(EmissionMap) ?? new MagickImage(MagickColors.White, 2, 2))
            using (var e1 = ImgProc.Multiply(emap, EmissionColor))
            using (var e2 = MaterialUtils.GetMagickImage(SecondEmissionMap) ?? new MagickImage(MagickColors.White, 2, 2))
            {
                if (UseEmissionAnimation)
                {
                    int newW = e1.Width / EmissionAnimationXSize;
                    int newH = e1.Height / EmissionAnimationYSize;
                    e1.Crop(newW, newH);
                    ImgProc.ResizeToSquare(e1);
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
                    using(var pc = result.GetPixels())
                    {
                        foreach(var p in pc)
                        {
                            var c = p.ToColor();
                            p.SetChannel(0, Saturate(c.R * EmissionIntensity));
                            p.SetChannel(1, Saturate(c.G * EmissionIntensity));
                            p.SetChannel(2, Saturate(c.B * EmissionIntensity));
                        }
                    }
                    return result;
                }
            }
        }

        private ushort Saturate(float value)
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

        private T GetEnum<T>(float value)
        {
            return (T)Enum.ToObject(typeof(T), (int)value);
        }
    }

    enum SunaoDecalMode
    {
        Override, Add, Multiply, MultiplyMono, EmissiveAdd, EmissiveOverride
    }

    enum SunaoDecalMirrorMode
    {
        Normal, Fixed, Mirror1, Mirror2, CopyMirror, CopyFixed
    }

    enum SunaoEmissionMode
    {
        Add, Multiply, Minus
    }
}
