using ImageMagick;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    public enum ShaderCategory {
        Generic,
        Arktoon,
        ArktoonEmissiveFreak,
        VRCMobile
    }

    public static class MaterialUtils
    {
        public static ShaderCategory GetShaderCategory(Material material)
        {
            var shaderName = material.shader.name;
            if (shaderName.StartsWith("arktoon/_Extra/EmissiveFreak/"))
            {
                return ShaderCategory.ArktoonEmissiveFreak;
            }
            if (shaderName.StartsWith("arktoon/"))
            {
                return ShaderCategory.Arktoon;
            }
            if (shaderName.StartsWith("VRChat/Mobile/"))
            {
                return ShaderCategory.VRCMobile;
            }
            return ShaderCategory.Generic;
        }

        public static MagickImage GetCompositedImage(Material material)
        {
            using (var main = GetMainImage(material))
            using (var emission = GetEmissionImage(material))
            using (var emissiveFreak = GetCompositedEmissiveFreakImage(material))
            {
                if (emission != null)
                {
                    var compose = ImgProc.Screen(main, emission);
                    if (emissiveFreak != null)
                    {
                        var compose2 = ImgProc.Screen(compose, emissiveFreak);
                        compose.Dispose();
                        return compose2;
                    }
                    return compose;
                }
                return new MagickImage(main);
            }
        }

        public static MagickImage GetMainImage(Material material)
        {
            return new MagickImage(AssetDatabase.GetAssetPath(material.mainTexture));
        }

        public static MagickImage GetEmissionImage(Material material)
        {
            if (UseEmission(material))
            {
                var emissionTexture = GetEmissionTexture(material);
                var emissionColor = GetEmissionColor(material);
                using (var emission = emissionTexture != null ?
                    new MagickImage(AssetDatabase.GetAssetPath(emissionTexture)) :
                    new MagickImage(MagickColors.White, 2, 2))
                {
                    return ImgProc.Multiply(emission, emissionColor);
                }
            }
            return null;
        }

        public static MagickImage GetCompositedEmissiveFreakImage(Material material)
        {
            if (!UseEmissiveFreak(material))
            {
                return null;
            }
            using (var emission1 = GetEmissiveFreakImage(material, 0))
            using (var emission2 = GetEmissiveFreakImage(material, 1))
            {
                if(IsColorOnlyImage(emission1) && !IsColorOnlyImage(emission2))
                {
                    emission1.Resize(emission2.Width, emission2.Height);
                }
                else if (!IsColorOnlyImage(emission1) && IsColorOnlyImage(emission2))
                {
                    emission2.Resize(emission1.Width, emission1.Height);
                }
                return ImgProc.Screen(emission1, emission2);
            }
        }

        public static MagickImage GetEmissiveFreakImage(Material material, int index)
        {
            if (UseEmissiveFreak(material))
            {
                var emissionTexture = GetEmissionTexture(material);
                var emissionColor = GetEmissionColor(material);
                using (var emission = emissionTexture != null ?
                    new MagickImage(AssetDatabase.GetAssetPath(emissionTexture)) :
                    new MagickImage(MagickColors.White, 2, 2))
                {
                    return ImgProc.Multiply(emission, emissionColor);
                }

            }
            return null;
        }

        private static bool IsColorOnlyImage(MagickImage image)
        {
            return image.Width == 2 && image.Height == 2;
        }

        private static bool UseEmission(Material material)
        {
            var category = GetShaderCategory(material);
            switch (category)
            {
                case ShaderCategory.Generic:
                    return material.shaderKeywords.Contains("_EMISSION");
                case ShaderCategory.Arktoon:
                    return true;
                case ShaderCategory.ArktoonEmissiveFreak:
                    return true;
                case ShaderCategory.VRCMobile:
                    return false;
                default:
                    return false;
            }
        }

        private static bool UseEmissiveFreak(Material material)
        {
            return GetShaderCategory(material) == ShaderCategory.ArktoonEmissiveFreak;
        }

        private static Texture GetEmissionTexture(Material material)
        {
            return material.GetTexture("_EmissionMap");
        }

        private static Color32 GetEmissionColor(Material material)
        {
            return material.GetColor("_EmissionColor");
        }

        private static Texture GetEmissiveFreakTexture(Material material, uint index)
        {
            return material.GetTexture($"_EmissiveFreak{index}Tex");
        }

        private static Color32 GetEmissiveFreakColor(Material material, uint index)
        {
            return material.GetColor($"_EmissiveFreak{index}Color");
        }
    }
}
