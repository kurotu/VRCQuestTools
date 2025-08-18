#pragma warning disable SA1402 // File may only contain a single type

using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Base class for Modular Avatar compatibility exceptions.
    /// </summary>
    public abstract class ModularAvatarCompatibilityException : System.Exception, IVRCQuestToolsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModularAvatarCompatibilityException"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        protected ModularAvatarCompatibilityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Gets the localized error message.
        /// </summary>
        public abstract string LocalizedMessage { get; }
    }

    /// <summary>
    /// Exception thrown when the Modular Avatar is using a legacy version.
    /// </summary>
    public class ModularAvatarLegacyException : ModularAvatarCompatibilityException
    {
        private const string RequiredVersion = "1.12.2";

        /// <summary>
        /// Initializes a new instance of the <see cref="ModularAvatarLegacyException"/> class.
        /// </summary>
        public ModularAvatarLegacyException()
            : base(new I18nEnglish().ModularAvatarLegacyExceptionMessage(RequiredVersion))
        {
        }

        /// <inheritdoc/>
        public override string LocalizedMessage => VRCQuestToolsSettings.I18nResource.ModularAvatarLegacyExceptionMessage(RequiredVersion);
    }

    /// <summary>
    /// Exception thrown when the Modular Avatar is using a breaking version.
    /// </summary>
    public class ModularAvatarBreakingException : ModularAvatarCompatibilityException
    {
        private const string BreakingVersion = "2.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="ModularAvatarBreakingException"/> class.
        /// </summary>
        public ModularAvatarBreakingException()
            : base(new I18nEnglish().ModularAvatarBreakingExceptionMessage(BreakingVersion))
        {
        }

        /// <inheritdoc/>
        public override string LocalizedMessage => VRCQuestToolsSettings.I18nResource.ModularAvatarBreakingExceptionMessage(BreakingVersion);
    }
}
