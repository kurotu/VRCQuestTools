namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Interface for material convert setting.
    /// </summary>
    public interface IMaterialConvertSettings
    {
        /// <summary>
        /// Gets a texture format for android.
        /// </summary>
        MobileTextureFormat MobileTextureFormat { get; }

        /// <summary>
        /// Load default assets from AssetDatabase.
        /// </summary>
        void LoadDefaultAssets();

        /// <summary>
        /// Get cache key for this setting.
        /// </summary>
        /// <returns>Cache key.</returns>
        string GetCacheKey();
    }
}
