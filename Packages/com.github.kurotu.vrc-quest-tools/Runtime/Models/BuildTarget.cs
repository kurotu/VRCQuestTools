namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Build target.
    /// </summary>
    public enum BuildTarget
    {
        /// <summary>
        /// Auto detect build target depending on Unity's target platform.
        /// </summary>
        Auto,

        /// <summary>
        /// PC build target.
        /// </summary>
        PC,

        /// <summary>
        /// Android build target.
        /// </summary>
        Android,
    }
}
