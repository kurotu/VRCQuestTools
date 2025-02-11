using System.Collections.Generic;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    internal class ReplacementMaterialError : SimpleError
    {
        private readonly Material replacementMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplacementMaterialError"/> class.
        /// </summary>
        /// <param name="objectReference">Reference for converting object.</param>
        /// <param name="exception">Throwed exception.</param>
        public ReplacementMaterialError(Component component, Material replacementMaterial)
        {
            this.replacementMaterial = replacementMaterial;
            _references = new List<ObjectReference> {
                ObjectRegistry.GetReference(component),
                ObjectRegistry.GetReference(replacementMaterial),
            };
        }

        public ReplacementMaterialError(AvatarConverterSettings settings, Material replacementMaterial)
        {
            this.replacementMaterial = replacementMaterial;
            _references = new List<ObjectReference> {
                ObjectRegistry.GetReference(settings),
                ObjectRegistry.GetReference(replacementMaterial),
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
