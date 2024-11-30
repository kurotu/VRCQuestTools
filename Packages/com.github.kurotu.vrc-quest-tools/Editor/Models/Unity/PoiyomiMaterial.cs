using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Represents UnityChanToonShader material.
    /// </summary>
    internal class PoiyomiMaterial : StandardMaterial
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoiyomiMaterial"/> class.
        /// </summary>
        /// <param name="material">Material.</param>
        internal PoiyomiMaterial(Material material)
            : base(material)
        {
        }

        /// <inheritdoc/>
        internal override Shader ToonLitBakeShader => Shader.Find("Hidden/VRCQuestTools/Poiyomi");
    }
}
