using System.Collections.Generic;
using System.IO;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf;
#if VQT_HAS_NDMF_ERROR_REPORT
using nadena.dev.ndmf.localization;
#else
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Error for material conversion.
    /// </summary>
    internal class MaterialConversionError : SimpleError
    {
        private readonly string materialName;
        private readonly string shaderName;
        private readonly System.Exception exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialConversionError"/> class.
        /// </summary>
        /// <param name="materialReference">Reference for converting material.</param>
        /// <param name="exception">Throwed exception.</param>
        public MaterialConversionError(ObjectReference materialReference, System.Exception exception)
        {
            materialName = materialReference.Object.name;
            shaderName = ((Material)materialReference.Object).shader.name;
            if (exception is MaterialConversionException)
            {
                this.exception = exception.InnerException ?? exception;
            }
            else
            {
                this.exception = exception;
            }
            _references = new List<ObjectReference> { materialReference };
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.MaterialConversionErrorTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.MaterialConversionErrorDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { materialName, shaderName, exception.GetType().Name, exception.Message, StackTrace };

        private string StackTrace => exception.StackTrace.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, string.Empty);
    }
}
