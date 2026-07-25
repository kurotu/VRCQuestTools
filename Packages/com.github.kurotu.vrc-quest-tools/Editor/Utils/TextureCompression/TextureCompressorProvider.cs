// <copyright file="TextureCompressorProvider.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Provides a texture compressor for a texture format.
    /// </summary>
    internal static class TextureCompressorProvider
    {
        /// <summary>
        /// Default astcenc quality preset used for texture compression. Corresponds to Unity's
        /// TextureCompressionQuality.Best.
        /// </summary>
        internal const string DefaultPreset = "-thorough";

        private static readonly UnityTextureCompressor UnityCompressor = new UnityTextureCompressor();

        // Not readonly: ResetForTesting() re-creates this alongside AstcencBinaryLocator's own cache, so a test that
        // resolves a different (or no) astcenc binary via AstcencBinaryLocator.ResetCacheForTesting() doesn't leave
        // this provider still handing out a compressor built from the previously-cached path/version.
        private static Lazy<AstcencTextureCompressor> lazyAstcencCompressor = new Lazy<AstcencTextureCompressor>(CreateAstcencCompressor);

        private static ITextureCompressor compressorOverrideForTesting;

        /// <summary>
        /// Gets a texture compressor to use for the format.
        /// </summary>
        /// <param name="format">Format to compress to. Null when the format is left unset for normal map compression.</param>
        /// <param name="isNormalMap">Whether the compression target is a normal map. astcenc is never selected for normal maps.</param>
        /// <returns>Texture compressor.</returns>
        internal static ITextureCompressor GetCompressor(TextureFormat? format, bool isNormalMap = false)
        {
            if (compressorOverrideForTesting != null)
            {
                return compressorOverrideForTesting;
            }

            if (!isNormalMap && format.HasValue && AstcUtility.TryGetBlockSize(format.Value, out _, out _) && AstcencBinaryLocator.GetAstcencPath() != null)
            {
                return lazyAstcencCompressor.Value;
            }

            return UnityCompressor;
        }

        /// <summary>
        /// Overrides the compressor returned by <see cref="GetCompressor"/> for tests.
        /// </summary>
        /// <param name="compressor">Compressor to use, or null to restore the normal selection logic.</param>
        internal static void SetCompressorForTesting(ITextureCompressor compressor)
        {
            compressorOverrideForTesting = compressor;
        }

        /// <summary>
        /// Restores the normal compressor selection logic after a test overrode it via <see cref="SetCompressorForTesting"/>.
        /// Also resets <see cref="AstcencBinaryLocator"/>'s cached resolution and this provider's cached astcenc
        /// compressor, so a subsequent <see cref="GetCompressor"/> call re-resolves the astcenc binary from scratch
        /// instead of reusing a compressor built from a path/version cached before this call.
        /// </summary>
        internal static void ResetForTesting()
        {
            compressorOverrideForTesting = null;
            AstcencBinaryLocator.ResetCacheForTesting();
            lazyAstcencCompressor = new Lazy<AstcencTextureCompressor>(CreateAstcencCompressor);
        }

        private static AstcencTextureCompressor CreateAstcencCompressor()
        {
            // GetCompressor only evaluates this Lazy after confirming AstcencBinaryLocator.GetAstcencPath() != null,
            // so the resolution is already cached and guaranteed non-null here; reading it back avoids spawning a
            // second astcenc process just to re-derive the version that AstcencBinaryLocator already queried while
            // resolving the path.
            var resolution = AstcencBinaryLocator.GetResolution().Value;
            return new AstcencTextureCompressor(resolution.Path, resolution.Version ?? "unknown", DefaultPreset);
        }
    }
}
