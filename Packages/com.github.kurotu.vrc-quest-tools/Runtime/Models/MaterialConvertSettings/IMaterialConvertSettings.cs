namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Interface for material convert setting.
    /// </summary>
    public interface IMaterialConvertSettings
    {
        /// <summary>
        /// Get cache key for this setting.
        /// </summary>
        /// <returns>Cache key.</returns>
        string GetCacheKey();
    }
}
