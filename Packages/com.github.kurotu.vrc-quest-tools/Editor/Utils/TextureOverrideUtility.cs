// <copyright file="TextureOverrideUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility class for extracting platform-specific texture import overrides.
    /// </summary>
    internal static class TextureOverrideUtility
    {
        /// <summary>
        /// Platform override result containing resolution and texture format.
        /// </summary>
        internal struct PlatformOverrideResult
        {
            /// <summary>
            /// Maximum texture size from overrides.
            /// </summary>
            public int maxTextureSize;

            /// <summary>
            /// Best quality texture format from overrides.
            /// </summary>
            public MobileTextureFormat textureFormat;

            /// <summary>
            /// Whether any platform overrides were found.
            /// </summary>
            public bool hasOverride;
        }

        /// <summary>
        /// Fallback texture format settings when no platform overrides are found.
        /// </summary>
        internal struct FallbackTextureSettings
        {
            /// <summary>
            /// Maximum texture size for fallback.
            /// </summary>
            public int maxTextureSize;

            /// <summary>
            /// Mobile texture format for fallback.
            /// </summary>
            public MobileTextureFormat mobileTextureFormat;
        }

        /// <summary>
        /// Result of texture format resolution containing format and size information.
        /// </summary>
        internal struct TextureFormatResult
        {
            /// <summary>
            /// Resolved mobile texture format.
            /// </summary>
            public MobileTextureFormat mobileTextureFormat;

            /// <summary>
            /// Maximum texture size (0 means no limit).
            /// </summary>
            public int maxTextureSize;

            /// <summary>
            /// Whether the result came from platform overrides (true) or fallback settings (false).
            /// </summary>
            public bool fromOverride;
        }

        /// <summary>
        /// Extracts the best platform override settings from a collection of source textures.
        /// </summary>
        /// <param name="sourceTextures">Collection of source textures to check for overrides.</param>
        /// <param name="buildTarget">Target build platform to check overrides for.</param>
        /// <returns>Platform override result with highest quality settings found.</returns>
        internal static PlatformOverrideResult GetBestPlatformOverride(IEnumerable<Texture> sourceTextures, UnityEditor.BuildTarget buildTarget)
        {
            var result = new PlatformOverrideResult
            {
                maxTextureSize = 0,
                textureFormat = MobileTextureFormat.ASTC_12x12, // Lowest quality as default
                hasOverride = false
            };

            foreach (var texture in sourceTextures)
            {
                if (texture == null)
                {
                    continue;
                }

                var texturePath = AssetDatabase.GetAssetPath(texture);
                if (string.IsNullOrEmpty(texturePath))
                {
                    continue;
                }

                var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                // Determine which platform(s) to check based on build target
                string primaryPlatform = null;
                string fallbackPlatform = null;

                if (buildTarget == UnityEditor.BuildTarget.Android)
                {
                    primaryPlatform = "Android";
                    // No fallback for Android
                }
                else if (buildTarget == UnityEditor.BuildTarget.iOS)
                {
                    primaryPlatform = "iPhone";
                    fallbackPlatform = "Android"; // iOS fallback to Android
                }

                // Check primary platform override
                if (!string.IsNullOrEmpty(primaryPlatform))
                {
                    var primaryOverride = importer.GetPlatformTextureSettings(primaryPlatform);
                    if (primaryOverride.overridden)
                    {
                        ProcessPlatformOverride(primaryOverride, ref result);
                        continue; // If primary override found, don't check fallback
                    }
                }

                // Check fallback platform override if primary not found
                if (!string.IsNullOrEmpty(fallbackPlatform))
                {
                    var fallbackOverride = importer.GetPlatformTextureSettings(fallbackPlatform);
                    if (fallbackOverride.overridden)
                    {
                        ProcessPlatformOverride(fallbackOverride, ref result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves the best texture format and size from platform overrides or fallback settings.
        /// </summary>
        /// <param name="sourceTextures">Source textures to check for platform overrides.</param>
        /// <param name="buildTarget">Target build platform to check overrides for.</param>
        /// <param name="fallbackSettings">Fallback settings if no overrides found.</param>
        /// <returns>Resolved texture format settings.</returns>
        internal static TextureFormatResult ResolveTextureFormat(IEnumerable<Texture> sourceTextures, UnityEditor.BuildTarget buildTarget, FallbackTextureSettings fallbackSettings)
        {
            // First try to get platform override settings
            var overrideResult = GetBestPlatformOverride(sourceTextures, buildTarget);

            if (overrideResult.hasOverride)
            {
                return new TextureFormatResult
                {
                    mobileTextureFormat = overrideResult.textureFormat,
                    maxTextureSize = overrideResult.maxTextureSize,
                    fromOverride = true
                };
            }

            // Fall back to provided settings
            return new TextureFormatResult
            {
                mobileTextureFormat = fallbackSettings.mobileTextureFormat,
                maxTextureSize = fallbackSettings.maxTextureSize,
                fromOverride = false
            };
        }

        /// <summary>
        /// Processes a single platform override and updates the result with better settings if found.
        /// </summary>
        /// <param name="platformSettings">Platform texture settings to process.</param>
        /// <param name="result">Result to update with better settings.</param>
        private static void ProcessPlatformOverride(TextureImporterPlatformSettings platformSettings, ref PlatformOverrideResult result)
        {
            result.hasOverride = true;

            // Use maximum resolution found
            if (platformSettings.maxTextureSize > result.maxTextureSize)
            {
                result.maxTextureSize = platformSettings.maxTextureSize;
            }

            // Use highest quality ASTC format found
            var format = GetMobileTextureFormatFromImporter(platformSettings.format);
            if (format.HasValue && IsHigherQualityFormat(format.Value, result.textureFormat))
            {
                result.textureFormat = format.Value;
            }
        }

        /// <summary>
        /// Converts TextureImporterFormat to MobileTextureFormat if it's an ASTC format.
        /// </summary>
        /// <param name="importerFormat">TextureImporterFormat to convert.</param>
        /// <returns>Corresponding MobileTextureFormat if ASTC, null otherwise.</returns>
        private static MobileTextureFormat? GetMobileTextureFormatFromImporter(TextureImporterFormat importerFormat)
        {
            return importerFormat switch
            {
                TextureImporterFormat.ASTC_4x4 => MobileTextureFormat.ASTC_4x4,
                TextureImporterFormat.ASTC_5x5 => MobileTextureFormat.ASTC_5x5,
                TextureImporterFormat.ASTC_6x6 => MobileTextureFormat.ASTC_6x6,
                TextureImporterFormat.ASTC_8x8 => MobileTextureFormat.ASTC_8x8,
                TextureImporterFormat.ASTC_10x10 => MobileTextureFormat.ASTC_10x10,
                TextureImporterFormat.ASTC_12x12 => MobileTextureFormat.ASTC_12x12,
                _ => null,
            };
        }

        /// <summary>
        /// Determines if one ASTC format is higher quality than another.
        /// </summary>
        /// <param name="format1">First format to compare.</param>
        /// <param name="format2">Second format to compare.</param>
        /// <returns>True if format1 is higher quality than format2.</returns>
        private static bool IsHigherQualityFormat(MobileTextureFormat format1, MobileTextureFormat format2)
        {
            // ASTC quality order: 4x4 (highest) > 5x5 > 6x6 > 8x8 > 10x10 > 12x12 (lowest)
            var qualityOrder = new[]
            {
            MobileTextureFormat.ASTC_4x4,
            MobileTextureFormat.ASTC_5x5,
            MobileTextureFormat.ASTC_6x6,
            MobileTextureFormat.ASTC_8x8,
            MobileTextureFormat.ASTC_10x10,
            MobileTextureFormat.ASTC_12x12,
        };

            var index1 = System.Array.IndexOf(qualityOrder, format1);
            var index2 = System.Array.IndexOf(qualityOrder, format2);

            // Lower index means higher quality
            return index1 < index2;
        }
    }
}
