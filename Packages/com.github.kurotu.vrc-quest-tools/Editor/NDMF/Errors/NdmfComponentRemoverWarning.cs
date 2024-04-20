using System.Collections.Generic;
using nadena.dev.ndmf;
#if VQT_HAS_NDMF_ERROR_REPORT
using nadena.dev.ndmf.localization;
#else
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Warning for removing unsupported component.
    /// </summary>
    internal class NdmfComponentRemoverWarning : SimpleError
    {
        private string componentTypeName;
        private string objectName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NdmfComponentRemoverWarning"/> class.
        /// </summary>
        /// <param name="componentType">Remove component type.</param>
        /// <param name="objectReference">ObjectReference for removed component.</param>
        public NdmfComponentRemoverWarning(System.Type componentType, ObjectReference objectReference)
        {
            this.componentTypeName = componentType.Name;
            this.objectName = objectReference.Object.name;
            _references = new List<ObjectReference> { objectReference };
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Information;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.RemovedUnsupportedComponentTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.RemovedUnsupportedComponentDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { componentTypeName, objectName };
    }
}
