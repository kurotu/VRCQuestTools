using System.Collections.Generic;
using nadena.dev.ndmf;
#if VQT_HAS_NDMF_ERROR_REPORT
using nadena.dev.ndmf.localization;
#else
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Error with simple string.
    /// </summary>
    internal class SimpleStringError : SimpleError
    {
        private readonly string title;
        private readonly string detail;
        private readonly string hint;
        private readonly ErrorSeverity severity;
        private readonly Localizer localizer = new Localizer("en-US", () => new List<(string, System.Func<string, string>)>
        {
            ("en-US", (key) => "{0}"),
        });

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleStringError"/> class.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="detail">Detail.</param>
        /// <param name="hint">Hint.</param>
        /// <param name="severity">Severity.</param>
        internal SimpleStringError(string title, string detail, string hint, ErrorSeverity severity)
        {
            this.title = title;
            this.detail = detail;
            this.hint = hint;
            this.severity = severity;
        }

        /// <inheritdoc/>
        public override Localizer Localizer => localizer;

        /// <inheritdoc/>
        public override string TitleKey => title;

        /// <inheritdoc/>
        public override string[] TitleSubst => new string[] { title };

        /// <inheritdoc/>
        public override string DetailsKey => detail;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new string[] { detail };

        /// <inheritdoc/>
        public override string HintKey => hint;

        /// <inheritdoc/>
        public override string[] HintSubst => new string[] { hint };

        /// <inheritdoc/>
        public override ErrorSeverity Severity => severity;
    }
}
