using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Error for package compatibility issues.
    /// </summary>
    internal class PackageCompatibilityError : SimpleError
    {
        private readonly PackageCompatibilityException exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageCompatibilityError"/> class.
        /// </summary>
        /// <param name="exception">Thrown exception.</param>
        public PackageCompatibilityError(PackageCompatibilityException exception)
        {
            this.exception = exception;
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.PackageCompatibilityErrorTitle;

        /// <inheritdoc/>
        public override string[] TitleSubst => new[] { exception.PackageDisplayName };

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.PackageCompatibilityErrorDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { exception.LocalizedMessage };
    }
}
