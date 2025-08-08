using KRT.VRCQuestTools.Utils;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Exception for lilToon compatibility issues.
    /// </summary>
    internal class LilToonCompatibilityException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonCompatibilityException"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public LilToonCompatibilityException(string message)
            : base($"Compatibility issue for lilToon {AssetUtility.LilToonVersion}. Please report this error: " + message)
        {
        }
    }
}
