using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material convert setting for MatCap Lit shader.
    /// </summary>
    [Serializable]
    public class MatCapLitConvertSettings : IMaterialConvertSettings, IToonLitConvertSettings
    {
        /// <summary>
        /// Whether to generate quest textures.
        /// </summary>
        public bool generateQuestTextures = true;

        /// <summary>
        /// Max texture size for quest.
        /// </summary>
        public TextureSizeLimit maxTextureSize = TextureSizeLimit.Max1024x1024;

        /// <summary>
        /// Texture format for android.
        /// </summary>
        public MobileTextureFormat mobileTextureFormat = MobileTextureFormat.ASTC_6x6;

        /// <summary>
        /// Texture brightness for quest. [0-1].
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float mainTextureBrightness = 0.83f;

        /// <summary>
        /// Whether to generate shadow from normal map.
        /// </summary>
        public bool generateShadowFromNormalMap = true;

        /// <summary>
        /// MatCap texture.
        /// </summary>
        public Texture matCapTexture;

        private static Lazy<FieldInfo[]> unitySerializableFields = new Lazy<FieldInfo[]>(() => GetUnitySerializableFields(typeof(MatCapLitConvertSettings)));

        /// <inheritdoc/>
        public bool GenerateQuestTextures => generateQuestTextures;

        /// <inheritdoc/>
        public TextureSizeLimit MaxTextureSize => maxTextureSize;

        /// <inheritdoc/>
        public MobileTextureFormat MobileTextureFormat => mobileTextureFormat;

        /// <inheritdoc/>
        public float MainTextureBrightness => mainTextureBrightness;

        /// <inheritdoc/>
        public bool GenerateShadowFromNormalMap => generateShadowFromNormalMap;

        /// <inheritdoc/>
        public void LoadDefaultAssets()
        {
            // nothing.
        }

        /// <inheritdoc/>
        public string GetCacheKey()
        {
            var container = new Dictionary<string, object>();
            var sb = new StringBuilder("{");
            foreach (var (field, i) in unitySerializableFields.Value.Select((f, i) => (f, i)))
            {
                var value = field.GetValue(this);
                sb.Append("\"" + field.Name + "\"");
                sb.Append(":");

                object valueObject;
                if (value is Texture texture)
                {
#if UNITY_EDITOR
                    valueObject = texture.imageContentsHash;
#else
                    valueObject = texture.GetInstanceID();
#endif
                }
                else
                {
                    valueObject = value;
                }
                sb.Append("\"" + valueObject + "\"");
                sb.Append(",");
            }
            sb.Append("}");
            var str = sb.ToString();
            return str;
        }

        private static FieldInfo[] GetUnitySerializableFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field =>
                    !field.IsStatic &&
                    !field.IsDefined(typeof(NonSerializedAttribute), inherit: true) &&
                    (field.IsPublic || field.IsDefined(typeof(SerializeField), inherit: true)))
                .ToArray();
        }
    }
}
