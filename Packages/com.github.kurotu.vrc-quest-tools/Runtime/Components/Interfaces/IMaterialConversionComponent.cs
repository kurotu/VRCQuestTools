using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Interface for material conversion component.
    /// </summary>
    public interface IMaterialConversionComponent : IMaterialOperatorComponent
    {
        /// <summary>
        /// Gets default material convert setting.
        /// </summary>
        public IMaterialConvertSettings DefaultMaterialConvertSettings { get; }

        /// <summary>
        /// Gets additional material convert settings.
        /// </summary>
        public AdditionalMaterialConvertSettings[] AdditionalMaterialConvertSettings { get; }

        /// <summary>
        /// Gets a value indicating whether to remove redundant material slots that are greater than the number of submeshes.
        /// </summary>
        public bool RemoveExtraMaterialSlots { get; }

        /// <summary>
        /// Gets NDMF phase to convert the avatar.
        /// </summary>
        public AvatarConverterNdmfPhase NdmfPhase { get; }

        /// <summary>
        /// Gets a value indicating whether to use default material convert settings.
        /// </summary>
        public bool IsPrimaryRoot { get; }
    }
}
