using UnityEngine;

#pragma warning disable SA1402 // file may only contain a single type
#pragma warning disable SA1649 // class should match with file name

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Exception for mesh flipper's mask is null.
    /// </summary>
    public class MeshFlipperMaskMissingException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshFlipperMaskMissingException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public MeshFlipperMaskMissingException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception for mesh flipper's mask is not readable.
    /// </summary>
    public class MeshFlipperMaskNotReadableException : System.Exception
    {
        /// <summary>
        /// Related texture.
        /// </summary>
        public readonly Texture2D texture;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshFlipperMaskNotReadableException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="texture">Texture.</param>
        public MeshFlipperMaskNotReadableException(string message, Texture2D texture)
            : base(message)
        {
            this.texture = texture;
        }
    }
}
