using UnityEngine;

#pragma warning disable SA1402 // file may only contain a single type
#pragma warning disable SA1649 // class should match with file name

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Exception for conversion error.
    /// </summary>
    /// <typeparam name="T">Source object type.</typeparam>
    internal abstract class ConversionException<T> : System.Exception
    {
        /// <summary>
        /// Source object.
        /// </summary>
        public readonly T source;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversionException{T}"/> class.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Exception which was originally throwed.</param>
        public ConversionException(string message, T source, System.Exception innerException)
            : base(message, innerException)
        {
            this.source = source;
        }
    }

    /// <summary>
    /// Exception for material conversion error.
    /// </summary>
    internal class MaterialConversionException : ConversionException<Material>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialConversionException"/> class.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Exception which was originally throwed.</param>
        public MaterialConversionException(string message, Material source, System.Exception innerException)
            : base(message, source, innerException)
        {
        }
    }

    /// <summary>
    /// Exception for animation clip conversion error.
    /// </summary>
    internal class AnimationClipConversionException : ConversionException<AnimationClip>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationClipConversionException"/> class.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Exception which was originally throwed.</param>
        public AnimationClipConversionException(string message, AnimationClip source, System.Exception innerException)
            : base(message, source, innerException)
        {
        }
    }

    /// <summary>
    /// Exception for animator controller conversion error.
    /// </summary>
    internal class AnimatorControllerConversionException : ConversionException<RuntimeAnimatorController>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatorControllerConversionException"/> class.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Exception which was originally throwed.</param>
        public AnimatorControllerConversionException(string message, RuntimeAnimatorController source, System.Exception innerException)
            : base(message, source, innerException)
        {
        }
    }

    /// <summary>
    /// Exception for replacement material is not allowed for mobile.
    /// </summary>
    internal class InvalidReplacementMaterialException : System.Exception
    {
        /// <summary>
        /// Component which has the error.
        /// </summary>
        public readonly Component component;

        /// <summary>
        /// Replacement material which has the error.
        /// </summary>
        public readonly Material replacementMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidReplacementMaterialException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="component">Component which has the error.</param>
        /// <param name="replacementMaterial">Replacement material which has the error.</param>
        public InvalidReplacementMaterialException(string message, Component component, Material replacementMaterial)
            : base(message)
        {
            this.component = component;
            this.replacementMaterial = replacementMaterial;
        }
    }
}
