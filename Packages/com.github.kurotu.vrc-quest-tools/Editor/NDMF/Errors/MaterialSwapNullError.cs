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
    /// Error for Material Swap component when original or replacement material is null.
    /// </summary>
    internal class MaterialSwapNullError : SimpleError
    {
        private readonly string originalMaterialName;
        private readonly string replacementMaterialName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialSwapNullError"/> class.
        /// </summary>
        /// <param name="materialSwap">Material Swap component.</param>
        /// <param name="mapping">Material mapping which has the error.</param>
        public MaterialSwapNullError(MaterialSwap materialSwap, MaterialSwap.MaterialMapping mapping)
        {
            _references = new List<ObjectReference>
            {
                NdmfObjectRegistry.GetReference(materialSwap),
            };
            originalMaterialName = mapping.originalMaterial ? mapping.originalMaterial.name : "None";
            replacementMaterialName = mapping.replacementMaterial ? mapping.replacementMaterial.name : "None";
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.MaterialSwapNullErrorTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.MaterialSwapNullErrorDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { originalMaterialName, replacementMaterialName };
    }
}
