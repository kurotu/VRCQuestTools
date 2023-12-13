namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material convert setting for Toon Lit shader.
    /// </summary>
    public interface IToonLitConvertSettings : IMaterialConvertSettings
    {
        /// <summary>
        /// Gets a value indicating whether to generate quest textures.
        /// </summary>
        bool GenerateQuestTextures { get; }

        /// <summary>
        /// Gets max texture size for quest.
        /// </summary>
        TextureSizeLimit MaxTextureSize { get; }

        /// <summary>
        /// Gets texture brightness for quest. [0-1].
        /// </summary>
        float MainTextureBrightness { get; }

        /// <summary>
        /// Gets a value indicating whether to generate shadow from normal map.
        /// </summary>
        bool GenerateShadowFromNormalMap { get; }
    }
}
