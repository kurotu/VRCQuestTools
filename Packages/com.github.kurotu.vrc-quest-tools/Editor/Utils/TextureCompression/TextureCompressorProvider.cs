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
        /// astcenc quality preset used for textures that end up on the avatar. Corresponds to Unity's
        /// TextureCompressionQuality.Best: AstcencBenchmarkTests measured the same quality as Unity for every
        /// size and block size tried.
        /// </summary>
        internal const string FinalPreset = "-thorough";

        /// <summary>
        /// astcenc quality preset used for NDMF editor previews. AstcencBenchmarkTests measured a quality
        /// difference from <see cref="FinalPreset"/> only in the fifth decimal place while running 1.5x-2.2x
        /// faster, and a preview that exaggerates compression artifacts errs on the safe side anyway.
        /// </summary>
        internal const string PreviewPreset = "-medium";

        private static readonly UnityTextureCompressor UnityCompressor = new UnityTextureCompressor();

        // Not readonly: ResetForTesting() re-creates these alongside AstcencBinaryLocator's own cache, so a test that
        // resolves a different (or no) astcenc binary via AstcencBinaryLocator.ResetCacheForTesting() doesn't leave
        // this provider still handing out a compressor built from the previously-cached path/version.
        private static Lazy<AstcencTextureCompressor> lazyFinalCompressor = CreateLazyCompressor(FinalPreset);

        private static Lazy<AstcencTextureCompressor> lazyPreviewCompressor = CreateLazyCompressor(PreviewPreset);

        private static ITextureCompressor compressorOverrideForTesting;

        /// <summary>
        /// Gets a texture compressor to use for the format. The selection depends only on the format: astcenc is
        /// used for any supported ASTC format (color or normal map alike) when a usable astcenc executable is
        /// available, since <see cref="AstcencTextureCompressor"/> implements both <see cref="ITextureCompressor.CompressTexture"/>
        /// and <see cref="ITextureCompressor.CompressNormalMap"/>. A null format (e.g. a non-mobile normal map,
        /// which <see cref="TextureUtility.ResolveEffectiveCompressionFormat"/> leaves for
        /// <see cref="UnityEditor.TextureGenerator"/> to decide) always falls back to Unity.
        /// </summary>
        /// <param name="format">Format to compress to. Null when the format is left unset for normal map compression.</param>
        /// <param name="forEditorPreview">Whether the texture is generated for an editor preview, which uses the faster <see cref="PreviewPreset"/>.</param>
        /// <returns>Texture compressor.</returns>
        internal static ITextureCompressor GetCompressor(TextureFormat? format, bool forEditorPreview = false)
        {
            if (compressorOverrideForTesting != null)
            {
                return compressorOverrideForTesting;
            }

            if (format.HasValue && AstcUtility.TryGetBlockSize(format.Value, out _, out _) && AstcencBinaryLocator.GetAstcencPath() != null)
            {
                return forEditorPreview ? lazyPreviewCompressor.Value : lazyFinalCompressor.Value;
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
        /// compressors, so a subsequent <see cref="GetCompressor"/> call re-resolves the astcenc binary from scratch
        /// instead of reusing a compressor built from a path/version cached before this call.
        /// </summary>
        internal static void ResetForTesting()
        {
            compressorOverrideForTesting = null;
            AstcencBinaryLocator.ResetCacheForTesting();
            lazyFinalCompressor = CreateLazyCompressor(FinalPreset);
            lazyPreviewCompressor = CreateLazyCompressor(PreviewPreset);
        }

        private static Lazy<AstcencTextureCompressor> CreateLazyCompressor(string preset)
        {
            return new Lazy<AstcencTextureCompressor>(() =>
            {
                // GetCompressor only evaluates this Lazy after confirming AstcencBinaryLocator.GetAstcencPath() != null,
                // so the resolution is already cached and guaranteed non-null here; reading it back avoids spawning a
                // second astcenc process just to re-derive the version that AstcencBinaryLocator already queried while
                // resolving the path.
                var resolution = AstcencBinaryLocator.GetResolution().Value;
                return new AstcencTextureCompressor(resolution.Path, resolution.Version ?? "unknown", preset);
            });
        }
    }
}
