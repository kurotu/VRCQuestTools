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
        public ConversionException(string message, T source, System.Exception innerException)
            : base(message, innerException)
        {
            this.source = source;
        }
    }

    internal class MaterialConversionException : ConversionException<Material>
    {
        public MaterialConversionException(string message, Material source, System.Exception innerException)
            : base(message, source, innerException)
        {
        }
    }

    internal class AnimationClipConversionException : ConversionException<AnimationClip>
    {
        public AnimationClipConversionException(string message, AnimationClip source, System.Exception innerException)
            : base(message, source, innerException)
        {
        }
    }

    internal class AnimatorControllerConversionException : ConversionException<RuntimeAnimatorController>
    {
        public AnimatorControllerConversionException(string message, RuntimeAnimatorController source, System.Exception innerException)
            : base(message, source, innerException)
        {
        }
    }
}
