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
        IMaterialConvertSettings DefaultMaterialConvertSettings { get; }

        /// <summary>
        /// Gets or sets additional material convert settings.
        /// </summary>
        AdditionalMaterialConvertSettings[] AdditionalMaterialConvertSettings { get; set; }

        /// <summary>
        /// Gets a value indicating whether to remove redundant material slots that are greater than the number of submeshes.
        /// </summary>
        bool RemoveExtraMaterialSlots { get; }

        /// <summary>
        /// Gets NDMF phase to convert the avatar.
        /// </summary>
        AvatarConverterNdmfPhase NdmfPhase { get; }

        /// <summary>
        /// Gets a value indicating whether to use default material convert settings.
        /// </summary>
        bool IsPrimaryRoot { get; }
    }
}
