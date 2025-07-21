using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Settings for standard lite material conversion.
    /// </summary>
    [Serializable]
    public class ToonStandardConvertSettings : IMaterialConvertSettings
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
        /// Whether to generate shadow ramp textures.
        /// </summary>
        public bool generateShadowRamp = true;

        /// <summary>
        /// Shadow fallback type.
        /// </summary>
        public Texture2D fallbackShadowRamp;

        /// <summary>
        /// Whether to use normal map.
        /// </summary>
        public bool useNormalMap = true;

        /// <summary>
        /// Whether to use emission texture.
        /// </summary>
        public bool useEmission = true;

        /// <summary>
        /// Whether to use occlusion texture.
        /// </summary>
        public bool useOcclusion = true;

        /// <summary>
        /// Whether to use specular features.
        /// </summary>
        public bool useSpecular = true;

        /// <summary>
        /// Whether to use matcap texture.
        /// </summary>
        public bool useMatcap = true;

        /// <summary>
        /// Whether to use rim lighting.
        /// </summary>
        public bool useRimLighting = true;

        private static Lazy<FieldInfo[]> unitySerializableFields = new Lazy<FieldInfo[]>(() => GetUnitySerializableFields(typeof(ToonStandardConvertSettings)));

        /// <inheritdoc/>
        public MobileTextureFormat MobileTextureFormat => mobileTextureFormat;

        /// <summary>
        /// Gets a default instance of <see cref="ToonStandardConvertSettings"/> with all features disabled.
        /// </summary>
        public static ToonStandardConvertSettings SimpleFeatures
        {
            get
            {
                var settings = new ToonStandardConvertSettings
                {
                    generateShadowRamp = true,
                };
                settings.SetAllFeatures(false);
                return settings;
            }
        }

        /// <summary>
        /// Enables or disables all features of the settings.
        /// </summary>
        /// <param name="value"></param>
        public void SetAllFeatures(bool value)
        {
            useNormalMap = value;
            useEmission = value;
            useOcclusion = value;
            useSpecular = value;
            useMatcap = value;
            useRimLighting = value;
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
                if (value is Texture texture && texture)
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

        /// <inheritdoc/>
        public void LoadDefaultAssets()
        {
            SetAllFeatures(false);
#if UNITY_EDITOR
            // RealisticVerySoft shadow
            var path = AssetDatabase.GUIDToAssetPath("5f304bf7a07313d43b8562d9eabce646");
            fallbackShadowRamp = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
#endif
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
