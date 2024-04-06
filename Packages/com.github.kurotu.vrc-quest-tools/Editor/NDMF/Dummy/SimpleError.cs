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
        /// Gets DetailsKey.
        /// </summary>
        public abstract string DetailsKey { get; }

        /// <summary>
        /// Gets DetailsSubst.
        /// </summary>
        public abstract string[] DetailsSubst { get; }
    }
}
#endif
