using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility class for handling textures.
    /// </summary>
    internal static class TextureUtility
    {
        private const int MinimumTextureSize = 4; // needs 4x4 for DXT.

        private static readonly HashSet<TextureFormat> UncompressedFormats = new HashSet<TextureFormat>
        {
            TextureFormat.Alpha8,
            TextureFormat.ARGB32,
            TextureFormat.ARGB4444,
            TextureFormat.BGRA32,
            TextureFormat.R16,
            TextureFormat.R8,
            TextureFormat.RFloat,
            TextureFormat.RG16,
            TextureFormat.RG32,
            TextureFormat.RGB24,
            TextureFormat.RGB48,
            TextureFormat.RGB565,
            TextureFormat.RGB9e5Float,
            TextureFormat.RGBA32,
            TextureFormat.RGBA4444,
            TextureFormat.RGBA64,
            TextureFormat.RGBAFloat,
            TextureFormat.RGBAHalf,
            TextureFormat.RGFloat,
            TextureFormat.RGHalf,
            TextureFormat.RHalf,
            TextureFormat.YUY2,
        };

        // https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextureImporterOverride.html
        private static readonly HashSet<TextureFormat> WindowsFormats = new HashSet<TextureFormat>
        {
            TextureFormat.BC4,
            TextureFormat.BC5,
            TextureFormat.BC6H,
            TextureFormat.BC7,
            TextureFormat.DXT1,
            TextureFormat.DXT1Crunched,
            TextureFormat.DXT5,
            TextureFormat.DXT5Crunched,
        };

        // https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextureImporterOverride.html
        // Contains "partial" formats to simplify.
        private static readonly HashSet<TextureFormat> AndroidFormats = new HashSet<TextureFormat>
        {
            TextureFormat.ASTC_4x4,
            TextureFormat.ASTC_5x5,
            TextureFormat.ASTC_6x6,
            TextureFormat.ASTC_8x8,
            TextureFormat.ASTC_10x10,
            TextureFormat.ASTC_12x12,
            TextureFormat.ASTC_HDR_4x4,
            TextureFormat.ASTC_HDR_5x5,
            TextureFormat.ASTC_HDR_6x6,
            TextureFormat.ASTC_HDR_8x8,
            TextureFormat.ASTC_HDR_10x10,
            TextureFormat.ASTC_HDR_12x12,
            TextureFormat.ETC2_RGB,
            TextureFormat.ETC2_RGBA1,
            TextureFormat.ETC2_RGBA8,
            TextureFormat.ETC2_RGBA8Crunched,
            TextureFormat.ETC_RGB4,
            TextureFormat.ETC_RGB4Crunched,
            TextureFormat.EAC_R,
            TextureFormat.EAC_R_SIGNED,
            TextureFormat.EAC_RG,
            TextureFormat.EAC_RG_SIGNED,
        };

        // https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextureImporterOverride.html
        // Contains "partial" formats to simplify.
        private static readonly HashSet<TextureFormat> IosFormats = new HashSet<TextureFormat>
        {
            TextureFormat.ASTC_4x4,
            TextureFormat.ASTC_5x5,
            TextureFormat.ASTC_6x6,
            TextureFormat.ASTC_8x8,
            TextureFormat.ASTC_10x10,
            TextureFormat.ASTC_12x12,
            TextureFormat.ASTC_HDR_4x4,
            TextureFormat.ASTC_HDR_5x5,
            TextureFormat.ASTC_HDR_6x6,
            TextureFormat.ASTC_HDR_8x8,
            TextureFormat.ASTC_HDR_10x10,
            TextureFormat.ASTC_HDR_12x12,
            TextureFormat.ETC2_RGB,
            TextureFormat.ETC2_RGBA1,
            TextureFormat.ETC2_RGBA8,
            TextureFormat.ETC2_RGBA8Crunched,
            TextureFormat.ETC_RGB4,
            TextureFormat.ETC_RGB4Crunched,
            TextureFormat.EAC_R,
            TextureFormat.EAC_R_SIGNED,
            TextureFormat.EAC_RG,
            TextureFormat.EAC_RG_SIGNED,
            TextureFormat.PVRTC_RGB2,
            TextureFormat.PVRTC_RGB4,
            TextureFormat.PVRTC_RGBA2,
            TextureFormat.PVRTC_RGBA4,
        };

        /// <summary>
        /// Creates a minimum empty texture.
        /// </summary>
        /// <returns>Created texture object.</returns>
        internal static Texture2D CreateMinimumEmptyTexture()
        {
            return new Texture2D(MinimumTextureSize, MinimumTextureSize);
        }

        /// <summary>
        /// Saves Texture2D as png asset.
        /// </summary>
        /// <param name="path">Path to save.</param>
        /// <param name="texture">Texture to save.</param>
        /// <param name="mobileFormat">Texture format for mobile build target.</param>
        /// <param name="isSRGB">Texture is sRGB.</param>
        /// <returns>Saved texture asset.</returns>
        internal static Texture2D SaveUncompressedTexture(string path, Texture2D texture, TextureFormat? mobileFormat, bool isSRGB = true)
        {
            var src = texture.isReadable ? texture : CopyAsReadable(texture, isSRGB);
            var png = src.EncodeToPNG();
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path);
            ConfigureTextureImporter(path, mobileFormat, isSRGB);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>
        /// Configures TextureImporter for the texture.
        /// </summary>
        /// <param name="path">Texture path.</param>
        /// <param name="mobileFormat">Texture format for mobile build target.</param>
        /// <param name="isSRGB">Texture is sRGB.</param>
        internal static void ConfigureTextureImporter(string path, TextureFormat? mobileFormat, bool isSRGB = true)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = isSRGB;
            importer.sRGBTexture = isSRGB;
            if (importer.mipmapEnabled)
            {
                importer.streamingMipmaps = true;
            }
            if (mobileFormat.HasValue)
            {
                var androidSettings = new TextureImporterPlatformSettings
                {
                    name = "Android",
                    overridden = true,
                    maxTextureSize = importer.maxTextureSize,
                    format = (TextureImporterFormat)mobileFormat.Value,
                };
                var iosSettings = new TextureImporterPlatformSettings
                {
                    name = "iPhone",
                    overridden = androidSettings.overridden,
                    maxTextureSize = androidSettings.maxTextureSize,
                    format = androidSettings.format,
                };
                importer.SetPlatformTextureSettings(androidSettings);
                importer.SetPlatformTextureSettings(iosSettings);
            }
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Configure TextureImporter for normal map.
        /// </summary>
        /// <param name="path">Texture path.</param>
        /// <param name="mobileFormat">Texture format for mobile build target.</param>
        internal static void ConfigureNormalMapImporter(string path, TextureFormat? mobileFormat)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.NormalMap;
            importer.alphaIsTransparency = false;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            if (mobileFormat.HasValue)
            {
                var androidSettings = new TextureImporterPlatformSettings
                {
                    name = "Android",
                    overridden = true,
                    maxTextureSize = importer.maxTextureSize,
                    format = (TextureImporterFormat)mobileFormat.Value,
                };
                var iosSettings = new TextureImporterPlatformSettings
                {
                    name = "iPhone",
                    overridden = androidSettings.overridden,
                    maxTextureSize = androidSettings.maxTextureSize,
                    format = androidSettings.format,
                };
                importer.SetPlatformTextureSettings(androidSettings);
                importer.SetPlatformTextureSettings(iosSettings);
            }
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Loads uncompressed image as Texture2D.
        /// </summary>
        /// <param name="texture">original texture.</param>
        /// <returns>Loaded texture.</returns>
        internal static Texture LoadUncompressedTexture(Texture texture)
        {
            if (texture == null)
            {
                return null;
            }

            if (texture.GetType() == typeof(RenderTexture))
            {
                return CreateColorTexture(Color.black);
            }

            var path = AssetDatabase.GetAssetPath(texture);
            if (path == "Resources/unity_builtin_extra")
            {
                return (Texture2D)UnityEngine.Object.Instantiate(texture);
            }

            if (!(texture is Texture2D))
            {
                return UnityEngine.Object.Instantiate(texture);
            }

            // unsaved asset
            if (string.IsNullOrEmpty(path))
            {
                return texture;
            }

            // already saved as an asset file
            var extension = Path.GetExtension(path).ToLower();
            if (extension == ".asset")
            {
                return texture;
            }

            var tex2 = LoadUncompressedTexture(path, false);
            tex2.wrapMode = texture.wrapMode;
            return tex2;
        }

        /// <summary>
        /// Loads uncompressed image as Texture2D.
        /// </summary>
        /// <see href="https://github.com/lilxyzw/lilToon/issues/17">lilxyzw/lilToon#17.</see>
        /// <param name="path">path to image.</param>
        /// <param name="makeReadable">instanciate a readable one.</param>
        /// <returns>Loaded texture.</returns>
        internal static Texture2D LoadUncompressedTexture(string path, bool makeReadable)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var extension = Path.GetExtension(path).ToLower();
            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
            {
                var isLinear = !importer.sRGBTexture;
                var tex = new Texture2D(MinimumTextureSize, MinimumTextureSize, TextureFormat.RGBA32, Texture.GenerateAllMips, linear: isLinear);
                var bytes = File.ReadAllBytes(Path.GetFullPath(path));
                tex.LoadImage(bytes);
                tex.filterMode = FilterMode.Bilinear;
                return tex;
            }

            if (!makeReadable)
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            const string AndroidPlatform = "Android";
            const string StandalonePlatform = "Standalone";
            var isReadable = importer.isReadable;
            var textureCompression = importer.textureCompression;
            var standaloneTextureSettings = importer.GetPlatformTextureSettings(StandalonePlatform);
            var androidTextureSettings = importer.GetPlatformTextureSettings(AndroidPlatform);

            // Set uncompressed settings.
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (standaloneTextureSettings.overridden)
            {
                var tmp = new TextureImporterPlatformSettings();
                standaloneTextureSettings.CopyTo(tmp);
                tmp.overridden = false;
                importer.SetPlatformTextureSettings(tmp);
            }
            if (androidTextureSettings.overridden)
            {
                var tmp = new TextureImporterPlatformSettings();
                androidTextureSettings.CopyTo(tmp);
                tmp.overridden = false;
                importer.SetPlatformTextureSettings(tmp);
            }
            importer.SaveAndReimport();

            var psd = AssetDatabase.LoadAssetAtPath<Texture>(path);
            var ret = UnityEngine.Object.Instantiate(psd);

            // Restore compression settings.
            importer.isReadable = isReadable;
            importer.textureCompression = textureCompression;
            if (standaloneTextureSettings.overridden)
            {
                importer.SetPlatformTextureSettings(standaloneTextureSettings);
            }
            if (androidTextureSettings.overridden)
            {
                importer.SetPlatformTextureSettings(androidTextureSettings);
            }
            importer.SaveAndReimport();

            if (!(ret is Texture2D))
            {
                throw new ArgumentException($"{path} is {ret.GetType().Name}");
            }

            return (Texture2D)ret;
        }

        /// <summary>
        /// Creates a single color texture.
        /// </summary>
        /// <param name="color">Color to use.</param>
        /// <param name="width">Texture width.</param>
        /// <param name="height">Texture height.</param>
        /// <returns>Created texture.</returns>
        internal static Texture2D CreateColorTexture(Color32 color, int width, int height)
        {
            var tex = new Texture2D(width, height);
            var pixels = tex.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels32(pixels);
            return tex;
        }

        /// <summary>
        /// Copy a texture as readable.
        /// </summary>
        /// <param name="texture">Texture to copy.</param>
        /// <param name="isDataSRGB">Texture is sRGB.</param>
        /// <returns>Readable texture.</returns>
        internal static Texture2D CopyAsReadable(Texture2D texture, bool isDataSRGB)
        {
            var copy = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1, isDataSRGB);
            var data = texture.GetRawTextureData();
            copy.LoadRawTextureData(data);
            copy.Apply(); // Do not use arguments to keep readable.
            return copy;
        }

        /// <summary>
        /// Bake a texture to a new one.
        /// </summary>
        /// <param name="input">Input texture.</param>
        /// <param name="isDataSRGB">Texture is sRGB.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <param name="useMipmap">Use mip map.</param>
        /// <param name="material">Material to bake with Graphics.Blit.</param>
        /// <param name="completion">Completion action.</param>
        /// <returns>Request to wait.</returns>
        internal static AsyncCallbackRequest BakeTexture(Texture input, bool isDataSRGB, int width, int height, bool useMipmap, Material material, Action<Texture2D> completion)
        {
            var desc = new RenderTextureDescriptor(input.width, input.height, RenderTextureFormat.ARGB32, 0, useMipmap ? input.mipmapCount : 1);
            desc.sRGB = isDataSRGB;

            var rt = RenderTexture.GetTemporary(desc);

            desc.width = width;
            desc.height = height;
            var rt2 = RenderTexture.GetTemporary(desc);
            var renderTextures = new List<RenderTexture> { rt, rt2 };

            var activeRT = RenderTexture.active;
            try
            {
                RenderTexture.active = rt;
                if (material)
                {
                    Graphics.Blit(input, rt, material);
                }
                else
                {
                    Graphics.Blit(input, rt);
                }

                DownscaleBlit(rt, isDataSRGB, rt2);

                return RequestReadbackRenderTexture(rt2, useMipmap, (result) =>
                {
                    foreach (var r in renderTextures)
                    {
                        RenderTexture.ReleaseTemporary(r);
                    }
                    completion?.Invoke(result);
                });
            }
            finally
            {
                RenderTexture.active = activeRT;
            }
        }

        /// <summary>
        /// Downscale a texture to a smaller one.
        /// </summary>
        /// <param name="input">Input texture.</param>
        /// <param name="isDataSRGB">Texture is sRGB.</param>
        /// <param name="output">RenderTexture to write output.</param>
        internal static void DownscaleBlit(Texture input, bool isDataSRGB, RenderTexture output)
        {
            var width = output.width;
            var height = output.height;
            var desc = new RenderTextureDescriptor(input.width, input.height, RenderTextureFormat.ARGB32, 0);
            desc.sRGB = isDataSRGB;

            var rt = RenderTexture.GetTemporary(desc);
            var renderTextures = new List<RenderTexture> { rt };

            var prev = RenderTexture.active;
            try
            {
                Graphics.Blit(input, rt);
                while (desc.width > width || desc.height > height)
                {
                    desc.width /= 2;
                    desc.height /= 2;
                    if (desc.width < width || desc.height < height)
                    {
                        desc.width = width;
                        desc.height = height;
                    }
                    var rt2 = RenderTexture.GetTemporary(desc);
                    RenderTexture.active = rt2;
                    Graphics.Blit(renderTextures.Last(), rt2);
                    renderTextures.Add(rt2);
                }
                Graphics.Blit(renderTextures.Last(), output);
            }
            finally
            {
                foreach (var r in renderTextures)
                {
                    RenderTexture.ReleaseTemporary(r);
                }
                RenderTexture.active = prev;
            }
        }

        /// <summary>
        /// Request readback of a render texture.
        /// </summary>
        /// <param name="renderTexture">Render texture to readback.</param>
        /// <param name="useMipmap">Whether to use mip map for the result texture.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Readback request to wait.</returns>
        internal static AsyncCallbackRequest RequestReadbackRenderTexture(RenderTexture renderTexture, bool useMipmap, Action<Texture2D> completion)
        {
            if (ShouldUseAsyncGPUReadback())
            {
                return new TextureGPUReadbackRequest(renderTexture, useMipmap, completion);
            }
            else
            {
                return new TextureCPUReadbackRequest(renderTexture, useMipmap, completion);
            }
        }

        /// <summary>
        /// Request readback of a render texture.
        /// </summary>
        /// <param name="renderTexture">Render texture to readback.</param>
        /// <param name="useMipmap">Whether to use mip map for the result texture.</param>
        /// <param name="linear">Whether to use linear color space for the result texture.</param>
        /// <param name="completion">Completion callback.</param>
        /// <returns>Readback request to wait.</returns>
        internal static AsyncCallbackRequest RequestReadbackRenderTexture(RenderTexture renderTexture, bool useMipmap, bool linear, Action<Texture2D> completion)
        {
            if (ShouldUseAsyncGPUReadback())
            {
                return new TextureGPUReadbackRequest(renderTexture, useMipmap, linear, completion);
            }
            else
            {
                return new TextureCPUReadbackRequest(renderTexture, useMipmap, linear, completion);
            }
        }

        /// <summary>
        /// Resizes a texture to desired size.
        /// </summary>
        /// <param name="texture">Texture to resize.</param>
        /// <param name="isDataSRGB">Texture is sRGB.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="completion">Completion action.</param>
        /// <returns>Request to wait.</returns>
        internal static AsyncCallbackRequest ResizeTexture(Texture2D texture, bool isDataSRGB, int width, int height, Action<Texture2D> completion)
        {
            return BakeTexture(texture, isDataSRGB, width, height, texture.mipmapCount > 1, null, completion);
        }

        /// <summary>
        /// Compresses a texture for the build target.
        /// </summary>
        /// <param name="texture">Texture to compress.</param>
        /// <param name="buildTarget">Build target. Usually it's EditorUserBuildSettings.activeBuildTarget.</param>
        /// <param name="mobileFormat">Format for mobile build target.</param>
        internal static void CompressTextureForBuildTarget(Texture2D texture, UnityEditor.BuildTarget buildTarget, TextureFormat mobileFormat)
        {
            var isMobile = buildTarget == UnityEditor.BuildTarget.Android || buildTarget == UnityEditor.BuildTarget.iOS;
            var format = isMobile ? mobileFormat : TextureFormat.DXT5;
            if (format == TextureFormat.DXT5)
            {
                if (texture.width % 4 != 0 || texture.height % 4 != 0)
                {
                    Debug.LogWarning($"[{VRCQuestTools.Name}] Texture {texture.name} is not a multiple of 4. Not compressed to DXT5.", texture);
                    return;
                }
            }
            EditorUtility.CompressTexture(texture, format, TextureCompressionQuality.Best);
        }

        /// <summary>
        /// Compresses a normal map texture.
        /// </summary>
        /// <param name="texture">Normal map texture (RGB).</param>
        /// <param name="buildTarget">Build target. Usually it's EditorUserBuildSettings.activeBuildTarget.</param>
        /// <param name="mobileFormat">Format for mobile build target.</param>
        /// <param name="readable">Whether to make output texture readable.</param>
        /// <returns>Compressed normal map.</returns>
        internal static Texture2D CompressNormalMap(Texture2D texture, UnityEditor.BuildTarget buildTarget, TextureFormat mobileFormat, bool readable = false)
        {
            var pixels = texture.GetPixels32(0);
            var isMobile = buildTarget == UnityEditor.BuildTarget.Android || buildTarget == UnityEditor.BuildTarget.iOS;
            using (var colors = new NativeArray<Color32>(pixels, Allocator.Temp))
            {
                var settings = new TextureGenerationSettings(TextureImporterType.NormalMap);
                settings.textureImporterSettings.readable = readable;
                settings.textureImporterSettings.mipmapEnabled = true;
                settings.textureImporterSettings.streamingMipmaps = true;
                settings.textureImporterSettings.wrapMode = texture.wrapMode;
                settings.textureImporterSettings.filterMode = texture.filterMode;
                settings.textureImporterSettings.aniso = texture.anisoLevel;
                settings.platformSettings.maxTextureSize = Math.Max(texture.width, texture.height);
                settings.sourceTextureInformation.width = texture.width;
                settings.sourceTextureInformation.height = texture.height;
                settings.sourceTextureInformation.containsAlpha = true;
                settings.sourceTextureInformation.hdr = false;
                if (isMobile)
                {
                    settings.platformSettings.format = (TextureImporterFormat)mobileFormat;
                }

                var output = TextureGenerator.GenerateTexture(settings, colors);
                output.texture.name = texture.name;
                return output.texture;
            }
        }

        /// <summary>
        /// Creates a single color 4x4 texture.
        /// </summary>
        /// <param name="color">Color to use.</param>
        /// <returns>Created texture.</returns>
        internal static Texture2D CreateColorTexture(Color32 color)
        {
            return CreateColorTexture(color, 4, 4);
        }

        /// <summary>
        /// Returns whether the texture format is known by the tool.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <returns>true when the format is known by the tool.</returns>
        internal static bool IsKnownTextureFormat(TextureFormat format)
        {
            return UncompressedFormats.Contains(format) || WindowsFormats.Contains(format) || AndroidFormats.Contains(format) || IosFormats.Contains(format);
        }

        /// <summary>
        /// Returns whether the texture format is supported by the build target.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="buildTarget">Build target.</param>
        /// <returns>true when the format is supported.</returns>
        internal static bool IsSupportedTextureFormat(TextureFormat format, UnityEditor.BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return UncompressedFormats.Contains(format) || WindowsFormats.Contains(format);
                case UnityEditor.BuildTarget.Android:
                    return UncompressedFormats.Contains(format) || AndroidFormats.Contains(format);
                case UnityEditor.BuildTarget.iOS:
                    return UncompressedFormats.Contains(format) || IosFormats.Contains(format);
                default:
                    throw new NotSupportedException($"Unsupported build target: {buildTarget}");
            }
        }

        /// <summary>
        /// Returns whether the texture format is uncompressed.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <returns>true when the format is for uncompressed.</returns>
        internal static bool IsUncompressedFormat(TextureFormat format)
        {
            return UncompressedFormats.Contains(format);
        }

        /// <summary>
        /// Sets whether the texture uses streaming mipmaps.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <param name="useStreamingMipmaps">Streaming Mipmaps.</param>
        internal static void SetStreamingMipMaps(Texture2D texture, bool useStreamingMipmaps)
        {
            var so = new SerializedObject(texture);
            so.Update();
            var streamingMipmaps = so.FindProperty("m_StreamingMipmaps");
            streamingMipmaps.boolValue = useStreamingMipmaps;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Gets whether the texture is a normal map asset.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <returns>true when the texture is a normal map. false when it's not an asset.</returns>
        internal static bool IsNormalMapAsset(Texture texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return false;
            }
            return importer.textureType == TextureImporterType.NormalMap;
        }

        /// <summary>
        /// Destroys a texture if it's not an asset.
        /// </summary>
        /// <param name="texture">Texture.</param>
        internal static void DestroyTexture(Texture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (AssetDatabase.GetAssetPath(texture) != null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Downscale normal map using compute shader.
        /// </summary>
        /// <param name="source">Source texture.</param>
        /// <param name="outputRGB">Output is RGB format.</param>
        /// <param name="targetWidth">Target width.</param>
        /// <param name="targetHeight">Target height.</param>
        /// <returns>Generated normal map.</returns>
        internal static Texture2D DownscaleNormalMap(Texture2D source, bool outputRGB, int targetWidth, int targetHeight)
        {
            var raFomrats = new TextureFormat[]
            {
                TextureFormat.DXT5,
                TextureFormat.DXT5Crunched,
                TextureFormat.BC7,
            };
            var inputRGB = !raFomrats.Contains(source.format);

            // ì¸óÕÇ∆èoóÕÇÃRenderTexture
            var d = new RenderTextureDescriptor(source.width, source.height, RenderTextureFormat.ARGB32)
            {
                useMipMap = false,
                sRGB = false,
                depthBufferBits = 0,
                enableRandomWrite = true,
            };
            RenderTexture srcRT = RenderTexture.GetTemporary(d);
            srcRT.Create();
            Graphics.Blit(source, srcRT);

            d.width = targetWidth;
            d.height = targetHeight;
            RenderTexture dstRT = RenderTexture.GetTemporary(d);
            dstRT.Create();

            // ComputeShader
            var shaderGUID = "dfdb9399f59780a47b40e0be254b49af";
            var path = AssetDatabase.GUIDToAssetPath(shaderGUID);
            var computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);

            int kernel = computeShader.FindKernel("CSMain");
            computeShader.SetTexture(kernel, "_Input", srcRT);
            computeShader.SetTexture(kernel, "_Result", dstRT);
            computeShader.SetBool("_InputRGB", inputRGB);
            computeShader.SetInts("_InputSize", source.width, source.height);
            computeShader.SetInts("_OutputSize", targetWidth, targetHeight);

            int threadGroupsX = Mathf.CeilToInt(targetWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(targetHeight / 8.0f);
            computeShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

            // åãâ ÇTexture2DÇ…ïœä∑
            var prevActive = RenderTexture.active;
            RenderTexture.active = dstRT;
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false, true);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            RenderTexture.active = prevActive;

            RenderTexture.ReleaseTemporary(srcRT);
            RenderTexture.ReleaseTemporary(dstRT);

            return result;
        }

        /// <summary>
        /// Reduces the size of a texture while maintaining its aspect ratio.
        /// </summary>
        /// <param name="width">Original width.</param>
        /// <param name="height">Original height.</param>
        /// <param name="maxSize">Maximum size.</param>
        /// <returns>Reduced size or original size.</returns>
        internal static (int Width, int Height) AspectFitReduction(int width, int height, int maxSize)
        {
            var scale = (float)maxSize / Math.Max(width, height);
            if (scale > 1.0f)
            {
                return (width, height);
            }
            var newWidth = Math.Round(width * scale);
            var newHeight = Math.Round(height * scale);
            return ((int)newWidth, (int)newHeight);
        }

        private static bool ShouldUseAsyncGPUReadback()
        {
#if UNITY_2022_1_OR_NEWER
            return SystemInfo.supportsAsyncGPUReadback;
#else
            return false;
#endif
        }
    }
}
