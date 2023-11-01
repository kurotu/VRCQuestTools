namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Texture size limit for quest.
    /// </summary>
    public enum TextureSizeLimit
    {
        /// <summary>
        /// No limit.
        /// </summary>
        NoLimit = 0,

        /// <summary>
        /// Max 256x256.
        /// </summary>
        Max256x256 = 256,

        /// <summary>
        /// Max 512x512.
        /// </summary>
        Max512x512 = 512,

        /// <summary>
        /// Max 1024x1024.
        /// </summary>
        Max1024x1024 = 1024,

        /// <summary>
        /// Max 2048x2048.
        /// </summary>
        Max2048x2048 = 2048,
    }
}
