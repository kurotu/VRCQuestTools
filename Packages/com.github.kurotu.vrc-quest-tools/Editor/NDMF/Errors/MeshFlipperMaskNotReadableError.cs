using System.Collections.Generic;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
#if VQT_HAS_NDMF_ERROR_REPORT
using nadena.dev.ndmf.localization;
#else
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Error for mesh flipper mask not readable.
    /// </summary>
    internal class MeshFlipperMaskNotReadableError : SimpleError
    {
        private readonly System.Exception exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshFlipperMaskNotReadableError"/> class.
        /// </summary>
        /// <param name="meshFlipper">Mesh Flipper component.</param>
        /// <param name="textureReference">Reference for mask texture.</param>
        public MeshFlipperMaskNotReadableError(MeshFlipper meshFlipper, ObjectReference textureReference)
        {
            _references = new List<ObjectReference> { NdmfObjectRegistry.GetReference(meshFlipper), textureReference };
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.MeshFlipperMaskNotReadableErrorTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.MeshFlipperMaskNotReadableErrorDescription;
    }
}
