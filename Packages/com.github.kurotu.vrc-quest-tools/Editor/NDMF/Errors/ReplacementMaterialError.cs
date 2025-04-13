using System.Collections.Generic;
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
    /// Error for replacement material.
    /// </summary>
    internal class ReplacementMaterialError : SimpleError
    {
        private readonly Material replacementMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplacementMaterialError"/> class.
        /// </summary>
        /// <param name="component">Component which has the error.</param>
        /// <param name="replacementMaterial">Replacement material which has the error.</param>
        public ReplacementMaterialError(Component component, Material replacementMaterial)
        {
            this.replacementMaterial = replacementMaterial;
            _references = new List<ObjectReference>
            {
                NdmfObjectRegistry.GetReference(component),
                NdmfObjectRegistry.GetReference(replacementMaterial),
            };
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.ReplacementMaterialErrorTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.ReplacementMaterialErrorDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { replacementMaterial.name, replacementMaterial.shader.name };
    }
}
