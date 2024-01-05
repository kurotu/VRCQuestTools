#if VQT_HAS_NDMF_ERROR_REPORT
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace KRT.VRCQuestTools.Ndmf
{
    internal class NdmfComponentRemoverWarning : SimpleError
    {
        private string componentTypeName;
        private string objectName;

        public NdmfComponentRemoverWarning(System.Type componentType, ObjectReference objectReference)
        {
            this.componentTypeName = componentType.Name;
            this.objectName = objectReference.Object.name;
            _references = new List<ObjectReference> { objectReference };
        }

        public override ErrorSeverity Severity => ErrorSeverity.Information;

        protected override Localizer Localizer => NdmfLocalizer.Instance;

        protected override string TitleKey => NdmfLocalizer.RemovedUnsupportedComponentTitle;

        protected override string DetailsKey => NdmfLocalizer.RemovedUnsupportedComponentDescription;

        protected override string[] DetailsSubst => new[] { componentTypeName, objectName };
    }
}
#endif
