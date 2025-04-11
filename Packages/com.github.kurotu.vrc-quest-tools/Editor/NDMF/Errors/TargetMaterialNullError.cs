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
    /// Error for null target material.
    /// </summary>
    internal class TargetMaterialNullError : SimpleError
    {
        private readonly string[] detailsSubst;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetMaterialNullError"/> class.
        /// </summary>
        /// <param name="component">Component which has the error.</param>
        internal TargetMaterialNullError(Component component)
        {
            _references = new List<ObjectReference>() { NdmfObjectRegistry.GetReference(component) };
            detailsSubst = new string[] { component.GetType().Name };
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.TargetMaterialNullErrorTitle;

        /// <inheritdoc/>
        public override string[] DetailsSubst => detailsSubst;
    }
}
