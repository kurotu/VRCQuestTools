#pragma warning disable SA1402 // File may only contain a single type

using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Base class for dependency compatibility exceptions.
    /// </summary>
    public abstract class PackageCompatibilityException : System.Exception, IVRCQuestToolsException
    {
        /// <summary>
        /// Gets the name of the package that caused the compatibility issue.
        /// </summary>
        public readonly string PackageDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageCompatibilityException"/> class.
        /// </summary>
        /// <param name="packageDisplayName">Name of the package that caused the compatibility issue.</param>
        /// <param name="message">Error message.</param>
        protected PackageCompatibilityException(string packageDisplayName, string message)
            : base(message)
        {
            PackageDisplayName = packageDisplayName;
        }

        /// <summary>
        /// Gets the localized message for the exception.
        /// </summary>
        public abstract string LocalizedMessage { get; }
    }

    /// <summary>
    /// Exception thrown when the package is using a legacy version.
    /// </summary>
    public class LegacyPackageException : PackageCompatibilityException
    {
        /// <summary>
        /// Gets the required version.
        /// </summary>
        public readonly string RequiredVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyPackageException"/> class.
        /// </summary>
        /// <param name="packageDisplayName">Name of the package that caused the compatibility issue.</param>
        /// <param name="requiredVersion">The required version.</param>
        public LegacyPackageException(string packageDisplayName, string requiredVersion)
            : base(packageDisplayName, new I18nEnglish().LegacyPackageExceptionMessage(packageDisplayName, requiredVersion))
        {
            RequiredVersion = requiredVersion;
        }

        /// <inheritdoc/>
        public override string LocalizedMessage => VRCQuestToolsSettings.I18nResource.LegacyPackageExceptionMessage(PackageDisplayName, RequiredVersion);
    }

    /// <summary>
    /// Exception thrown when the package is using a breaking version.
    /// </summary>
    public class BreakingPackageException : PackageCompatibilityException
    {
        /// <summary>
        /// Gets the breaking version.
        /// </summary>
        public readonly string BreakingVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="BreakingPackageException"/> class.
        /// </summary>
        /// <param name="packageDisplayName">Name of the package that caused the compatibility issue.</param>
        /// <param name="breakingVersion">The breaking version.</param>
        public BreakingPackageException(string packageDisplayName, string breakingVersion)
            : base(packageDisplayName, new I18nEnglish().BreakingPackageExceptionMessage(packageDisplayName, breakingVersion))
        {
            BreakingVersion = breakingVersion;
        }

        /// <inheritdoc/>
        public override string LocalizedMessage => VRCQuestToolsSettings.I18nResource.BreakingPackageExceptionMessage(PackageDisplayName, BreakingVersion);
    }
}
