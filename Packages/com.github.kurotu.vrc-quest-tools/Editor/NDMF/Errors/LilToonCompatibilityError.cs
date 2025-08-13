using System.Collections.Generic;
using KRT.VRCQuestTools.Models.Unity;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Error for lilToon compatibility issues.
    /// </summary>
    internal class LilToonCompatibilityError : SimpleError
    {
        private readonly LilToonCompatibilityException exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonCompatibilityError"/> class.
        /// </summary>
        /// <param name="exception">Thrown exception.</param>
        public LilToonCompatibilityError(LilToonCompatibilityException exception)
        {
            this.exception = exception;
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.LilToonCompatibilityErrorTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.LilToonCompatibilityErrorDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { exception.LocalizedMessage };
    }
}
