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
    /// Feature selection mode for ToonStandard conversion.
    /// </summary>
    public enum ToonStandardFeaturesMode
    {
        /// <summary>Opt-in: features are disabled by default; user enables individually.</summary>
        OptIn,

        /// <summary>Opt-out: features are enabled by default; user disables individually.</summary>
        OptOut,
    }

    /// <summary>
    /// Marks a field as a ToonStandard feature toggle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ToonStandardFeatureAttribute : Attribute
    {
        /// <summary>Gets the display order of this feature.</summary>
        public int Order { get; }

        /// <summary>Gets the serialization version in which this feature was introduced.</summary>
        public int IntroVersion { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToonStandardFeatureAttribute"/> class.
        /// </summary>
        /// <param name="order">Display order.</param>
        /// <param name="introVersion">Serialization version when this feature was introduced.</param>
        public ToonStandardFeatureAttribute(int order, int introVersion = 0)
        {
            Order = order;
            IntroVersion = introVersion;
        }
    }

    /// <summary>
    /// Settings for standard lite material conversion.
    /// </summary>
    [Serializable]
    public class ToonStandardConvertSettings : IMaterialConvertSettings, ISerializationCallbackReceiver
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
        public MobileTextureFormat mobileTextureFormat = MobileTextureFormat.NoOverride;

        /// <summary>
        /// Max texture size for mask textures.
        /// </summary>
        public TextureSizeLimit maskMaxTextureSize = TextureSizeLimit.Max512x512;

        /// <summary>
        /// Texture format for mask textures on android.
        /// </summary>
        public MobileTextureFormat maskMobileTextureFormat = MobileTextureFormat.NoOverride;

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
        [ToonStandardFeature(0)]
        public bool useNormalMap = true;

        /// <summary>
        /// Whether to use emission texture.
        /// </summary>
        [ToonStandardFeature(1)]
        public bool useEmission = true;

        /// <summary>
        /// Whether to use occlusion texture.
        /// </summary>
        [ToonStandardFeature(2)]
        public bool useOcclusion = true;

        /// <summary>
        /// Whether to use specular features.
        /// </summary>
        [ToonStandardFeature(3)]
        public bool useSpecular = true;

        /// <summary>
        /// Whether to use matcap texture.
        /// </summary>
        [ToonStandardFeature(4)]
        public bool useMatcap = true;

        /// <summary>
        /// Whether to use rim lighting.
        /// </summary>
        [ToonStandardFeature(5)]
        public bool useRimLighting = true;

        /// <summary>Current serialization schema version.</summary>
        private const int CurrentVersion = 0;

        /// <summary>
        /// Feature selection mode.
        /// </summary>
        public ToonStandardFeaturesMode featureMode = ToonStandardFeaturesMode.OptIn;

        /// <summary>Serialized schema version for forward compatibility.</summary>
        [SerializeField]
        private int serializedVersion = 0;

        private static readonly string[] cacheKeyExcludedFieldNames = { nameof(featureMode), nameof(serializedVersion) };

        private static readonly Lazy<FieldInfo[]> unitySerializableFields = new Lazy<FieldInfo[]>(() =>
            GetUnitySerializableFields(typeof(ToonStandardConvertSettings))
                .Where(f => !cacheKeyExcludedFieldNames.Contains(f.Name))
                .ToArray());

        private static readonly Lazy<FieldInfo[]> featureFields = new Lazy<FieldInfo[]>(() =>
            typeof(ToonStandardConvertSettings)
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(f => f.IsDefined(typeof(ToonStandardFeatureAttribute), inherit: false))
                .OrderBy(f => f.GetCustomAttribute<ToonStandardFeatureAttribute>().Order)
                .ToArray());

        private static readonly Lazy<IReadOnlyList<string>> featurePropertyNamesLazy = new Lazy<IReadOnlyList<string>>(() =>
            featureFields.Value.Select(f => f.Name).ToList().AsReadOnly());

        /// <summary>Gets feature field names in display order.</summary>
        public static IReadOnlyList<string> FeaturePropertyNames => featurePropertyNamesLazy.Value;

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

        /// <inheritdoc/>
        public MobileTextureFormat MobileTextureFormat => mobileTextureFormat;

        /// <summary>
        /// Enables or disables all features of the settings.
        /// </summary>
        /// <param name="value">true or false.</param>
        public void SetAllFeatures(bool value)
        {
            foreach (var field in featureFields.Value)
            {
                field.SetValue(this, value);
            }
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
            featureMode = ToonStandardFeaturesMode.OptIn;
            SetAllFeatures(false);
            serializedVersion = CurrentVersion;
#if UNITY_EDITOR
            // RealisticVerySoft shadow
            var path = AssetDatabase.GUIDToAssetPath("5f304bf7a07313d43b8562d9eabce646");
            fallbackShadowRamp = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
#endif
        }

        /// <inheritdoc/>
        public void OnBeforeSerialize()
        {
            serializedVersion = CurrentVersion;
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
        {
            foreach (var field in featureFields.Value)
            {
                var attr = field.GetCustomAttribute<ToonStandardFeatureAttribute>();
                if (serializedVersion < attr.IntroVersion)
                {
                    field.SetValue(this, featureMode == ToonStandardFeaturesMode.OptOut);
                }
            }
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
