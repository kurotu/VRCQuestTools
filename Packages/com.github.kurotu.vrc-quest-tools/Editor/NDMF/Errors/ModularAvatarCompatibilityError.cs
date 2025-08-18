using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Error for Modular Avatar compatibility issues.
    /// </summary>
    internal class ModularAvatarCompatibilityError : SimpleError
    {
        private readonly ModularAvatarCompatibilityException exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModularAvatarCompatibilityError"/> class.
        /// </summary>
        /// <param name="exception">Thrown exception.</param>
        public ModularAvatarCompatibilityError(ModularAvatarCompatibilityException exception)
        {
            this.exception = exception;
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.ModularAvatarCompatibilityErrorTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.ModularAvatarCompatibilityErrorDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { exception.LocalizedMessage };
    }
}
