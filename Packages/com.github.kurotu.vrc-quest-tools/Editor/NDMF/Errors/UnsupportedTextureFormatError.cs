using System.Linq;
using nadena.dev.ndmf;
#if VQT_HAS_NDMF_ERROR_REPORT
using nadena.dev.ndmf.localization;
#endif
using UnityEditor;
using UnityEngine;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Error for unsupported texture format.
    /// </summary>
    internal class UnsupportedTextureFormatError : SimpleError
    {
        private readonly TextureFormat format;
        private readonly BuildTarget buildTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedTextureFormatError"/> class.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="buildTarget">Build target.</param>
        /// <param name="textures">Textures.</param>
        public UnsupportedTextureFormatError(TextureFormat format, BuildTarget buildTarget, Texture[] textures)
        {
            this.format = format;
            this.buildTarget = buildTarget;
            _references = textures.Select(NdmfObjectRegistry.GetReference).ToList();
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.NonFatal;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.UnsupportedTextureFormatTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.UnsupportedTextureFormatDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { format.ToString(), buildTarget.ToString() };
    }
}
