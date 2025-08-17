#pragma warning disable SA1402 // File may only contain a single type

using KRT.VRCQuestTools.I18n;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Exception for lilToon compatibility issues.
    /// </summary>
    public abstract class LilToonCompatibilityException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonCompatibilityException"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        protected LilToonCompatibilityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Gets the localized error message.
        /// </summary>
        public abstract string LocalizedMessage { get; }
    }

    /// <summary>
    /// Exception thrown when lilToon is not supported.
    /// </summary>
    public class LilToonLegacyException : LilToonCompatibilityException
    {
        private const string RequiredVersion = "1.10.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonLegacyException"/> class.
        /// </summary>
        public LilToonLegacyException()
            : base(new I18nEnglish().LilToonLegacyExceptionMessage(RequiredVersion))
        {
        }

        /// <inheritdoc/>
        public override string LocalizedMessage => VRCQuestToolsSettings.I18nResource.LilToonLegacyExceptionMessage(RequiredVersion);
    }

    /// <summary>
    /// Exception thrown when lilToon is not supported.
    /// </summary>
    public class LilToonBreakingException : LilToonCompatibilityException
    {
        private const string BreakingVersion = "3.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonBreakingException"/> class.
        /// </summary>
        public LilToonBreakingException()
            : base(new I18nEnglish().LilToonBreakingExceptionMessage(BreakingVersion))
        {
        }

        /// <inheritdoc/>
        public override string LocalizedMessage => VRCQuestToolsSettings.I18nResource.LilToonBreakingExceptionMessage(BreakingVersion);
    }

    /// <summary>
    /// Exception thrown when lilToon is not detected.
    /// </summary>
    public class LilToonWrongInstallationException : LilToonCompatibilityException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonWrongInstallationException"/> class.
        /// </summary>
        public LilToonWrongInstallationException()
            : base(new I18nEnglish().LilToonWrongInstallationExceptionMessage)
        {
        }

        /// <inheritdoc/>
        public override string LocalizedMessage => VRCQuestToolsSettings.I18nResource.LilToonWrongInstallationExceptionMessage;
    }
}
