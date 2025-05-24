namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// NDMF phase for AvatarConverterSettings.
    /// </summary>
    public enum AvatarConverterNdmfPhase
    {
        /// <summary>Transforming phase.</summary>
        Transforming,

        /// <summary>Optimizing phase.</summary>
        Optimizing,

        /// <summary>Auto.</summary>
        Auto,
    }

    /// <summary>
    /// Extension methods for AvatarConverterNdmfPhase.
    /// </summary>
#pragma warning disable SA1649 // SA1649 File name should match first type name
    public static class AvatarConverterNdmfPhaseExtension
#pragma warning restore SA1649
    {
        /// <summary>
        /// Resolve the NDMF phase for project configuration.
        /// </summary>
        /// <param name="phase">This object.</param>
        /// <returns>Resolve phase other than Auto.</returns>
        public static AvatarConverterNdmfPhase Resolve(this AvatarConverterNdmfPhase phase)
        {
            if (phase == AvatarConverterNdmfPhase.Auto)
            {
#if VQT_VRCFURY
                return AvatarConverterNdmfPhase.Transforming;
#else
                return AvatarConverterNdmfPhase.Optimizing;
#endif
            }
            return phase;
        }
    }
}
