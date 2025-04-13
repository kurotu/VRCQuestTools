// <copyright file="LilToonMaterial.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// lilToon material.
    /// </summary>
    internal class LilToonMaterial : MaterialBase, IStandardLiteConvertable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal LilToonMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Shader.Find("Hidden/VRCQuestTools/lilToon");

        internal override Shader StandardLiteMainBakeShader => Shader.Find("Hidden/VRCQuestTools/StandardLite/lilToon_main");

        internal override Shader StandardLiteMetallicSmoothnessBakeShader => Shader.Find("Hidden/VRCQuestTools/StandardLite/lilToon_metallic_smoothness");

        public bool UseStandardLiteEmission => Material.GetTexture("_EmissionMap") != null; // TODO: Fix this later.

        public bool UseStandardLiteNormalMap => Material.GetTexture("_BumpMap") != null;

        public bool UseStandardLiteMetallicSmoothness => Material.GetFloat("_UseReflection") > 0.0f;

        #region IStandardLiteConvertable

        /// <inheritdoc/>
        public Material ConvertToStandardLite()
        {
            var newShader = Shader.Find("VRChat/Mobile/Standard Lite");
            var newMaterial = new Material(newShader)
            {
                color = Material.color,
                doubleSidedGI = Material.doubleSidedGI,
                enableInstancing = true,
                globalIlluminationFlags = Material.globalIlluminationFlags,
                hideFlags = Material.hideFlags,
                mainTexture = Material.mainTexture ?? Material.GetTexture("_MainTex"),
                mainTextureOffset = Material.mainTextureOffset,
                mainTextureScale = Material.mainTextureScale,
                name = $"{Material.name}_{newShader.name.Split('/').Last()}",
                renderQueue = Material.renderQueue,
                shader = newShader,
                shaderKeywords = null,
            };

            var mats = new[] { Material };

            var useReflection = UseStandardLiteMetallicSmoothness;
            var applyReflection = Material.GetFloat("_ApplyReflection") > 0.0;
            if (useReflection && applyReflection)
            {
                newMaterial.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                newMaterial.SetFloat("_GlossyReflections", 1);
            }
            else
            {
                newMaterial.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
                newMaterial.SetFloat("_GlossyReflections", 0);
            }
            newMaterial.SetFloat("_Metallic", useReflection ? Material.GetFloat("_Metallic") : 0.0f);
            newMaterial.SetFloat("_Glossiness", useReflection ? Material.GetFloat("_Smoothness") : 0.0f);
            newMaterial.SetTexture("_BumpMap", UseStandardLiteNormalMap ? Material.GetTexture("_BumpMap") : null);

            var useEmission = UseStandardLiteEmission;
            if (useEmission)
            {
                newMaterial.EnableKeyword("_EMISSION");
                var so = new SerializedObject(newMaterial);
                so.Update();
                so.FindProperty("m_LightmapFlags").intValue = 6;
                so.ApplyModifiedProperties();
            }
            else
            {
                newMaterial.DisableKeyword("_EMISSION");
            }
            newMaterial.SetTexture("_EmissionMap", useEmission ? Material.GetTexture("_EmissionMap") : null);
            newMaterial.SetColor("_EmissionColor", useEmission ? Material.GetColor("_EmissionColor") : Color.white);

            return newMaterial;
        }

        public AsyncCallbackRequest GenerateStandardLiteMain(StandardLiteConvertSettings settings, System.Action<Texture2D> completion)
        {
            var rt = MainBake(Material, 0);
            return AssetUtility.RequestReadbackRenderTexture(rt, true, (tex) =>
            {
                Object.DestroyImmediate(rt);
                completion?.Invoke(tex);
            });
        }

        public AsyncCallbackRequest GenerateStandardLiteMetallicSmoothness(StandardLiteConvertSettings settings, System.Action<Texture2D> completion)
        {
            var hasMainTex = Material.mainTexture != null;
            var mainTexSize = hasMainTex ? Material.mainTexture.width : (int)settings.maxTextureSize;
            var textureSize = System.Math.Min(mainTexSize, (int)settings.maxTextureSize);

            var bakeMat = new Material(Material);
#if UNITY_2022_1_OR_NEWER
            bakeMat.parent = null;
#endif
            bakeMat.shader = Shader.Find("Hidden/VRCQuestTools/StandardLite/lilToon_metallic_smoothness");

            var rt = RenderTexture.GetTemporary(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(null, rt, bakeMat);
            return AssetUtility.RequestReadbackRenderTexture(rt, true, (tex) =>
            {
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(bakeMat);
                completion?.Invoke(tex);
            });
        }

        public AsyncCallbackRequest GenerateStandardLiteNormalMap(StandardLiteConvertSettings settings, System.Action<Texture2D> completion)
        {
            // TODO: Rewrite down sampling.
            var normal = Material.GetTexture("_BumpMap") as Texture2D;
            return AssetUtility.BakeTexture(normal, (int)settings.maxTextureSize, (int)settings.maxTextureSize, true, (texture) =>
            {
                completion?.Invoke(texture);
            });
        }

        public AsyncCallbackRequest GenerateStandardLiteEmission(StandardLiteConvertSettings settings, System.Action<Texture2D> completion)
        {
            // TODO: Rewrite bake emission.
            var emission = Material.GetTexture("_EmissionMap") as Texture2D;
            return AssetUtility.BakeTexture(emission, (int)settings.maxTextureSize, (int)settings.maxTextureSize, true, (texture) =>
            {
                completion?.Invoke(texture);
            });
        }

        #endregion

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateToonLitImage(IToonLitConvertSettings settings, System.Action<Texture2D> completion)
        {
            var main = DisposableObject.New(MainBake(Material, 0));
            return EmissionBake(main.Object, Material, settings, (texture) =>
            {
                main.Dispose();
                completion?.Invoke(texture);
            });
        }

        private static LilToonSetting LoadShaderSetting()
        {
            var path = LilToonInspector.GetShaderSettingPath();
            var lilToonSettingType = SystemUtility.GetTypeByName("lilToonSetting");
            var lilToonSetting = AssetDatabase.LoadAssetAtPath(path, lilToonSettingType);
            return new LilToonSetting(lilToonSetting);
        }

        private static void CopyMaterialProperty(Material target, Material source, MaterialProperty property)
        {
            if (property.name == null)
            {
                // The property is missing in the source material.
                return;
            }
            var targetProp = MaterialEditor.GetMaterialProperty(new[] { target }, property.name);
            if (targetProp.name == null)
            {
                Debug.LogWarning(
                    $"[{VRCQuestTools.Name}] Property {property.name} not found in target material.\n" +
                    $"Material: {source}, Shader: {source.shader.name}");
            }
            else if (targetProp.type != property.type)
            {
                Debug.LogWarning(
                    $"[{VRCQuestTools.Name}] Property {property.name} type mismatch: target: {targetProp.type}, source: {property.type}.\n" +
                    $"Material: {source}, Shader: {source.shader.name}");
            }
            switch (property.type)
            {
                case MaterialProperty.PropType.Color:
                    target.SetColor(property.name, property.colorValue);
                    break;
                case MaterialProperty.PropType.Float:
                    target.SetFloat(property.name, property.floatValue);
                    break;
                case MaterialProperty.PropType.Range:
                    target.SetFloat(property.name, property.floatValue);
                    break;
                case MaterialProperty.PropType.Texture:
                    target.SetTexture(property.name, property.textureValue);
                    break;
                case MaterialProperty.PropType.Vector:
                    target.SetVector(property.name, property.vectorValue);
                    break;
#if UNITY_2021_1_OR_NEWER
                case MaterialProperty.PropType.Int:
                    target.SetInt(property.name, property.intValue);
                    break;
#endif
            }
        }

        /// <summary>
        /// Reused codes from lilInspector.cs v1.2.12 with some modification.
        /// </summary>
        /// <remarks>
        /// lilToon: Licensed under MIT License by lilxyzw. See NOTICE.txt.
        /// </remarks>
        /// <param name="material">Material to bake main textures.</param>
        /// <param name="bakeType">Bake type: 0: All.</param>
        private RenderTexture MainBake(Material material, int bakeType)
        {
            var shaderSetting = LoadShaderSetting();
            var ltsbaker = Shader.Find("Hidden/ltsother_baker");
            var defaultHSVG = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);
            var mats = new[] { material };
            var mainColor = MaterialEditor.GetMaterialProperty(mats, "_Color");
            var mainTex = MaterialEditor.GetMaterialProperty(mats, "_MainTex");
            var mainTexHSVG = MaterialEditor.GetMaterialProperty(mats, "_MainTexHSVG");
            var mainGradationStrength = MaterialEditor.GetMaterialProperty(mats, "_MainGradationStrength");
            var mainGradationTex = MaterialEditor.GetMaterialProperty(mats, "_MainGradationTex");
            var mainColorAdjustMask = MaterialEditor.GetMaterialProperty(mats, "_MainColorAdjustMask");

            var useMain2ndTex = MaterialEditor.GetMaterialProperty(mats, "_UseMain2ndTex");
            var mainColor2nd = MaterialEditor.GetMaterialProperty(mats, "_Color2nd");
            var main2ndTex = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTex");
            var main2ndTexAngle = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexAngle");
            var main2ndTexIsDecal = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexIsDecal");
            var main2ndTexIsLeftOnly = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexIsLeftOnly");
            var main2ndTexIsRightOnly = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexIsRightOnly");
            var main2ndTexShouldCopy = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexShouldCopy");
            var main2ndTexShouldFlipMirror = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexShouldFlipMirror");
            var main2ndTexShouldFlipCopy = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexShouldFlipCopy");
            var main2ndTexIsMSDF = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexIsMSDF");
            var main2ndBlendMask = MaterialEditor.GetMaterialProperty(mats, "_Main2ndBlendMask");
            var main2ndTexBlendMode = MaterialEditor.GetMaterialProperty(mats, "_Main2ndTexBlendMode");

            var useMain3rdTex = MaterialEditor.GetMaterialProperty(mats, "_UseMain3rdTex");
            var mainColor3rd = MaterialEditor.GetMaterialProperty(mats, "_Color3rd");
            var main3rdTex = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTex");
            var main3rdTexAngle = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexAngle");
            var main3rdTexIsDecal = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexIsDecal");
            var main3rdTexIsLeftOnly = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexIsLeftOnly");
            var main3rdTexIsRightOnly = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexIsRightOnly");
            var main3rdTexShouldCopy = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexShouldCopy");
            var main3rdTexShouldFlipMirror = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexShouldFlipMirror");
            var main3rdTexShouldFlipCopy = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexShouldFlipCopy");
            var main3rdTexIsMSDF = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexIsMSDF");
            var main3rdBlendMask = MaterialEditor.GetMaterialProperty(mats, "_Main3rdBlendMask");
            var main3rdTexBlendMode = MaterialEditor.GetMaterialProperty(mats, "_Main3rdTexBlendMode");

            // bool shouldBake1st = (bakeType == 1 || bakeType == 4) && mainTex.textureValue != null;
            bool shouldNotBakeColor = (bakeType == 1 || bakeType == 4) && mainColor.colorValue == Color.white && mainTexHSVG.vectorValue == defaultHSVG;
            bool cannotBake1st = mainTex.textureValue == null;
            bool shouldNotBake2nd = (bakeType == 2 || bakeType == 5) && useMain2ndTex.floatValue == 0.0;
            bool shouldNotBake3rd = (bakeType == 3 || bakeType == 6) && useMain3rdTex.floatValue == 0.0;
            bool shouldNotBakeAll = bakeType == 0 && mainColor.colorValue == Color.white && mainTexHSVG.vectorValue == defaultHSVG && useMain2ndTex.floatValue == 0.0 && useMain3rdTex.floatValue == 0.0;
            /*
            if (cannotBake1st)
            {
                return null;
            }
            else if (shouldNotBakeColor)
            {
                Debug.Log("Should not need to bake");
                return null;
            }
            else if (shouldNotBake2nd)
            {
                Debug.Log("Should not bake 2nd");
                return null;
            }
            else if (shouldNotBake3rd)
            {
                Debug.Log("Should not bake 3rd");
                return null;
            }
            else if (shouldNotBakeAll)
            {
                Debug.Log("Should not bake all");
                return null;
            }*/
            if (false)
            {
            }
            else
            {
                bool bake2nd = (bakeType == 0 || bakeType == 2 || bakeType == 5) && useMain2ndTex.floatValue != 0.0;
                bool bake3rd = (bakeType == 0 || bakeType == 3 || bakeType == 6) && useMain3rdTex.floatValue != 0.0;

                // run bake
                // Texture bufMainTexture = mainTex.textureValue;
                Material hsvgMaterial = new Material(ltsbaker);

                Texture srcTexture = AssetUtility.CreateMinimumEmptyTexture();
                Texture srcMain2 = AssetUtility.CreateMinimumEmptyTexture();
                Texture srcMain3 = AssetUtility.CreateMinimumEmptyTexture();
                Texture srcMask2 = AssetUtility.CreateMinimumEmptyTexture();
                Texture srcMask3 = AssetUtility.CreateMinimumEmptyTexture();

                CopyMaterialProperty(hsvgMaterial, material, mainColor);
                CopyMaterialProperty(hsvgMaterial, material, mainTexHSVG);
                CopyMaterialProperty(hsvgMaterial, material, mainColorAdjustMask);
                hsvgMaterial.SetFloat(mainGradationStrength.name, 0.0f);

                if (CheckFeature(shaderSetting.LIL_FEATURE_MAIN_GRADATION_MAP))
                {
                    CopyMaterialProperty(hsvgMaterial, material, mainGradationStrength);
                    CopyMaterialProperty(hsvgMaterial, material, mainGradationTex);
                }

                srcTexture = AssetUtility.LoadUncompressedTexture(material.GetTexture(mainTex.name));
                if (srcTexture != null)
                {
                    hsvgMaterial.SetTexture(mainTex.name, srcTexture);
                }
                else
                {
                    srcTexture = AssetUtility.CreateMinimumEmptyTexture();
                    hsvgMaterial.SetTexture(mainTex.name, Texture2D.whiteTexture);
                }

                if (bake2nd)
                {
                    CopyMaterialProperty(hsvgMaterial, material, useMain2ndTex);
                    CopyMaterialProperty(hsvgMaterial, material, mainColor2nd);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexAngle);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexIsDecal);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexIsLeftOnly);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexIsRightOnly);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexShouldCopy);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexShouldFlipMirror);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexShouldFlipCopy);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexIsMSDF);
                    CopyMaterialProperty(hsvgMaterial, material, main2ndTexBlendMode);
                    hsvgMaterial.SetTextureOffset(main2ndTex.name, material.GetTextureOffset(main2ndTex.name));
                    hsvgMaterial.SetTextureScale(main2ndTex.name, material.GetTextureScale(main2ndTex.name));
                    hsvgMaterial.SetTextureOffset(main2ndBlendMask.name, material.GetTextureOffset(main2ndBlendMask.name));
                    hsvgMaterial.SetTextureScale(main2ndBlendMask.name, material.GetTextureScale(main2ndBlendMask.name));

                    Object.DestroyImmediate(srcMain2);
                    srcMain2 = AssetUtility.LoadUncompressedTexture(material.GetTexture(main2ndTex.name));
                    if (srcMain2 != null)
                    {
                        hsvgMaterial.SetTexture(main2ndTex.name, srcMain2);
                    }
                    else
                    {
                        srcMain2 = AssetUtility.CreateMinimumEmptyTexture();
                        hsvgMaterial.SetTexture(main2ndTex.name, Texture2D.whiteTexture);
                    }

                    Object.DestroyImmediate(srcMask2);
                    srcMask2 = AssetUtility.LoadUncompressedTexture(material.GetTexture(main2ndBlendMask.name));
                    if (srcMask2 != null)
                    {
                        hsvgMaterial.SetTexture(main2ndBlendMask.name, srcMask2);
                    }
                    else
                    {
                        srcMask2 = AssetUtility.CreateMinimumEmptyTexture();
                        hsvgMaterial.SetTexture(main2ndBlendMask.name, Texture2D.whiteTexture);
                    }
                }

                if (bake3rd)
                {
                    CopyMaterialProperty(hsvgMaterial, material, useMain3rdTex);
                    CopyMaterialProperty(hsvgMaterial, material, mainColor3rd);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexAngle);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexIsDecal);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexIsLeftOnly);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexIsRightOnly);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexShouldCopy);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexShouldFlipMirror);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexShouldFlipCopy);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexIsMSDF);
                    CopyMaterialProperty(hsvgMaterial, material, main3rdTexBlendMode);
                    hsvgMaterial.SetTextureOffset(main3rdTex.name, material.GetTextureOffset(main3rdTex.name));
                    hsvgMaterial.SetTextureScale(main3rdTex.name, material.GetTextureScale(main3rdTex.name));
                    hsvgMaterial.SetTextureOffset(main3rdBlendMask.name, material.GetTextureOffset(main3rdBlendMask.name));
                    hsvgMaterial.SetTextureScale(main3rdBlendMask.name, material.GetTextureScale(main3rdBlendMask.name));

                    Object.DestroyImmediate(srcMain3);
                    srcMain3 = AssetUtility.LoadUncompressedTexture(material.GetTexture(main3rdTex.name));
                    if (srcMain3 != null)
                    {
                        hsvgMaterial.SetTexture(main3rdTex.name, srcMain3);
                    }
                    else
                    {
                        srcMain3 = AssetUtility.CreateMinimumEmptyTexture();
                        hsvgMaterial.SetTexture(main3rdTex.name, Texture2D.whiteTexture);
                    }

                    Object.DestroyImmediate(srcMask3);
                    srcMask3 = AssetUtility.LoadUncompressedTexture(material.GetTexture(main3rdBlendMask.name));
                    if (srcMask3 != null)
                    {
                        hsvgMaterial.SetTexture(main3rdBlendMask.name, srcMask3);
                    }
                    else
                    {
                        srcMask3 = AssetUtility.CreateMinimumEmptyTexture();
                        hsvgMaterial.SetTexture(main3rdBlendMask.name, Texture2D.whiteTexture);
                    }
                }

                var rt = new RenderTexture(srcTexture.width, srcTexture.height, 0, RenderTextureFormat.ARGB32);
                var activeRT = RenderTexture.active;
                try
                {
                    Graphics.Blit(srcTexture, rt, hsvgMaterial);
                    return rt;
                }
                finally
                {
                    RenderTexture.active = activeRT;
                }
            }
        }

        /// <summary>
        /// Additional step for emission.
        /// </summary>
        /// <param name="main">Baked main texture.</param>
        /// <param name="material">Material to bake.</param>
        private AsyncCallbackRequest EmissionBake(RenderTexture main, Material material, IToonLitConvertSettings settings, System.Action<Texture2D> completion)
        {
            var maxTextureSize = (int)settings.MaxTextureSize;
            var width = main.width;
            var height = main.height;
            if (maxTextureSize > 0)
            {
                width = System.Math.Min(maxTextureSize, width);
                height = System.Math.Min(maxTextureSize, height);
            }

            var shaderSetting = LoadShaderSetting();

            if (!shaderSetting.LIL_FEATURE_EMISSION_1ST && !shaderSetting.LIL_FEATURE_EMISSION_2ND)
            {
                return AssetUtility.RequestReadbackRenderTexture(main, true, (baked) =>
                {
                    baked.filterMode = FilterMode.Bilinear;
                });
            }

            var mats = new[] { material };
            var emissionMap = MaterialEditor.GetMaterialProperty(mats, "_EmissionMap");
            var emissionBlendMask = MaterialEditor.GetMaterialProperty(mats, "_EmissionBlendMask");
            var emissionGradTex = MaterialEditor.GetMaterialProperty(mats, "_EmissionGradTex");
            var emission2ndMap = MaterialEditor.GetMaterialProperty(mats, "_Emission2ndMap");
            var emission2ndBlendMask = MaterialEditor.GetMaterialProperty(mats, "_Emission2ndBlendMask");
            var emission2ndGradTex = MaterialEditor.GetMaterialProperty(mats, "_Emission2ndGradTex");

            using (var baker = DisposableObject.New(Object.Instantiate(material)))
            using (var srcEmissionMap = DisposableObject.New(AssetUtility.LoadUncompressedTexture(emissionMap.textureValue)))
            using (var srcEmissionBlendMask = DisposableObject.New(AssetUtility.LoadUncompressedTexture(emissionBlendMask.textureValue)))
            using (var srcEmissionGradTex = DisposableObject.New(AssetUtility.LoadUncompressedTexture(emissionGradTex.textureValue)))
            using (var srcEmission2ndMap = DisposableObject.New(AssetUtility.LoadUncompressedTexture(emission2ndMap.textureValue)))
            using (var srcEmission2ndBlendMask = DisposableObject.New(AssetUtility.LoadUncompressedTexture(emission2ndBlendMask.textureValue)))
            using (var srcEmission2ndGradTex = DisposableObject.New(AssetUtility.LoadUncompressedTexture(emission2ndGradTex.textureValue)))
            {
                var lilBaker = Shader.Find("Hidden/VRCQuestTools/lilToon");
#if UNITY_2022_1_OR_NEWER
                baker.Object.parent = null;
#endif
                baker.Object.shader = lilBaker;
                baker.Object.SetFloat("_VQT_MainTexBrightness", settings.MainTextureBrightness);
                baker.Object.SetFloat("_VQT_GenerateShadow", settings.GenerateShadowFromNormalMap ? 1 : 0);
                baker.Object.mainTexture = main;
                baker.Object.mainTextureOffset = new Vector2(0.0f, 0.0f);
                baker.Object.mainTextureScale = new Vector2(1.0f, 1.0f);
                baker.Object.color = Color.white;
                baker.Object.SetFloat("_LIL_FEATURE_NORMAL_1ST", shaderSetting.LIL_FEATURE_NORMAL_1ST ? 1.0f : 0.0f);
                baker.Object.SetFloat("_LIL_FEATURE_EMISSION_1ST", shaderSetting.LIL_FEATURE_EMISSION_1ST ? 1.0f : 0.0f);
                baker.Object.SetFloat("_LIL_FEATURE_EMISSION_2ND", shaderSetting.LIL_FEATURE_EMISSION_2ND ? 1.0f : 0.0f);
                baker.Object.SetFloat("_LIL_FEATURE_ANIMATE_EMISSION_UV", shaderSetting.LIL_FEATURE_ANIMATE_EMISSION_UV ? 1.0f : 0.0f);
                baker.Object.SetFloat("_LIL_FEATURE_ANIMATE_EMISSION_MASK_UV", shaderSetting.LIL_FEATURE_ANIMATE_EMISSION_MASK_UV ? 1.0f : 0.0f);
                baker.Object.SetFloat("_LIL_FEATURE_EMISSION_GRADATION", shaderSetting.LIL_FEATURE_EMISSION_GRADATION ? 1.0f : 0.0f);

                baker.Object.SetTexture(emissionMap.name, srcEmissionMap.Object);
                baker.Object.SetTexture(emissionBlendMask.name, srcEmissionBlendMask.Object);
                baker.Object.SetTexture(emissionGradTex.name, srcEmissionGradTex.Object);
                baker.Object.SetTexture(emission2ndMap.name, srcEmission2ndMap.Object);
                baker.Object.SetTexture(emission2ndBlendMask.name, srcEmission2ndBlendMask.Object);
                baker.Object.SetTexture(emission2ndGradTex.name, srcEmission2ndGradTex.Object);

                return AssetUtility.BakeTexture(main, baker.Object, width, height, true, completion);
            }
        }

        private bool CheckFeature(bool feature)
        {
            var isMulti = false;
            return isMulti || feature;
        }

        private class LilToonSetting
        {
            private static readonly SemVer LilToon130 = new SemVer("1.3.0");

            private Object settingObject;

            public LilToonSetting(Object obj)
            {
                settingObject = obj;
            }

            public bool LIL_FEATURE_MAIN_GRADATION_MAP => IsFeatureEnabled("LIL_FEATURE_MAIN_GRADATION_MAP");

            public bool LIL_FEATURE_NORMAL_1ST => IsFeatureEnabled("LIL_FEATURE_NORMAL_1ST");

            public bool LIL_FEATURE_EMISSION_1ST => IsFeatureEnabled("LIL_FEATURE_EMISSION_1ST");

            public bool LIL_FEATURE_EMISSION_2ND => IsFeatureEnabled("LIL_FEATURE_EMISSION_2ND");

            public bool LIL_FEATURE_ANIMATE_EMISSION_UV => IsFeatureEnabled("LIL_FEATURE_ANIMATE_EMISSION_UV");

            public bool LIL_FEATURE_ANIMATE_EMISSION_MASK_UV => IsFeatureEnabled("LIL_FEATURE_ANIMATE_EMISSION_MASK_UV");

            public bool LIL_FEATURE_EMISSION_GRADATION => IsFeatureEnabled("LIL_FEATURE_EMISSION_GRADATION");

            private bool IsFeatureEnabled(string name)
            {
                // lilToon v1.3.0 no longer needs shader setting in edit mode.
                if (AssetUtility.LilToonVersion >= LilToon130)
                {
                    return true;
                }
                return GetFieldValue<bool>(name);
            }

            private T GetFieldValue<T>(string name)
            {
                if (!AssetUtility.IsLilToonImported())
                {
                    throw new LilToonCompatibilityException("lilToon not found in Assets.");
                }
                var lilToonSetting = SystemUtility.GetTypeByName("lilToonSetting");
                if (lilToonSetting == null)
                {
                    throw new LilToonCompatibilityException($"lilToon found, but lilToonSetting not found");
                }
                var field = lilToonSetting.GetField(name);
                if (field == null)
                {
                    throw new LilToonCompatibilityException($"Field {lilToonSetting.Name}.{name} not found");
                }
                return (T)field.GetValue(settingObject);
            }
        }

        private class LilToonInspector
        {
            public static string GetShaderSettingPath()
            {
                return Invoke<string>("GetShaderSettingPath");
            }

            private static T Invoke<T>(string name)
                where T : class
            {
                if (!AssetUtility.IsLilToonImported())
                {
                    throw new LilToonCompatibilityException("lilToon not found in Assets");
                }
                var lilToonInspector = SystemUtility.GetTypeByName("lilToon.lilToonInspector");
                if (lilToonInspector == null)
                {
                    throw new LilToonCompatibilityException("lilToon found, but lilToon.lilToonInspector not found");
                }
                var method = lilToonInspector.GetMethod(name);
                if (method == null)
                {
                    throw new LilToonCompatibilityException($"{lilToonInspector.Name}.{name} not found");
                }
                return method.Invoke(null, null) as T;
            }
        }

        private class LilToonCompatibilityException : System.Exception
        {
            public LilToonCompatibilityException(string message)
                : base($"Compatibility issue for lilToon {AssetUtility.LilToonVersion}. Please report this error: " + message)
            {
            }
        }
    }
}
