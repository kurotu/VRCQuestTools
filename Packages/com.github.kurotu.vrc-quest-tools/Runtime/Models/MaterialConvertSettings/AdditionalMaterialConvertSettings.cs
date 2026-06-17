using System;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Represents additional material convert settings.
    /// </summary>
    [Serializable]
    public class AdditionalMaterialConvertSettings
    {
        /// <summary>
        /// Target material to convert.
        /// </summary>
        [SerializeField]
        public Material targetMaterial = null;

        /// <summary>
        /// Material convert settings.
        /// </summary>
        [SerializeReference]
        public IMaterialConvertSettings materialConvertSettings = new ToonLitConvertSettings();

        /// <summary>
        /// Load default assets for the material convert settings.
        /// </summary>
        public void LoadDefaultAssets()
        {
            materialConvertSettings.LoadDefaultAssets();
        }

        /// <summary>
        /// Gets the cache key for the additional material convert settings.
        /// </summary>
        /// <returns>Cache key.</returns>
        public string GetCacheKey()
        {
            var materialKey = targetMaterial != null ? GetContentCacheKey(targetMaterial) : "null";
            return $"{materialKey}_{materialConvertSettings.GetCacheKey()}";
        }

        /// <summary>
        /// Get content cache key for material.
        /// </summary>
        /// <param name="material">Target material.</param>
        /// <returns>Cache key.</returns>
        private static string GetContentCacheKey(Material material)
        {
            var sb = new StringBuilder(material.shader.name);

            sb.Append("LocalKeywords_");
            foreach (var keyword in material.shaderKeywords)
            {
                sb.Append(keyword);
            }

            var propertyCount = material.shader.GetPropertyCount();
            for (int i = 0; i < propertyCount; i++)
            {
                var type = material.shader.GetPropertyType(i);
                var name = material.shader.GetPropertyName(i);
                switch (type)
                {
                    case ShaderPropertyType.Color:
                        sb.Append($"{name}_{material.GetColor(name)}");
                        break;
                    case ShaderPropertyType.Vector:
                        sb.Append($"{name}_{material.GetVector(name)}");
                        break;
                    case ShaderPropertyType.Float:
                        sb.Append($"{name}_{material.GetFloat(name)}");
                        break;
                    case ShaderPropertyType.Range:
                        sb.Append($"{name}__{material.GetFloat(name)}");
                        break;
                    case ShaderPropertyType.Texture:
                        var tex = material.GetTexture(name);
                        var hash = GetImageContentsHash(tex);
                        sb.Append($"{name}_{hash}");
                        break;
                    case ShaderPropertyType.Int:
                        sb.Append($"{name}_{material.GetInteger(name)}");
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Requests the image content hash of a texture.
        /// </summary>
        /// <param name="texture">The texture to compute the content hash for.</param>
        /// <returns>Contents Hash.</returns>
        private static Hash128 GetImageContentsHash(Texture texture)
        {
            switch (texture)
            {
                case null:
                    return default;
                case RenderTexture:
                    // RenderTexture's imageContentsHash is 0, so we generate a random hash.
                    return Hash128.Compute(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
                default:
#if UNITY_EDITOR
                    return texture.imageContentsHash;
#else
                    return Hash128.Compute(texture.GetInstanceID());
#endif
            }
        }
    }
}
