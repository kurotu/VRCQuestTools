#if !VQT_HAS_NDMF_ERROR_REPORT
using System.Collections.Generic;

namespace KRT.VRCQuestTools.Ndmf.Dummy
{
    /// <summary>
    /// SimpleError is a dummy class for NDMF.
    /// </summary>
    internal abstract class SimpleError
    {
        /// <summary>
        /// Dummy references.
        /// </summary>
#pragma warning disable SA1309
        internal List<ObjectReference> _references;
#pragma warning restore SA1309

        /// <summary>
        /// Gets ErrorSeverity.
        /// </summary>
        public abstract ErrorSeverity Severity { get; }

        /// <summary>
        /// Gets Localizer.
        /// </summary>
        public abstract Localizer Localizer { get; }

        /// <summary>
        /// Gets TitleKey.
        /// </summary>
        public abstract string TitleKey { get; }

        /// <summary>
        /// Gets TitleSubst.
        /// </summary>
        public virtual string[] TitleSubst => new string[] { };

        /// <summary>
        /// Gets DetailsKey.
        /// </summary>
        public virtual string DetailsKey => TitleKey + ":description";

        /// <summary>
        /// Gets DetailsSubst.
        /// </summary>
        public virtual string[] DetailsSubst => new string[] { };

        /// <summary>
        /// Gets HintKey.
        /// </summary>
        public virtual string HintKey => TitleKey + ":hint";

        /// <summary>
        /// Gets HintSubst.
        /// </summary>
        public virtual string[] HintSubst => new string[] { };

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(Localizer.Get(DetailsKey), DetailsSubst);
        }
    }
}
#endif
